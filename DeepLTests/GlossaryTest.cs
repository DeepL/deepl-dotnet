// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using Xunit;

namespace DeepLTests {
  public sealed class GlossaryTest : BaseDeepLTest {
    private const string InvalidGlossaryId = "invalid_glossary_id";
    private const string GlossaryNamePrefix = "deepl-dotnet-test-glossary";
    private const string NonexistentGlossaryId = "96ab91fd-e715-41a1-adeb-5d701f84a483";
    private const string SourceLang = "en";
    private const string TargetLang = "de";
    private readonly GlossaryEntries _testEntries = GlossaryEntries.FromTsv("Hello\tHallo");

    [Fact]
    public void TestGlossaryEntries() {
      Assert.Throws<ArgumentException>(() => GlossaryEntries.FromTsv(""));
      Assert.Throws<ArgumentException>(() => GlossaryEntries.FromTsv("Küche\tKitchen\nKüche\tCuisine"));
      Assert.Throws<ArgumentException>(() => GlossaryEntries.FromTsv("A\tB\tC"));

      Assert.Throws<ArgumentException>(() => new GlossaryEntries(new Dictionary<string, string>()));
      Assert.Throws<ArgumentException>(() => new GlossaryEntries(new[] { ("Küche", "Kitchen"), ("Küche", "Cuisine") }));
      Assert.Throws<ArgumentException>(() => new GlossaryEntries(new Dictionary<string, string> { { "A", "B\tC" } }));
    }

