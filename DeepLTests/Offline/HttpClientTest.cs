// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeepL;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  /// <summary>Retries and timeouts of the default HTTP client. These tests wait for the real back-off delay.</summary>
  public sealed class HttpClientTest : OfflineTest {
    private const string UsageReply = "{\"character_count\":1,\"character_limit\":2}";
    private static readonly TimeSpan PerRetryTimeout = TimeSpan.FromSeconds(10);

    private static Func<Task<HttpResponseMessage>> Slow() =>
          async () => {
            await Task.Delay(TimeSpan.FromSeconds(3));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
          };

    [Fact]
    public async Task RetriesOn429() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson("{}", (HttpStatusCode)429);
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson(UsageReply);
      await CreateClientWithRetries(1, PerRetryTimeout).GetUsageAsync();
    }

    [Fact]
    public async Task RetriesOn5xx() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson("{}", HttpStatusCode.BadGateway);
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson(UsageReply);
      await CreateClientWithRetries(1, PerRetryTimeout).GetUsageAsync();
    }

    [Fact]
    public async Task RetriesPostWithTheSameBody() {
      ExpectForm(HttpMethod.Post, "v2/translate", ("target_lang", "de"), ("show_billed_characters", "1"), ("text", "a"))
            .RespondJson("{}", (HttpStatusCode)429);
      ExpectForm(HttpMethod.Post, "v2/translate", ("target_lang", "de"), ("show_billed_characters", "1"), ("text", "a"))
            .RespondJson("{\"translations\":[{\"detected_source_language\":\"EN\",\"text\":\"b\",\"billed_characters\":1}]}");
      await CreateClientWithRetries(1, PerRetryTimeout).TranslateTextAsync("a", null, "DE");
    }

    [Fact]
    public async Task RetriesTranslationMemoryUploadWithTheFullBody() {
      const string url = "https://storage.example.com/upload";
      const string body = "<tmx/>";
      Http.Expect(HttpMethod.Put, url).WithContent(body).Respond(HttpStatusCode.ServiceUnavailable, "text/plain", "busy");
      Http.Expect(HttpMethod.Put, url).WithContent(body).Respond(HttpStatusCode.OK);
      await CreateClientWithRetries(1, PerRetryTimeout)
            .UploadTranslationMemoryFileAsync(url, new MemoryStream(Encoding.UTF8.GetBytes(body)));
    }

    [Fact]
    public async Task RetriesAfterPerRetryTimeout() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").Respond(Slow());
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson(UsageReply);
      await CreateClientWithRetries(1, TimeSpan.FromMilliseconds(200)).GetUsageAsync();
    }

    [Fact]
    public async Task RunsAttemptUntilBackoffDeadlineWhenLongerThanTimeout() {
      // The first backoff is 1 s, so a 100 ms timeout is extended and the 300 ms reply arrives in time.
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .Respond(
                  async () => {
                    await Task.Delay(TimeSpan.FromMilliseconds(300));
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(UsageReply) };
                  });
      await CreateClientWithRetries(1, TimeSpan.FromMilliseconds(100)).GetUsageAsync();
    }

    /// <summary>Returns a fixed value from <see cref="Random.NextDouble" />.</summary>
    private sealed class FixedRandom : Random {
      private readonly double _value;
      public FixedRandom(double value) => _value = value;
      public override double NextDouble() => _value;
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.999)]
    public void FirstBackoffIsOneSecondWithoutJitter(double random) =>
          Assert.Equal(
                TimeSpan.FromSeconds(1),
                DeepL.Internal.DeepLHttpClient.ComputeBackoff(0, new FixedRandom(random)));

    [Fact]
    public async Task KeepsInfinitePerRetryTimeoutInfinite() {
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .Respond(
                  async () => {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(UsageReply) };
                  });
      await CreateClientWithRetries(0, Timeout.InfiniteTimeSpan).GetUsageAsync();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void RejectsZeroOrNegativePerRetryTimeout(int milliseconds) =>
          Assert.Throws<ArgumentOutOfRangeException>(
                () => new DeepLClient(
                      "key",
                      new DeepLClientOptions { PerRetryConnectionTimeout = TimeSpan.FromMilliseconds(milliseconds) }));

    [Fact]
    public void DefaultClientHasNoOverallTimeout() {
      using var client = DeepL.Internal.DeepLHttpClient.CreateDefaultHttpClient(new DeepLClientOptions(), Http)
            .HttpClient;
      Assert.Equal(Timeout.InfiniteTimeSpan, client.Timeout);
    }

    [Fact]
    public async Task DoesNotRetryOtherExceptions() {
      var attempts = 0;
      Http.When(HttpMethod.Get, ApiUrl + "/v2/usage")
            .With(
                  _ => {
                    attempts++;
                    return true;
                  })
            .Throw(new InvalidOperationException("not transient"));
      var exception = await Assert.ThrowsAsync<ConnectionException>(
            () => CreateClientWithRetries(2, PerRetryTimeout).GetUsageAsync());
      Assert.Equal(1, attempts);
      Assert.Null(exception.ShouldRetry);
    }

    /// <summary>Makes every GET of v2/usage throw the given exception and counts the attempts.</summary>
    private Func<int> ThrowOnUsage(Exception exception) {
      var attempts = 0;
      Http.When(HttpMethod.Get, ApiUrl + "/v2/usage")
            .With(
                  _ => {
                    attempts++;
                    return true;
                  })
            .Throw(exception);
      return () => attempts;
    }

    [Fact]
    public async Task DoesNotRetryTlsFailure() {
      var attempts = ThrowOnUsage(
            new HttpRequestException(
                  "The SSL connection could not be established",
                  new AuthenticationException("The remote certificate is invalid")));
      var exception = await Assert.ThrowsAsync<ConnectionException>(
            () => CreateClientWithRetries(2, PerRetryTimeout).GetUsageAsync());
      Assert.Equal(1, attempts());
      Assert.False(exception.ShouldRetry);
    }

    [Fact]
    public async Task DoesNotRetryProxyTunnelRejection() {
      // SocketsHttpHandler reports a rejected CONNECT as an HttpRequestException without an inner exception.
      var attempts = ThrowOnUsage(
            new HttpRequestException(
                  "The proxy tunnel request to proxy 'http://proxy:3128/' failed with status code '403'."));
      var exception = await Assert.ThrowsAsync<ConnectionException>(
            () => CreateClientWithRetries(2, PerRetryTimeout).GetUsageAsync());
      Assert.Equal(1, attempts());
      Assert.False(exception.ShouldRetry);
    }

    [Fact]
    public async Task RetriesConnectionReset() {
      var attempts = ThrowOnUsage(
            new HttpRequestException(
                  "reset",
                  new IOException("read", new SocketException((int)SocketError.ConnectionReset))));
      var exception = await Assert.ThrowsAsync<ConnectionException>(
            () => CreateClientWithRetries(1, PerRetryTimeout).GetUsageAsync());
      Assert.Equal(2, attempts());
      Assert.True(exception.ShouldRetry);
    }

    [Fact]
    public async Task ThrowsTooManyRequestsExceptionWhenRetriesAreExhausted() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson("{}", (HttpStatusCode)429);
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson("{}", (HttpStatusCode)429);
      await Assert.ThrowsAsync<TooManyRequestsException>(
            () => CreateClientWithRetries(1, PerRetryTimeout).GetUsageAsync());
    }

    [Fact]
    public async Task DoesNotRetryWhenRetriesAreZero() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson("{}", (HttpStatusCode)429);
      await Assert.ThrowsAsync<TooManyRequestsException>(
            () => CreateClientWithRetries(0, PerRetryTimeout).GetUsageAsync());
    }

    [Fact]
    public async Task DoesNotRetryOn4xx() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson("{\"message\":\"bad\"}", HttpStatusCode.BadRequest);
      await Assert.ThrowsAsync<DeepLException>(() => CreateClientWithRetries(2, PerRetryTimeout).GetUsageAsync());
    }

    [Fact]
    public async Task ThrowsConnectionExceptionWhenPerRetryTimeoutIsExceededWithoutRetries() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").Respond(Slow());
      var exception = await Assert.ThrowsAsync<ConnectionException>(
            () => CreateClientWithRetries(0, TimeSpan.FromMilliseconds(200)).GetUsageAsync());
      Assert.StartsWith("Request timed out", exception.Message);
    }

    [Fact]
    public async Task ThrowsConnectionExceptionWhenOverallTimeoutIsExceeded() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").Respond(Slow());
      var client = CreateClient(
            options => options.ClientFactory = () => new HttpClientAndDisposeFlag {
              HttpClient = new System.Net.Http.HttpClient(Http) { Timeout = TimeSpan.FromMilliseconds(200) },
              DisposeClient = true
            });
      var exception = await Assert.ThrowsAsync<ConnectionException>(() => client.GetUsageAsync());
      Assert.StartsWith("Request timed out", exception.Message);
    }

    [Fact]
    public async Task ThrowsOperationCanceledExceptionWhenCancelledByCallerAndHandlerThrowsPlainOne() {
      using var cancellation = new CancellationTokenSource();
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .Respond(
                  () => {
                    cancellation.Cancel();
                    throw new OperationCanceledException(cancellation.Token);
                  });
      await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CreateClient().GetUsageAsync(cancellation.Token));
    }

    [Fact]
    public async Task ThrowsConnectionExceptionWhenRequestFails() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").Throw(new HttpRequestException("refused"));
      var exception = await Assert.ThrowsAsync<ConnectionException>(() => CreateClient().GetUsageAsync());
      Assert.Equal("Request failed: refused", exception.Message);
    }
  }
}
