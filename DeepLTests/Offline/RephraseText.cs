// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DeepL;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class RephraseText : OfflineTest {
    private const string Reply =
          "{\"improvements\":[{\"text\":\"better\",\"target_language\":\"EN-US\",\"detected_source_language\":\"EN\"}]}";

    [Fact]
    public async Task SendsTargetLanguageAndText() {
      ExpectForm(HttpMethod.Post, "v2/write/rephrase", ("target_lang", "en-US"), ("text", "good"))
            .RespondJson(Reply);
      await CreateClient().RephraseTextAsync("good", LanguageCode.EnglishAmerican);
    }

    [Fact]
    public async Task SendsOnlyTextWhenTargetLanguageIsNull() {
      ExpectForm(HttpMethod.Post, "v2/write/rephrase", ("text", "good")).RespondJson(Reply);
      await CreateClient().RephraseTextAsync("good", null);
    }

    [Fact]
    public async Task SendsWritingStyleAndTone() {
      ExpectForm(
                  HttpMethod.Post,
                  "v2/write/rephrase",
                  ("target_lang", "en"),
                  ("writing_style", "business"),
                  ("tone", "friendly"),
                  ("text", "good"))
            .RespondJson(Reply);
      await CreateClient().RephraseTextAsync(
            "good",
            LanguageCode.English,
            new TextRephraseOptions { WritingStyle = "business", WritingTone = "friendly" });
    }

    [Fact]
    public async Task SendsTextArrayInOrder() {
      Expect(HttpMethod.Post, "v2/write/rephrase")
            .WithContent("target_lang=en-US&text=a&text=b")
            .RespondJson(
                  "{\"improvements\":[{\"text\":\"x\",\"target_language\":\"EN-US\",\"detected_source_language\":\"EN\"}," +
                  "{\"text\":\"y\",\"target_language\":\"EN-US\",\"detected_source_language\":\"DE\"}]}");
      await CreateClient().RephraseTextAsync(new[] { "a", "b" }, LanguageCode.EnglishAmerican);
    }

    [Fact]
    public async Task ParsesImprovement() {
      ExpectForm(HttpMethod.Post, "v2/write/rephrase", ("text", "good")).RespondJson(Reply);
      var result = await CreateClient().RephraseTextAsync("good", null);
      Assert.Equal("en", result.DetectedSourceLanguageCode);
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenTextIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().RephraseTextAsync("", "en"));

    [Fact]
    public async Task ThrowsArgumentExceptionWhenTargetLanguageIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().RephraseTextAsync("a", ""));
  }
}
