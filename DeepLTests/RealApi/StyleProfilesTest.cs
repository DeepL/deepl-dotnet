// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Linq;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class StyleProfilesTest {
    [Fact]
    public async Task SendsTheStyleProfileRequestsCreateGetListUpdateAndDelete() {
      var client = RealApiClient.CreateClient();
      var styleRule = await client.CreateStyleRuleAsync(RealApiHelpers.GetUniqueName("styleProfileRule"), "de");
      StyleProfile? created = null;
      try {
        var name = RealApiHelpers.GetUniqueName("styleProfile");
        created = await client.CreateStyleProfileAsync(
              name,
              new CreateStyleProfileOptions {
                Description = "Contract wording", StyleRuleIds = new[] { styleRule.StyleId }
              });
        Assert.Equal(name, created.Name);
        Assert.Equal("Contract wording", created.Description);
        Assert.Equal(new[] { styleRule.StyleId }, created.StyleRuleIds);
        Assert.Empty(created.GlossaryIds);
        Assert.Empty(created.TranslationMemoryIds);

        var profileId = created.StyleProfileId;
        var fetched = await RealApiHelpers.RetryWhileNotFound(() => client.GetStyleProfileAsync(profileId));
        Assert.Equal(name, fetched.Name);
        Assert.Equal("Contract wording", fetched.Description);
        Assert.Equal(new[] { styleRule.StyleId }, fetched.StyleRuleIds);

        var listed = false;
        for (var page = 0; !listed && page < 20; page++) {
          var profiles = await client.ListStyleProfilesAsync(page, 100);
          if (profiles.Length == 0) {
            break;
          }

          listed = profiles.Any(profile => profile.StyleProfileId == profileId);
        }

        Assert.True(listed);

        var updated = await client.UpdateStyleProfileAsync(
              created,
              new UpdateStyleProfileOptions {
                Name = name + "-renamed", Industry = "Legal", Description = "", StyleRuleIds = new string[0]
              });
        Assert.Equal(name + "-renamed", updated.Name);
        Assert.Equal("Legal", updated.Industry);
        Assert.Null(updated.Description);
        Assert.Empty(updated.StyleRuleIds);

        await client.DeleteStyleProfileAsync(created);
        await Assert.ThrowsAsync<NotFoundException>(() => client.GetStyleProfileAsync(profileId));
      } finally {
        if (created != null) {
          try {
            await client.DeleteStyleProfileAsync(created.StyleProfileId);
          } catch (NotFoundException) {
            // Already deleted by the test: teardown only has to make sure it does not leak.
          }
        }

        try {
          await client.DeleteStyleRuleAsync(styleRule.StyleId);
        } catch (NotFoundException) {
          // Already gone.
        }
      }
    }
  }
}
