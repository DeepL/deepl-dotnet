// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class Glossaries : OfflineTest {
    private const string GlossaryReply =
          "{\"glossary_id\":\"g1\",\"name\":\"My glossary\",\"ready\":true,\"source_lang\":\"en\",\"target_lang\":\"de\"," +
          "\"creation_time\":\"2021-08-03T14:16:18.329Z\",\"entry_count\":2}";

    private static readonly GlossaryEntries Entries = new GlossaryEntries(new[] { ("Apple", "Apfel"), ("Banana", "Banane") });

    [Fact]
    public async Task SendsTsvEntriesWhenCreating() {
      ExpectForm(
                  HttpMethod.Post,
                  "v2/glossaries",
                  ("name", "My glossary"),
                  ("source_lang", "en"),
                  ("target_lang", "de"),
                  ("entries_format", "tsv"),
                  ("entries", "Apple\tApfel\nBanana\tBanane"))
            .RespondJson(GlossaryReply);
      await CreateClient().CreateGlossaryAsync("My glossary", "en", "de", Entries);
    }

    [Fact]
    public async Task SendsLanguageCodesWithoutRegionWhenCreating() {
      ExpectForm(
                  HttpMethod.Post,
                  "v2/glossaries",
                  ("name", "n"),
                  ("source_lang", "en"),
                  ("target_lang", "pt"),
                  ("entries_format", "tsv"),
                  ("entries", "Apple\tApfel\nBanana\tBanane"))
            .RespondJson(GlossaryReply);
      await CreateClient().CreateGlossaryAsync("n", "EN-US", "pt-BR", Entries);
    }

    [Fact]
    public async Task SendsCsvEntriesWhenCreatingFromCsv() {
      const string csv = "sourceEntry1,targetEntry1,en,de\n\"source\"\"Entry\",\"target,Entry\",en,de";
      ExpectForm(
                  HttpMethod.Post,
                  "v2/glossaries",
                  ("name", "n"),
                  ("source_lang", "en"),
                  ("target_lang", "de"),
                  ("entries_format", "csv"),
                  ("entries", csv))
            .RespondJson(GlossaryReply);
      await CreateClient().CreateGlossaryFromCsvAsync("n", "en", "de", new MemoryStream(Encoding.UTF8.GetBytes(csv)));
    }

    [Fact]
    public async Task SendsLargeGlossaryInOneRequest() {
      var pairs = new Dictionary<string, string>();
      for (var i = 0; i < 10000; i++) {
        pairs.Add($"Source-{i}", $"Target-{i}");
      }

      var entries = new GlossaryEntries(pairs);
      Assert.True(entries.ToTsv().Length > 100000);
      ExpectForm(
                  HttpMethod.Post,
                  "v2/glossaries",
                  ("name", "n"),
                  ("source_lang", "en"),
                  ("target_lang", "de"),
                  ("entries_format", "tsv"),
                  ("entries", entries.ToTsv()))
            .RespondJson(GlossaryReply);
      await CreateClient().CreateGlossaryAsync("n", "en", "de", entries);
    }

    [Fact]
    public async Task ParsesGlossaryInfo() {
      ExpectNoBody(HttpMethod.Get, "v2/glossaries/g1").RespondJson(GlossaryReply);
      var glossary = await CreateClient().GetGlossaryAsync("g1");
      Assert.Equal("g1", glossary.GlossaryId);
      Assert.Equal("en", glossary.SourceLanguageCode);
      Assert.Equal("de", glossary.TargetLanguageCode);
      Assert.Equal(new DateTime(2021, 8, 3, 14, 16, 18, 329, DateTimeKind.Utc), glossary.CreationTime.ToUniversalTime());
      Assert.Equal(2, glossary.EntryCount);
    }

    [Fact]
    public async Task ParsesGlossaryList() {
      ExpectNoBody(HttpMethod.Get, "v2/glossaries").RespondJson("{\"glossaries\":[" + GlossaryReply + "]}");
      var glossaries = await CreateClient().ListGlossariesAsync();
      Assert.Equal("g1", Assert.Single(glossaries).GlossaryId);
    }

    [Fact]
    public async Task SendsDeleteToGlossaryPath() {
      ExpectNoBody(HttpMethod.Delete, "v2/glossaries/g1").Respond(HttpStatusCode.NoContent);
      await CreateClient().DeleteGlossaryAsync("g1");
    }

    [Fact]
    public async Task ParsesTsvEntries() {
      ExpectNoBody(HttpMethod.Get, "v2/glossaries/g1/entries")
            .WithHeaders("Accept", "text/tab-separated-values")
            .Respond("text/tab-separated-values", "Apple\tApfel\nBanana\tBanane");
      var entries = await CreateClient().GetGlossaryEntriesAsync("g1");
      Assert.Equal(Entries.ToDictionary(), entries.ToDictionary());
    }

    [Fact]
    public async Task PollsUntilGlossaryIsReady() {
      ExpectNoBody(HttpMethod.Get, "v2/glossaries/g1").RespondJson(GlossaryReply.Replace("\"ready\":true", "\"ready\":false"));
      ExpectNoBody(HttpMethod.Get, "v2/glossaries/g1").RespondJson(GlossaryReply);
      await CreateClient().WaitUntilGlossaryReadyAsync("g1");
    }

    [Fact]
    public async Task ThrowsGlossaryNotFoundExceptionWhenStatusIs404() {
      var client = CreateClient();
      ExpectNoBody(HttpMethod.Get, "v2/glossaries/x").RespondJson("{\"message\":\"gone\"}", HttpStatusCode.NotFound);
      ExpectNoBody(HttpMethod.Delete, "v2/glossaries/x").RespondJson("{}", HttpStatusCode.NotFound);
      ExpectNoBody(HttpMethod.Get, "v2/glossaries/x/entries").RespondJson("{}", HttpStatusCode.NotFound);
      var exception = await Assert.ThrowsAsync<GlossaryNotFoundException>(() => client.GetGlossaryAsync("x"));
      Assert.Equal("Glossary not found, message: gone", exception.Message);
      await Assert.ThrowsAsync<GlossaryNotFoundException>(() => client.DeleteGlossaryAsync("x"));
      await Assert.ThrowsAsync<GlossaryNotFoundException>(() => client.GetGlossaryEntriesAsync("x"));
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenNameIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().CreateGlossaryAsync("", "en", "de", Entries));

    [Fact]
    public void ThrowsArgumentExceptionWhenEntriesAreInvalid() {
      Assert.Throws<ArgumentException>(() => GlossaryEntries.FromTsv(""));
      Assert.Throws<ArgumentException>(() => GlossaryEntries.FromTsv("Küche\tKitchen\nKüche\tCuisine"));
      Assert.Throws<ArgumentException>(() => GlossaryEntries.FromTsv("A\tB\tC"));
      Assert.Throws<ArgumentException>(() => new GlossaryEntries(new Dictionary<string, string>()));
      Assert.Throws<ArgumentException>(() => new GlossaryEntries(new[] { ("Küche", "Kitchen"), ("Küche", "Cuisine") }));
      Assert.Throws<ArgumentException>(() => new GlossaryEntries(new Dictionary<string, string> { { "A", "B\tC" } }));
    }
  }
}
