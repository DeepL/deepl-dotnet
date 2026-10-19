// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Linq;
using System.Threading.Tasks;
using DeepL;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class Glossaries {
    [Fact]
    public async Task SendsTheV2GlossaryRequestsCreateGetListEntriesAndDelete() {
      var client = RealApiClient.CreateClient();
      var name = RealApiHelpers.GetUniqueName("glossary");
      var entries = new GlossaryEntries(new[] { ("Apple", "Apfel"), ("Banana", "Banane") });

      var glossary = await client.CreateGlossaryAsync(name, "en", "de", entries);
      try {
        Assert.NotEmpty(glossary.GlossaryId);
        Assert.Equal(name, glossary.Name);
        Assert.Equal("en", glossary.SourceLanguageCode);
        Assert.Equal("de", glossary.TargetLanguageCode);
        Assert.Equal(2, glossary.EntryCount);

        var fetched = await RealApiHelpers.RetryWhileNotFound(() => client.GetGlossaryAsync(glossary.GlossaryId));
        Assert.Equal(glossary.GlossaryId, fetched.GlossaryId);
        Assert.Equal(name, fetched.Name);
        Assert.Equal(glossary.CreationTime, fetched.CreationTime);

        var listed = await client.ListGlossariesAsync();
        Assert.Contains(listed, info => info.GlossaryId == glossary.GlossaryId);

        var fetchedEntries = await client.GetGlossaryEntriesAsync(glossary.GlossaryId);
        Assert.Equal(entries.ToDictionary(), fetchedEntries.ToDictionary());
      } finally {
        await client.DeleteGlossaryAsync(glossary.GlossaryId);
      }
    }
  }
}
