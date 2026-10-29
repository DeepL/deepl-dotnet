// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class TranslateTextTest : OfflineTest {
    private const string Reply =
          "{\"translations\":[{\"detected_source_language\":\"EN\",\"text\":\"Hallo\",\"billed_characters\":5," +
          "\"model_type_used\":\"quality_optimized\"}]}";

    private Task<TextResult> Translate(TextTranslateOptions? options, params (string Key, string Value)[] expected) {
      var fields = new List<(string Key, string Value)> {
        ("target_lang", "de"), ("show_billed_characters", "1"), ("text", "Hello")
      };
      fields.AddRange(expected);
      ExpectForm(HttpMethod.Post, "v2/translate", fields.ToArray()).RespondJson(Reply);
      return CreateClient().TranslateTextAsync("Hello", null, "DE", options);
    }

    [Fact]
    public async Task SendsOnlyRequiredFieldsByDefault() {
      await Translate(null);
      await Translate(new TextTranslateOptions());
    }

    [Fact]
    public async Task SendsLanguageCodesStandardized() {
      ExpectForm(
                  HttpMethod.Post,
                  "v2/translate",
                  ("source_lang", "de"),
                  ("target_lang", "en-US"),
                  ("show_billed_characters", "1"),
                  ("text", "Hallo"))
            .RespondJson(Reply);
      await CreateClient().TranslateTextAsync("Hallo", "dE", "EN-us");
    }

    [Fact]
    public async Task SendsTextArrayInOrder() {
      Expect(HttpMethod.Post, "v2/translate")
            .WithContent("target_lang=de&show_billed_characters=1&text=a+b&text=%C3%A4")
            .RespondJson(
                  "{\"translations\":[{\"detected_source_language\":\"EN\",\"text\":\"x\",\"billed_characters\":3}," +
                  "{\"detected_source_language\":\"DE\",\"text\":\"y\",\"billed_characters\":1}]}");
      var results = await CreateClient().TranslateTextAsync(new[] { "a b", "ä" }, null, "DE");
      Assert.Equal(new[] { "en", "de" }, results.Select(result => result.DetectedSourceLanguageCode));
    }

    [Theory]
    [InlineData(Formality.Less, "less")]
    [InlineData(Formality.More, "more")]
    [InlineData(Formality.PreferLess, "prefer_less")]
    [InlineData(Formality.PreferMore, "prefer_more")]
    public Task SendsFormality(Formality formality, string expected) =>
          Translate(new TextTranslateOptions { Formality = formality }, ("formality", expected));

    [Theory]
    [InlineData(SentenceSplittingMode.Off, "0")]
    [InlineData(SentenceSplittingMode.NoNewlines, "nonewlines")]
    public Task SendsSentenceSplittingMode(SentenceSplittingMode mode, string expected) =>
          Translate(new TextTranslateOptions { SentenceSplittingMode = mode }, ("split_sentences", expected));

    [Theory]
    [InlineData(ModelType.LatencyOptimized, "latency_optimized")]
    [InlineData(ModelType.QualityOptimized, "quality_optimized")]
    [InlineData(ModelType.PreferQualityOptimized, "prefer_quality_optimized")]
    public Task SendsModelType(ModelType modelType, string expected) =>
          Translate(new TextTranslateOptions { ModelType = modelType }, ("model_type", expected));

    [Fact]
    public Task SendsPreserveFormatting() =>
          Translate(new TextTranslateOptions { PreserveFormatting = true }, ("preserve_formatting", "1"));

    [Fact]
    public Task SendsOutlineDetectionOnlyWhenDisabled() =>
          Translate(new TextTranslateOptions { OutlineDetection = false }, ("outline_detection", "0"));

    [Fact]
    public Task SendsContext() => Translate(new TextTranslateOptions { Context = "a knife" }, ("context", "a knife"));

    [Fact]
    public Task SendsTagHandlingAndVersion() =>
          Translate(
                new TextTranslateOptions { TagHandling = "html", TagHandlingVersion = "v2" },
                ("tag_handling", "html"),
                ("tag_handling_version", "v2"));

    [Fact]
    public Task SendsTagHandlingModeAndVersionConstants() =>
          Translate(
                new TextTranslateOptions { TagHandling = TagHandlingMode.Xml, TagHandlingVersion = TagHandlingVersion.V1 },
                ("tag_handling", "xml"),
                ("tag_handling_version", "v1"));

    [Fact]
    public Task SendsStringValuesUnchangedInsteadOfEnums() =>
          Translate(
                new TextTranslateOptions {
                  Formality = Formality.Less,
                  FormalityValue = "new_formality",
                  SentenceSplittingMode = SentenceSplittingMode.Off,
                  SplitSentences = "new_mode",
                  ModelType = ModelType.LatencyOptimized,
                  ModelTypeValue = "new_model"
                },
                ("formality", "new_formality"),
                ("split_sentences", "new_mode"),
                ("model_type", "new_model"));

    [Fact]
    public Task SendsTagListsCommaSeparated() =>
          Translate(
                new TextTranslateOptions {
                  TagHandling = "xml",
                  IgnoreTags = { "raw", "code" },
                  NonSplittingTags = { "span" },
                  SplittingTags = { "title", "par" }
                },
                ("tag_handling", "xml"),
                ("ignore_tags", "raw,code"),
                ("non_splitting_tags", "span"),
                ("splitting_tags", "title,par"));

    [Fact]
    public Task SendsCustomInstructions() =>
          Translate(
                new TextTranslateOptions { CustomInstructions = { "Use informal language", "Be concise" } },
                ("custom_instructions", "Use informal language"),
                ("custom_instructions", "Be concise"));

