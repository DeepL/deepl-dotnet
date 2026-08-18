// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DeepL;
using Xunit;

namespace DeepLTests {
  public sealed class GeneralTest : BaseDeepLTest {
    /// <summary>
    ///   Tests assembly version begins with current version number.
    /// </summary>
    [Fact]
    public void TestVersion() {
      Assert.Equal("1.22.1", Translator.Version());

      // Note the assembly version must remain unchanged for binary compatibility, excepting the major version.
      Assert.Equal("1.0.0.0", typeof(Translator).Assembly.GetName().Version?.ToString());
    }

    /// <summary>
    ///   Tests translations of pre-prepared example texts to ensure translation is working.
    ///   The texts are translations of "proton beam".
    /// </summary>
    [Fact]
    public async Task TestExampleTranslation() {
      var translator = CreateTestTranslator();
      foreach (var sourceLanguage in ExpectedSourceLanguages()) {
        var inputText = ExampleText(sourceLanguage);
        var sourceLang = LanguageCode.RemoveRegionalVariant(sourceLanguage);
        var result = await translator.TranslateTextAsync(inputText, sourceLang, "en-US");
        Assert.Contains("proton", result.Text.ToLowerInvariant());
        Assert.Equal(inputText.Length, result.BilledCharacters);
      }
    }

    [Fact]
    public async Task TestDefaultUserAgentHeader() {
      var mockHandler = getMockHandler("{\"character_count\": 180118,\"character_limit\": 1250000}");
      var translator = new Translator(
            AuthKey,
            new TranslatorOptions {
              ClientFactory = () =>
                    new HttpClientAndDisposeFlag { HttpClient = new HttpClient(mockHandler), DisposeClient = true, }
            });
      var usage = await translator.GetUsageAsync();
      Assert.Single(mockHandler.requests);
      var userAgentHeader = mockHandler.requests[0].Headers.UserAgent;
      Assert.Contains("deepl-dotnet/", userAgentHeader.ToString());
      Assert.Contains("(", userAgentHeader.ToString());
      Assert.Contains("dotnet-clr/", userAgentHeader.ToString());
    }

    [Fact]
    public async Task TestOptInUserAgentHeader() {
      var mockHandler = getMockHandler("{\"character_count\": 180118,\"character_limit\": 1250000}");
      var translator = new Translator(
            AuthKey,
            new TranslatorOptions {
              sendPlatformInfo = true,
              ClientFactory = () =>
                    new HttpClientAndDisposeFlag { HttpClient = new HttpClient(mockHandler), DisposeClient = true, }
            });
      var usage = await translator.GetUsageAsync();
      Assert.Single(mockHandler.requests);
      var userAgentHeader = mockHandler.requests[0].Headers.UserAgent;
      Assert.Contains("deepl-dotnet/", userAgentHeader.ToString());
      Assert.Contains("(", userAgentHeader.ToString());
      Assert.Contains("dotnet-clr/", userAgentHeader.ToString());
    }

    [Fact]
    public async Task TestOptOutUserAgentHeader() {
      var mockHandler = getMockHandler("{\"character_count\": 180118,\"character_limit\": 1250000}");
      var translator = new Translator(
            AuthKey,
            new TranslatorOptions {
              sendPlatformInfo = false,
              ClientFactory = () =>
                    new HttpClientAndDisposeFlag { HttpClient = new HttpClient(mockHandler), DisposeClient = true, }
            });
      var usage = await translator.GetUsageAsync();
      Assert.Single(mockHandler.requests);
      var userAgentHeader = mockHandler.requests[0].Headers.UserAgent;
      Assert.Contains("deepl-dotnet/", userAgentHeader.ToString());
      Assert.DoesNotContain("(", userAgentHeader.ToString());
      Assert.DoesNotContain("dotnet-clr/", userAgentHeader.ToString());
    }

    [Fact]
    public async Task TestDefaultUserAgentHeaderWithAppInfo() {
      var mockHandler = getMockHandler("{\"character_count\": 180118,\"character_limit\": 1250000}");
      var translator = new Translator(
            AuthKey,
            new TranslatorOptions {
              sendPlatformInfo = true,
              appInfo = new AppInfo { AppName = "my-dotnet-test-app", AppVersion = "1.2.3" },
              ClientFactory = () =>
                    new HttpClientAndDisposeFlag { HttpClient = new HttpClient(mockHandler), DisposeClient = true, }
            });
      var usage = await translator.GetUsageAsync();
      Assert.Single(mockHandler.requests);
      var userAgentHeader = mockHandler.requests[0].Headers.UserAgent;
      Assert.Contains("deepl-dotnet/", userAgentHeader.ToString());
      Assert.Contains("(", userAgentHeader.ToString());
      Assert.Contains("dotnet-clr/", userAgentHeader.ToString());
      Assert.Contains("my-dotnet-test-app/1.2.3", userAgentHeader.ToString());
    }

