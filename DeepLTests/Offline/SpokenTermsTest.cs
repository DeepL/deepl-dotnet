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
              ExtraBodyParameters = new Dictionary<string, object?> { ["name"] = "Override", ["extra"] = "1" }
            });
      await client.UpdateSpokenTermsAsync(
            spokenTerms,
            new UpdateSpokenTermsOptions {
              Name = "Renamed",
              ExtraBodyParameters = new Dictionary<string, object?> { ["name"] = "Override", ["extra"] = "2" }
            });
      await client.ReplaceSpokenTermsListAsync(
            "st1",
            en,
            new ReplaceSpokenTermsListOptions {
              ExtraBodyParameters = new Dictionary<string, object?> { ["lang"] = "de", ["extra"] = "3" }
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
    public async Task RejectsANullSpokenTermsObjectWithoutSendingARequest() {
      var client = CreateClient();
      var spokenTerms = (SpokenTerms)null!;
      var calls = new Func<Task>[] {
        () => client.GetSpokenTermsAsync(spokenTerms),
        () => client.UpdateSpokenTermsAsync(spokenTerms, new UpdateSpokenTermsOptions { Name = "n" }),
        () => client.GetSpokenTermsEntriesAsync(spokenTerms, "en"),
        () => client.ReplaceSpokenTermsListAsync(spokenTerms, new SpokenTermsEntries("en", new[] { "DeepL" })),
        () => client.DeleteSpokenTermsAsync(spokenTerms),
        () => client.DeleteSpokenTermsListAsync(spokenTerms, "en"),
      };
      foreach (var call in calls) {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(call);
        Assert.Equal("spokenTerms", exception.ParamName);
      }
    }

    [Fact]
    public async Task ThrowsArgumentNullExceptionNamingTheNullArgument() {
      var client = CreateClient();
      var entries = new SpokenTermsEntries("en", new[] { "DeepL" });
      var calls = new (string Param, Func<Task> Call)[] {
        ("name", () => client.CreateSpokenTermsAsync(null!, new[] { entries })),
        ("termLists", () => client.CreateSpokenTermsAsync("n", null!)),
        ("termLists", () => client.CreateSpokenTermsAsync("n", new SpokenTermsEntries[] { null! })),
        ("TermLists", () => client.UpdateSpokenTermsAsync(
              "st1",
              new UpdateSpokenTermsOptions { TermLists = new SpokenTermsEntries[] { null! } })),
        ("entries", () => client.ReplaceSpokenTermsListAsync("st1", null!)),
        ("entries", () => Task.FromResult(new SpokenTermsEntries("en", null!))),
        ("entries", () => Task.FromResult(new SpokenTermsEntries("en", new[] { "a", null! }))),
        ("spokenTermsId", () => client.GetSpokenTermsAsync((string)null!)),
        ("spokenTermsId", () => client.UpdateSpokenTermsAsync((string)null!, new UpdateSpokenTermsOptions { Name = "n" })),
        ("spokenTermsId", () => client.GetSpokenTermsEntriesAsync((string)null!, "en")),
        ("spokenTermsId", () => client.ReplaceSpokenTermsListAsync((string)null!, entries)),
        ("spokenTermsId", () => client.DeleteSpokenTermsAsync((string)null!)),
        ("spokenTermsId", () => client.DeleteSpokenTermsListAsync((string)null!, "en")),
      };
      foreach (var (param, call) in calls) {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(call);
        Assert.Equal(param, exception.ParamName);
      }
    }

    [Fact]
    public async Task RejectsEmptyOrNullLanguageWithoutSendingARequest() {
      var client = CreateClient();
      foreach (var lang in new string?[] { "", " ", null }) {
        var invalid = new SpokenTermsEntries(lang!, new[] { "DeepL" });
        await Assert.ThrowsAnyAsync<ArgumentException>(() => client.CreateSpokenTermsAsync("Terms", new[] { invalid }));
        await Assert.ThrowsAnyAsync<ArgumentException>(
              () => client.UpdateSpokenTermsAsync("st1", new UpdateSpokenTermsOptions { TermLists = new[] { invalid } }));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => client.ReplaceSpokenTermsListAsync("st1", invalid));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => client.GetSpokenTermsEntriesAsync("st1", lang!));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => client.DeleteSpokenTermsListAsync("st1", lang!));
      }
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
