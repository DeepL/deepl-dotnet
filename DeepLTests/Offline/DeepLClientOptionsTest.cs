// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DeepL;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class DeepLClientOptionsTest : OfflineTest {
    private const string UsageReply = "{\"character_count\":1,\"character_limit\":2}";

    private async Task<string> SendAndCaptureUserAgent(Action<DeepLClientOptions> configure) {
      string? userAgent = null;
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .With(
                  request => {
                    userAgent = request.Headers.UserAgent.ToString();
                    return true;
                  })
            .RespondJson(UsageReply);
      await CreateClient(configure).GetUsageAsync();
      return userAgent!;
    }

    [Fact]
    public async Task SendsPlatformInfoInUserAgentByDefault() {
      var userAgent = await SendAndCaptureUserAgent(_ => { });
      Assert.StartsWith("deepl-dotnet/", userAgent);
      Assert.Contains(" (", userAgent);
      Assert.Contains("dotnet-clr/", userAgent);
    }

    [Fact]
    public async Task SendsOnlyLibraryVersionInUserAgentWhenPlatformInfoIsOff() {
      var userAgent = await SendAndCaptureUserAgent(options => options.sendPlatformInfo = false);
      Assert.Equal("deepl-dotnet/" + DeepLClient.Version(), userAgent);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SendsAppInfoInUserAgent(bool sendPlatformInfo) {
      var userAgent = await SendAndCaptureUserAgent(
            options => {
              options.sendPlatformInfo = sendPlatformInfo;
              options.appInfo = new AppInfo { AppName = "my-dotnet-test-app", AppVersion = "1.2.3" };
            });
      Assert.EndsWith(" my-dotnet-test-app/1.2.3", userAgent);
      Assert.Equal(sendPlatformInfo, userAgent.Contains("dotnet-clr/"));
    }

    [Fact]
    public async Task SendsCustomHeadersAndTrimmedAuthKey() {
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .WithHeaders("X-Custom", "1")
            .WithHeaders("Authorization", "DeepL-Auth-Key " + AuthKey)
            .RespondJson(UsageReply);
      var client = new DeepLClient(
            "  " + AuthKey + " ",
            new DeepLClientOptions {
              ServerUrl = ApiUrl,
              Headers = new Dictionary<string, string?> { { "X-Custom", "1" } },
              ClientFactory = () => new HttpClientAndDisposeFlag { HttpClient = Http.ToHttpClient(), DisposeClient = true }
            });
      await client.GetUsageAsync();
    }

    [Fact]
    public async Task SendsGivenUserAgentAndAuthorizationInsteadOfDefaults() {
      Http.Expect(HttpMethod.Get, ApiUrl + "/v2/usage")
            .WithHeaders("User-Agent", "my-agent")
            .WithHeaders("Authorization", "Bearer abc")
            .RespondJson(UsageReply);
      var client = CreateClient(
            options => options.Headers = new Dictionary<string, string?> {
              { "user-agent", "my-agent" }, { "AUTHORIZATION", "Bearer abc" }
            });
      await client.GetUsageAsync();
    }

    [Fact]
    public async Task KeepsTechnicalHeadersOfTheRequest() {
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v2/translate",
                  ("text", new[] { "a" }),
                  ("target_lang", "de"),
                  ("show_billed_characters", true))
            .With(request => request.Headers.Host == null && request.Headers.TransferEncodingChunked != true)
            .RespondJson("{\"translations\":[{\"detected_source_language\":\"EN\",\"text\":\"b\"}]}");
      var client = CreateClient(
            options => options.Headers = new Dictionary<string, string?> {
              { "Host", "example.com" },
              { "Content-Type", "text/plain" },
              { "content-length", "1" },
              { "Transfer-Encoding", "chunked" }
            });
      await client.TranslateTextAsync("a", null, "de");
    }

    [Fact]
    public async Task SendsContentHeadersOnTheRequestContentAndSkipsThemWithoutBody() {
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v2/translate",
                  ("text", new[] { "a" }),
                  ("target_lang", "de"),
                  ("show_billed_characters", true))
            .With(request => request.Content!.Headers.ContentLanguage.Contains("de"))
            .RespondJson("{\"translations\":[{\"detected_source_language\":\"EN\",\"text\":\"b\"}]}");
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson(UsageReply);
      var client = CreateClient(
            options => options.Headers = new Dictionary<string, string?> { { "Content-Language", "de" } });
      await client.TranslateTextAsync("a", null, "de");
      await client.GetUsageAsync();
    }

    [Fact]
    public async Task KeepsTheSdksAcceptHeader() {
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .With(request => request.Headers.GetValues("Accept").SequenceEqual(new[] { "application/json" }))
            .RespondJson(UsageReply);
      await CreateClient(options => options.Headers = new Dictionary<string, string?> { { "accept", "text/plain" } })
            .GetUsageAsync();
    }

    [Fact]
    public async Task SendsRequestsToFreeServerWhenAuthKeyEndsWithFx() {
      Http.Expect(HttpMethod.Get, "https://api-free.deepl.com/v2/usage").RespondJson(UsageReply);
      var client = new DeepLClient(
            "abc:fx",
            new DeepLClientOptions {
              ClientFactory = () => new HttpClientAndDisposeFlag { HttpClient = Http.ToHttpClient(), DisposeClient = true }
            });
      await client.GetUsageAsync();
    }

    [Fact]
    public async Task SendsRequestsToProServerWhenAuthKeyHasNoFx() {
      Http.Expect(HttpMethod.Get, "https://api.deepl.com/v2/usage").RespondJson(UsageReply);
      var client = new DeepLClient(
            "abc",
            new DeepLClientOptions {
              ClientFactory = () => new HttpClientAndDisposeFlag { HttpClient = Http.ToHttpClient(), DisposeClient = true }
            });
      await client.GetUsageAsync();
    }

    [Theory]
    [InlineData("https://external-api.example.com/deepl/")]
    [InlineData("https://external-api.example.com/deepl")]
    public async Task KeepsPathPrefixOfServerUrl(string serverUrl) {
      Http.Expect(HttpMethod.Get, "https://external-api.example.com/deepl/v2/usage").RespondJson(UsageReply);
      await CreateClient(options => options.ServerUrl = serverUrl).GetUsageAsync();
    }

    [Fact]
    public void ThrowsUriFormatExceptionWhenServerUrlIsInvalid() =>
          Assert.Throws<UriFormatException>(() => CreateClient(options => options.ServerUrl = "http:/api.deepl.com"));

    [Theory]
    [InlineData("proxy.example.com", UriKind.Relative)]
    [InlineData("proxy.example.com:3128", UriKind.Absolute)]
    [InlineData("ftp://proxy.example.com", UriKind.Absolute)]
    public void ThrowsArgumentExceptionWhenProxyIsNotAnAbsoluteHttpUrl(string proxy, UriKind kind) {
      Assert.Throws<ArgumentException>(
            () => new DeepLClient("key", new DeepLClientOptions { Proxy = new Uri(proxy, kind) }));
      Assert.Throws<ArgumentException>(
            () => new DeepLAdminClient("key", new DeepLAdminClientOptions { Proxy = new Uri(proxy, kind) }));
    }

    [Fact]
    public void ThrowsArgumentExceptionWhenAuthKeyIsEmpty() {
      Assert.Throws<ArgumentException>(() => new DeepLClient(""));
      Assert.Throws<ArgumentException>(() => new DeepLClient("  "));
      Assert.Throws<ArgumentNullException>(() => new DeepLClient(null!));
    }

    [Fact]
    public void ThrowsArgumentNullExceptionWhenClientFactoryReturnsNoClient() =>
          Assert.Throws<ArgumentNullException>(
                () => CreateClient(
                      options => options.ClientFactory = () => new HttpClientAndDisposeFlag { HttpClient = null! }));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DisposesClientOfFactoryOnlyWhenAsked(bool disposeClient) {
      var client = new TrackingHttpClient(Http);
      var deepLClient = new DeepLClient(
            AuthKey,
            new DeepLClientOptions {
              ClientFactory = () => new HttpClientAndDisposeFlag { DisposeClient = disposeClient, HttpClient = client }
            });
      deepLClient.Dispose();
      Assert.Equal(disposeClient, client.WasDisposed);
    }

    [Fact]
    public void ReportsFreeAccountAuthKey() {
      Assert.True(DeepLClient.IsFreeAccountAuthKey("ABCD:fx "));
      Assert.False(DeepLClient.IsFreeAccountAuthKey("ABCD "));
    }

    [Fact]
    public void ReportsVersions() {
      Assert.Equal("1.23.0", DeepLClient.Version());
      // The assembly version must remain unchanged for binary compatibility, except for the major version.
      Assert.Equal("1.0.0.0", typeof(DeepLClient).Assembly.GetName().Version?.ToString());
    }

    private sealed class TrackingHttpClient : System.Net.Http.HttpClient {
      public TrackingHttpClient(HttpMessageHandler handler) : base(handler, false) { }

      public bool WasDisposed { get; private set; }

      protected override void Dispose(bool disposing) {
        WasDisposed = true;
        base.Dispose(disposing);
      }
    }
  }
}