    [Fact]
    public async Task TestOptInUserAgentHeaderWithAppInfo() {
      var mockHandler = getMockHandler("{\"character_count\": 180118,\"character_limit\": 1250000}");
      var translator = new Translator(
            AuthKey,
            new TranslatorOptions {
              sendPlatformInfo = true,
              appInfo = new AppInfo { AppName = "my-dotnet-test-app", AppVersion = "1.2.3" },
              ClientFactory = () =>
                    new HttpClientAndDisposeFlag { HttpClient = new HttpClient(mockHandler), DisposeClient = true, }
            });
      var usage = await translator.GetUsageAsync();
      Assert.Single(mockHandler.requests);
      var userAgentHeader = mockHandler.requests[0].Headers.UserAgent;
      Assert.Contains("deepl-dotnet/", userAgentHeader.ToString());
      Assert.Contains("(", userAgentHeader.ToString());
      Assert.Contains("dotnet-clr/", userAgentHeader.ToString());
      Assert.Contains("my-dotnet-test-app/1.2.3", userAgentHeader.ToString());
    }

    [Fact]
    public async Task TestOptOutUserAgentHeaderWithAppInfo() {
      var mockHandler = getMockHandler("{\"character_count\": 180118,\"character_limit\": 1250000}");
      var translator = new Translator(
            AuthKey,
            new TranslatorOptions {
              sendPlatformInfo = false,
              appInfo = new AppInfo { AppName = "my-dotnet-test-app", AppVersion = "1.2.3" },
              ClientFactory = () =>
                    new HttpClientAndDisposeFlag { HttpClient = new HttpClient(mockHandler), DisposeClient = true, }
            });
      var usage = await translator.GetUsageAsync();
      Assert.Single(mockHandler.requests);
      var userAgentHeader = mockHandler.requests[0].Headers.UserAgent;
      Assert.Contains("deepl-dotnet/", userAgentHeader.ToString());
      Assert.DoesNotContain("(", userAgentHeader.ToString());
      Assert.DoesNotContain("dotnet-clr/", userAgentHeader.ToString());
      Assert.Contains("my-dotnet-test-app/1.2.3", userAgentHeader.ToString());
    }

    [Fact]
    public async Task TestInvalidAuthKey() {
      var translator = new Translator("invalid", new TranslatorOptions { ServerUrl = ServerUrl });
      await Assert.ThrowsAsync<AuthorizationException>(() => translator.GetUsageAsync());
    }

    [RealServerOnlyFact]
    public async Task TestMixedDirectionText() {
      var translator = CreateTestTranslator();
      var options = new TextTranslateOptions { TagHandling = "xml", IgnoreTags = { "ignore" } };
      const string arIgnorePart = "<ignore>يجب تجاهل هذا الجزء.</ignore>";
      const string enSentenceWithArIgnorePart =
            "<p>This is a <b>short</b> <i>sentence</i>.</p>" + arIgnorePart + " This is another sentence.";
      const string enIgnorePart = "<ignore>This part should be ignored.</ignore>";
      const string arSentenceWithEnIgnorePart =
            "<p>هذه <i>جملة</i> <b>قصيرة</b>. " + enIgnorePart + " هذه جملة أخرى.</p>";

      var enResult = await translator.TranslateTextAsync(enSentenceWithArIgnorePart, null, "en-US", options);
      Assert.Contains(arIgnorePart, enResult.Text);
      var arResult = await translator.TranslateTextAsync(arSentenceWithEnIgnorePart, null, "ar", options);
      Assert.Contains(enIgnorePart, arResult.Text);
    }

    [Fact]
    public void TestModelTypeEnum() {
      var apiValueSet = new HashSet<string>();
      // Check each enum value gives a valid, distinct ApiValue
      foreach (ModelType? i in Enum.GetValues(typeof(ModelType))) {
        var apiValue = i!.Value.ToApiValue();
        Assert.DoesNotContain(apiValue, apiValueSet);
        apiValueSet.Add(apiValue);
      }
    }

    [Fact]
    public void TestInvalidServerUrl() =>
          Assert.ThrowsAny<Exception>(
                () => new Translator(AuthKey, new TranslatorOptions { ServerUrl = "http:/api.deepl.com" }));

