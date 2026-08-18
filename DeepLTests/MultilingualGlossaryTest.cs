// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using Xunit;

namespace DeepLTests {
  public sealed class MultilingualGlossaryTest : BaseDeepLTest {
    private const string InvalidGlossaryId = "invalid_glossary_id";
    private const string GlossaryNamePrefix = "deepl-dotnet-test-glossary";
    private const string NonexistentGlossaryId = "96ab91fd-e715-41a1-adeb-5d701f84a483";
    private const string SourceLang = "en";
    private const string TargetLang = "de";
    private static readonly GlossaryEntries TestEntries = GlossaryEntries.FromTsv("Hello\tHallo");

    private readonly MultilingualGlossaryDictionaryEntries _testDictionary =
          new MultilingualGlossaryDictionaryEntries(SourceLang, TargetLang, TestEntries);

    [Fact]
    public async Task TestMultilingualGlossaryCreate() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryCreate));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var glossaryDicts = new[] {
              new MultilingualGlossaryDictionaryEntries(
                    SourceLang,
                    TargetLang,
                    new GlossaryEntries(new[] { ("Hello", "Hallo") })),
              new MultilingualGlossaryDictionaryEntries(
                    TargetLang,
                    SourceLang,
                    new GlossaryEntries(new[] { ("Hallo", "Hello") }))
        };
        var glossary =
              glossaryCleanup.Capture(await client.CreateMultilingualGlossaryAsync(glossaryName, glossaryDicts));

        Assert.Equal(glossaryName, glossary.Name);
        AssertGlossaryDictionariesEquivalent(glossaryDicts, glossary.Dictionaries);

        var getResult = await client.GetMultilingualGlossaryAsync(glossary.GlossaryId);
        Assert.Equal(getResult.Name, glossary.Name);
        Assert.Equal(getResult.CreationTime, glossary.CreationTime);
        AssertGlossaryDictionariesEquivalent(glossaryDicts, getResult.Dictionaries);
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryCreateLarge() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryCreate));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var entryPairs = new Dictionary<string, string>();
        for (var i = 0; i < 10000; i++) {
          entryPairs.Add($"Source-{i}", $"Target-{i}");
        }

        var entries = new GlossaryEntries(entryPairs);
        Assert.True(entries.ToTsv().Length > 100000);
        var glossaryDicts = new[] { new MultilingualGlossaryDictionaryEntries(SourceLang, TargetLang, entries) };


        var glossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(
                    glossaryName,
                    glossaryDicts));

        Assert.Equal(glossaryName, glossary.Name);
        AssertGlossaryDictionariesEquivalent(glossaryDicts, glossary.Dictionaries);
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryCreateCsv() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryCreateCsv));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var expectedEntries =
              new GlossaryEntries(new[] { ("sourceEntry1", "targetEntry1"), ("source\"Entry", "target,Entry") });

        const string csvContent = "sourceEntry1,targetEntry1,en,de\n\"source\"\"Entry\",\"target,Entry\",en,de";
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var glossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryFromCsvAsync(
                    glossaryName,
                    "en",
                    "de",
                    csvStream));
        var glossaryDict = await client.GetMultilingualGlossaryDictionaryEntriesAsync(glossary.GlossaryId, "en", "de");
        Assert.Equal(
              expectedEntries.ToDictionary(),
              glossaryDict.Entries.ToDictionary());
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryCreateInvalid() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryCreateInvalid));
      var glossaryName = glossaryCleanup.GlossaryName;
      var glossaryDict = new MultilingualGlossaryDictionaryEntries("en", "XX", TestEntries);
      try {
        await Assert.ThrowsAsync<ArgumentException>(
              () => client.CreateMultilingualGlossaryAsync("", new[] { _testDictionary }));
        await Assert.ThrowsAsync<DeepLException>(
              () => client.CreateMultilingualGlossaryAsync(
                    glossaryName,
                    new[] { glossaryDict }));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryGet() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryGet));
      var glossaryName = glossaryCleanup.GlossaryName;
      var glossaryDicts = new[] { _testDictionary };
      try {
        var createdGlossary =
              glossaryCleanup.Capture(await client.CreateMultilingualGlossaryAsync(glossaryName, glossaryDicts));
        var glossary = await client.GetMultilingualGlossaryAsync(createdGlossary.GlossaryId);
        Assert.Equal(createdGlossary.GlossaryId, glossary.GlossaryId);
        Assert.Equal(glossaryName, glossary.Name);
        Assert.Equal(createdGlossary.CreationTime, glossary.CreationTime);
        AssertGlossaryDictionariesEquivalent(glossaryDicts, createdGlossary.Dictionaries);

        await Assert.ThrowsAsync<DeepLException>(() => client.GetMultilingualGlossaryAsync(InvalidGlossaryId));
        await Assert.ThrowsAsync<GlossaryNotFoundException>(
              () => client.GetMultilingualGlossaryAsync(NonexistentGlossaryId));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryGetEntries() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryGetEntries));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var entries = new GlossaryEntries(
              new[] {
                    ("Apple", "Apfel"), ("Banana", "Banane"), ("A%=&", "B&=%"), ("\u0394\u3041", "\u6DF1"),
                    ("\uD83E\uDEA8", "\uD83E\uDEB5")
              });
        var glossaryDicts = new[] { new MultilingualGlossaryDictionaryEntries(SourceLang, TargetLang, entries) };
        var createdGlossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(
                    glossaryName,
                    glossaryDicts));
        Assert.Equal(
              entries.ToDictionary(),
              (await client.GetMultilingualGlossaryDictionaryEntriesAsync(createdGlossary, SourceLang, TargetLang))
              .Entries
              .ToDictionary());
        Assert.Equal(
              entries.ToDictionary(),
              (await client.GetMultilingualGlossaryDictionaryEntriesAsync(
                    createdGlossary.GlossaryId,
                    SourceLang,
                    TargetLang))
              .Entries
              .ToDictionary());
        Assert.Equal(
              entries.ToDictionary(),
              (await client.GetMultilingualGlossaryDictionaryEntriesAsync(
                    createdGlossary,
                    createdGlossary.Dictionaries[0]))
              .Entries
              .ToDictionary());
        Assert.Equal(
              entries.ToDictionary(),
              (await client.GetMultilingualGlossaryDictionaryEntriesAsync(
                    createdGlossary.GlossaryId,
                    createdGlossary.Dictionaries[0]))
              .Entries.ToDictionary());

        await Assert.ThrowsAsync<DeepLException>(
              () => client.GetMultilingualGlossaryDictionaryEntriesAsync(InvalidGlossaryId, SourceLang, TargetLang));
        await Assert.ThrowsAsync<GlossaryNotFoundException>(
              () => client.GetMultilingualGlossaryDictionaryEntriesAsync(
                    NonexistentGlossaryId,
                    SourceLang,
                    TargetLang));
        await Assert.ThrowsAsync<DeepLException>(
              () => client.GetMultilingualGlossaryDictionaryEntriesAsync(createdGlossary.GlossaryId, "en", "XX"));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryList() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryList));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        glossaryCleanup.Capture(await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { _testDictionary }));
        var glossaries = await client.ListMultilingualGlossariesAsync();
        Assert.Contains(glossaries, glossary => glossary.Name == glossaryName);
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryDelete() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryDelete));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var glossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { _testDictionary }));
        await client.DeleteMultilingualGlossaryAsync(glossary);
        await Assert.ThrowsAsync<GlossaryNotFoundException>(() => client.DeleteMultilingualGlossaryAsync(glossary));

        await Assert.ThrowsAsync<DeepLException>(() => client.DeleteMultilingualGlossaryAsync(InvalidGlossaryId));
        await Assert.ThrowsAsync<GlossaryNotFoundException>(
              () => client.DeleteMultilingualGlossaryAsync(NonexistentGlossaryId));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryDictionaryDelete() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryDictionaryDelete));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var glossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { _testDictionary }));
        await client.DeleteMultilingualGlossaryDictionaryAsync(
              glossary,
              _testDictionary.SourceLanguageCode,
              _testDictionary.TargetLanguageCode);

        await Assert.ThrowsAsync<GlossaryNotFoundException>(
              () => client.DeleteMultilingualGlossaryDictionaryAsync(glossary, "en", "de"));
        await Assert.ThrowsAsync<DeepLException>(
              () => client.DeleteMultilingualGlossaryDictionaryAsync(InvalidGlossaryId, "en", "de"));
        await Assert.ThrowsAsync<GlossaryNotFoundException>(
              () => client.DeleteMultilingualGlossaryDictionaryAsync(NonexistentGlossaryId, "en", "de"));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryReplaceDictionary() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryReplaceDictionary));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var entries = new List<(string Key, string Value)> { ("key1", "value1") };
        var createdGlossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(
                    glossaryName,
                    new[] {
                          new MultilingualGlossaryDictionaryEntries(
                                SourceLang,
                                TargetLang,
                                new GlossaryEntries(entries))
                    }));
        entries.Add(("key2", "value2"));
        var updatedGlossaryDict = await client.ReplaceMultilingualGlossaryDictionaryAsync(
              createdGlossary,
              SourceLang,
              TargetLang,
              new GlossaryEntries(entries));

        AssertGlossaryDictionariesEquivalent(
              new[] { new MultilingualGlossaryDictionaryEntries(SourceLang, TargetLang, new GlossaryEntries(entries)) },
              new[] { updatedGlossaryDict });
        entries.Add(("key3", "value3"));
        updatedGlossaryDict = await client.ReplaceMultilingualGlossaryDictionaryAsync(
              createdGlossary.GlossaryId,
              SourceLang,
              TargetLang,
              new GlossaryEntries(entries));

        AssertGlossaryDictionariesEquivalent(
              new[] { new MultilingualGlossaryDictionaryEntries(SourceLang, TargetLang, new GlossaryEntries(entries)) },
              new[] { updatedGlossaryDict });

        entries.Add(("key4", "value4"));
        var expectedDict = new MultilingualGlossaryDictionaryEntries(
              SourceLang,
              TargetLang,
              new GlossaryEntries(entries));
        updatedGlossaryDict =
              await client.ReplaceMultilingualGlossaryDictionaryAsync(createdGlossary.GlossaryId, expectedDict);

        AssertGlossaryDictionariesEquivalent(new[] { expectedDict }, new[] { updatedGlossaryDict });

        entries.Add(("key5", "value5"));
        expectedDict = new MultilingualGlossaryDictionaryEntries(SourceLang, TargetLang, new GlossaryEntries(entries));
        updatedGlossaryDict = await client.ReplaceMultilingualGlossaryDictionaryAsync(createdGlossary, expectedDict);

        AssertGlossaryDictionariesEquivalent(new[] { expectedDict }, new[] { updatedGlossaryDict });

        await Assert.ThrowsAsync<DeepLException>(
              () => client.ReplaceMultilingualGlossaryDictionaryAsync(
                    InvalidGlossaryId,
                    SourceLang,
                    TargetLang,
                    new GlossaryEntries(entries)));
        await Assert.ThrowsAsync<GlossaryNotFoundException>(
              () => client.ReplaceMultilingualGlossaryDictionaryAsync(
                    NonexistentGlossaryId,
                    SourceLang,
                    TargetLang,
                    new GlossaryEntries(entries)));
        await Assert.ThrowsAsync<DeepLException>(
              () => client.ReplaceMultilingualGlossaryDictionaryAsync(
                    createdGlossary.GlossaryId,
                    "en",
                    "XX",
                    new GlossaryEntries(entries)));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryUpdateName() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryUpdateName));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var glossary =
              glossaryCleanup.Capture(
                    await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { _testDictionary }));
        var newName = "New Glossary Name";
        var updatedGlossary = await client.UpdateMultilingualGlossaryNameAsync(glossary.GlossaryId, newName);
        Assert.Equal(newName, updatedGlossary.Name);
        var getGlossaryResult = await client.GetMultilingualGlossaryAsync(glossary.GlossaryId);
        Assert.Equal(updatedGlossary.Name, getGlossaryResult.Name);

        await Assert.ThrowsAsync<DeepLException>(
              () => client.UpdateMultilingualGlossaryNameAsync(InvalidGlossaryId, newName));
        await Assert.ThrowsAsync<GlossaryNotFoundException>(
              () => client.UpdateMultilingualGlossaryNameAsync(NonexistentGlossaryId, newName));
        await Assert.ThrowsAsync<ArgumentException>(
              () => client.UpdateMultilingualGlossaryNameAsync(glossary.GlossaryId, ""));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryUpdateDictionaryFromCsv() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryUpdateDictionaryFromCsv));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var glossary =
              glossaryCleanup.Capture(
                    await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { _testDictionary }));
        var expectedEntries =
              new GlossaryEntries(new[] { ("Hello", "Guten Tag"), ("Goodbye", "Auf Wiedersehen") });

        const string csvContent = "Hello,Guten Tag\nGoodbye,Auf Wiedersehen";
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var updatedGlossary = await client.UpdateMultilingualGlossaryDictionaryFromCsvAsync(
              glossary,
              "en",
              "de",
              csvStream);
        Assert.Single(updatedGlossary.Dictionaries);
        Assert.Equal(updatedGlossary.Dictionaries[0].EntryCount, expectedEntries.ToDictionary().Count);
        var glossaryDict = await client.GetMultilingualGlossaryDictionaryEntriesAsync(glossary.GlossaryId, "en", "de");
        Assert.Equal(
              expectedEntries.ToDictionary(),
              glossaryDict.Entries.ToDictionary());
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryUpdateDictionary() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestMultilingualGlossaryUpdateDictionary));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var entries = new List<(string Key, string Value)> { ("key1", "value1") };
        var createdGlossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(
                    glossaryName,
                    new[] {
                          new MultilingualGlossaryDictionaryEntries(
                                SourceLang,
                                TargetLang,
                                new GlossaryEntries(entries))
                    }));
        entries = new List<(string Key, string Value)> { ("key1", "value2") };
        var updatedGlossary = await client.UpdateMultilingualGlossaryDictionaryAsync(
              createdGlossary,
              SourceLang,
              TargetLang,
              new GlossaryEntries(entries));

        AssertGlossaryDictionariesEquivalent(
              new[] { new MultilingualGlossaryDictionaryEntries(SourceLang, TargetLang, new GlossaryEntries(entries)) },
              updatedGlossary.Dictionaries);
        entries = new List<(string Key, string Value)> { ("key1", "value3") };
        updatedGlossary = await client.UpdateMultilingualGlossaryDictionaryAsync(
              createdGlossary.GlossaryId,
              SourceLang,
              TargetLang,
              new GlossaryEntries(entries));

        AssertGlossaryDictionariesEquivalent(
              new[] { new MultilingualGlossaryDictionaryEntries(SourceLang, TargetLang, new GlossaryEntries(entries)) },
              updatedGlossary.Dictionaries);

        entries = new List<(string Key, string Value)> { ("key1", "value4") };
        var expectedDict = new MultilingualGlossaryDictionaryEntries(
              SourceLang,
              TargetLang,
              new GlossaryEntries(entries));
        updatedGlossary =
              await client.UpdateMultilingualGlossaryDictionaryAsync(createdGlossary.GlossaryId, expectedDict);

        AssertGlossaryDictionariesEquivalent(new[] { expectedDict }, updatedGlossary.Dictionaries);

        entries = new List<(string Key, string Value)> { ("key1", "value5") };
        expectedDict = new MultilingualGlossaryDictionaryEntries(SourceLang, TargetLang, new GlossaryEntries(entries));
        updatedGlossary = await client.UpdateMultilingualGlossaryDictionaryAsync(createdGlossary, expectedDict);

        AssertGlossaryDictionariesEquivalent(new[] { expectedDict }, updatedGlossary.Dictionaries);

        await Assert.ThrowsAsync<DeepLException>(
              () => client.ReplaceMultilingualGlossaryDictionaryAsync(
                    InvalidGlossaryId,
                    SourceLang,
                    TargetLang,
                    new GlossaryEntries(entries)));
        await Assert.ThrowsAsync<GlossaryNotFoundException>(
              () => client.ReplaceMultilingualGlossaryDictionaryAsync(
                    NonexistentGlossaryId,
                    SourceLang,
                    TargetLang,
                    new GlossaryEntries(entries)));
        await Assert.ThrowsAsync<DeepLException>(
              () => client.ReplaceMultilingualGlossaryDictionaryAsync(
                    createdGlossary.GlossaryId,
                    "en",
                    "XX",
                    new GlossaryEntries(entries)));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestMultilingualGlossaryReplaceDictionaryFromCsv() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(
            client,
            nameof(TestMultilingualGlossaryReplaceDictionaryFromCsv));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var glossary =
              glossaryCleanup.Capture(
                    await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { _testDictionary }));
        var expectedEntries =
              new GlossaryEntries(new[] { ("sourceEntry1", "targetEntry1"), ("source\"Entry", "target,Entry") });

        const string csvContent = "sourceEntry1,targetEntry1\n\"source\"\"Entry\",\"target,Entry\"";
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var replacedGlossaryDict = await client.ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
              glossary,
              "en",
              "de",
              csvStream);
        Assert.Equal(replacedGlossaryDict.EntryCount, expectedEntries.ToDictionary().Count);
        var glossaryDict = await client.GetMultilingualGlossaryDictionaryEntriesAsync(glossary.GlossaryId, "en", "de");
        Assert.Equal(
              expectedEntries.ToDictionary(),
              glossaryDict.Entries.ToDictionary());
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryTranslateTextSentence() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestGlossaryTranslateTextSentence));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var sourceLang = "en";
        var targetLang = "de";
        var inputText = "The artist was awarded a prize.";
        var entries = new Dictionary<string, string> { { "artist", "Maler" }, { "prize", "Gewinn" } };
        var glossaryDict = new MultilingualGlossaryDictionaryEntries(
              sourceLang,
              targetLang,
              new GlossaryEntries(entries));
        var glossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { glossaryDict }));
        var result = await client.TranslateTextAsync(
              inputText,
              sourceLang,
              targetLang,
              new TextTranslateOptions { GlossaryId = glossary.GlossaryId });
        if (!IsMockServer) {
          Assert.Contains("Maler", result.Text);
          Assert.Contains("Gewinn", result.Text);
        }

        // It is also possible to specify MultilingualGlossaryInfo
        result = await client.TranslateTextAsync(
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
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestGlossaryTranslateTextSentence));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var textsEn = new[] { "Apple", "Banana" };
        var textsDe = new[] { "Apfel", "Banane" };
        var entriesEnDe = new Dictionary<string, string>();
        var entriesDeEn = new Dictionary<string, string>();
        for (var i = 0; i < textsEn.Length; i++) {
          entriesDeEn[textsDe[i]] = textsEn[i];
          entriesEnDe[textsEn[i]] = textsDe[i];
        }

        var glossaryDictEnDe = new MultilingualGlossaryDictionaryEntries("en", "de", new GlossaryEntries(entriesEnDe));
        var glossaryDictDeEn = new MultilingualGlossaryDictionaryEntries("de", "en", new GlossaryEntries(entriesDeEn));

        var glossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { glossaryDictEnDe, glossaryDictDeEn }));


        var result = await client.TranslateTextAsync(textsEn, "en", "de", new TextTranslateOptions(glossary));
        Assert.Equal(textsDe, result.Select(textResult => textResult.Text));

        result = await client.TranslateTextAsync(
              textsDe,
              "DE",
              "EN-US",
              new TextTranslateOptions { GlossaryId = glossary.GlossaryId });
        Assert.Equal(textsEn, result.Select(textResult => textResult.Text));

        result = await client.TranslateTextAsync(
              textsDe,
              "DE",
              "EN-US",
              new TextTranslateOptions(glossary));
        Assert.Equal(textsEn, result.Select(textResult => textResult.Text));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryTranslateDocument() {
      var client = CreateTestClient();
      var sourceLang = "en";
      var targetLang = "de";
      var inputText = "artist\nprize";
      var entries = new Dictionary<string, string> { { "artist", "Maler" }, { "prize", "Gewinn" } };
      var glossaryDict = new MultilingualGlossaryDictionaryEntries(
            sourceLang,
            targetLang,
            new GlossaryEntries(entries));
      var tempDir = TempDir();
      var inputFilePath = Path.Combine(tempDir, "example_document.txt");
      var inputFileInfo = new FileInfo(inputFilePath);
      File.Delete(inputFilePath);
      File.WriteAllText(inputFilePath, inputText);
      var outputFilePath = Path.Combine(tempDir, "output_document.txt");
      var outputFileInfo = new FileInfo(outputFilePath);
      File.Delete(outputFilePath);

      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestGlossaryTranslateDocument));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var glossary =
              glossaryCleanup.Capture(
                    await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { glossaryDict }));

        await client.TranslateDocumentAsync(
              inputFileInfo,
              outputFileInfo,
              "en",
              "de",
              new DocumentTranslateOptions(glossary));
        Assert.Equal("Maler\nGewinn", File.ReadAllText(outputFilePath));

        File.Delete(outputFilePath);
        await client.TranslateDocumentAsync(
              inputFileInfo,
              outputFileInfo,
              "en",
              "de",
              new DocumentTranslateOptions { GlossaryId = glossary.GlossaryId });
        Assert.Equal("Maler\nGewinn", File.ReadAllText(outputFilePath));
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryTranslateTextInvalid() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestGlossaryTranslateTextInvalid));
      var glossaryName = glossaryCleanup.GlossaryName;
      var glossaryDictEnDe = new MultilingualGlossaryDictionaryEntries("en", "de", TestEntries);
      var glossaryDictDeEn = new MultilingualGlossaryDictionaryEntries("de", "en", TestEntries);
      try {
        var glossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { glossaryDictEnDe, glossaryDictDeEn }));
        var exception = await Assert.ThrowsAsync<ArgumentException>(
              () => client.TranslateTextAsync(
                    "test",
                    "de",
                    "en",
                    new TextTranslateOptions { GlossaryId = glossary.GlossaryId }));
        Assert.Contains("targetLanguageCode=\"en\" is deprecated", exception.Message);
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryTranslateTextWithoutSourceLang() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestGlossaryTranslateTextWithoutSourceLang));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var entries = new Dictionary<string, string> { { "artist", "Maler" }, { "prize", "Gewinn" } };
        var glossaryDict = new MultilingualGlossaryDictionaryEntries("en", "de", new GlossaryEntries(entries));
        var glossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { glossaryDict }));

        // No source language: DeepL detects it and resolves the glossary's dictionary from the result.
        var result = await client.TranslateTextAsync(
              "The artist was awarded a prize.",
              null,
              "de",
              new TextTranslateOptions { GlossaryId = glossary.GlossaryId });

        if (!IsMockServer) {
          Assert.Equal("en", result.DetectedSourceLanguageCode);
          Assert.Contains("Maler", result.Text);
          Assert.Contains("Gewinn", result.Text);
        }
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryIdsTranslateTextWithoutSourceLang() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(client, nameof(TestGlossaryIdsTranslateTextWithoutSourceLang));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var entries = new Dictionary<string, string> { { "artist", "Maler" } };
        var glossaryDict = new MultilingualGlossaryDictionaryEntries("en", "de", new GlossaryEntries(entries));
        var glossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { glossaryDict }));

        var options = new TextTranslateOptions { GlossaryIds = new List<string> { glossary.GlossaryId } };
        var result = await client.TranslateTextAsync("The artist was awarded a prize.", null, "de", options);

        if (!IsMockServer) {
          Assert.Equal("en", result.DetectedSourceLanguageCode);
          Assert.Contains("Maler", result.Text);
        }
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    [Fact]
    public async Task TestGlossaryTranslateDocumentWithoutSourceLangThrows() {
      var client = CreateTestClient();
      var glossaryCleanup = new GlossaryCleanupUtility(
            client,
            nameof(TestGlossaryTranslateDocumentWithoutSourceLangThrows));
      var glossaryName = glossaryCleanup.GlossaryName;
      try {
        var glossaryDict = new MultilingualGlossaryDictionaryEntries("en", "de", TestEntries);
        var glossary = glossaryCleanup.Capture(
              await client.CreateMultilingualGlossaryAsync(glossaryName, new[] { glossaryDict }));

        // Document translation cannot detect the source language when a glossary is used.
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("The artist was awarded a prize."));
        var exception = await Assert.ThrowsAsync<ArgumentException>(
              () => client.TranslateDocumentUploadAsync(
                    stream,
                    "test.txt",
                    null,
                    "de",
                    new DocumentTranslateOptions { GlossaryId = glossary.GlossaryId }));
        Assert.Contains("sourceLanguageCode is required", exception.Message);
      } finally {
        await glossaryCleanup.Cleanup();
      }
    }

    // Utility function for determining if a list of MultilingualGlossaryDictionaryEntries objects (that have entries) matches
    // a list of MultilingualGlossaryDictionaryInfo (that do not contain entries, but just a count of the number of entries for
    // that glossary dictionary
    private void AssertGlossaryDictionariesEquivalent(
          MultilingualGlossaryDictionaryEntries[] expectedDicts,
          MultilingualGlossaryDictionaryInfo[] actualDicts) {
      Assert.Equal(expectedDicts.Length, actualDicts.Length);
      foreach (var expectedDict in expectedDicts) {
        var actualDict = actualDicts.FirstOrDefault(
              d => string.Equals(
                         d.SourceLanguageCode,
                         expectedDict.SourceLanguageCode,
                         StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(
                         d.TargetLanguageCode,
                         expectedDict.TargetLanguageCode,
                         StringComparison.OrdinalIgnoreCase));

        Assert.Equal(expectedDict.SourceLanguageCode.ToLower(), actualDict?.SourceLanguageCode.ToLower());
        Assert.Equal(expectedDict.TargetLanguageCode.ToLower(), actualDict?.TargetLanguageCode.ToLower());
        Assert.Equal(expectedDict.Entries.ToDictionary().Count(), actualDict?.EntryCount);
      }
    }

    // Utility class for labelling test glossaries and deleting them at test completion
    private sealed class GlossaryCleanupUtility {
      private readonly DeepLClient _client;
      public readonly string GlossaryName;
      private string? _glossaryId;

      public GlossaryCleanupUtility(DeepLClient client, string testName) {
        _client = client;
        var uuid = Guid.NewGuid();
        GlossaryName = $"{GlossaryNamePrefix}: {testName} {uuid}";
      }

      public MultilingualGlossaryInfo Capture(MultilingualGlossaryInfo glossary) {
        _glossaryId = glossary.GlossaryId;
        return glossary;
      }

      public async Task Cleanup() {
        try {
          if (_glossaryId != null) {
            await _client.DeleteMultilingualGlossaryAsync(_glossaryId);
          }
        } catch {
          // All exceptions ignored
        }
      }
    }
  }
}
