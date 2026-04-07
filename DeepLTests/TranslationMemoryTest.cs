// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using Xunit;

namespace DeepLTests {
  public sealed class TranslationMemoryTest : BaseDeepLTest {
    private const string DefaultTranslationMemoryId = "a74d88fb-ed2a-4943-a664-a4512398b994";

    [MockServerOnlyFact]
    public async Task TestListTranslationMemories() {
      var client = CreateTestClient();
      var translationMemories = await client.ListTranslationMemoriesAsync(0, 10);
      Assert.NotNull(translationMemories);
      Assert.True(translationMemories.Length > 0);
      Assert.Equal(DefaultTranslationMemoryId, translationMemories[0].TranslationMemoryId);
      Assert.Equal("Default Translation Memory", translationMemories[0].Name);
      Assert.NotNull(translationMemories[0].SourceLanguage);
      Assert.NotNull(translationMemories[0].TargetLanguages);
      Assert.True(translationMemories[0].SegmentCount >= 0);
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTextWithTranslationMemoryId() {
      // Note: this test may use the mock server that will not translate the text,
      // therefore we do not check the translated result.
      var client = CreateTestClient();
      const string exampleText = "Hallo, Welt!";

      var result = await client.TranslateTextAsync(
            exampleText,
            "de",
            "en-US",
            new TextTranslateOptions { TranslationMemoryId = DefaultTranslationMemoryId });

      Assert.NotNull(result);
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTextWithTranslationMemoryIdAndThreshold() {
      // Note: this test may use the mock server that will not translate the text,
      // therefore we do not check the translated result.
      var client = CreateTestClient();
      const string exampleText = "Hallo, Welt!";

      var result = await client.TranslateTextAsync(
            exampleText,
            "de",
            "en-US",
            new TextTranslateOptions {
              TranslationMemoryId = DefaultTranslationMemoryId,
              TranslationMemoryThreshold = 80
            });

      Assert.NotNull(result);
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTextWithThresholdWithoutIdThrows() {
      var client = CreateTestClient();
      await Assert.ThrowsAsync<ArgumentException>(() =>
            client.TranslateTextAsync("Hello", "en", "de",
                  new TextTranslateOptions { TranslationMemoryThreshold = 80 }));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTextWithInvalidThresholdThrows() {
      var client = CreateTestClient();
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.TranslateTextAsync("Hello", "en", "de",
                  new TextTranslateOptions {
                        TranslationMemoryId = DefaultTranslationMemoryId,
                        TranslationMemoryThreshold = 101
                  }));
    }
  }
}
