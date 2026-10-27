// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Linq;
using System.Threading.Tasks;
using DeepL;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class AdminClientTest {
    private static async Task AssertKeyIsListedAsync(DeepLAdminClient client, string keyId) {
      const int attempts = 5;
      for (var attempt = 1; attempt <= attempts; attempt++) {
        var listed = await client.GetDeveloperKeysAsync();
        if (listed.Any(listedKey => listedKey.KeyId == keyId)) {
          return;
        }

        if (attempt < attempts) {
          await Task.Delay(TimeSpan.FromSeconds(1));
        }
      }

      Assert.True(false, $"Developer key {keyId} was not listed after {attempts} attempts");
    }

    private static readonly DateTime Today = DateTime.UtcNow.Date;
    private static readonly DateTime WeekAgo = Today.AddDays(-7);

    [Fact]
    public async Task SendsGetV2AdminAnalyticsAndParsesTheReport() {
      using var client = RealApiClient.CreateAdminClient();

      var report = await client.GetUsageAnalyticsAsync(WeekAgo, Today, AdminUsageGroupBy.KeyAndDay);

      Assert.NotNull(report.TotalUsage);
      Assert.Equal("keyAndDay", report.GroupBy);
      Assert.Equal((DateTime?)WeekAgo, report.StartDate);
      Assert.NotNull(report.EndDate);
    }

    [Fact]
    public async Task SendsGetV2AdminAnalyticsCustomTagsAndParsesTheReport() {
      using var client = RealApiClient.CreateAdminClient();

      var report = await client.GetCustomTagUsageAnalyticsAsync(WeekAgo, Today, CustomTagUsageAggregateBy.Day);

      Assert.Equal("day", report.AggregateBy);
      Assert.NotNull(report.StartDate);
      Assert.NotNull(report.EndDate);
    }

    [Fact]
    public async Task SendsPostGetAndPutV2AdminDeveloperKeys() {
      using var client = RealApiClient.CreateAdminClient();

      var key = await client.CreateDeveloperKeyAsync(RealApiHelpers.GetUniqueName("developer-key"));
      var failed = false;
      var deactivated = false;
      try {
        Assert.False(key.IsDeactivated);

        await AssertKeyIsListedAsync(client, key.KeyId);

        var renamedLabel = RealApiHelpers.GetUniqueName("developer-key-renamed");
        Assert.Equal(renamedLabel, (await client.RenameDeveloperKeyAsync(key, renamedLabel)).Label);

        var limited = await client.SetDeveloperKeyUsageLimitsAsync(
              key,
              new DeveloperKeyUsageLimitsUpdate { Characters = 5000, SpeechToTextMilliseconds = null });
        Assert.Equal(5000, limited.UsageLimits.Characters);
        Assert.Null(limited.UsageLimits.SpeechToTextMilliseconds);

        Assert.True((await client.DeactivateDeveloperKeyAsync(key)).IsDeactivated);
        deactivated = true;
      } catch (Exception) {
        failed = true;
        throw;
      } finally {
        if (!deactivated) {
          try {
            await client.DeactivateDeveloperKeyAsync(key);
          } catch (NotFoundException) {
          } catch (Exception) when (failed) {
          }
        }
      }
    }
  }
}
