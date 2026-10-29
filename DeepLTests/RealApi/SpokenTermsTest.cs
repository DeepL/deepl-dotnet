// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Linq;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class SpokenTermsTest {
    [Fact]
    public async Task SendsTheSpokenTermsRequestsCreateGetListUpdateReadReplaceAndDelete() {
      var client = RealApiClient.CreateClient();
      var name = RealApiHelpers.GetUniqueName("spokenTerms");

      var created = await client.CreateSpokenTermsAsync(
            name,
            new[] { new SpokenTermsEntries("en", new[] { "DeepL", "webhook" }) });
      try {
        Assert.Equal(name, created.Name);
        var createdList = Assert.Single(created.TermLists);
        Assert.Equal(("en", 2L), (createdList.Lang, createdList.EntryCount));

        var fetched = await RealApiHelpers.RetryWhileNotFound(() => client.GetSpokenTermsAsync(created.SpokenTermsId));
        Assert.Equal(name, fetched.Name);
        Assert.Contains(await client.ListSpokenTermsAsync(), item => item.SpokenTermsId == created.SpokenTermsId);

        var updated = await client.UpdateSpokenTermsAsync(
              created,
              new UpdateSpokenTermsOptions { TermLists = new[] { new SpokenTermsEntries("en", new[] { "authentication" }) } });
        Assert.Equal(3, Assert.Single(updated.TermLists).EntryCount);
        var merged = await client.GetSpokenTermsEntriesAsync(created, "en");
        Assert.Equal(new[] { "DeepL", "authentication", "webhook" }.OrderBy(x => x), merged.Entries.OrderBy(x => x));

        var replaced = await client.ReplaceSpokenTermsListAsync(created, new SpokenTermsEntries("en", new[] { "API" }));
        Assert.Equal(1, replaced.EntryCount);
        Assert.Equal(new[] { "API" }, (await client.GetSpokenTermsEntriesAsync(created, "en")).Entries);

        await client.ReplaceSpokenTermsListAsync(created, new SpokenTermsEntries("de", new[] { "Schnittstelle" }));
        await client.DeleteSpokenTermsListAsync(created, "de");
        Assert.Equal("en", Assert.Single((await client.GetSpokenTermsAsync(created)).TermLists).Lang);
      } finally {
        try {
          await client.DeleteSpokenTermsAsync(created.SpokenTermsId);
        } catch (NotFoundException) {
          // Already gone: teardown only has to make sure it does not leak.
        }
      }

      await Assert.ThrowsAsync<NotFoundException>(() => client.GetSpokenTermsAsync(created.SpokenTermsId));
    }
  }
}
