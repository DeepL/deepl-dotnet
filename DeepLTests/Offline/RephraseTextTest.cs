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
  public sealed class RephraseTextTest : OfflineTest {
    private const string Reply =
          "{\"improvements\":[{\"text\":\"better\",\"target_language\":\"EN-US\",\"detected_source_language\":\"EN\"}]}";

    [Fact]
    public async Task SendsTargetLanguageAndText() {
      ExpectForm(HttpMethod.Post, "v2/write/rephrase", ("target_lang", "en-US"), ("text", "good"))
            .RespondJson(Reply);
      await CreateClient().RephraseTextAsync("good", "en-US");
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
            "en",
            new TextRephraseOptions { WritingStyle = "business", WritingTone = "friendly" });
    }

    [Fact]
    public async Task SendsTextArrayInOrder() {
      Expect(HttpMethod.Post, "v2/write/rephrase")
            .WithContent("target_lang=en-US&text=a&text=b")
            .RespondJson(
                  "{\"improvements\":[{\"text\":\"x\",\"target_language\":\"EN-US\",\"detected_source_language\":\"EN\"}," +
                  "{\"text\":\"y\",\"target_language\":\"EN-US\",\"detected_source_language\":\"DE\"}]}");
      await CreateClient().RephraseTextAsync(new[] { "a", "b" }, "en-US");
    }

    [Fact]
    public async Task ParsesImprovement() {
      ExpectForm(HttpMethod.Post, "v2/write/rephrase", ("text", "good")).RespondJson(Reply);
      var result = await CreateClient().RephraseTextAsync("good", null);
      Assert.Equal("en", result.DetectedSourceLanguageCode);
    }

    [Fact]
    public async Task SendsOnlyTextWhenTargetLanguageIsOmitted() {
      ExpectForm(HttpMethod.Post, "v2/write/rephrase", ("text", "good")).RespondJson(Reply);
      await CreateClient().RephraseTextAsync("good");
    }

    [Fact]
    public async Task SendsExtraBodyParametersLastReplacingFields() {
      Expect(HttpMethod.Post, "v2/write/rephrase")
            .WithContent("target_lang=en&text=good&writing_style=simple&extra=1")
            .RespondJson(Reply);
      await CreateClient().RephraseTextAsync(
            "good",
            "en",
            new TextRephraseOptions {
                  WritingStyle = "business",
                  ExtraBodyParameters = new Dictionary<string, string> { ["writing_style"] = "simple", ["extra"] = "1" }
            });
    }

    [Fact]
    public async Task SendsWritingStyleEnumAsApiValue() {
      ExpectForm(HttpMethod.Post, "v2/write/rephrase", ("writing_style", "prefer_simple"), ("text", "good"))
            .RespondJson(Reply);
      await CreateClient().RephraseTextAsync("good", null, new TextRephraseOptions(WritingStyle.PreferSimple));
    }

    [Fact]
    public async Task SendsWritingToneEnumAsApiValue() {
      ExpectForm(HttpMethod.Post, "v2/write/rephrase", ("tone", "prefer_enthusiastic"), ("text", "good"))
            .RespondJson(Reply);
      await CreateClient().RephraseTextAsync("good", null, new TextRephraseOptions(WritingTone.PreferEnthusiastic));
    }

    [Fact]
    public void MapsEveryWritingStyleAndToneToItsApiValue() {
      Assert.Equal(
            new[] {
                  "academic", "business", "casual", "default", "prefer_academic", "prefer_business", "prefer_casual",
                  "prefer_simple", "simple"
            },
            Enum.GetValues(typeof(WritingStyle)).Cast<WritingStyle>().Select(style => style.ToApiValue()));
      Assert.Equal(
            new[] {
                  "confident", "default", "diplomatic", "enthusiastic", "friendly", "prefer_confident",
                  "prefer_diplomatic", "prefer_enthusiastic", "prefer_friendly"
            },
            Enum.GetValues(typeof(WritingTone)).Cast<WritingTone>().Select(tone => tone.ToApiValue()));
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenTextIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().RephraseTextAsync("", "en"));

    [Fact]
    public async Task ThrowsArgumentExceptionWhenTargetLanguageIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().RephraseTextAsync("a", ""));
  }
}