#pragma warning disable CS0618 // The deprecated *Id properties stay covered by the existing tests.
    [Fact]
    public Task SendsGlossaryId() =>
          Translate(new TextTranslateOptions { GlossaryId = "g1" }, ("glossary_id", "g1"));
#pragma warning restore CS0618

    [Fact]
    public Task SendsGlossaryOfGlossaryInfo() {
      var glossary = new MultilingualGlossaryInfo("g1", "n", new MultilingualGlossaryDictionaryInfo[0], DateTime.UtcNow);
      return Translate(new TextTranslateOptions { Glossary = glossary }, ("glossary_id", "g1"));
    }

    [Fact]
    public async Task NullResourceObjectsLeaveTheOptionsUnset() {
      GlossaryInfo? glossary = null;
      StyleRuleInfo? styleRule = null;
      StyleProfile? styleProfile = null;
      TranslationMemoryInfo? translationMemory = null;
      var options = new TextTranslateOptions {
        Glossary = glossary, StyleRule = styleRule, StyleProfile = styleProfile, TranslationMemory = translationMemory
      };
      Assert.Null(options.Glossary);
      Assert.Null(options.StyleRule);
      Assert.Null(options.StyleProfile);
      Assert.Null(options.TranslationMemory);
      await Translate(options);
    }

    [Fact]
    public Task SendsGlossaryIdsMixingIdsAndGlossaryObjects() {
      var glossary = new GlossaryInfo("g2", "n", true, "en", "de", DateTime.UtcNow, 1);
      var multilingualGlossary =
            new MultilingualGlossaryInfo("g3", "n", new MultilingualGlossaryDictionaryInfo[0], DateTime.UtcNow);
      // The C# 8 compiler of the .NET Core 3.1 SDK ignores [NotNullIfNotNull] on the conversion, hence the "!".
      var options = new TextTranslateOptions { GlossaryIds = { "g1", ((string)glossary)! } };
      options.GlossaryIds.Add(((string)multilingualGlossary)!);
      return Translate(options, ("glossary_ids", "g1,g2,g3"));
    }

    [Fact]
    public Task SendsGlossaryIdOfGlossaryInfo() {
      var glossary = new GlossaryInfo("g1", "n", true, "en", "de", DateTime.UtcNow, 1);
      return Translate(new TextTranslateOptions(glossary), ("glossary_id", "g1"));
    }

    [Fact]
    public Task SendsGlossaryIdsCommaSeparated() =>
          Translate(new TextTranslateOptions { GlossaryIds = { "g1", "g2" } }, ("glossary_ids", "g1,g2"));

    [Fact]
    public Task SendsGlossaryIdsOfMultilingualGlossaryInfos() {
      var glossaries = new[] {
        new MultilingualGlossaryInfo("g1", "a", new MultilingualGlossaryDictionaryInfo[0], DateTime.UtcNow),
        new MultilingualGlossaryInfo("g2", "b", new MultilingualGlossaryDictionaryInfo[0], DateTime.UtcNow)
      };
      return Translate(new TextTranslateOptions(glossaries), ("glossary_ids", "g1,g2"));
    }

