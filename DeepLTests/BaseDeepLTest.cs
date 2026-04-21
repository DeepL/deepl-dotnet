// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DeepL;
using Xunit;

namespace DeepLTests {
  public class BaseDeepLTest {
    protected static readonly bool IsMockServer = Environment.GetEnvironmentVariable("DEEPL_MOCK_SERVER_PORT") != null;
    protected static readonly string AuthKey;
    protected static readonly string? ServerUrl;
    protected static readonly string? ProxyUrl;
    protected static readonly Dictionary<string, string> DocMinificationTestFilesMapping;
    private static Random _random = new Random();

    static BaseDeepLTest() {
      if (IsMockServer) {
        AuthKey = "mock_server";
        ServerUrl = Environment.GetEnvironmentVariable("DEEPL_SERVER_URL") ?? throw new Exception(
              "DEEPL_SERVER_URL environment variable must be set when using mock server.");
      } else {
        AuthKey = Environment.GetEnvironmentVariable("DEEPL_AUTH_KEY") ?? throw new Exception(
              "DEEPL_AUTH_KEY environment variable must be set unless using mock server.");
        ServerUrl = Environment.GetEnvironmentVariable("DEEPL_SERVER_URL");
      }

      ProxyUrl = Environment.GetEnvironmentVariable("DEEPL_PROXY_URL");
      DocMinificationTestFilesMapping = new Dictionary<string, string>() {
            { ".docx", "example_document_template.docx" },
            { ".pptx", "example_presentation_template.pptx" },
            { ".zip", "example_zip_template.zip" }
      };
    }

    protected static DeepLClient CreateTestClient(bool randomAuthKey = false) {
      var authKey = randomAuthKey ? Guid.NewGuid().ToString() : AuthKey;
      return ServerUrl == null
            ? new DeepLClient(authKey)
            : new DeepLClient(authKey, new DeepLClientOptions { ServerUrl = ServerUrl });
    }

    protected static Translator CreateTestTranslator(bool randomAuthKey = false) {
      var authKey = randomAuthKey ? Guid.NewGuid().ToString() : AuthKey;
      return ServerUrl == null
            ? new Translator(authKey)
            : new Translator(authKey, new TranslatorOptions { ServerUrl = ServerUrl });
    }

    protected static Translator CreateTestTranslatorWithMockSession(
          string testName,
          SessionOptions sessionOptions,
          TranslatorOptions? translatorOptions = null,
          bool randomAuthKey = false) {
      if (!IsMockServer) {
        return CreateTestTranslator();
      }

      var authKey = randomAuthKey ? Guid.NewGuid().ToString() : AuthKey;
      var sessionHeaders = CreateSessionHeaders(testName, sessionOptions);
      translatorOptions = translatorOptions ?? new TranslatorOptions();
      translatorOptions.ServerUrl = ServerUrl;
      translatorOptions.Headers = sessionHeaders;
      return new Translator(authKey, translatorOptions);
    }

    protected static MockHttpMessageHandler getMockHandler(String responseMessage) {
      var response = new HttpResponseMessage(HttpStatusCode.OK);
      response.Content = new StringContent(responseMessage);
      return new MockHttpMessageHandler(response);
    }

    protected static string ExampleText(string language) {
      switch (language) {
        case "ar":
          return "شعاع البروتون";
        case "bg":
          return "протонен лъч";
        case "cs":
          return "protonový paprsek";
        case "da":
          return "Protonstråle";
        case "de":
          return "Protonenstrahl";
        case "el":
          return "δέσμη πρωτονίων";
        case "en":
        case "en-GB":
        case "en-US":
          return "proton beam";
        case "es":
          return "haz de protones";
        case "et":
          return "prootonikiirgus";
        case "fi":
          return "protonisäde";
        case "fr":
          return "faisceau de protons";
        case "hu":
          return "protonnyaláb";
        case "id":
          return "sinar proton";
        case "it":
          return "fascio di protoni";
        case "ja":
          return "陽子線";
        case "ko":
          return "양성자 빔";
        case "lt":
          return "protonų spindulys";
        case "lv":
          return "protonu staru kūlis";
        case "nb":
          return "protonstråle";
        case "nl":
          return "protonenbundel";
        case "pl":
          return "wiązka protonów";
        case "pt":
        case "pt-BR":
          return "feixe de prótons";
        case "pt-PT":
          return "feixe de protões";
        case "ro":
          return "fascicul de protoni";
        case "ru":
          return "протонный пучок";
        case "sk":
          return "protónový lúč";
        case "sl":
          return "protonski žarek";
        case "sv":
          return "protonstråle";
        case "tr":
          return "proton ışını";
        case "uk":
          return "протонний пучок";
        case "zh":
          return "质子束";
        default:
          throw new Exception("no example text for language " + language);
      }
    }