    [Fact]
    public async Task TestServerUrlWithPathPrefix() {
      // Verify that a ServerUrl with a path prefix (e.g. a reverse-proxy base path) is preserved
      // when constructing API request URLs. Previously, relative URIs starting with "/" caused
      // RFC 3986 URI resolution to discard the base path.
      const string usageResponseJson = "{\"character_count\": 0, \"character_limit\": 0}";
      var handler = getMockHandler(usageResponseJson);
      var options = new TranslatorOptions {
        ServerUrl = "https://external-api.example.com/deepl/",
        ClientFactory = () => new HttpClientAndDisposeFlag {
          HttpClient = new HttpClient(handler),
          DisposeClient = true
        }
      };
      var translator = new Translator("test-auth-key:fx", options);
      await translator.GetUsageAsync();

      Assert.Single(handler.requests);
      var requestUri = handler.requests[0].RequestUri!.ToString();
      Assert.StartsWith("https://external-api.example.com/deepl/", requestUri);
    }

    [Fact]
    public async Task TestUsage() {
      var translator = CreateTestTranslator();
      var usage = await translator.GetUsageAsync();
      Assert.Contains("Usage this billing period", usage.ToString()!);
    }

    [Fact]
    public async Task TestGetSourceLanguages() {
      var translator = CreateTestTranslator();
      var sourceLanguages = await translator.GetSourceLanguagesAsync();

      foreach (var expectedSourceLanguage in ExpectedSourceLanguages()) {
        Assert.Contains(sourceLanguages, language => language.Code == expectedSourceLanguage);
      }

      Assert.Equal(sourceLanguages[0], sourceLanguages[0]);
      Assert.NotEqual(sourceLanguages[0], sourceLanguages[1]);
    }

    [Fact]
    public async Task TestGetTargetLanguages() {
      var translator = CreateTestTranslator();
      var targetLanguages = await translator.GetTargetLanguagesAsync();

      foreach (var expectedTargetLanguage in ExpectedTargetLanguages()) {
        Assert.Contains(targetLanguages, language => language.Code == expectedTargetLanguage);
      }

      Assert.Equal(targetLanguages[0], targetLanguages[0]);
      Assert.NotEqual(targetLanguages[0], targetLanguages[1]);
    }

    [Fact]
    public async Task TestGetGlossaryLanguages() {
      var translator = CreateTestTranslator();
      var glossaryLanguages = await translator.GetGlossaryLanguagesAsync();
      Assert.True(glossaryLanguages.Length > 0);
      foreach (var glossaryLanguagePair in glossaryLanguages) {
        Assert.True(glossaryLanguagePair.SourceLanguageCode.Length > 0);
        Assert.True(glossaryLanguagePair.TargetLanguageCode.Length > 0);
      }
    }

    [Fact]
    public void TestServerUrlSelectedBasedOnAuthKey() {
      Assert.True(Translator.AuthKeyIsFreeAccount("ABCD:fx "));
      Assert.False(Translator.AuthKeyIsFreeAccount("ABCD "));
    }

    [MockProxyServerOnlyFact]
    public async Task TestProxyUsage() {
      var translator = CreateTestTranslatorWithMockSession(
            nameof(TestUsageNoResponse),
            new SessionOptions { ExpectProxy = true },
            new TranslatorOptions {
              ServerUrl = ServerUrl,
              ClientFactory =
                        () => {
                          var handler = new HttpClientHandler() { Proxy = new WebProxy(ProxyUrl), UseProxy = true, };

                          return new HttpClientAndDisposeFlag {
                            HttpClient = new HttpClient(handler),
                            DisposeClient = true,
                          };
                        }
            });

      await translator.GetUsageAsync();
    }

