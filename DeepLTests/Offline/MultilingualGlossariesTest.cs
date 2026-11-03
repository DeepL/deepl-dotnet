// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

    /// <summary>Returns the JSON <c>dictionaries</c> array of the given (source, target, entries, format) tuples.</summary>
    private static Dictionary<string, string>[] Dictionaries(
          params (string Source, string Target, string Entries, string Format)[] dictionaries) =>
          Array.ConvertAll(
                dictionaries,
                dictionary => new Dictionary<string, string> {
                      ["source_lang"] = dictionary.Source,
                      ["target_lang"] = dictionary.Target,
                      ["entries"] = dictionary.Entries,
                      ["entries_format"] = dictionary.Format
                });

    private static MemoryStream CsvStream() => new MemoryStream(Encoding.UTF8.GetBytes(Csv));

    private static MultilingualGlossaryInfo Info() =>
          new MultilingualGlossaryInfo("g1", "n", new MultilingualGlossaryDictionaryInfo[0], DateTime.UtcNow);

    [Fact]
    public async Task SendsDictionariesAsJsonArrayWhenCreating() {
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v3/glossaries",
                  ("name", "My glossary"),
                  ("dictionaries", Dictionaries(("en", "de", "Apple\tApfel", "tsv"), ("en", "fr", "Apple\tPomme", "tsv"))))
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
    public async Task SendsTrimmedTsvTermsWhenCreatingFromTsv() {
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v3/glossaries",
                  ("name", "My glossary"),
                  ("dictionaries", Dictionaries(("en", "de", "Apple\tApfel\nPear\tBirne", "tsv"))))
            .RespondJson(GlossaryReply);
      await CreateClient().CreateMultilingualGlossaryAsync(
            "My glossary",
            new[] {
              new MultilingualGlossaryDictionaryEntries(
                    "en",
                    "de",
                    GlossaryEntries.FromTsv("  Apple \t Apfel  \r\n\r\n Pear\tBirne \r\n"))
            });
    }

    [Fact]
    public async Task SendsCsvDictionaryWhenCreatingFromCsv() {
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v3/glossaries",
                  ("name", "n"),
                  ("dictionaries", Dictionaries(("en", "de", Csv, "csv"))))
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
      ExpectJsonFields(
                  Patch,
                  "v3/glossaries/g1",
                  ("name", "New name")).RespondJson(GlossaryReply);
      await CreateClient().UpdateMultilingualGlossaryNameAsync("g1", "New name");
    }

    [Fact]
    public async Task SendsDictionaryAsJsonArrayWhenUpdatingDictionary() {
      for (var i = 0; i < 3; i++) {
        ExpectJsonFields(Patch, "v3/glossaries/g1", ("dictionaries", Dictionaries(("en", "de", "Apple\tApfel", "tsv"))))
              .RespondJson(GlossaryReply);
      }
      var client = CreateClient();
      await client.UpdateMultilingualGlossaryDictionaryAsync("g1", "en", "de", Apple);
      await client.UpdateMultilingualGlossaryDictionaryAsync(Info(), "en", "de", Apple);
      await client.UpdateMultilingualGlossaryDictionaryAsync(
            Info(),
            new MultilingualGlossaryDictionaryEntries("en", "de", Apple));
    }

    [Fact]
    public async Task SendsCsvDictionaryWhenUpdatingDictionaryFromCsv() {
      ExpectJsonFields(
                  Patch,
                  "v3/glossaries/g1",
                  ("dictionaries", Dictionaries(("en", "de", Csv, "csv"))))
            .RespondJson(GlossaryReply);
      await CreateClient().UpdateMultilingualGlossaryDictionaryFromCsvAsync("g1", "en", "de", CsvStream());
    }

    [Fact]
    public async Task SendsExtraBodyParametersLastInPatchBodyWhenUpdatingWithOptions() {
      ExpectJsonFields(
                  Patch,
                  "v3/glossaries/g1",
                  ("name", "New name"),
                  ("extra", "1")).RespondJson(GlossaryReply);
      ExpectJsonFields(
                  Patch,
                  "v3/glossaries/g1",
                  ("dictionaries", Dictionaries(("en", "de", "Apple\tApfel", "tsv"))),
                  ("extra", "1"))
            .RespondJson(GlossaryReply);
      var client = CreateClient();
      await client.UpdateMultilingualGlossaryNameAsync(
            "g1",
            "New name",
            new BaseRequestOptions { ExtraBodyParameters = new Dictionary<string, object?> { ["extra"] = "1" } });
      await client.UpdateMultilingualGlossaryDictionaryAsync(
            "g1",
            "en",
            "de",
            Apple,
            new BaseRequestOptions {
              ExtraBodyParameters = new Dictionary<string, object?> { ["extra"] = "1" }
            });
    }

    [Fact]
    public async Task SendsJsonPutWhenReplacingDictionary() {
      const string body =
            "{\"source_lang\":\"en\",\"target_lang\":\"de\",\"entries\":\"Apple\\tApfel\",\"entries_format\":\"tsv\"}";
      ExpectJson(HttpMethod.Put, "v3/glossaries/g1/dictionaries", body).RespondJson(DictionaryReply);
      ExpectJson(HttpMethod.Put, "v3/glossaries/g1/dictionaries", body).RespondJson(DictionaryReply);
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
    public async Task SendsJsonCsvWhenReplacingDictionaryFromCsv() {
      ExpectJson(
                  HttpMethod.Put,
                  "v3/glossaries/g1/dictionaries",
                  "{\"source_lang\":\"en\",\"target_lang\":\"de\",\"entries\":" + JsonSerializer.Serialize(Csv) +
                  ",\"entries_format\":\"csv\"}")
            .RespondJson(DictionaryReply);
      await CreateClient().ReplaceMultilingualGlossaryDictionaryFromCsvAsync("g1", "en", "de", CsvStream());
    }

    [Fact]
    public async Task SendsExtraBodyParametersWhenReplacingDictionaryWithOptions() {
      const string body =
            "{\"source_lang\":\"en\",\"target_lang\":\"de\",\"entries\":\"Apple\\tApfel\"," +
            "\"entries_format\":\"x\",\"extra\":\"1\"}";
      var csvBody = "{\"source_lang\":\"en\",\"target_lang\":\"de\",\"entries\":" + JsonSerializer.Serialize(Csv) +
                    ",\"entries_format\":\"x\",\"extra\":\"1\"}";
      for (var i = 0; i < 4; i++) {
        ExpectJson(HttpMethod.Put, "v3/glossaries/g1/dictionaries", body).RespondJson(DictionaryReply);
      }

      ExpectJson(HttpMethod.Put, "v3/glossaries/g1/dictionaries", csvBody).RespondJson(DictionaryReply);
      ExpectJson(HttpMethod.Put, "v3/glossaries/g1/dictionaries", csvBody).RespondJson(DictionaryReply);
      var options = new ReplaceMultilingualGlossaryDictionaryOptions {
        ExtraBodyParameters = new Dictionary<string, object?> { ["entries_format"] = "x", ["extra"] = "1" }
      };
      var dictionary = new MultilingualGlossaryDictionaryEntries("en", "de", Apple);
      var client = CreateClient();
      await client.ReplaceMultilingualGlossaryDictionaryAsync("g1", "en", "de", Apple, options);
      await client.ReplaceMultilingualGlossaryDictionaryAsync(Info(), "en", "de", Apple, options);
      await client.ReplaceMultilingualGlossaryDictionaryAsync("g1", dictionary, options);
      await client.ReplaceMultilingualGlossaryDictionaryAsync(Info(), dictionary, options);
      await client.ReplaceMultilingualGlossaryDictionaryFromCsvAsync("g1", "en", "de", CsvStream(), options);
      await client.ReplaceMultilingualGlossaryDictionaryFromCsvAsync(Info(), "en", "de", CsvStream(), options);
    }

    [Fact]
    public async Task SendsExtraBodyParametersWhenCreatingWithOptions() {
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v3/glossaries",
                  ("name", "n"),
                  ("dictionaries", Dictionaries(("en", "de", "Apple\tApfel", "tsv"))),
                  ("extra", "1"))
            .RespondJson(GlossaryReply);
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v3/glossaries",
                  ("name", "n"),
                  ("dictionaries", Dictionaries(("en", "de", Csv, "csv"))),
                  ("extra", "1"))
            .RespondJson(GlossaryReply);
      var options = new CreateMultilingualGlossaryOptions {
        ExtraBodyParameters = new Dictionary<string, object?> { ["extra"] = "1" }
      };
      var client = CreateClient();
      await client.CreateMultilingualGlossaryAsync(
            "n",
            new[] { new MultilingualGlossaryDictionaryEntries("en", "de", Apple) },
            options);
      await client.CreateMultilingualGlossaryFromCsvAsync("n", "en", "de", CsvStream(), options);
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWhenCreateReturns404() {
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v3/glossaries",
                  ("name", "n"),
                  ("dictionaries", Dictionaries(("en", "de", "Apple\tApfel", "tsv"))))
            .RespondJson("{}", HttpStatusCode.NotFound);
      ExpectJsonFields(
                  HttpMethod.Post,
                  "v3/glossaries",
                  ("name", "n"),
                  ("dictionaries", Dictionaries(("en", "de", Csv, "csv"))))
            .RespondJson("{}", HttpStatusCode.NotFound);
      var client = CreateClient();
      await Assert.ThrowsAsync<NotFoundException>(
            () => client.CreateMultilingualGlossaryAsync(
                  "n",
                  new[] { new MultilingualGlossaryDictionaryEntries("en", "de", Apple) }));
      await Assert.ThrowsAsync<NotFoundException>(
            () => client.CreateMultilingualGlossaryFromCsvAsync("n", "en", "de", CsvStream()));
    }

    [Fact]
    public async Task SendsOnlySetFieldsAsJsonWhenUpdating() {
      ExpectJson(
                  Patch,
                  "v3/glossaries/g1",
                  "{\"name\":\"New name\",\"dictionaries\":[{\"source_lang\":\"en\",\"target_lang\":\"de\"," +
                  "\"entries\":\"Apple\\tApfel\",\"entries_format\":\"tsv\"}],\"extra\":\"1\"}")
            .RespondJson(GlossaryReply);
      ExpectJson(Patch, "v3/glossaries/g1", "{\"name\":\"New name\"}").RespondJson(GlossaryReply);
      ExpectJson(Patch, "v3/glossaries/g1", "{}").RespondJson(GlossaryReply);
      var client = CreateClient();
      var updated = await client.UpdateMultilingualGlossaryAsync(
            "g1",
            new UpdateMultilingualGlossaryOptions {
              Name = "New name",
              Dictionaries = new[] { new MultilingualGlossaryDictionaryEntries("en", "de", Apple) },
              ExtraBodyParameters = new Dictionary<string, object?> { ["extra"] = "1" }
            });
      Assert.Equal("g1", updated.GlossaryId);
      await client.UpdateMultilingualGlossaryAsync(Info(), new UpdateMultilingualGlossaryOptions { Name = "New name" });
      await client.UpdateMultilingualGlossaryAsync("g1");
    }

    [Fact]
    public async Task AcceptsGlossaryObjectWhenGettingAndRenaming() {
      ExpectNoBody(HttpMethod.Get, "v3/glossaries/g1").RespondJson(GlossaryReply);
      ExpectJsonFields(
                  Patch,
                  "v3/glossaries/g1",
                  ("name", "New name")).RespondJson(GlossaryReply);
      var client = CreateClient();
      await client.GetMultilingualGlossaryAsync(Info());
      await client.UpdateMultilingualGlossaryNameAsync(Info(), "New name");
    }

    [Fact]
    public async Task EncodesGlossaryIdInPaths() {
      const string path = "v3/glossaries/a%2Fb%3Fc";
      ExpectNoBody(HttpMethod.Get, path).RespondJson(GlossaryReply);
      ExpectNoBody(HttpMethod.Get, path + "/entries")
            .RespondJson(
                  "{\"dictionaries\":[{\"source_lang\":\"en\",\"target_lang\":\"de\"," +
                  "\"entries\":\"Apple\\tApfel\",\"entries_format\":\"tsv\"}]}");
      ExpectNoBody(HttpMethod.Delete, path).Respond(HttpStatusCode.NoContent);
      ExpectNoBody(HttpMethod.Delete, path + "/dictionaries").Respond(HttpStatusCode.NoContent);
      Expect(HttpMethod.Put, path + "/dictionaries").RespondJson(DictionaryReply);
      Expect(Patch, path).RespondJson(GlossaryReply);
      Expect(Patch, path).RespondJson(GlossaryReply);
      Expect(Patch, path).RespondJson(GlossaryReply);
      var client = CreateClient();
      await client.GetMultilingualGlossaryAsync("a/b?c");
      await client.GetMultilingualGlossaryDictionaryEntriesAsync("a/b?c", "en", "de");
      await client.DeleteMultilingualGlossaryAsync("a/b?c");
      await client.DeleteMultilingualGlossaryDictionaryAsync("a/b?c", "en", "de");
      await client.ReplaceMultilingualGlossaryDictionaryAsync("a/b?c", "en", "de", Apple);
      await client.UpdateMultilingualGlossaryNameAsync("a/b?c", "n");
      await client.UpdateMultilingualGlossaryDictionaryAsync("a/b?c", "en", "de", Apple);
      await client.UpdateMultilingualGlossaryAsync("a/b?c", new UpdateMultilingualGlossaryOptions { Name = "n" });
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
    public async Task ParsesEntriesThatClientSideValidationWouldReject() {
      // A duplicate source term is rejected when sending entries, but must not fail reading what the server returns.
      ExpectNoBody(HttpMethod.Get, "v3/glossaries/g1/entries")
            .RespondJson(
                  "{\"dictionaries\":[{\"source_lang\":\"en\",\"target_lang\":\"de\"," +
                  "\"entries\":\"Apple\\tApfel\\nApple\\tApfel\",\"entries_format\":\"tsv\"}]}");
      var dictionary = await CreateClient().GetMultilingualGlossaryDictionaryEntriesAsync("g1", "en", "de");
      Assert.Equal("Apple\tApfel\nApple\tApfel", dictionary.Entries.ToTsv());
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWhenNoDictionaryIsReturned() {
      ExpectNoBody(HttpMethod.Get, "v3/glossaries/g1/entries").RespondJson("{\"dictionaries\":[]}");
      await Assert.ThrowsAsync<NotFoundException>(
            () => CreateClient().GetMultilingualGlossaryDictionaryEntriesAsync("g1", "en", "de"));
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWhenStatusIs404() {
      ExpectNoBody(HttpMethod.Get, "v3/glossaries/x").RespondJson("{}", HttpStatusCode.NotFound);
      await Assert.ThrowsAsync<NotFoundException>(() => CreateClient().GetMultilingualGlossaryAsync("x"));
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