    protected static string[] ExpectedSourceLanguages() =>
          new[] {
                LanguageCode.Bulgarian, LanguageCode.Czech, LanguageCode.Danish, LanguageCode.German,
                LanguageCode.Greek, LanguageCode.English, LanguageCode.Spanish, LanguageCode.Estonian,
                LanguageCode.Finnish, LanguageCode.French, LanguageCode.Hungarian, LanguageCode.Indonesian,
                LanguageCode.Italian, LanguageCode.Japanese, LanguageCode.Korean, LanguageCode.Lithuanian,
                LanguageCode.Latvian, LanguageCode.Norwegian, LanguageCode.Dutch, LanguageCode.Polish,
                LanguageCode.Portuguese, LanguageCode.Romanian, LanguageCode.Russian, LanguageCode.Slovak,
                LanguageCode.Slovenian, LanguageCode.Swedish, LanguageCode.Turkish, LanguageCode.Ukrainian,
                LanguageCode.Chinese
          };

    protected static string[] ExpectedTargetLanguages() =>
          new[] {
                LanguageCode.Bulgarian, LanguageCode.Czech, LanguageCode.Danish, LanguageCode.German,
                LanguageCode.Greek, LanguageCode.EnglishBritish, LanguageCode.EnglishAmerican, LanguageCode.Spanish,
                LanguageCode.Estonian, LanguageCode.Finnish, LanguageCode.French, LanguageCode.Hungarian,
                LanguageCode.Indonesian, LanguageCode.Italian, LanguageCode.Japanese, LanguageCode.Korean,
                LanguageCode.Lithuanian, LanguageCode.Latvian, LanguageCode.Norwegian, LanguageCode.Dutch,
                LanguageCode.Polish, LanguageCode.PortugueseBrazilian, LanguageCode.PortugueseEuropean,
                LanguageCode.Romanian, LanguageCode.Russian, LanguageCode.Slovak, LanguageCode.Slovenian,
                LanguageCode.Swedish, LanguageCode.Turkish, LanguageCode.Ukrainian, LanguageCode.Chinese
          };

    private static Dictionary<string, string?> CreateSessionHeaders(string testName, SessionOptions options) {
      if (!IsMockServer) {
        return new Dictionary<string, string?>();
      }

      var uuid = Guid.NewGuid();
      var headers = new Dictionary<string, string?> {
            { "mock-server-session", $"deepl-dotnet-test/{testName}/{uuid}" }
      };

      if (options.NoResponse != null) {
        headers["mock-server-session-no-response-count"] = options.NoResponse.ToString();
      }

      if (options.RespondWith429 != null) {
        headers["mock-server-session-429-count"] = options.RespondWith429.ToString();
      }

      if (options.InitCharacterLimit != null) {
        headers["mock-server-session-init-character-limit"] = options.InitCharacterLimit.ToString();
      }

      if (options.InitDocumentLimit != null) {
        headers["mock-server-session-init-document-limit"] = options.InitDocumentLimit.ToString();
      }

      if (options.InitTeamDocumentLimit != null) {
        headers["mock-server-session-init-team-document-limit"] =
              options.InitTeamDocumentLimit.ToString();
      }

      if (options.DocumentFailure != null) {
        headers["mock-server-session-doc-failure"] = options.DocumentFailure.ToString();
      }

      if (options.DocumentQueueTime != null) {
        headers["mock-server-session-doc-queue-time"] =
              ((int)options.DocumentQueueTime.Value.TotalMilliseconds).ToString();
      }

      if (options.DocumentTranslateTime != null) {
        headers["mock-server-session-doc-translate-time"] =
              ((int)options.DocumentTranslateTime.Value.TotalMilliseconds).ToString();
      }

      if (options.ExpectProxy != null) {
        headers["mock-server-session-expect-proxy"] = options.ExpectProxy.Value ? "1" : "0";
      }

      return headers;
    }

