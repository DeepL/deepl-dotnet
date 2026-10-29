// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class SpokenTermsTest : OfflineTest {
    private const string SpokenTermsReply =
          "{\"spoken_terms_id\":\"st1\",\"name\":\"Terms\",\"creation_time\":\"2026-10-23T17:15:53.434769Z\"," +
          "\"term_lists\":[{\"lang\":\"en\",\"entry_count\":2}]}";

    [Fact]
    public async Task JoinsEntriesWithNewlinesWhenSendingAndSplitsThemWhenReading() {
      ExpectJson(
                  HttpMethod.Post,
                  "v3/spoken-terms",
                  "{\"name\":\"Terms\",\"term_lists\":[{\"lang\":\"en\",\"entries\":\"a\\nb\"}]}")
            .RespondJson(SpokenTermsReply);
      ExpectNoBody(HttpMethod.Get, "v3/spoken-terms/st1/entries")
            .WithExactQueryString("lang=en")
            .RespondJson("{\"lang\":\"en\",\"entries\":\"a\\nb\\n\"}");
      ExpectJson(HttpMethod.Put, "v3/spoken-terms/st1/term-lists", "{\"lang\":\"en\",\"entries\":\"webhook\\nDeepL\"}")
            .RespondJson("{\"lang\":\"en\",\"entry_count\":2}");
      var client = CreateClient();

      var spokenTerms = await client.CreateSpokenTermsAsync("Terms", new[] { new SpokenTermsEntries("en", new[] { "a", "b" }) });
      var entries = await client.GetSpokenTermsEntriesAsync(spokenTerms, "en");

      Assert.Equal("en", entries.Lang);
      Assert.Equal(new[] { "a", "b" }, entries.Entries);
      await client.ReplaceSpokenTermsListAsync(spokenTerms, new SpokenTermsEntries("en", new[] { "webhook\n", " DeepL" }));
    }

    [Fact]
    public async Task UpdateSendsOnlyTheOptionsSet() {
      ExpectJson(Patch, "v3/spoken-terms/st1", "{\"name\":\"Renamed\"}").RespondJson(SpokenTermsReply);
      ExpectJson(Patch, "v3/spoken-terms/st1", "{\"term_lists\":[{\"lang\":\"en\",\"entries\":\"a\"}]}")
            .RespondJson(SpokenTermsReply);
      var client = CreateClient();

      var spokenTerms = await client.UpdateSpokenTermsAsync("st1", new UpdateSpokenTermsOptions { Name = "Renamed" });
      await client.UpdateSpokenTermsAsync(
            spokenTerms,
            new UpdateSpokenTermsOptions { TermLists = new[] { new SpokenTermsEntries("en", new[] { "a" }) } });
    }

    [Fact]
    public async Task SendsExtraBodyParametersLastInCreateUpdateAndReplace() {
      ExpectJson(
                  HttpMethod.Post,
                  "v3/spoken-terms",
                  "{\"name\":\"Override\",\"term_lists\":[{\"lang\":\"en\",\"entries\":\"a\"}],\"extra\":\"1\"}")
            .RespondJson(SpokenTermsReply);
      ExpectJson(Patch, "v3/spoken-terms/st1", "{\"name\":\"Override\",\"extra\":\"2\"}")
            .RespondJson(SpokenTermsReply);
      ExpectJson(HttpMethod.Put, "v3/spoken-terms/st1/term-lists", "{\"lang\":\"de\",\"entries\":\"a\",\"extra\":\"3\"}")
            .RespondJson("{\"lang\":\"de\",\"entry_count\":1}");
      var client = CreateClient();
      var en = new SpokenTermsEntries("en", new[] { "a" });

      var spokenTerms = await client.CreateSpokenTermsAsync(
            "Terms",
            new[] { en },
            new CreateSpokenTermsOptions {
              ExtraBodyParameters = new Dictionary<string, string> { ["name"] = "Override", ["extra"] = "1" }
            });
      await client.UpdateSpokenTermsAsync(
            spokenTerms,
            new UpdateSpokenTermsOptions {
              Name = "Renamed",
              ExtraBodyParameters = new Dictionary<string, string> { ["name"] = "Override", ["extra"] = "2" }
            });
      await client.ReplaceSpokenTermsListAsync(
            "st1",
            en,
            new ReplaceSpokenTermsListOptions {
              ExtraBodyParameters = new Dictionary<string, string> { ["lang"] = "de", ["extra"] = "3" }
            });
    }

    [Fact]
    public async Task RejectsTermContainingNewlineWithoutSendingARequest() {
      var client = CreateClient();
      var invalid = new[] { new SpokenTermsEntries("en", new[] { "DeepL", "web\nhook" }) };

      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateSpokenTermsAsync("Terms", invalid));
      await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateSpokenTermsAsync("st1", new UpdateSpokenTermsOptions { TermLists = invalid }));
      await Assert.ThrowsAsync<ArgumentException>(() => client.ReplaceSpokenTermsListAsync("st1", invalid[0]));
    }

    [Fact]
    public async Task ParsesMicrosecondCreationTimeAndMissingTermLists() {
      ExpectNoBody(HttpMethod.Get, "v3/spoken-terms")
            .RespondJson(
                  "{\"spoken_terms\":[" + SpokenTermsReply + "," +
                  "{\"spoken_terms_id\":\"st2\",\"name\":\"Empty\",\"creation_time\":\"2026-10-23T17:15:53.434769Z\"}]}");

      var listed = await CreateClient().ListSpokenTermsAsync();

      Assert.Equal(2, listed.Length);
      Assert.Equal(
            new DateTime(2026, 10, 23, 17, 15, 53, DateTimeKind.Utc).AddTicks(4347690),
            listed[0].CreationTime.ToUniversalTime());
      var termList = Assert.Single(listed[0].TermLists);
      Assert.Equal("en", termList.Lang);
      Assert.Equal(2, termList.EntryCount);
      Assert.Empty(listed[1].TermLists);
    }
  }
}
