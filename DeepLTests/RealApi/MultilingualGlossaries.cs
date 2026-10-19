// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Linq;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class MultilingualGlossaries {
    [Fact]
    public async Task SendsTheV3GlossaryRequestsCreateGetListEntriesRenameReplaceAndDelete() {
      var client = RealApiClient.CreateClient();
      var name = RealApiHelpers.GetUniqueName("glossary-v3");
      var enDe = new GlossaryEntries(new[] { ("Apple", "Apfel") });
      var enFr = new GlossaryEntries(new[] { ("Apple", "Pomme") });

      var glossary = await client.CreateMultilingualGlossaryAsync(
            name,
            new[] {
              new MultilingualGlossaryDictionaryEntries("en", "de", enDe),
              new MultilingualGlossaryDictionaryEntries("en", "fr", enFr)
            });
      try {
        Assert.NotEmpty(glossary.GlossaryId);
        Assert.Equal(name, glossary.Name);
        Assert.Equal(
              new[] { "en-de", "en-fr" },
              glossary.Dictionaries.Select(d => $"{d.SourceLanguageCode}-{d.TargetLanguageCode}").OrderBy(pair => pair));

        var fetched = await RealApiHelpers.RetryWhileNotFound(
              () => client.GetMultilingualGlossaryAsync(glossary.GlossaryId));
        Assert.Equal(glossary.GlossaryId, fetched.GlossaryId);
        Assert.Equal(name, fetched.Name);

        var listed = await client.ListMultilingualGlossariesAsync();
        Assert.Contains(listed, info => info.GlossaryId == glossary.GlossaryId);

        var fetchedEntries = await client.GetMultilingualGlossaryDictionaryEntriesAsync(glossary, "en", "de");
        Assert.Equal(enDe.ToDictionary(), fetchedEntries.Entries.ToDictionary());

        var renamed = await client.UpdateMultilingualGlossaryNameAsync(glossary.GlossaryId, name + "-renamed");
        Assert.Equal(name + "-renamed", renamed.Name);

        var replaced = await client.ReplaceMultilingualGlossaryDictionaryAsync(
              glossary,
              "en",
              "de",
              new GlossaryEntries(new[] { ("Cherry", "Kirsche") }));
        Assert.Equal("en", replaced.SourceLanguageCode);
        Assert.Equal("de", replaced.TargetLanguageCode);
        Assert.Equal(1, replaced.EntryCount);

        await client.DeleteMultilingualGlossaryDictionaryAsync(glossary, "en", "fr");
      } finally {
        await client.DeleteMultilingualGlossaryAsync(glossary.GlossaryId);
      }
    }
  }
}