#pragma warning disable CS0618
    [Fact]
    public Task SendsStyleId() => Translate(new TextTranslateOptions { StyleId = "s1" }, ("style_id", "s1"));
#pragma warning restore CS0618

    [Fact]
    public Task SendsStyleRuleOfStyleRuleInfo() {
      var styleRule = new StyleRuleInfo("s1", "n", DateTime.UtcNow, DateTime.UtcNow, "en", 1, null, null);
      return Translate(new TextTranslateOptions { StyleRule = styleRule }, ("style_id", "s1"));
    }

    [Fact]
    public Task SendsStyleIdOfStyleRuleInfo() {
      var styleRule = new StyleRuleInfo("s1", "n", DateTime.UtcNow, DateTime.UtcNow, "en", 1, null, null);
      return Translate(new TextTranslateOptions(styleRule), ("style_id", "s1"));
    }

    // Translate() passes no source language: a style profile must not require one.
    [Fact]
    public Task SendsStyleProfile() =>
          Translate(new TextTranslateOptions { StyleProfile = "sp1" }, ("style_profile_id", "sp1"));

    [Fact]
    public Task SendsStyleProfileOfStyleProfileObject() {
      var styleProfile = new StyleProfile("sp1", "n", null, null, null, null, null, false, null, null);
      return Translate(new TextTranslateOptions { StyleProfile = styleProfile }, ("style_profile_id", "sp1"));
    }

    [Fact]
    public Task SendsStyleProfileIdOfStyleProfile() {
      var styleProfile = new StyleProfile("sp1", "n", null, null, null, null, null, false, null, null);
      return Translate(new TextTranslateOptions(styleProfile), ("style_profile_id", "sp1"));
    }

#pragma warning disable CS0618
    [Fact]
    public Task SendsTranslationMemoryIdAndThreshold() =>
          Translate(
                new TextTranslateOptions { TranslationMemoryId = "tm1", TranslationMemoryThreshold = 80 },
                ("translation_memory_id", "tm1"),
                ("translation_memory_threshold", "80"));
