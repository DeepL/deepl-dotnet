// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class AdminClientTest : OfflineTest {
    private const string KeyId = "ca7d5694-96eb-4263-a9a4-7f7e4211529e:20c2abcf-4c3c-4cd6-8ae8-8bd2a7d4da38";

    private const string DeveloperKeyJson = @"{
      ""key_id"": """ + KeyId + @""",
      ""label"": ""developer key prod"",
      ""creation_time"": ""2025-07-08T08:15:29.362Z"",
      ""deactivated_time"": null,
      ""is_deactivated"": false,
      ""usage_limits"": {
        ""characters"": 5000, ""speech_to_text_milliseconds"": null, ""speech_to_speech_milliseconds"": 60000}
    }";

    private static readonly DeveloperKey Key = JsonSerializer.Deserialize<DeveloperKey>(DeveloperKeyJson)!;

    [Fact]
    public void TestConstructorRequiresAdminKey() {
      Assert.Throws<ArgumentNullException>(() => new DeepLAdminClient(null!));
      Assert.Throws<ArgumentException>(() => new DeepLAdminClient(" "));
    }

    [Fact]
    public async Task TestGetUsageAnalytics() {
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics?start_date=2025-09-29&end_date=2025-10-01&group_by=key_and_day")
            .RespondJson(
                  @"{""usage_report"": {
                    ""total_usage"": {
                      ""total_characters"": 9619, ""text_translation_characters"": 4892,
                      ""document_translation_characters"": 0, ""text_improvement_characters"": 4727,
                      ""speech_to_text_minutes"": 107.46},
                    ""group_by"": ""keyAndDay"",
                    ""start_date"": ""2025-09-29T00:00:00"",
                    ""end_date"": ""2025-10-01T00:00:00"",
                    ""key_and_day_usages"": [{
                      ""api_key"": ""dc88****3a2c"", ""api_key_label"": ""Staging API Key"",
                      ""usage_date"": ""2025-09-29T00:00:00Z"",
                      ""usage"": {
                        ""total_characters"": 315, ""text_translation_characters"": 159,
                        ""document_translation_characters"": 0, ""text_improvement_characters"": 156,
                        ""speech_to_text_minutes"": 11.94}}]}}");

      var report = await CreateAdminClient().GetUsageAnalyticsAsync(
            new DateTime(2025, 9, 29, 23, 59, 0),
            new DateTime(2025, 10, 1),
            AdminUsageGroupBy.KeyAndDay);

      Assert.Equal(9619, report.TotalUsage!.TotalCharacters);
      Assert.Equal(107.46, report.TotalUsage.SpeechToTextMinutes);
      Assert.Equal("keyAndDay", report.GroupBy);
      Assert.Equal(new DateTime(2025, 9, 29), report.StartDate);
      Assert.Equal(DateTimeKind.Utc, report.StartDate!.Value.Kind);
      Assert.Empty(report.KeyUsages);
      var usage = Assert.Single(report.KeyAndDayUsages);
      Assert.Equal("Staging API Key", usage.ApiKeyLabel);
      Assert.Equal(new DateTime(2025, 9, 29, 0, 0, 0, DateTimeKind.Utc), usage.UsageDate!.Value.ToUniversalTime());
      Assert.Equal(156, usage.Usage.TextImprovementCharacters);
    }

    [Fact]
    public async Task TestGetCustomTagUsageAnalytics() {
      ExpectNoBody(
                  HttpMethod.Get,
                  "v2/admin/analytics/custom-tags?start_date=2026-05-03&end_date=2026-05-15&aggregate_by=day&page=2")
            .RespondJson(
                  @"{""custom_tag_usage_report"": {
                    ""aggregate_by"": ""day"",
                    ""start_date"": ""2026-05-03T00:00:00"",
                    ""end_date"": ""2026-05-15T00:00:00"",
                    ""next_page"": 3,
                    ""usage"": [{
                      ""custom_tag"": ""example-custom-tag"",
                      ""usage_date"": ""2026-05-04T00:00:00Z"",
                      ""breakdown"": {
                        ""total_characters"": 380, ""text_translation_characters"": 380,
                        ""text_improvement_characters"": 0, ""speech_to_text_minutes"": 12.5,
                        ""speech_to_speech_minutes"": 4.2}}]}}");

      var report = await CreateAdminClient().GetCustomTagUsageAnalyticsAsync(
            new DateTime(2026, 5, 3, 12, 30, 0),
            new DateTime(2026, 5, 15),
            CustomTagUsageAggregateBy.Day,
            2);

      Assert.Equal("day", report.AggregateBy);
      Assert.Equal(3, report.NextPage);
      Assert.Equal(DateTimeKind.Utc, report.EndDate!.Value.Kind);
      var usage = Assert.Single(report.Usage);
      Assert.Equal("example-custom-tag", usage.CustomTag);
      Assert.Equal(4.2, usage.Breakdown.SpeechToSpeechMinutes);
      Assert.NotNull(usage.UsageDate);
    }

    [Fact]
    public async Task TestCreateDeveloperKey() {
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", @"{""label"":""developer key prod""}")
            .RespondJson(DeveloperKeyJson);

      var key = await CreateAdminClient().CreateDeveloperKeyAsync(new CreateDeveloperKeyOptions { Label = "developer key prod" });

      Assert.Equal(KeyId, key.KeyId);
      Assert.Equal("developer key prod", key.Label);
      Assert.Equal(2025, key.CreationTime!.Value.Year);
      Assert.Null(key.DeactivatedTime);
      Assert.False(key.IsDeactivated);
      Assert.Equal(5000, key.UsageLimits!.Characters);
      Assert.Null(key.UsageLimits!.SpeechToTextMilliseconds);
      Assert.Equal(60000, key.UsageLimits!.SpeechToSpeechMilliseconds);
    }

    [Fact]
    public async Task TestGetDeveloperKeys() {
      var deactivated = DeveloperKeyJson
            .Replace(@"""deactivated_time"": null", @"""deactivated_time"": ""2025-07-09T08:15:29.362Z""")
            .Replace(@"""is_deactivated"": false", @"""is_deactivated"": true");
      ExpectNoBody(HttpMethod.Get, "v2/admin/developer-keys").RespondJson($"[{DeveloperKeyJson}, {deactivated}]");

      var keys = await CreateAdminClient().ListDeveloperKeysAsync();

      Assert.Equal(new[] { false, true }, Array.ConvertAll(keys, key => key.IsDeactivated));
      Assert.NotNull(keys[1].DeactivatedTime);
    }

    [Fact]
    public async Task TestGetDeveloperKeysWithoutCreationTime() {
      var withoutCreationTime = DeveloperKeyJson.Replace(@"""2025-07-08T08:15:29.362Z""", "null");
      ExpectNoBody(HttpMethod.Get, "v2/admin/developer-keys").RespondJson($"[{withoutCreationTime}]");

      var keys = await CreateAdminClient().ListDeveloperKeysAsync();

      Assert.Null(Assert.Single(keys).CreationTime);
    }

    [Fact]
    public async Task TestModifyDeveloperKey() {
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/deactivate", $@"{{""key_id"":""{KeyId}""}}")
            .RespondJson(DeveloperKeyJson);
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/label", $@"{{""key_id"":""{KeyId}"",""label"":""renamed""}}")
            .RespondJson(DeveloperKeyJson);
      var client = CreateAdminClient();

      await client.DeactivateDeveloperKeyAsync(Key);
      await client.RenameDeveloperKeyAsync(KeyId, "renamed");
    }

    [Fact]
    public async Task TestDeveloperKeyRequestsSendExtraBodyParametersLast() {
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", @"{""label"":""override"",""extra"":""1""}")
            .RespondJson(DeveloperKeyJson);
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", @"{""extra"":""1""}").RespondJson(DeveloperKeyJson);
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/deactivate", $@"{{""key_id"":""{KeyId}"",""extra"":""1""}}")
            .RespondJson(DeveloperKeyJson);
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/deactivate", $@"{{""key_id"":""other""}}")
            .RespondJson(DeveloperKeyJson);
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/label", $@"{{""key_id"":""{KeyId}"",""label"":""x"",""extra"":""1""}}")
            .RespondJson(DeveloperKeyJson);
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/label", $@"{{""key_id"":""{KeyId}"",""label"":""override""}}")
            .RespondJson(DeveloperKeyJson);
      ExpectJson(
                  HttpMethod.Put,
                  "v2/admin/developer-keys/limits",
                  $@"{{""key_id"":""{KeyId}"",""characters"":1,""extra"":""1""}}")
            .RespondJson(DeveloperKeyJson);
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/limits", $@"{{""key_id"":""{KeyId}"",""characters"":""2""}}")
            .RespondJson(DeveloperKeyJson);
      var client = CreateAdminClient();
      var extra = new Dictionary<string, string> { ["extra"] = "1" };
      var limits = new DeveloperKeyUsageLimitsUpdate { Characters = 1 };

      await client.CreateDeveloperKeyAsync(
            new CreateDeveloperKeyOptions {
              Label = "label",
              ExtraBodyParameters = new Dictionary<string, string> { ["label"] = "override", ["extra"] = "1" }
            });
      await client.CreateDeveloperKeyAsync(new CreateDeveloperKeyOptions { ExtraBodyParameters = extra });
      await client.DeactivateDeveloperKeyAsync(KeyId, new DeactivateDeveloperKeyOptions { ExtraBodyParameters = extra });
      await client.DeactivateDeveloperKeyAsync(
            Key,
            new DeactivateDeveloperKeyOptions {
              ExtraBodyParameters = new Dictionary<string, string> { ["key_id"] = "other" }
            });
      await client.RenameDeveloperKeyAsync(Key, "x", new RenameDeveloperKeyOptions { ExtraBodyParameters = extra });
      await client.RenameDeveloperKeyAsync(
            KeyId,
            "x",
            new RenameDeveloperKeyOptions {
              ExtraBodyParameters = new Dictionary<string, string> { ["label"] = "override" }
            });
      await client.SetDeveloperKeyUsageLimitsAsync(
            KeyId,
            limits,
            new SetDeveloperKeyUsageLimitsOptions { ExtraBodyParameters = extra });
      await client.SetDeveloperKeyUsageLimitsAsync(
            Key,
            limits,
            new SetDeveloperKeyUsageLimitsOptions {
              ExtraBodyParameters = new Dictionary<string, string> { ["characters"] = "2" }
            });
    }

    [Fact]
    public async Task TestRejectsEmptyKeyIds() {
      var client = CreateAdminClient();
      var limits = new DeveloperKeyUsageLimitsUpdate { Characters = 0 };

      await Assert.ThrowsAsync<ArgumentException>(() => client.RenameDeveloperKeyAsync("", "renamed"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.DeactivateDeveloperKeyAsync(" "));
      await Assert.ThrowsAsync<ArgumentException>(() => client.SetDeveloperKeyUsageLimitsAsync("", limits));
      await Assert.ThrowsAsync<ArgumentNullException>(() => client.DeactivateDeveloperKeyAsync((DeveloperKey)null!));
    }

    [Fact]
    public async Task TestDateTimeArgumentsAreSentAsUtcDate() {
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics?start_date=2026-10-19&end_date=2026-10-21")
            .RespondJson("{}");
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics/custom-tags?start_date=2026-10-19&end_date=2026-10-21")
            .RespondJson("{}");
      var client = CreateAdminClient();
      // 2026-10-20T00:30+02:00 is 2026-10-19T22:30Z
      var start = new DateTimeOffset(2026, 10, 20, 0, 30, 0, TimeSpan.FromHours(2)).LocalDateTime;
      var end = new DateTime(2026, 10, 21, 23, 59, 0, DateTimeKind.Utc);

      await client.GetUsageAnalyticsAsync(start, end);
      await client.GetCustomTagUsageAnalyticsAsync(start, end);
    }

    [Fact]
    public async Task TestUnspecifiedDateTimeIsTreatedAsUtc() {
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics?start_date=2026-10-20&end_date=2026-10-21")
            .RespondJson("{}");
      await CreateAdminClient().GetUsageAnalyticsAsync(
            new DateTime(2026, 10, 20, 23, 59, 0, DateTimeKind.Unspecified),
            new DateTime(2026, 10, 21, 0, 1, 0, DateTimeKind.Unspecified));
    }

    [Fact]
    public async Task TestStringDatesAreSent() {
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics?start_date=2026-10-19&end_date=2026-10-20&group_by=key")
            .RespondJson("{}");
      ExpectNoBody(
                  HttpMethod.Get,
                  "v2/admin/analytics/custom-tags?start_date=2026-10-19&end_date=2026-10-20&aggregate_by=day&page=2")
            .RespondJson("{}");
      var client = CreateAdminClient();

      await client.GetUsageAnalyticsAsync("2026-10-19", "2026-10-20", AdminUsageGroupBy.Key);
      await client.GetCustomTagUsageAnalyticsAsync("2026-10-19", "2026-10-20", CustomTagUsageAggregateBy.Day, 2);
    }

    [Theory]
    [InlineData("2026-10-1")]
    [InlineData("2026-13-01")]
    [InlineData("20261001")]
    [InlineData("2026-10-01T00:00:00Z")]
    [InlineData("")]
    public async Task TestInvalidStringDatesAreRejected(string invalid) {
      var client = CreateAdminClient();

      await Assert.ThrowsAsync<ArgumentException>(() => client.GetUsageAnalyticsAsync(invalid, "2026-10-20"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.GetUsageAnalyticsAsync("2026-10-19", invalid));
      await Assert.ThrowsAsync<ArgumentException>(() => client.GetCustomTagUsageAnalyticsAsync(invalid, "2026-10-20"));
      await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetUsageAnalyticsAsync(null!, "2026-10-20"));
    }

    [Theory]
    [InlineData(@"""characters"":0", 0L, null, null, true, false, false)]
    [InlineData(@"""speech_to_text_milliseconds"":null", null, null, null, false, true, false)]
    [InlineData(
          @"""characters"":1000,""speech_to_text_milliseconds"":60000,""speech_to_speech_milliseconds"":null",
          1000L, 60000L, null, true, true, true)]
    public async Task TestSetDeveloperKeyUsageLimitsSendsOnlyAssignedLimits(
          string expectedLimits,
          long? characters,
          long? speechToText,
          long? speechToSpeech,
          bool assignCharacters,
          bool assignSpeechToText,
          bool assignSpeechToSpeech) {
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/limits", $@"{{""key_id"":""{KeyId}"",{expectedLimits}}}")
            .RespondJson(DeveloperKeyJson);
      var limits = new DeveloperKeyUsageLimitsUpdate();
      if (assignCharacters) {
        limits.Characters = characters;
      }

      if (assignSpeechToText) {
        limits.SpeechToTextMilliseconds = speechToText;
      }

      if (assignSpeechToSpeech) {
        limits.SpeechToSpeechMilliseconds = speechToSpeech;
      }

      await CreateAdminClient().SetDeveloperKeyUsageLimitsAsync(Key, limits);
    }

    [Fact]
    public async Task TestSetDeveloperKeyUsageLimitsRejectsMissingLimits() {
      await Assert.ThrowsAsync<ArgumentNullException>(
            () => CreateAdminClient().SetDeveloperKeyUsageLimitsAsync(KeyId, null!));
    }

    [Fact]
    public async Task TestMalformedResponseValuesDoNotFailTheCall() {
      ExpectNoBody(HttpMethod.Get, "v2/admin/developer-keys")
            .RespondJson(
                  @"[""skipped"", 5, null, {},
                    {""key_id"": ""id-2"", ""label"": ""kept"", ""creation_time"": ""not a timestamp"",
                     ""deactivated_time"": 12, ""is_deactivated"": true,
                     ""usage_limits"": {""characters"": 5000.0, ""speech_to_text_milliseconds"": ""60000"",
                                        ""speech_to_speech_milliseconds"": ""many""}},
                    {""key_id"": ""id-3"", ""usage_limits"": ""none""}]");
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics?start_date=2025-09-29&end_date=2025-10-01")
            .RespondJson(
                  @"{""usage_report"": {
                    ""total_usage"": {""total_characters"": ""12.0"", ""text_translation_characters"": 7.0,
                      ""document_translation_characters"": ""x"", ""speech_to_text_minutes"": {}},
                    ""group_by"": 5, ""start_date"": ""2025-09-29T00:00:00"", ""end_date"": ""soon"",
                    ""key_usages"": [3, {}, {""api_key"": ""k***1"", ""api_key_label"": ""kept"",
                                           ""usage"": {""total_characters"": 9}}],
                    ""key_and_day_usages"": ""none""}}");
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics/custom-tags?start_date=2026-05-03&end_date=2026-05-15")
            .RespondJson(
                  @"{""custom_tag_usage_report"": {
                    ""aggregate_by"": ""week"", ""start_date"": ""2026-05-03T00:00:00"", ""end_date"": 7,
                    ""next_page"": ""two"",
                    ""usage"": [""skipped"", {}, {""custom_tag"": ""tag"", ""usage_date"": ""bad"",
                      ""breakdown"": {""total_characters"": 5.0, ""speech_to_speech_minutes"": ""1.5""}}]}}");
      var client = CreateAdminClient();

      var keys = await client.ListDeveloperKeysAsync();

      Assert.Equal(new[] { "id-2", "id-3" }, Array.ConvertAll(keys, key => key.KeyId));
      Assert.Equal("kept", keys[0].Label);
      Assert.Null(keys[0].CreationTime);
      Assert.Null(keys[0].DeactivatedTime);
      Assert.True(keys[0].IsDeactivated);
      Assert.Equal(5000, keys[0].UsageLimits!.Characters);
      Assert.Equal(60000, keys[0].UsageLimits!.SpeechToTextMilliseconds);
      Assert.Null(keys[0].UsageLimits!.SpeechToSpeechMilliseconds);
      Assert.Null(keys[1].UsageLimits);

      var report = await client.GetUsageAnalyticsAsync(new DateTime(2025, 9, 29), new DateTime(2025, 10, 1));

      Assert.Equal(12, report.TotalUsage!.TotalCharacters);
      Assert.Equal(7, report.TotalUsage.TextTranslationCharacters);
      Assert.Equal(0, report.TotalUsage.DocumentTranslationCharacters);
      Assert.Equal(0, report.TotalUsage.SpeechToTextMinutes);
      Assert.Null(report.GroupBy);
      Assert.Equal(new DateTime(2025, 9, 29), report.StartDate);
      Assert.Null(report.EndDate);
      Assert.Equal(new[] { "", "k***1" }, Array.ConvertAll(report.KeyUsages, usage => usage.ApiKey));
      Assert.Equal("kept", report.KeyUsages[1].ApiKeyLabel);
      Assert.Equal(9, report.KeyUsages[1].Usage.TotalCharacters);
      Assert.Null(report.KeyUsages[1].UsageDate);
      Assert.Empty(report.KeyAndDayUsages);

      var tagReport = await client.GetCustomTagUsageAnalyticsAsync(
            new DateTime(2026, 5, 3),
            new DateTime(2026, 5, 15));

      Assert.Equal("week", tagReport.AggregateBy);
      Assert.Null(tagReport.EndDate);
      Assert.Null(tagReport.NextPage);
      Assert.Equal(new[] { "", "tag" }, Array.ConvertAll(tagReport.Usage, usage => usage.CustomTag));
      Assert.Null(tagReport.Usage[1].UsageDate);
      Assert.Equal(5, tagReport.Usage[1].Breakdown.TotalCharacters);
      Assert.Equal(1.5, tagReport.Usage[1].Breakdown.SpeechToSpeechMinutes);
    }

    [Theory]
    [InlineData("")]
    [InlineData(@",""label"": null, ""usage_limits"": null")]
    public async Task TestMissingLabelAndUsageLimitsAreNull(string fields) {
      ExpectNoBody(HttpMethod.Get, "v2/admin/developer-keys").RespondJson($@"[{{""key_id"": ""id-1""{fields}}}]");
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics?start_date=2025-09-29&end_date=2025-10-01")
            .RespondJson(
                  $@"{{""usage_report"": {{""key_usages"": [{{""api_key"": ""k***1""{fields.Replace("label", "api_key_label")}}}]}}}}");
      var client = CreateAdminClient();

      var key = Assert.Single(await client.ListDeveloperKeysAsync());
      var report = await client.GetUsageAnalyticsAsync(new DateTime(2025, 9, 29), new DateTime(2025, 10, 1));

      Assert.Null(key.Label);
      Assert.Null(key.UsageLimits);
      Assert.Null(Assert.Single(report.KeyUsages).ApiKeyLabel);
    }

    [Theory]
    [InlineData(@"""false""", false)]
    [InlineData("null", false)]
    [InlineData(@"""true""", false)]
    [InlineData("1", false)]
    [InlineData("true", true)]
    public async Task TestIsDeactivatedIsTrueOnlyForJsonTrue(string value, bool expected) {
      ExpectNoBody(HttpMethod.Get, "v2/admin/developer-keys")
            .RespondJson($@"[{{""key_id"": ""id-1"", ""is_deactivated"": {value}}}]");

      var keys = await CreateAdminClient().ListDeveloperKeysAsync();

      Assert.Equal(expected, Assert.Single(keys).IsDeactivated);
    }

    [Fact]
    public async Task TestKeyResponseWithoutKeyIdThrowsDeepLException() {
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", "{}").RespondJson(@"{""label"": ""no id""}");
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/deactivate", $@"{{""key_id"":""{KeyId}""}}")
            .RespondJson(@"{""key_id"": 5}");
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/label", $@"{{""key_id"":""{KeyId}"",""label"":""x""}}")
            .RespondJson("[]");
      ExpectJson(HttpMethod.Put, "v2/admin/developer-keys/limits", $@"{{""key_id"":""{KeyId}"",""characters"":1}}")
            .RespondJson(@"{""label"": ""no id""}");
      var client = CreateAdminClient();

      await Assert.ThrowsAsync<DeepLException>(() => client.CreateDeveloperKeyAsync());
      await Assert.ThrowsAsync<DeepLException>(() => client.DeactivateDeveloperKeyAsync(KeyId));
      await Assert.ThrowsAsync<DeepLException>(() => client.RenameDeveloperKeyAsync(KeyId, "x"));
      await Assert.ThrowsAsync<DeepLException>(
            () => client.SetDeveloperKeyUsageLimitsAsync(KeyId, new DeveloperKeyUsageLimitsUpdate { Characters = 1 }));
    }

    [Fact]
    public async Task TestDeveloperKeyListSkipsEntriesWithoutKeyId() {
      ExpectNoBody(HttpMethod.Get, "v2/admin/developer-keys")
            .RespondJson($@"[{{""label"": ""no id""}}, {{""key_id"": """"}}, 7, {DeveloperKeyJson}]");

      var keys = await CreateAdminClient().ListDeveloperKeysAsync();

      Assert.Equal(KeyId, Assert.Single(keys).KeyId);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData(@"""text""")]
    [InlineData("null")]
    public async Task TestDeveloperKeyListThatIsNotAnArrayThrowsDeepLException(string body) {
      ExpectNoBody(HttpMethod.Get, "v2/admin/developer-keys").RespondJson(body);

      await Assert.ThrowsAsync<DeepLException>(() => CreateAdminClient().ListDeveloperKeysAsync());
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("")]
    public async Task TestAnalyticsThatAreNotAnObjectThrowDeepLException(string body) {
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics?start_date=2025-09-29&end_date=2025-10-01").RespondJson(body);
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics/custom-tags?start_date=2026-05-03&end_date=2026-05-15")
            .RespondJson(body);
      var client = CreateAdminClient();

      await Assert.ThrowsAsync<DeepLException>(
            () => client.GetUsageAnalyticsAsync(new DateTime(2025, 9, 29), new DateTime(2025, 10, 1)));
      await Assert.ThrowsAsync<DeepLException>(
            () => client.GetCustomTagUsageAnalyticsAsync(new DateTime(2026, 5, 3), new DateTime(2026, 5, 15)));
    }

    [Fact]
    public async Task TestSpeechToSpeechMinutesInUsageBreakdown() {
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics?start_date=2025-09-29&end_date=2025-10-01")
            .RespondJson(
                  @"{""usage_report"": {
                    ""total_usage"": {""total_characters"": 1, ""speech_to_speech_minutes"": 3.5},
                    ""key_usages"": [{""api_key"": ""k***1"", ""usage"": {""speech_to_speech_minutes"": 1.25}},
                                     {""api_key"": ""k***2"", ""usage"": {""total_characters"": 2}}]}}");

      var report = await CreateAdminClient()
            .GetUsageAnalyticsAsync(new DateTime(2025, 9, 29), new DateTime(2025, 10, 1));

      Assert.Equal(3.5, report.TotalUsage!.SpeechToSpeechMinutes);
      Assert.Equal(1.25, report.KeyUsages[0].Usage.SpeechToSpeechMinutes);
      Assert.Equal(0, report.KeyUsages[1].Usage.SpeechToSpeechMinutes);
    }

    [Fact]
    public async Task TestMissingReportsAndNullFieldsGetNeutralDefaults() {
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics?start_date=2025-09-29&end_date=2025-10-01").RespondJson("{}");
      ExpectNoBody(HttpMethod.Get, "v2/admin/analytics/custom-tags?start_date=2026-05-03&end_date=2026-05-15")
            .RespondJson(@"{""custom_tag_usage_report"": {""next_page"": null, ""usage"": null}}");
      var client = CreateAdminClient();

      var report = await client.GetUsageAnalyticsAsync(new DateTime(2025, 9, 29), new DateTime(2025, 10, 1));
      var tagReport = await client.GetCustomTagUsageAnalyticsAsync(
            new DateTime(2026, 5, 3),
            new DateTime(2026, 5, 15));

      Assert.Null(report.TotalUsage);
      Assert.Null(report.StartDate);
      Assert.Empty(report.KeyUsages);
      Assert.Null(tagReport.NextPage);
      Assert.Empty(tagReport.Usage);
    }

    [Fact]
    public async Task TestCreateDeveloperKeyIsNotRetriedAfterServerError() {
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", "{}")
            .RespondJson("{}", HttpStatusCode.ServiceUnavailable);

      var exception = await Assert.ThrowsAsync<DeepLException>(
            () => CreateAdminClientWithRetries(1).CreateDeveloperKeyAsync());
      Assert.Equal(503, exception.HttpStatusCode);
      Assert.False(exception.ShouldRetry);
    }

    [Fact]
    public async Task TestCreateDeveloperKeyServerErrorIsNotMarkedRetryable() {
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", "{}").RespondJson("{}", HttpStatusCode.InternalServerError);

      var exception = await Assert.ThrowsAsync<DeepLException>(() => CreateAdminClient().CreateDeveloperKeyAsync());
      Assert.False(exception.ShouldRetry);
    }

    [Fact]
    public async Task TestCreateDeveloperKeyIsNotRetriedAfterConnectionReset() {
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", "{}")
            .Throw(new HttpRequestException("reset", new SocketException((int)SocketError.ConnectionReset)));

      var exception = await Assert.ThrowsAsync<ConnectionException>(
            () => CreateAdminClientWithRetries(1).CreateDeveloperKeyAsync());
      Assert.Equal("Request failed: reset", exception.Message);
      Assert.False(exception.ShouldRetry);
    }

    [Fact]
    public async Task TestCreateDeveloperKeyIsRetriedAfter429() {
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", "{}").RespondJson("{}", (HttpStatusCode)429);
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", "{}").RespondJson(DeveloperKeyJson);

      await CreateAdminClientWithRetries(1).CreateDeveloperKeyAsync();
    }

    [Fact]
    public async Task TestCreateDeveloperKeyIsRetriedAfterNameResolutionFailure() {
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", "{}")
            .Throw(new HttpRequestException("dns", new SocketException((int)SocketError.HostNotFound)));
      ExpectJson(HttpMethod.Post, "v2/admin/developer-keys", "{}").RespondJson(DeveloperKeyJson);

      await CreateAdminClientWithRetries(1).CreateDeveloperKeyAsync();
    }

    [Fact]
    public async Task TestOtherAdminRequestsAreRetriedAfterServerError() {
      ExpectNoBody(HttpMethod.Get, "v2/admin/developer-keys").RespondJson("{}", HttpStatusCode.ServiceUnavailable);
      ExpectNoBody(HttpMethod.Get, "v2/admin/developer-keys").RespondJson("[]");

      Assert.Empty(await CreateAdminClientWithRetries(1).ListDeveloperKeysAsync());
    }

    [Fact]
    public async Task TestForbiddenThrowsAuthorizationException() {
      ExpectNoBody(HttpMethod.Get, "v2/admin/developer-keys")
            .RespondJson(@"{""message"": ""Forbidden""}", HttpStatusCode.Forbidden);

      await Assert.ThrowsAsync<AuthorizationException>(() => CreateAdminClient().ListDeveloperKeysAsync());
    }

    private DeepLAdminClient CreateAdminClient() =>
          new DeepLAdminClient(
                AuthKey,
                new DeepLAdminClientOptions {
                  ClientFactory = () =>
                        new HttpClientAndDisposeFlag { HttpClient = Http.ToHttpClient(), DisposeClient = true }
                });

    private DeepLAdminClient CreateAdminClientWithRetries(int maximumNetworkRetries) =>
          new DeepLAdminClient(
                AuthKey,
                new DeepLAdminClientOptions {
                  ClientFactory = () => DeepL.Internal.DeepLHttpClient.CreateDefaultHttpClient(
                        new DeepLAdminClientOptions { MaximumNetworkRetries = maximumNetworkRetries },
                        Http)
                });
  }
}