    protected static string TempDir() {
      var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(path);
      return path;
    }

    protected static string GetFullPathForTestFile(string testFileName) {
      return Path.Combine(Directory.GetCurrentDirectory(), "resources", testFileName);
    }

    protected static string CreateMinifiedTestDocument(string extension, string outputDirectory) {
      var extractionDir = TempDir();
      var testFilePath = GetFullPathForTestFile(DocMinificationTestFilesMapping[extension]);
      var outputFilePath = Path.Combine(outputDirectory, "test_document" + extension);
      ZipFile.ExtractToDirectory(testFilePath, extractionDir);
      var characters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ~!@#$%^&*()_+=-<,>.?:";
      var length = 90000000;
      var createText = new string(
            Enumerable.Repeat(characters, length)
                  .Select(s => s[_random.Next(s.Length)])
                  .ToArray());
      File.WriteAllText(Path.Combine(extractionDir, "placeholder_image.png"), createText);
      ZipFile.CreateFromDirectory(extractionDir, outputFilePath);
      Directory.Delete(extractionDir, true);
      return outputFilePath;
    }

    protected bool AreDirectoriesEqual(string dir1, string dir2) {
      var dir1Info = new DirectoryInfo(dir1);
      var dir2Info = new DirectoryInfo(dir2);

      var dir1Files = dir1Info.GetFiles("*.*", SearchOption.AllDirectories);
      var dir2Files = dir2Info.GetFiles("*.*", SearchOption.AllDirectories);

      var dir1Hashes = dir1Files.ToDictionary(k => k.Name, GetHashForFile);
      var dir2Hashes = dir2Files.ToDictionary(k => k.Name, GetHashForFile);

      return dir1Hashes.Keys.Count == dir2Hashes.Keys.Count &&
             dir1Hashes.All(kvp => dir2Hashes.ContainsKey(kvp.Key) && dir2Hashes[kvp.Key].SequenceEqual(kvp.Value));
    }

    private byte[] GetHashForFile(FileInfo file) {
      using var fileStream = file.OpenRead();
      using var md5 = MD5.Create();
      return md5.ComputeHash(fileStream);
    }

    protected struct SessionOptions {
      public int? NoResponse;
      public int? RespondWith429;
      public int? InitCharacterLimit;
      public int? InitDocumentLimit;
      public int? InitTeamDocumentLimit;
      public int? DocumentFailure;
      public TimeSpan? DocumentQueueTime;
      public TimeSpan? DocumentTranslateTime;
      public bool? ExpectProxy;
    }

    protected sealed class MockServerOnlyFact : FactAttribute {
      public MockServerOnlyFact() {
        if (!IsMockServer) {
          Skip = "Only run if using mock server";
        }
      }
    }

    protected sealed class MockProxyServerOnlyFact : FactAttribute {
      public MockProxyServerOnlyFact() {
        if (!IsMockServer || ProxyUrl == null) {
          Skip = "Only run if using mock server with proxy";
        }
      }
    }

    protected sealed class RealServerOnlyFact : FactAttribute {
      public RealServerOnlyFact() {
        if (IsMockServer) {
          Skip = "Only run if using real server";
        }
      }
    }

    /// <summary>
    ///   Class to mock HTTP requests the library makes. Supports returning a constant response to every request
    ///   through <see cref="MockHttpMessageHandler.defaultResponse" />.
    ///   If we ever need more complex mocking functionality, we should drop this and use a mocking library.
    /// </summary>
    protected class MockHttpMessageHandler : HttpMessageHandler {
      /// <summary>
      ///   List of requests made through this mock. Use to make assertions in your tests after the code has run.
      /// </summary>
      public List<HttpRequestMessage> requests;

      /// <summary>
      ///   Default response returned on every HTTP request. If we need more complex functionality,
      ///   we should use a proper mocking library, for example Moq
      /// </summary>
      public HttpResponseMessage defaultResponse;

      public MockHttpMessageHandler(HttpResponseMessage response) : base() {
        defaultResponse = response;
        requests = new List<HttpRequestMessage>();
      }

      protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
        this.requests.Add(request);
        await Task.Delay(0);
        return defaultResponse;
      }
    }
  }
}