    [Fact]
    public async Task TestGlossaryCreate() {
      var translator = CreateTestTranslator();
      var glossaryCleanup = new GlossaryCleanupUtility(translator, nameof(TestGlossaryCreate));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var entries = new GlossaryEntries(new[] { ("Hello", "Hallo") });
        var glossary = glossaryCleanup.Capture(
              await translator.CreateGlossaryAsync(
                    glossaryName,
                    SourceLang,
                    TargetLang,
                    entries));

        Assert.Equal(glossaryName, glossary.Name);
        Assert.Equal(SourceLang, glossary.SourceLanguageCode);
        Assert.Equal(TargetLang, glossary.TargetLanguageCode);
        Assert.Equal(1, glossary.EntryCount);

        var getResult = await translator.GetGlossaryAsync(glossary.GlossaryId);
        Assert.Equal(getResult.Name, glossary.Name);
        Assert.Equal(getResult.SourceLanguageCode, glossary.SourceLanguageCode);
        Assert.Equal(getResult.TargetLanguageCode, glossary.TargetLanguageCode);
        Assert.Equal(getResult.CreationTime, glossary.CreationTime);
        Assert.Equal(getResult.EntryCount, glossary.EntryCount);
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryCreateLarge() {
      var translator = CreateTestTranslator();
      var glossaryCleanup = new GlossaryCleanupUtility(translator, nameof(TestGlossaryCreate));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var entryPairs = new Dictionary<string, string>();
        for (var i = 0; i < 10000; i++) {
          entryPairs.Add($"Source-{i}", $"Target-{i}");
        }

        var entries = new GlossaryEntries(entryPairs);
        Assert.True(entries.ToTsv().Length > 100000);
        var glossary = glossaryCleanup.Capture(
              await translator.CreateGlossaryAsync(
                    glossaryName,
                    SourceLang,
                    TargetLang,
                    entries));

        Assert.Equal(glossaryName, glossary.Name);
        Assert.Equal(SourceLang, glossary.SourceLanguageCode);
        Assert.Equal(TargetLang, glossary.TargetLanguageCode);
        Assert.Equal(entryPairs.Count, glossary.EntryCount);
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryCreateCsv() {
      var translator = CreateTestTranslator();
      var glossaryCleanup = new GlossaryCleanupUtility(translator, nameof(TestGlossaryCreateCsv));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var expectedEntries =
              new GlossaryEntries(new[] { ("sourceEntry1", "targetEntry1"), ("source\"Entry", "target,Entry") });

        const string csvContent = "sourceEntry1,targetEntry1,en,de\n\"source\"\"Entry\",\"target,Entry\",en,de";
        var csvStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        var glossary = glossaryCleanup.Capture(
              await translator.CreateGlossaryFromCsvAsync(
                    glossaryName,
                    "en",
                    "de",
                    csvStream));

        Assert.Equal(
              expectedEntries.ToDictionary(),
              (await translator.GetGlossaryEntriesAsync(glossary.GlossaryId)).ToDictionary());
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryCreateInvalid() {
      var translator = CreateTestTranslator();
      var glossaryCleanup = new GlossaryCleanupUtility(translator, nameof(TestGlossaryCreateInvalid));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        await Assert.ThrowsAsync<ArgumentException>(
              () => translator.CreateGlossaryAsync("", SourceLang, TargetLang, _testEntries));
        await Assert.ThrowsAsync<DeepLException>(
              () => translator.CreateGlossaryAsync(glossaryName, "EN", "XX", _testEntries));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryGet() {
      var translator = CreateTestTranslator();
      var glossaryCleanup = new GlossaryCleanupUtility(translator, nameof(TestGlossaryGet));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var createdGlossary = glossaryCleanup.Capture(
              await translator.CreateGlossaryAsync(glossaryName, SourceLang, TargetLang, _testEntries));
        var glossary = await translator.GetGlossaryAsync(createdGlossary.GlossaryId);
        Assert.Equal(createdGlossary.GlossaryId, glossary.GlossaryId);
        Assert.Equal(glossaryName, glossary.Name);
        Assert.Equal(SourceLang, glossary.SourceLanguageCode);
        Assert.Equal(TargetLang, glossary.TargetLanguageCode);
        Assert.Equal(createdGlossary.CreationTime, glossary.CreationTime);
        Assert.Equal(_testEntries.ToDictionary().Count, glossary.EntryCount);

        await Assert.ThrowsAsync<DeepLException>(() => translator.GetGlossaryAsync(InvalidGlossaryId));
        await Assert.ThrowsAsync<GlossaryNotFoundException>(() => translator.GetGlossaryAsync(NonexistentGlossaryId));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryGetEntries() {
      var translator = CreateTestTranslator();
      var glossaryCleanup = new GlossaryCleanupUtility(translator, nameof(TestGlossaryGetEntries));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var entries = new GlossaryEntries(
              new[] {
                    ("Apple", "Apfel"), ("Banana", "Banane"), ("A%=&", "B&=%"), ("\u0394\u3041", "\u6DF1"),
                    ("\uD83E\uDEA8", "\uD83E\uDEB5")
              });
        var createdGlossary = glossaryCleanup.Capture(
              await translator.CreateGlossaryAsync(
                    glossaryName,
                    SourceLang,
                    TargetLang,
                    entries));
        Assert.Equal(
              entries.ToDictionary(),
              (await translator.GetGlossaryEntriesAsync(createdGlossary)).ToDictionary());
        Assert.Equal(
              entries.ToDictionary(),
              (await translator.GetGlossaryEntriesAsync(createdGlossary.GlossaryId)).ToDictionary());

        await Assert.ThrowsAsync<DeepLException>(() => translator.GetGlossaryEntriesAsync(InvalidGlossaryId));
        await Assert.ThrowsAsync<GlossaryNotFoundException>(
              () => translator.GetGlossaryEntriesAsync(NonexistentGlossaryId));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryList() {
      var translator = CreateTestTranslator();
      var glossaryCleanup = new GlossaryCleanupUtility(translator, nameof(TestGlossaryList));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        glossaryCleanup.Capture(
              await translator.CreateGlossaryAsync(glossaryName, SourceLang, TargetLang, _testEntries));
        var glossaries = await translator.ListGlossariesAsync();
        Assert.Contains(glossaries, glossary => glossary.Name == glossaryName);
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryDelete() {
      var translator = CreateTestTranslator();
      var glossaryCleanup = new GlossaryCleanupUtility(translator, nameof(TestGlossaryDelete));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var glossary = glossaryCleanup.Capture(
              await translator.CreateGlossaryAsync(glossaryName, SourceLang, TargetLang, _testEntries));
        await translator.DeleteGlossaryAsync(glossary);
        await Assert.ThrowsAsync<GlossaryNotFoundException>(() => translator.DeleteGlossaryAsync(glossary));

        await Assert.ThrowsAsync<DeepLException>(() => translator.DeleteGlossaryAsync(InvalidGlossaryId));
        await Assert.ThrowsAsync<GlossaryNotFoundException>(
              () => translator.DeleteGlossaryAsync(NonexistentGlossaryId));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryTranslateTextSentence() {
      var translator = CreateTestTranslator();
      var glossaryCleanup = new GlossaryCleanupUtility(translator, nameof(TestGlossaryTranslateTextSentence));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var sourceLang = "EN";
        var targetLang = "DE";
        var inputText = "The artist was awarded a prize.";
        var entries = new Dictionary<string, string> { { "artist", "Maler" }, { "prize", "Gewinn" } };
        var glossary = glossaryCleanup.Capture(
              await translator.CreateGlossaryAsync(
                    glossaryName,
                    sourceLang,
                    targetLang,
                    new GlossaryEntries(entries)));
        var result = await translator.TranslateTextAsync(
              inputText,
              sourceLang,
              targetLang,
              new TextTranslateOptions { GlossaryId = glossary.GlossaryId });
        if (!IsMockServer) {
          Assert.Contains("Maler", result.Text);
          Assert.Contains("Gewinn", result.Text);
        }

        // It is also possible to specify GlossaryInfo
        result = await translator.TranslateTextAsync(
              inputText,
              sourceLang,
              targetLang,
              new TextTranslateOptions(glossary) { Formality = Formality.More });
        if (!IsMockServer) {
          Assert.Contains("Maler", result.Text);
          Assert.Contains("Gewinn", result.Text);
        }
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryTranslateTextBasic() {
      var translator = CreateTestTranslator();
      var glossaryCleanupEnDe = new GlossaryCleanupUtility(translator, nameof(TestGlossaryTranslateTextBasic) + "EnDe");
      var glossaryCleanupDeEn = new GlossaryCleanupUtility(translator, nameof(TestGlossaryTranslateTextBasic) + "DeEn");
      var glossaryNameEnDe = glossaryCleanupEnDe.GlossaryName;
      var glossaryNameDeEn = glossaryCleanupDeEn.GlossaryName;
      try {
        var textsEn = new[] { "Apple", "Banana" };
        var textsDe = new[] { "Apfel", "Banane" };
        var entriesEnDe = new Dictionary<string, string>();
        var entriesDeEn = new Dictionary<string, string>();
        for (var i = 0; i < textsEn.Length; i++) {
          entriesDeEn[textsDe[i]] = textsEn[i];
          entriesEnDe[textsEn[i]] = textsDe[i];
        }

        var glossaryEnDe = glossaryCleanupEnDe.Capture(
              await translator.CreateGlossaryAsync(glossaryNameEnDe, "EN", "DE", new GlossaryEntries(entriesEnDe)));
        var glossaryDeEn = glossaryCleanupDeEn.Capture(
              await translator.CreateGlossaryAsync(glossaryNameDeEn, "DE", "EN", new GlossaryEntries(entriesDeEn)));

        var result = await translator.TranslateTextAsync(textsEn, "en", "de", new TextTranslateOptions(glossaryEnDe));
        Assert.Equal(textsDe, result.Select(textResult => textResult.Text));

        result = await translator.TranslateTextAsync(
              textsDe,
              "DE",
              "EN-US",
              new TextTranslateOptions { GlossaryId = glossaryDeEn.GlossaryId });
        Assert.Equal(textsEn, result.Select(textResult => textResult.Text));

        result = await translator.TranslateTextAsync(
              textsDe,
              "DE",
              "EN-US",
              new TextTranslateOptions(glossaryDeEn));
        Assert.Equal(textsEn, result.Select(textResult => textResult.Text));
      } finally {
        await glossaryCleanupEnDe.Cleanup();
        await glossaryCleanupDeEn.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryTranslateDocument() {
      var translator = CreateTestTranslator();
      var sourceLang = "EN";
      var targetLang = "DE";
      var inputText = "artist\nprize";
      var entries = new Dictionary<string, string> { { "artist", "Maler" }, { "prize", "Gewinn" } };
      var tempDir = TempDir();
      var inputFilePath = Path.Combine(tempDir, "example_document.txt");
      var inputFileInfo = new FileInfo(inputFilePath);
      File.Delete(inputFilePath);
      File.WriteAllText(inputFilePath, inputText);
      var outputFilePath = Path.Combine(tempDir, "output_document.txt");
      var outputFileInfo = new FileInfo(outputFilePath);
      File.Delete(outputFilePath);

      var glossaryCleanup = new GlossaryCleanupUtility(translator, nameof(TestGlossaryTranslateDocument));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var glossary = glossaryCleanup.Capture(
              await translator.CreateGlossaryAsync(
                    glossaryName,
                    sourceLang,
                    targetLang,
                    new GlossaryEntries(entries)));

        await translator.TranslateDocumentAsync(
              inputFileInfo,
              outputFileInfo,
              "EN",
              "DE",
              new DocumentTranslateOptions(glossary));
        Assert.Equal("Maler\nGewinn", File.ReadAllText(outputFilePath));

        File.Delete(outputFilePath);
        await translator.TranslateDocumentAsync(
              inputFileInfo,
              outputFileInfo,
              "EN",
              "DE",
              new DocumentTranslateOptions { GlossaryId = glossary.GlossaryId });
        Assert.Equal("Maler\nGewinn", File.ReadAllText(outputFilePath));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryTranslateTextInvalid() {
      var translator = CreateTestTranslator();
      var glossaryCleanupEnDe = new GlossaryCleanupUtility(
            translator,
            nameof(TestGlossaryTranslateTextInvalid) + "EnDe");
      var glossaryCleanupDeEn = new GlossaryCleanupUtility(
            translator,
            nameof(TestGlossaryTranslateTextInvalid) + "DeEn");
      var glossaryNameEnDe = glossaryCleanupEnDe.GlossaryName;
      var glossaryNameDeEn = glossaryCleanupDeEn.GlossaryName;
      try {
        var glossaryEnDe = glossaryCleanupEnDe.Capture(
              await translator.CreateGlossaryAsync(glossaryNameEnDe, "EN", "DE", _testEntries));
        var glossaryDeEn = glossaryCleanupDeEn.Capture(
              await translator.CreateGlossaryAsync(glossaryNameDeEn, "DE", "EN", _testEntries));
        var exception = await Assert.ThrowsAsync<ArgumentException>(
              () => translator.TranslateTextAsync(
                    "test",
                    "DE",
                    "EN",
                    new TextTranslateOptions { GlossaryId = glossaryDeEn.GlossaryId }));
        Assert.Contains("targetLanguageCode=\"en\" is deprecated", exception.Message);
      } finally {
        await glossaryCleanupEnDe.Cleanup();
        await glossaryCleanupDeEn.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryIdsTranslateTextSentence() {
      var translator = CreateTestTranslator();
      var glossaryCleanupArtist = new GlossaryCleanupUtility(
            translator,
            nameof(TestGlossaryIdsTranslateTextSentence) + "Artist");
      var glossaryCleanupPrize = new GlossaryCleanupUtility(
            translator,
            nameof(TestGlossaryIdsTranslateTextSentence) + "Prize");
      try {
        var sourceLang = "EN";
        var targetLang = "DE";
        var inputText = "The artist was awarded a prize.";
        var glossaryArtist = glossaryCleanupArtist.Capture(
              await translator.CreateGlossaryAsync(
                    glossaryCleanupArtist.GlossaryName,
                    sourceLang,
                    targetLang,
                    new GlossaryEntries(new Dictionary<string, string> { { "artist", "Maler" } })));
        var glossaryPrize = glossaryCleanupPrize.Capture(
              await translator.CreateGlossaryAsync(
                    glossaryCleanupPrize.GlossaryName,
                    sourceLang,
                    targetLang,
                    new GlossaryEntries(new Dictionary<string, string> { { "prize", "Gewinn" } })));

        var options = new TextTranslateOptions();
        options.GlossaryIds.Add(glossaryArtist.GlossaryId);
        options.GlossaryIds.Add(glossaryPrize.GlossaryId);
        var result = await translator.TranslateTextAsync(inputText, sourceLang, targetLang, options);
        if (!IsMockServer) {
          Assert.Contains("Maler", result.Text);
          Assert.Contains("Gewinn", result.Text);
        }

        // It is also possible to specify GlossaryInfo objects via the constructor
        result = await translator.TranslateTextAsync(
              inputText,
              sourceLang,
              targetLang,
              new TextTranslateOptions(new[] { glossaryArtist, glossaryPrize }));
        if (!IsMockServer) {
          Assert.Contains("Maler", result.Text);
          Assert.Contains("Gewinn", result.Text);
        }
      } finally {
        await glossaryCleanupArtist.Cleanup();
        await glossaryCleanupPrize.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryIdsTranslateDocument() {
      var translator = CreateTestTranslator();
      var sourceLang = "EN";
      var targetLang = "DE";
      var inputText = "artist\nprize";
      var tempDir = TempDir();
      var inputFilePath = Path.Combine(tempDir, "example_document.txt");
      var inputFileInfo = new FileInfo(inputFilePath);
      File.Delete(inputFilePath);
      File.WriteAllText(inputFilePath, inputText);
      var outputFilePath = Path.Combine(tempDir, "output_document.txt");
      var outputFileInfo = new FileInfo(outputFilePath);
      File.Delete(outputFilePath);

      var glossaryCleanupArtist = new GlossaryCleanupUtility(
            translator,
            nameof(TestGlossaryIdsTranslateDocument) + "Artist");
      var glossaryCleanupPrize = new GlossaryCleanupUtility(
            translator,
            nameof(TestGlossaryIdsTranslateDocument) + "Prize");
      try {
        var glossaryArtist = glossaryCleanupArtist.Capture(
              await translator.CreateGlossaryAsync(
                    glossaryCleanupArtist.GlossaryName,
                    sourceLang,
                    targetLang,
                    new GlossaryEntries(new Dictionary<string, string> { { "artist", "Maler" } })));
        var glossaryPrize = glossaryCleanupPrize.Capture(
              await translator.CreateGlossaryAsync(
                    glossaryCleanupPrize.GlossaryName,
                    sourceLang,
                    targetLang,
                    new GlossaryEntries(new Dictionary<string, string> { { "prize", "Gewinn" } })));

        var options = new DocumentTranslateOptions();
        options.GlossaryIds.Add(glossaryArtist.GlossaryId);
        options.GlossaryIds.Add(glossaryPrize.GlossaryId);
        await translator.TranslateDocumentAsync(inputFileInfo, outputFileInfo, "EN", "DE", options);
        Assert.Equal("Maler\nGewinn", File.ReadAllText(outputFilePath));
      } finally {
        await glossaryCleanupArtist.Cleanup();
        await glossaryCleanupPrize.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryIdsTranslateTextInvalid() {
      var translator = CreateTestTranslator();
      var glossaryCleanupEnDe = new GlossaryCleanupUtility(
            translator,
            nameof(TestGlossaryIdsTranslateTextInvalid) + "EnDe");
      var glossaryCleanupDeEn = new GlossaryCleanupUtility(
            translator,
            nameof(TestGlossaryIdsTranslateTextInvalid) + "DeEn");
      try {
        var glossaryEnDe = glossaryCleanupEnDe.Capture(
              await translator.CreateGlossaryAsync(glossaryCleanupEnDe.GlossaryName, "EN", "DE", _testEntries));
        var glossaryDeEn = glossaryCleanupDeEn.Capture(
              await translator.CreateGlossaryAsync(glossaryCleanupDeEn.GlossaryName, "DE", "EN", _testEntries));

        // GlossaryId and GlossaryIds cannot be used together
        var bothOptions = new TextTranslateOptions { GlossaryId = glossaryEnDe.GlossaryId };
        bothOptions.GlossaryIds.Add(glossaryDeEn.GlossaryId);
        var exception = await Assert.ThrowsAsync<ArgumentException>(
              () => translator.TranslateTextAsync("test", "EN", "DE", bothOptions));
        Assert.Contains("cannot be used together", exception.Message);

        // GlossaryIds must not contain more than 5 glossary IDs
        var tooManyOptions = new TextTranslateOptions();
        for (var i = 0; i < 6; i++) {
          tooManyOptions.GlossaryIds.Add(glossaryEnDe.GlossaryId);
        }

        exception = await Assert.ThrowsAsync<ArgumentException>(
              () => translator.TranslateTextAsync("test", "EN", "DE", tooManyOptions));
        Assert.Contains("more than 5", exception.Message);
      } finally {
        await glossaryCleanupEnDe.Cleanup();
        await glossaryCleanupDeEn.Cleanup();
      }
    }

    // Utility class for labelling test glossaries and deleting them at test completion
    private sealed class GlossaryCleanupUtility {
      private readonly Translator _translator;
      public readonly string GlossaryName;
      private string? _glossaryId;

      public GlossaryCleanupUtility(Translator translator, string testName) {
        _translator = translator;
        var uuid = Guid.NewGuid();
        GlossaryName = $"{GlossaryNamePrefix}: {testName} {uuid}";
      }

      public GlossaryInfo Capture(GlossaryInfo glossary) {
        _glossaryId = glossary.GlossaryId;
        return glossary;
      }

      public async Task Cleanup() {
        try {
          if (_glossaryId != null) {
            await _translator.DeleteGlossaryAsync(_glossaryId);
          }
        } catch {
          // All exceptions ignored
        }
      }
    }
  }
}
