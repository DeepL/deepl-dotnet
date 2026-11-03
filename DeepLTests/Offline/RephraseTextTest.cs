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
      ExpectJsonFields(HttpMethod.Post, "v2/write/rephrase", ("target_lang", "en-US"), ("text", new[] { "good" }))
            .RespondJson(Reply);
      await CreateClient().RephraseTextAsync("good", "en-US");
    }

    [Fact]
    public async Task SendsOnlyTextWhenTargetLanguageIsNull() {
      ExpectJsonFields(HttpMethod.Post, "v2/write/rephrase", ("text", new[] { "good" })).RespondJson(Reply);
      await CreateClient().RephraseTextAsync("good", null);
    }

    [Fact]
    public async Task SendsWritingStyleAndTone() {
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v2/write/rephrase",
                  ("target_lang", "en"),
                  ("writing_style", "business"),
                  ("tone", "friendly"),
                  ("text", new[] { "good" }))
            .RespondJson(Reply);
      await CreateClient().RephraseTextAsync(
            "good",
            "en",
            new TextRephraseOptions { WritingStyle = "business", WritingTone = "friendly" });
    }

    [Fact]
    public async Task SendsTextArrayInOrder() {
      ExpectJsonFields(HttpMethod.Post, "v2/write/rephrase", ("target_lang", "en-US"), ("text", new[] { "a", "b" }))
            .RespondJson(
                  "{\"improvements\":[{\"text\":\"x\",\"target_language\":\"EN-US\",\"detected_source_language\":\"EN\"}," +
                  "{\"text\":\"y\",\"target_language\":\"EN-US\",\"detected_source_language\":\"DE\"}]}");
      await CreateClient().RephraseTextAsync(new[] { "a", "b" }, "en-US");
    }

    [Fact]
    public async Task ParsesImprovement() {
      ExpectJsonFields(HttpMethod.Post, "v2/write/rephrase", ("text", new[] { "good" })).RespondJson(Reply);
      var result = await CreateClient().RephraseTextAsync("good", null);
      Assert.Equal("en", result.DetectedSourceLanguageCode);
    }

    [Fact]
    public async Task SendsOnlyTextWhenTargetLanguageIsOmitted() {
      ExpectJsonFields(HttpMethod.Post, "v2/write/rephrase", ("text", new[] { "good" })).RespondJson(Reply);
      await CreateClient().RephraseTextAsync("good");
    }

    [Fact]
    public async Task SendsExtraBodyParametersLastReplacingFields() {
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v2/write/rephrase",
                  ("target_lang", "en"),
                  ("text", new[] { "good" }),
                  ("writing_style", "simple"),
                  ("extra", "1"))
            .RespondJson(Reply);
      await CreateClient().RephraseTextAsync(
            "good",
            "en",
            new TextRephraseOptions {
                  WritingStyle = "business",
                  ExtraBodyParameters = new Dictionary<string, object?> { ["writing_style"] = "simple", ["extra"] = "1" }
            });
    }

    [Fact]
    public async Task SendsWritingStyleEnumAsApiValue() {
      ExpectJsonFields(HttpMethod.Post, "v2/write/rephrase", ("writing_style", "prefer_simple"), ("text", new[] { "good" }))
            .RespondJson(Reply);
      await CreateClient().RephraseTextAsync("good", null, new TextRephraseOptions(WritingStyle.PreferSimple));
    }

    [Fact]
    public async Task SendsWritingToneEnumAsApiValue() {
      ExpectJsonFields(HttpMethod.Post, "v2/write/rephrase", ("tone", "prefer_enthusiastic"), ("text", new[] { "good" }))
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

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"improvements\":[]}")]
    public async Task ThrowsDeepLExceptionWhenImprovementsAreMissing(string reply) {
      ExpectJsonFields(HttpMethod.Post, "v2/write/rephrase", ("text", new[] { "good" })).RespondJson(reply);
      await Assert.ThrowsAsync<DeepLException>(() => CreateClient().RephraseTextAsync("good"));
    }

    [Fact]
    public async Task ThrowsDeepLExceptionWhenImprovementsAreFewerThanTexts() {
      ExpectJsonFields(HttpMethod.Post, "v2/write/rephrase", ("text", new[] { "a", "b" })).RespondJson(Reply);
      await Assert.ThrowsAsync<DeepLException>(() => CreateClient().RephraseTextAsync(new[] { "a", "b" }));
    }

    [Fact]
    public async Task ThrowsDeepLExceptionWhenDetectedSourceLanguageIsMissing() {
      ExpectJsonFields(HttpMethod.Post, "v2/write/rephrase", ("text", new[] { "good" }))
            .RespondJson("{\"improvements\":[{\"text\":\"better\",\"target_language\":\"EN-US\"}]}");
      await Assert.ThrowsAsync<DeepLException>(() => CreateClient().RephraseTextAsync("good"));
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenTextIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().RephraseTextAsync("", "en"));

    [Fact]
    public async Task ThrowsArgumentExceptionWhenTargetLanguageIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().RephraseTextAsync("a", ""));
  }
}
