// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.IO;
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

    private static (FileInfo Input, FileInfo Output) CreateDocumentPaths() {
      var tempDir = TempDir();
      var inputFilePath = Path.Combine(tempDir, "example_document.txt");
      File.Delete(inputFilePath);
      File.WriteAllText(inputFilePath, "Hallo, Welt!");
      var outputFilePath = Path.Combine(tempDir, "output_document.txt");
      File.Delete(outputFilePath);
      return (new FileInfo(inputFilePath), new FileInfo(outputFilePath));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithTranslationMemoryId() {
      // Note: this test may use the mock server that will not translate the text,
      // therefore we do not check the translated result.
      var translator = CreateTestTranslator();
      var (input, output) = CreateDocumentPaths();

      await translator.TranslateDocumentAsync(
            input,
            output,
            "de",
            "en-US",
            new DocumentTranslateOptions { TranslationMemoryId = DefaultTranslationMemoryId });

      Assert.True(File.Exists(output.FullName));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithTranslationMemoryIdAndThreshold() {
      // Note: this test may use the mock server that will not translate the text,
      // therefore we do not check the translated result.
      var translator = CreateTestTranslator();
      var (input, output) = CreateDocumentPaths();

      await translator.TranslateDocumentAsync(
            input,
            output,
            "de",
            "en-US",
            new DocumentTranslateOptions {
              TranslationMemoryId = DefaultTranslationMemoryId,
              TranslationMemoryThreshold = 80
            });

      Assert.True(File.Exists(output.FullName));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithTranslationMemoryInfo() {
      var client = CreateTestClient();
      var translationMemories = await client.ListTranslationMemoriesAsync(0, 10);
      var translationMemory = translationMemories[0];
      var (input, output) = CreateDocumentPaths();

      await client.TranslateDocumentAsync(
            input,
            output,
            "de",
            "en-US",
            new DocumentTranslateOptions(translationMemory));

      Assert.True(File.Exists(output.FullName));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithThresholdWithoutIdThrows() {
      var translator = CreateTestTranslator();
      var (input, _) = CreateDocumentPaths();
      // Option validation is performed at upload time and throws the raw
      // ArgumentException; the all-in-one TranslateDocumentAsync would wrap it
      // in a DocumentTranslationException.
      await Assert.ThrowsAsync<ArgumentException>(() =>
            translator.TranslateDocumentUploadAsync(input, "de", "en-US",
                  new DocumentTranslateOptions { TranslationMemoryThreshold = 80 }));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithInvalidThresholdThrows() {
      var translator = CreateTestTranslator();
      var (input, _) = CreateDocumentPaths();
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            translator.TranslateDocumentUploadAsync(input, "de", "en-US",
                  new DocumentTranslateOptions {
                    TranslationMemoryId = DefaultTranslationMemoryId,
                    TranslationMemoryThreshold = 101
                  }));
    }
  }
}
