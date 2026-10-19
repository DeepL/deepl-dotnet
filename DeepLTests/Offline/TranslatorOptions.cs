// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DeepL;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class TranslatorOptions : OfflineTest {
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
      Assert.Equal("deepl-dotnet/" + DeepL.Translator.Version(), userAgent);
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

    [Fact]
    public void ThrowsArgumentExceptionWhenAuthKeyIsEmpty() {
      Assert.Throws<ArgumentException>(() => new DeepL.Translator(""));
      Assert.Throws<ArgumentException>(() => new DeepL.Translator("  "));
      Assert.Throws<ArgumentNullException>(() => new DeepL.Translator(null!));
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
      var translator = new DeepL.Translator(
            AuthKey,
            new DeepL.TranslatorOptions {
              ClientFactory = () => new HttpClientAndDisposeFlag { DisposeClient = disposeClient, HttpClient = client }
            });
      translator.Dispose();
      Assert.Equal(disposeClient, client.WasDisposed);
    }

    [Fact]
    public void ReportsAuthKeyOfFreeAccount() {
      Assert.True(DeepL.Translator.AuthKeyIsFreeAccount("ABCD:fx "));
      Assert.False(DeepL.Translator.AuthKeyIsFreeAccount("ABCD "));
    }

    [Fact]
    public void ReportsVersions() {
      Assert.Equal("1.22.1", DeepL.Translator.Version());
      // The assembly version must remain unchanged for binary compatibility, except for the major version.
      Assert.Equal("1.0.0.0", typeof(DeepL.Translator).Assembly.GetName().Version?.ToString());
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
