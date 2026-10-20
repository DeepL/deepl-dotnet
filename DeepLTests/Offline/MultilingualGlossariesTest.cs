// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class MultilingualGlossariesTest : OfflineTest {
    private const string GlossaryReply =
          "{\"glossary_id\":\"g1\",\"name\":\"My glossary\",\"dictionaries\":[" +
          "{\"source_lang\":\"en\",\"target_lang\":\"de\",\"entry_count\":1}," +
          "{\"source_lang\":\"en\",\"target_lang\":\"fr\",\"entry_count\":2}]," +
          "\"creation_time\":\"2021-08-03T14:16:18.329Z\"}";

    private const string DictionaryReply = "{\"source_lang\":\"en\",\"target_lang\":\"de\",\"entry_count\":1}";
    private const string Csv = "Apple,Apfel,en,de\n\"A,B\",\"C\"\"D\",en,de";

    private static readonly GlossaryEntries Apple = new GlossaryEntries(new[] { ("Apple", "Apfel") });

    private static MemoryStream CsvStream() => new MemoryStream(Encoding.UTF8.GetBytes(Csv));

    private static MultilingualGlossaryInfo Info() =>
          new MultilingualGlossaryInfo("g1", "n", new MultilingualGlossaryDictionaryInfo[0], DateTime.UtcNow);

    [Fact]
    public async Task SendsBracketedDictionaryFieldsWhenCreating() {
      ExpectForm(
                  HttpMethod.Post,
                  "v3/glossaries",
                  ("name", "My glossary"),
                  ("dictionaries[0].source_lang", "en"),
                  ("dictionaries[0].target_lang", "de"),
                  ("dictionaries[0].entries", "Apple\tApfel"),
                  ("dictionaries[0].entries_format", "tsv"),
                  ("dictionaries[1].source_lang", "en"),
                  ("dictionaries[1].target_lang", "fr"),
                  ("dictionaries[1].entries", "Apple\tPomme"),
                  ("dictionaries[1].entries_format", "tsv"))
            .RespondJson(GlossaryReply);
      var glossary = await CreateClient().CreateMultilingualGlossaryAsync(
            "My glossary",
            new[] {
              new MultilingualGlossaryDictionaryEntries("en", "de", Apple),
              new MultilingualGlossaryDictionaryEntries(
                    "en",
                    "fr",
                    new GlossaryEntries(new[] { ("Apple", "Pomme") }))
            });
      Assert.Equal("g1", glossary.GlossaryId);
      Assert.Equal(new DateTime(2021, 8, 3, 14, 16, 18, 329, DateTimeKind.Utc), glossary.CreationTime.ToUniversalTime());
      Assert.Equal("fr", glossary.Dictionaries[1].TargetLanguageCode);
      Assert.Equal(2, glossary.Dictionaries[1].EntryCount);
    }

    [Fact]
    public async Task SendsCsvDictionaryWhenCreatingFromCsv() {
      ExpectForm(
                  HttpMethod.Post,
                  "v3/glossaries",
                  ("name", "n"),
                  ("dictionaries[0].source_lang", "en"),
                  ("dictionaries[0].target_lang", "de"),
                  ("dictionaries[0].entries", Csv),
                  ("dictionaries[0].entries_format", "csv"))
            .RespondJson(GlossaryReply);
      await CreateClient().CreateMultilingualGlossaryFromCsvAsync("n", "en", "de", CsvStream());
    }

    [Fact]
    public async Task ParsesGlossaryInfoAndList() {
      ExpectNoBody(HttpMethod.Get, "v3/glossaries/g1").RespondJson(GlossaryReply);
      ExpectNoBody(HttpMethod.Get, "v3/glossaries").RespondJson("{\"glossaries\":[" + GlossaryReply + "]}");
      var client = CreateClient();
      await client.GetMultilingualGlossaryAsync("g1");
      Assert.Equal("g1", Assert.Single(await client.ListMultilingualGlossariesAsync()).GlossaryId);
    }

    [Fact]
    public async Task SendsNameWhenRenaming() {
      ExpectForm(Patch, "v3/glossaries/g1", ("name", "New name")).RespondJson(GlossaryReply);
      await CreateClient().UpdateMultilingualGlossaryNameAsync("g1", "New name");
    }

    [Fact]
    public async Task SendsBracketedDictionaryWhenUpdatingDictionary() {
      var fields = new[] {
        ("dictionaries[0].source_lang", "en"), ("dictionaries[0].target_lang", "de"),
        ("dictionaries[0].entries", "Apple\tApfel"), ("dictionaries[0].entries_format", "tsv")
      };
      ExpectForm(Patch, "v3/glossaries/g1", fields).RespondJson(GlossaryReply);
      ExpectForm(Patch, "v3/glossaries/g1", fields).RespondJson(GlossaryReply);
      ExpectForm(Patch, "v3/glossaries/g1", fields).RespondJson(GlossaryReply);
      var client = CreateClient();
      await client.UpdateMultilingualGlossaryDictionaryAsync("g1", "en", "de", Apple);
      await client.UpdateMultilingualGlossaryDictionaryAsync(Info(), "en", "de", Apple);
      await client.UpdateMultilingualGlossaryDictionaryAsync(
            Info(),
            new MultilingualGlossaryDictionaryEntries("en", "de", Apple));
    }

    [Fact]
    public async Task SendsCsvDictionaryWhenUpdatingDictionaryFromCsv() {
      ExpectForm(
                  Patch,
                  "v3/glossaries/g1",
                  ("dictionaries[0].source_lang", "en"),
                  ("dictionaries[0].target_lang", "de"),
                  ("dictionaries[0].entries", Csv),
                  ("dictionaries[0].entries_format", "csv"))
            .RespondJson(GlossaryReply);
      await CreateClient().UpdateMultilingualGlossaryDictionaryFromCsvAsync("g1", "en", "de", CsvStream());
    }

    [Fact]
    public async Task SendsPutWhenReplacingDictionary() {
      var fields = new[] {
        ("source_lang", "en"), ("target_lang", "de"), ("entries_format", "tsv"), ("entries", "Apple\tApfel")
      };
      ExpectForm(HttpMethod.Put, "v3/glossaries/g1/dictionaries", fields).RespondJson(DictionaryReply);
      ExpectForm(HttpMethod.Put, "v3/glossaries/g1/dictionaries", fields).RespondJson(DictionaryReply);
      var client = CreateClient();
      var replaced = await client.ReplaceMultilingualGlossaryDictionaryAsync("g1", "en", "de", Apple);
      Assert.Equal("en", replaced.SourceLanguageCode);
      Assert.Equal("de", replaced.TargetLanguageCode);
      Assert.Equal(1, replaced.EntryCount);
      await client.ReplaceMultilingualGlossaryDictionaryAsync(
            "g1",
            new MultilingualGlossaryDictionaryEntries("en", "de", Apple));
    }

    [Fact]
    public async Task SendsCsvWhenReplacingDictionaryFromCsv() {
      ExpectForm(
                  HttpMethod.Put,
                  "v3/glossaries/g1/dictionaries",
                  ("source_lang", "en"),
                  ("target_lang", "de"),
                  ("entries_format", "csv"),
                  ("entries", Csv))
            .RespondJson(DictionaryReply);
      await CreateClient().ReplaceMultilingualGlossaryDictionaryFromCsvAsync("g1", "en", "de", CsvStream());
    }

    [Fact]
    public async Task SendsLanguagePairInQueryWhenDeletingDictionary() {
      ExpectNoBody(HttpMethod.Delete, "v3/glossaries/g1/dictionaries")
            .WithExactQueryString("source_lang=en&target_lang=de")
            .Respond(HttpStatusCode.NoContent);
      await CreateClient().DeleteMultilingualGlossaryDictionaryAsync("g1", "en", "de");
    }

    [Fact]
    public async Task SendsDeleteToGlossaryPath() {
      ExpectNoBody(HttpMethod.Delete, "v3/glossaries/g1").Respond(HttpStatusCode.NoContent);
      await CreateClient().DeleteMultilingualGlossaryAsync("g1");
    }

    [Fact]
    public async Task SendsLanguagePairInQueryAndParsesEntries() {
      ExpectNoBody(HttpMethod.Get, "v3/glossaries/g1/entries")
            .WithExactQueryString("source_lang=en&target_lang=de")
            .RespondJson(
                  "{\"dictionaries\":[{\"source_lang\":\"en\",\"target_lang\":\"de\"," +
                  "\"entries\":\"Apple\\tApfel\\nBanana\\tBanane\",\"entries_format\":\"tsv\"}]}");
      var dictionary = await CreateClient().GetMultilingualGlossaryDictionaryEntriesAsync("g1", "en", "de");
      Assert.Equal("en", dictionary.SourceLanguageCode);
      Assert.Equal("de", dictionary.TargetLanguageCode);
      Assert.Equal(
            new System.Collections.Generic.Dictionary<string, string> { { "Apple", "Apfel" }, { "Banana", "Banane" } },
            dictionary.Entries.ToDictionary());
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWhenNoDictionaryIsReturned() {
      ExpectNoBody(HttpMethod.Get, "v3/glossaries/g1/entries").RespondJson("{\"dictionaries\":[]}");
      await Assert.ThrowsAsync<NotFoundException>(
            () => CreateClient().GetMultilingualGlossaryDictionaryEntriesAsync("g1", "en", "de"));
    }

    [Fact]
    public async Task ThrowsGlossaryNotFoundExceptionWhenStatusIs404() {
      ExpectNoBody(HttpMethod.Get, "v3/glossaries/x").RespondJson("{}", HttpStatusCode.NotFound);
      await Assert.ThrowsAsync<GlossaryNotFoundException>(() => CreateClient().GetMultilingualGlossaryAsync("x"));
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenArgumentsAreEmpty() {
      var client = CreateClient();
      var dictionary = new MultilingualGlossaryDictionaryEntries("en", "de", Apple);
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateMultilingualGlossaryAsync("", new[] { dictionary }));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateMultilingualGlossaryAsync("n", new MultilingualGlossaryDictionaryEntries[0]));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateMultilingualGlossaryFromCsvAsync("n", "", "de", CsvStream()));
      await Assert.ThrowsAsync<ArgumentException>(() => client.GetMultilingualGlossaryAsync(" "));
      await Assert.ThrowsAsync<ArgumentException>(() => client.GetMultilingualGlossaryDictionaryEntriesAsync("", "en", "de"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.GetMultilingualGlossaryDictionaryEntriesAsync("g1", "", "de"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteMultilingualGlossaryAsync(""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteMultilingualGlossaryDictionaryAsync("g1", "en", ""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.ReplaceMultilingualGlossaryDictionaryAsync("", "en", "de", Apple));
      await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateMultilingualGlossaryNameAsync("g1", " "));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.UpdateMultilingualGlossaryDictionaryAsync("g1", "", "de", Apple));
    }
  }
}