#pragma warning restore CS0618

    [Fact]
    public Task SendsTranslationMemoryOfTranslationMemoryInfoAndThreshold() {
      var translationMemory = new TranslationMemoryInfo("tm1", "n", "en", new[] { "de" }, 1);
      return Translate(
            new TextTranslateOptions { TranslationMemory = translationMemory, TranslationMemoryThreshold = 80 },
            ("translation_memory_id", "tm1"),
            ("translation_memory_threshold", "80"));
    }

    [Fact]
    public Task SendsTranslationMemoryIdOfTranslationMemoryInfo() {
      var translationMemory = new TranslationMemoryInfo("tm1", "n", "en", new[] { "de" }, 1);
      return Translate(new TextTranslateOptions(translationMemory), ("translation_memory_id", "tm1"));
    }

    [Fact]
    public async Task SendsExtraBodyParametersLast() {
      ExpectForm(
                  HttpMethod.Post,
                  "v2/translate",
                  ("target_lang", "FR"),
                  ("show_billed_characters", "1"),
                  ("text", "Hello"),
                  ("debug", "1"))
            .RespondJson(Reply);
      await CreateClient().TranslateTextAsync(
            "Hello",
            null,
            "DE",
            new TextTranslateOptions {
              ExtraBodyParameters = new Dictionary<string, string> { { "target_lang", "FR" }, { "debug", "1" } }
            });
    }

    [Fact]
    public async Task ExtraTextParameterReplacesTexts() {
      Expect(HttpMethod.Post, "v2/translate")
            .WithContent("target_lang=de&show_billed_characters=1&text=x")
            .RespondJson(Reply);
      await CreateClient().TranslateTextAsync(
            new[] { "a", "b" },
            null,
            "DE",
            new TextTranslateOptions { ExtraBodyParameters = new Dictionary<string, string> { { "text", "x" } } });
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"translations\":[]}")]
    [InlineData("{\"translations\":[{\"detected_source_language\":\"EN\",\"text\":\"x\"}]}")]
    public async Task ThrowsDeepLExceptionWhenTranslationsAreMissing(string reply) {
      Expect(HttpMethod.Post, "v2/translate").RespondJson(reply);
      await Assert.ThrowsAsync<DeepLException>(
            () => CreateClient().TranslateTextAsync(new[] { "a", "b" }, null, "DE"));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"translations\":[]}")]
    public async Task ThrowsDeepLExceptionForSingleTextWhenTranslationsAreMissing(string reply) {
      Expect(HttpMethod.Post, "v2/translate").RespondJson(reply);
      await Assert.ThrowsAsync<DeepLException>(() => CreateClient().TranslateTextAsync("a", null, "DE"));
    }

    [Fact]
    public async Task ThrowsDeepLExceptionWhenDetectedSourceLanguageIsMissing() {
      Expect(HttpMethod.Post, "v2/translate").RespondJson("{\"translations\":[{\"text\":\"Hallo\"}]}");
      await Assert.ThrowsAsync<DeepLException>(() => CreateClient().TranslateTextAsync("Hello", null, "DE"));
    }

    [Theory]
    [InlineData(",\"tag_handling_version\":\"v2\"", "v2")]
    [InlineData(",\"tag_handling_version\":\"v3\"", "v3")]
    [InlineData("", null)]
    [InlineData(",\"tag_handling_version\":2", null)]
    public async Task ParsesTagHandlingVersion(string field, string? expected) {
      Expect(HttpMethod.Post, "v2/translate")
            .RespondJson(
                  "{\"translations\":[{\"detected_source_language\":\"EN\",\"text\":\"Hallo\"," +
                  "\"billed_characters\":5" + field + "}]}");
      var result = await CreateClient().TranslateTextAsync("Hello", null, "DE");
      Assert.Equal(expected, result.TagHandlingVersion);
      Assert.Equal("Hallo", result.Text);
      Assert.Equal(5, result.BilledCharacters);
    }

    [Fact]
    public async Task ParsesResult() {
      ExpectForm(HttpMethod.Post, "v2/translate", ("target_lang", "de"), ("show_billed_characters", "1"), ("text", "Hello"))
            .RespondJson(
                  "{\"translations\":[{\"detected_source_language\":\"PT-BR\",\"text\":\"Hallo\"," +
                  "\"billed_characters\":5,\"model_type_used\":\"latency_optimized\"}]}");
      var result = await CreateClient().TranslateTextAsync("Hello", null, "DE");
      Assert.Equal("pt-BR", result.DetectedSourceLanguageCode);
      Assert.Equal("Hallo", $"{result}");
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenTextIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().TranslateTextAsync("", null, "DE"));

    [Fact]
    public async Task ThrowsArgumentExceptionWhenTargetLanguageIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().TranslateTextAsync("a", null, ""));

    [Fact]
    public async Task ThrowsArgumentExceptionWhenSourceLanguageIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().TranslateTextAsync("a", "", "DE"));

    [Fact]
    public async Task ThrowsArgumentExceptionWhenThresholdHasNoTranslationMemory() {
      var options = new TextTranslateOptions { TranslationMemoryThreshold = 80 };
      await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().TranslateTextAsync("a", null, "DE", options));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task ThrowsArgumentOutOfRangeExceptionWhenThresholdIsOutOfRange(int threshold) {
      var options = new TextTranslateOptions { TranslationMemory = "tm1", TranslationMemoryThreshold = threshold };
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => CreateClient().TranslateTextAsync("a", null, "DE", options));
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenGlossaryAndGlossaryIdsAreCombined() {
      var options = new TextTranslateOptions { Glossary = "g1", GlossaryIds = { "g2" } };
      var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => CreateClient().TranslateTextAsync("a", null, "DE", options));
      Assert.Equal("Glossary and GlossaryIds cannot be used together", exception.Message);
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenMoreThanFiveGlossaryIdsAreGiven() {
      var options = new TextTranslateOptions { GlossaryIds = { "1", "2", "3", "4", "5", "6" } };
      await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().TranslateTextAsync("a", null, "DE", options));
    }

    [Fact]
    public void MapsEachModelTypeToDistinctApiValue() {
      var values = Enum.GetValues(typeof(ModelType)).Cast<ModelType>().Select(type => type.ToApiValue()).ToArray();
      Assert.Equal(values.Length, values.Distinct().Count());
    }
  }
}
