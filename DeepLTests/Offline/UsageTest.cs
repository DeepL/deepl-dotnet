// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Net.Http;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class UsageTest : OfflineTest {
    [Fact]
    public async Task ParsesCharacterAndDocumentCounts() {
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .RespondJson(
                  "{\"character_count\":20,\"character_limit\":20,\"document_count\":0,\"document_limit\":1}");
      var usage = await CreateClient().GetUsageAsync();
      Assert.Equal(20, usage.Character!.Count);
      Assert.Equal(20, usage.Character.Limit);
      Assert.True(usage.Character.LimitReached);
      Assert.False(usage.Document!.LimitReached);
      Assert.Null(usage.TeamDocument);
      Assert.True(usage.AnyLimitReached);
      Assert.Contains("Characters: 20 of 20", usage.ToString());
      Assert.Contains("Documents: 0 of 1", usage.ToString());
      Assert.DoesNotContain("Team documents", usage.ToString());
    }

    [Fact]
    public async Task ParsesTeamDocumentLimitOnly() {
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .RespondJson("{\"team_document_count\":1,\"team_document_limit\":1}");
      var usage = await CreateClient().GetUsageAsync();
      Assert.Null(usage.Character);
      Assert.Null(usage.Document);
      Assert.True(usage.TeamDocument!.LimitReached);
      Assert.True(usage.AnyLimitReached);
      Assert.Contains("Team documents: 1 of 1", usage.ToString());
      Assert.DoesNotContain("Characters", usage.ToString());
    }

    [Fact]
    public async Task ReportsNoLimitReachedBelowLimits() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson("{\"character_count\":1,\"character_limit\":2}");
      Assert.False((await CreateClient().GetUsageAsync()).AnyLimitReached);
    }
  }
}
