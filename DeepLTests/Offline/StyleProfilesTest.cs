// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class StyleProfilesTest : OfflineTest {
    private const string ProfileReply =
          "{\"style_profile_id\":\"3f1c9d0e-7a24-4b58-9c31-8f0e5d2a1b47\",\"name\":\"Legal, German\"," +
          "\"description\":\"Contract wording for the German market.\",\"industry\":\"Legal\"," +
          "\"glossary_ids\":[\"4d0a2e1c-6b93-4f7a-8d15-2c6e9a3b7f81\"]," +
          "\"style_rule_ids\":[\"bd0a38f3-1831-440b-a8dd-2c702e2325ab\"]," +
          "\"translation_memory_ids\":[\"a74d88fb-ed2a-4943-a664-a4512398b994\"]," +
          "\"glossary_priority_order_enabled\":true," +
          "\"creation_time\":\"2026-04-01T16:34:25.223Z\",\"updated_time\":\"2026-08-06T09:12:44.108Z\"}";

    private const string MinimalReply =
          "{\"style_profile_id\":\"sp1\",\"name\":\"Minimal\",\"glossary_ids\":[],\"style_rule_ids\":[]," +
          "\"translation_memory_ids\":[],\"glossary_priority_order_enabled\":false," +
          "\"creation_time\":\"2026-10-26T16:27:10.2982869Z\",\"updated_time\":\"2026-10-26T16:27:10.2982869Z\"," +
          "\"example_document_ids\":[],\"spoken_terms_ids\":[\"x\"]}";

    [Fact]
    public async Task SendsPagingInQueryOnlyWhenSet() {
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles")
            .WithExactQueryString("page=2&page_size=25")
            .RespondJson("{\"style_profiles\":[" + ProfileReply + "],\"total_count\":51}");
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles")
            .WithExactQueryString(string.Empty)
            .RespondJson("{\"total_count\":0}");
      var client = CreateClient();

      Assert.Equal("Legal, German", Assert.Single(await client.ListStyleProfilesAsync(2, 25)).Name);
      Assert.Empty(await client.ListStyleProfilesAsync());
    }

    [Fact]
    public async Task SendsOnlyTheNameWhenCreatingWithoutOptions() {
      ExpectJson(HttpMethod.Post, "v3/style_profiles", "{\"name\":\"Minimal\"}").RespondJson(MinimalReply);

      var created = await CreateClient().CreateStyleProfileAsync("Minimal");

      Assert.Equal("sp1", created.StyleProfileId);
    }

    [Fact]
    public async Task SendsEveryOptionWhenCreating() {
      ExpectJson(
                  HttpMethod.Post,
                  "v3/style_profiles",
                  "{\"name\":\"Legal\",\"description\":\"D\",\"industry\":\"I\",\"glossary_ids\":[\"g2\",\"g1\"]," +
                  "\"style_rule_ids\":[\"s1\"],\"translation_memory_ids\":[\"t1\"]," +
                  "\"glossary_priority_order_enabled\":true}")
            .RespondJson(ProfileReply);

      await CreateClient().CreateStyleProfileAsync(
            "Legal",
            new CreateStyleProfileOptions {
              Description = "D",
              Industry = "I",
              GlossaryIds = new List<string> { "g2", "g1" },
              StyleRuleIds = new[] { "s1" },
              TranslationMemoryIds = new[] { "t1" },
              GlossaryPriorityOrderEnabled = true
            });
    }

    [Fact]
    public async Task SendsOnlyTheSetFieldsWhenUpdatingIncludingEmptyValues() {
      ExpectJson(Patch, "v3/style_profiles/sp1", "{\"name\":\"New\",\"description\":\"\",\"style_rule_ids\":[]}")
            .RespondJson(MinimalReply);
      ExpectJson(Patch, "v3/style_profiles/sp1", "{}").RespondJson(MinimalReply);
      var client = CreateClient();

      var updated = await client.UpdateStyleProfileAsync(
            "sp1",
            new UpdateStyleProfileOptions { Name = "New", Description = "", StyleRuleIds = new string[0] });
      await client.UpdateStyleProfileAsync(updated);
    }

    [Fact]
    public async Task AcceptsAStyleProfileObjectAndEscapesTheId() {
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles/a%20b%2Fc").RespondJson(MinimalReply);
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles/sp1").RespondJson(MinimalReply);
      ExpectJson(Patch, "v3/style_profiles/sp1", "{\"industry\":\"Legal\"}").RespondJson(MinimalReply);
      Expect(HttpMethod.Delete, "v3/style_profiles/sp1").Respond(HttpStatusCode.NoContent);
      Expect(HttpMethod.Delete, "v3/style_profiles/a%20b%2Fc").Respond(HttpStatusCode.NoContent);
      var client = CreateClient();

      var profile = await client.GetStyleProfileAsync("a b/c");
      await client.GetStyleProfileAsync(profile);
      await client.UpdateStyleProfileAsync(profile, new UpdateStyleProfileOptions { Industry = "Legal" });
      await client.DeleteStyleProfileAsync(profile);
      await client.DeleteStyleProfileAsync("a b/c");
    }

    [Fact]
    public async Task RejectsAnEmptyIdWithoutSendingARequest() {
      var client = CreateClient();

      await Assert.ThrowsAsync<ArgumentException>(() => client.GetStyleProfileAsync(""));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.UpdateStyleProfileAsync("", new UpdateStyleProfileOptions { Name = "New" }));
      await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteStyleProfileAsync(""));
    }

    [Fact]
    public async Task ParsesEveryFieldOfTheSpecExample() {
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles/sp1").RespondJson(ProfileReply);

      var profile = await CreateClient().GetStyleProfileAsync("sp1");

      Assert.Equal("3f1c9d0e-7a24-4b58-9c31-8f0e5d2a1b47", profile.StyleProfileId);
      Assert.Equal("Legal, German", profile.Name);
      Assert.Equal("Contract wording for the German market.", profile.Description);
      Assert.Equal("Legal", profile.Industry);
      Assert.Equal(new[] { "4d0a2e1c-6b93-4f7a-8d15-2c6e9a3b7f81" }, profile.GlossaryIds);
      Assert.Equal(new[] { "bd0a38f3-1831-440b-a8dd-2c702e2325ab" }, profile.StyleRuleIds);
      Assert.Equal(new[] { "a74d88fb-ed2a-4943-a664-a4512398b994" }, profile.TranslationMemoryIds);
      Assert.True(profile.GlossaryPriorityOrderEnabled);
      Assert.Equal(
            new DateTime(2026, 4, 1, 16, 34, 25, 223, DateTimeKind.Utc),
            profile.CreationTime.ToUniversalTime());
      Assert.Equal(
            new DateTime(2026, 8, 6, 9, 12, 44, 108, DateTimeKind.Utc),
            profile.UpdatedTime.ToUniversalTime());
    }

    [Fact]
    public async Task ParsesMissingOptionalFieldsSevenDigitTimestampsAndIgnoresUnknownFields() {
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles/sp1").RespondJson(MinimalReply);
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles/sp2")
            .RespondJson(
                  "{\"style_profile_id\":\"sp2\",\"name\":\"Bare\",\"description\":null,\"glossary_ids\":null," +
                  "\"creation_time\":\"2026-04-01T16:34:25.223Z\",\"updated_time\":\"2026-08-06T09:12:44.108Z\"}");
      var client = CreateClient();

      var profile = await client.GetStyleProfileAsync("sp1");
      var bare = await client.GetStyleProfileAsync("sp2");

      Assert.Null(profile.Description);
      Assert.Null(profile.Industry);
      Assert.Empty(profile.StyleRuleIds);
      Assert.Equal(
            new DateTime(2026, 10, 26, 16, 27, 10, DateTimeKind.Utc).AddTicks(2982869),
            profile.CreationTime.ToUniversalTime());
      Assert.Null(bare.Description);
      Assert.Empty(bare.GlossaryIds);
      Assert.Empty(bare.StyleRuleIds);
      Assert.Empty(bare.TranslationMemoryIds);
      Assert.False(bare.GlossaryPriorityOrderEnabled);
    }

    [Fact]
    public async Task ToleratesNonStringIds() {
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles")
            .RespondJson(
                  "{\"style_profiles\":[{\"style_profile_id\":\"sp1\",\"name\":\"Odd\",\"description\":\"D\"," +
                  "\"glossary_ids\":[\"g1\",42,null,{\"id\":\"x\"},\"g2\"],\"style_rule_ids\":\"s1\"," +
                  "\"translation_memory_ids\":[\"t1\"],\"glossary_priority_order_enabled\":true," +
                  "\"creation_time\":\"2026-04-01T16:34:25.223Z\",\"updated_time\":\"2026-08-06T09:12:44.108Z\"}]}");

      var profile = Assert.Single(await CreateClient().ListStyleProfilesAsync());

      Assert.Equal(new[] { "g1", "g2" }, profile.GlossaryIds);
      Assert.Empty(profile.StyleRuleIds);
      Assert.Equal(new[] { "t1" }, profile.TranslationMemoryIds);
      Assert.Equal(("sp1", "Odd", "D"), (profile.StyleProfileId, profile.Name, profile.Description));
      Assert.True(profile.GlossaryPriorityOrderEnabled);
    }

    [Fact]
    public async Task SendsTheNameFromTheUpdateOptions() {
      ExpectJson(Patch, "v3/style_profiles/sp1", "{\"name\":\"Renamed\"}").RespondJson(MinimalReply);
      var client = CreateClient();

      await client.UpdateStyleProfileAsync("sp1", new UpdateStyleProfileOptions { Name = "Renamed" });
    }

    [Fact]
    public async Task MergesExtraBodyParametersLastWhenCreatingAndUpdating() {
      ExpectJson(
                  HttpMethod.Post,
                  "v3/style_profiles",
                  "{\"name\":\"Legal\",\"description\":\"extra\",\"beta\":\"on\"}")
            .RespondJson(MinimalReply);
      ExpectJson(Patch, "v3/style_profiles/sp1", "{\"name\":\"extra\",\"industry\":\"I\",\"beta\":\"on\"}")
            .RespondJson(MinimalReply);
      var client = CreateClient();

      await client.CreateStyleProfileAsync(
            "Legal",
            new CreateStyleProfileOptions {
              Description = "D",
              ExtraBodyParameters = new Dictionary<string, string> { ["description"] = "extra", ["beta"] = "on" }
            });
      await client.UpdateStyleProfileAsync(
            "sp1",
            new UpdateStyleProfileOptions {
              Name = "New",
              Industry = "I",
              ExtraBodyParameters = new Dictionary<string, string> { ["name"] = "extra", ["beta"] = "on" }
            });
    }

    [Fact]
    public async Task SendsTheIdsOfResourceObjectsInTheComponentLists() {
      var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      var glossary = new GlossaryInfo("g1", "G", true, "en", "de", created, 1);
      var multilingualGlossary =
            new MultilingualGlossaryInfo("g2", "M", new MultilingualGlossaryDictionaryInfo[0], created);
      var styleRule = new StyleRuleInfo("s1", "S", created, created, "de", 1, null, null);
      var translationMemory = new TranslationMemoryInfo("t1", "T", "en", new[] { "de" }, 0);
      ExpectJson(
                  HttpMethod.Post,
                  "v3/style_profiles",
                  "{\"name\":\"Legal\",\"glossary_ids\":[\"g2\"],\"style_rule_ids\":[\"s1\"]," +
                  "\"translation_memory_ids\":[\"t1\"]}")
            .RespondJson(MinimalReply);
      ExpectJson(Patch, "v3/style_profiles/sp1", "{\"glossary_ids\":[\"g1\"]}").RespondJson(MinimalReply);
      ExpectJson(Patch, "v3/style_profiles/sp1", "{\"glossary_ids\":[\"g2\",\"g1\",\"g3\"]}")
            .RespondJson(MinimalReply);
      var client = CreateClient();

      var createOptions = new CreateStyleProfileOptions();
      createOptions.SetGlossaries(new[] { multilingualGlossary });
      createOptions.SetStyleRules(new[] { styleRule });
      createOptions.SetTranslationMemories(new List<TranslationMemoryInfo> { translationMemory });
      await client.CreateStyleProfileAsync("Legal", createOptions);
      var updateOptions = new UpdateStyleProfileOptions();
      updateOptions.SetGlossaries(new[] { glossary });
      await client.UpdateStyleProfileAsync("sp1", updateOptions);
      await client.UpdateStyleProfileAsync(
            "sp1",
            new UpdateStyleProfileOptions { GlossaryIds = new string[] { multilingualGlossary, glossary, "g3" } });
    }

    [Fact]
    public async Task ThrowsOnAnUnparseableRequiredTimestamp() {
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles/sp1")
            .RespondJson(
                  MinimalReply.Replace("2026-10-26T16:27:10.2982869Z\",\"updated", "not a timestamp\",\"updated"));

      await Assert.ThrowsAsync<DeepLException>(() => CreateClient().GetStyleProfileAsync("sp1"));
    }

    [Fact]
    public async Task ThrowsOnAMissingRequiredTimestamp() {
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles/sp1")
            .RespondJson(MinimalReply.Replace("\"creation_time\":\"2026-10-26T16:27:10.2982869Z\",", ""));
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles/sp2")
            .RespondJson(MinimalReply.Replace(",\"updated_time\":\"2026-10-26T16:27:10.2982869Z\"", ""));
      var client = CreateClient();

      var missingCreation =
            await Assert.ThrowsAsync<DeepLException>(() => client.GetStyleProfileAsync("sp1"));
      var missingUpdate = await Assert.ThrowsAsync<DeepLException>(() => client.GetStyleProfileAsync("sp2"));

      Assert.Contains("creation_time", missingCreation.Message);
      Assert.Contains("updated_time", missingUpdate.Message);
    }

    [Fact]
    public async Task SkipsNonObjectEntriesInTheList() {
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles")
            .RespondJson("{\"style_profiles\":[" + MinimalReply + ",null,42,\"x\"],\"total_count\":4}");

      var profile = Assert.Single(await CreateClient().ListStyleProfilesAsync());

      Assert.Equal(("sp1", "Minimal"), (profile.StyleProfileId, profile.Name));
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionNamingTheStyleProfileWhenStatusIs404() {
      ExpectNoBody(HttpMethod.Get, "v3/style_profiles/sp1")
            .RespondJson("{\"message\":\"Style profile not found\"}", HttpStatusCode.NotFound);

      var exception =
            await Assert.ThrowsAsync<NotFoundException>(() => CreateClient().GetStyleProfileAsync("sp1"));

      Assert.StartsWith("Style profile not found", exception.Message);
    }
  }
}