    [MockServerOnlyFact]
    public async Task TestUsageNoResponse() {
      // Lower the retry count and timeout for this test
      var translator = CreateTestTranslatorWithMockSession(
            nameof(TestUsageNoResponse),
            new SessionOptions { NoResponse = 2 },
            new TranslatorOptions {
              PerRetryConnectionTimeout = TimeSpan.FromMilliseconds(1),
              MaximumNetworkRetries = 0
            });

      await Assert.ThrowsAsync<ConnectionException>(() => translator.GetUsageAsync());
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTooManyRequests() {
      // Lower the retry count and timeout for this test
      var translator = CreateTestTranslatorWithMockSession(
            nameof(TestTranslateTooManyRequests),
            new SessionOptions { RespondWith429 = 2 },
            new TranslatorOptions { PerRetryConnectionTimeout = TimeSpan.FromSeconds(1), MaximumNetworkRetries = 1 });

      await Assert.ThrowsAsync<TooManyRequestsException>(
            () =>
                  translator.TranslateTextAsync(ExampleText("en"), null, "DE"));
    }

    [MockServerOnlyFact]
    public async Task TestUsageOverrun() {
      const int characterLimit = 20;
      const int documentLimit = 1;
      var translator = CreateTestTranslatorWithMockSession(
            nameof(TestUsageOverrun),
            new SessionOptions { InitCharacterLimit = characterLimit, InitDocumentLimit = documentLimit },
            randomAuthKey: true);

      var usage = await translator.GetUsageAsync();
      Assert.Equal(0, usage.Character?.Count);
      Assert.Equal(0, usage.Document?.Count);
      Assert.Equal(characterLimit, usage.Character?.Limit);
      Assert.Equal(documentLimit, usage.Document?.Limit);
      Assert.Contains("Characters: 0 of 20", usage.ToString()!);
      Assert.Contains("Documents: 0 of 1", usage.ToString()!);

      var tempDir = TempDir();
      var inputPath = Path.Combine(tempDir, "example_document.txt");
      File.WriteAllText(inputPath, new string('a', characterLimit));
      var outputPath = Path.Combine(tempDir, "example_document_output.txt");
      File.Delete(outputPath);

      await translator.TranslateDocumentAsync(new FileInfo(inputPath), new FileInfo(outputPath), null, "DE");

      usage = await translator.GetUsageAsync();
      Assert.True(usage.AnyLimitReached);
      Assert.True(usage.Document?.LimitReached ?? false);
      Assert.True(usage.Character?.LimitReached ?? false);
      Assert.False(usage.TeamDocument?.LimitReached ?? false);

      await Assert.ThrowsAsync<IOException>(
            async () => await translator.TranslateDocumentAsync(
                  new FileInfo(inputPath),
                  new FileInfo(outputPath),
                  null,
                  "DE"));
      File.Delete(outputPath);

      var exception = await Assert.ThrowsAsync<DocumentTranslationException>(
            () =>
                  translator.TranslateDocumentAsync(new FileInfo(inputPath), new FileInfo(outputPath), null, "DE"));
      Assert.Null(exception.DocumentHandle);
      Assert.Equal(typeof(QuotaExceededException), exception.InnerException!.GetType());

      await Assert.ThrowsAsync<QuotaExceededException>(
            () => translator.TranslateTextAsync(ExampleText("en"), null, "DE"));
    }

    [MockServerOnlyFact]
    public async Task TestUsageTeamDocumentLimit() {
      const int teamDocumentLimit = 1;
      var translator = CreateTestTranslatorWithMockSession(
            nameof(TestUsageOverrun),
            new SessionOptions {
              InitCharacterLimit = 0,
              InitDocumentLimit = 0,
              InitTeamDocumentLimit = teamDocumentLimit
            },
            randomAuthKey: true);


      var usage = await translator.GetUsageAsync();
      Assert.False(usage.AnyLimitReached);
      Assert.Equal(0, usage.TeamDocument?.Count);
      Assert.Equal(teamDocumentLimit, usage.TeamDocument?.Limit);
      Assert.DoesNotContain("Characters", usage.ToString()!);
      Assert.DoesNotContain("Documents", usage.ToString()!);
      Assert.Contains("Team documents: 0 of 1", usage.ToString()!);

      var tempDir = TempDir();
      var inputPath = Path.Combine(tempDir, "example_document.txt");
      File.WriteAllText(inputPath, "a");
      var outputPath = Path.Combine(tempDir, "example_document_output.txt");
      File.Delete(outputPath);
      await translator.TranslateDocumentAsync(new FileInfo(inputPath), new FileInfo(outputPath), null, "DE");

      usage = await translator.GetUsageAsync();
      Assert.True(usage.AnyLimitReached);
      Assert.False(usage.Document?.LimitReached ?? false);
      Assert.False(usage.Character?.LimitReached ?? false);
      Assert.True(usage.TeamDocument?.LimitReached ?? false);
    }

    [Fact]
    public void TestClientFactory() {
      var clientWrapper = new HttpClientWrapper();

      HttpClientAndDisposeFlag Factory() {
        return new HttpClientAndDisposeFlag { DisposeClient = false, HttpClient = clientWrapper };
      }

      var translator = new Translator(AuthKey, new TranslatorOptions { ClientFactory = Factory });
      translator.Dispose();
      Assert.False(clientWrapper.WasDisposed);
    }

    private class HttpClientWrapper : HttpClient {
      public bool WasDisposed { get; private set; }

      protected override void Dispose(bool disposing) {
        WasDisposed = true;
        base.Dispose(disposing);
      }
    }
  }
}
