// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DeepL.Model;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class StyleRules : OfflineTest {
    private const string RuleReply =
          "{\"style_id\":\"s1\",\"name\":\"My rule\",\"creation_time\":\"2025-01-02T03:04:05Z\"," +
          "\"updated_time\":\"2025-01-03T03:04:05Z\",\"language\":\"en\",\"version\":3," +
          "\"configured_rules\":{\"dates_and_times\":{\"calendar_era\":\"use_bc_and_ad\"}}," +
          "\"custom_instructions\":[{\"id\":\"i1\",\"label\":\"Tone\",\"prompt\":\"Be formal\",\"source_language\":\"de\"}]}";

    private const string InstructionReply = "{\"id\":\"i1\",\"label\":\"L\",\"prompt\":\"P\",\"source_language\":\"de\"}";

    private static ConfiguredRules Rules() =>
          new ConfiguredRules(datesAndTimes: new Dictionary<string, string> { { "calendar_era", "use_bc_and_ad" } });

    [Fact]
    public async Task SendsPagingAndDetailedInQuery() {
      ExpectNoBody(HttpMethod.Get, "v3/style_rules")
            .WithExactQueryString("page=2&page_size=25&detailed=true")
            .RespondJson("{\"style_rules\":[" + RuleReply + "]}");
      ExpectNoBody(HttpMethod.Get, "v3/style_rules")
            .WithExactQueryString("detailed=false")
            .RespondJson("{\"style_rules\":[]}");
      var client = CreateClient();
      Assert.Equal("s1", Assert.Single(await client.GetAllStyleRulesAsync(2, 25, true)).StyleId);
      Assert.Empty(await client.GetAllStyleRulesAsync(detailed: false));
    }

    [Fact]
    public async Task ParsesStyleRule() {
      ExpectNoBody(HttpMethod.Get, "v3/style_rules/s1").RespondJson(RuleReply);
      var rule = await CreateClient().GetStyleRuleAsync("s1");
      Assert.Equal("s1", rule.StyleId);
      Assert.Equal("My rule", rule.Name);
      Assert.Equal("en", rule.Language);
      Assert.Equal(3, rule.Version);
      Assert.Equal(new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc), rule.CreationTime.ToUniversalTime());
      Assert.Equal(new DateTime(2025, 1, 3, 3, 4, 5, DateTimeKind.Utc), rule.UpdatedTime.ToUniversalTime());
      Assert.Equal("use_bc_and_ad", rule.ConfiguredRules!.DatesAndTimes!["calendar_era"]);
      Assert.Null(rule.ConfiguredRules.Numbers);
      var instruction = Assert.Single(rule.CustomInstructions!);
      Assert.Equal("i1", instruction.Id);
      Assert.Equal("de", instruction.SourceLanguage);
    }

    [Fact]
    public async Task SendsJsonWithRulesAndInstructionsWhenCreating() {
      ExpectJson(
                  HttpMethod.Post,
                  "v3/style_rules",
                  "{\"name\":\"My rule\",\"language\":\"en\"," +
                  "\"configured_rules\":{\"dates_and_times\":{\"calendar_era\":\"use_bc_and_ad\"}}," +
                  "\"custom_instructions\":[{\"label\":\"Tone\",\"prompt\":\"Be formal\",\"source_language\":\"de\"}]}")
            .RespondJson(RuleReply);
      await CreateClient().CreateStyleRuleAsync(
            "My rule",
            "en",
            Rules(),
            new[] { new CustomInstruction("Tone", "Be formal", "de") });
    }

    [Fact]
    public async Task OmitsOptionalFieldsWhenCreating() {
      ExpectJson(HttpMethod.Post, "v3/style_rules", "{\"name\":\"My rule\",\"language\":\"en\"}").RespondJson(RuleReply);
      await CreateClient().CreateStyleRuleAsync("My rule", "en");
    }

    [Fact]
    public async Task SendsJsonNameWhenRenaming() {
      ExpectJson(Patch, "v3/style_rules/s1", "{\"name\":\"New\"}").RespondJson(RuleReply);
      await CreateClient().UpdateStyleRuleNameAsync("s1", "New");
    }

    [Fact]
    public async Task SendsJsonRulesWhenUpdatingConfiguredRules() {
      ExpectJson(
                  HttpMethod.Put,
                  "v3/style_rules/s1/configured_rules",
                  "{\"dates_and_times\":{\"calendar_era\":\"use_bc_and_ad\"}}")
            .RespondJson(RuleReply);
      await CreateClient().UpdateStyleRuleConfiguredRulesAsync("s1", Rules());
    }

    [Fact]
    public async Task SendsDeleteToStyleRulePath() {
      ExpectNoBody(HttpMethod.Delete, "v3/style_rules/s1").Respond(HttpStatusCode.NoContent);
      await CreateClient().DeleteStyleRuleAsync("s1");
    }

    [Fact]
    public async Task EscapesStyleIdInPath() {
      ExpectNoBody(HttpMethod.Get, "v3/style_rules/a%20b").RespondJson(RuleReply);
      await CreateClient().GetStyleRuleAsync("a b");
    }

    [Fact]
    public async Task SendsJsonWhenCreatingCustomInstruction() {
      ExpectJson(
                  HttpMethod.Post,
                  "v3/style_rules/s1/custom_instructions",
                  "{\"label\":\"L\",\"prompt\":\"P\",\"source_language\":\"de\"}")
            .RespondJson(InstructionReply);
      ExpectJson(HttpMethod.Post, "v3/style_rules/s1/custom_instructions", "{\"label\":\"L\",\"prompt\":\"P\"}")
            .RespondJson(InstructionReply);
      var client = CreateClient();
      var instruction = await client.CreateStyleRuleCustomInstructionAsync("s1", "L", "P", "de");
      Assert.Equal("i1", instruction.Id);
      await client.CreateStyleRuleCustomInstructionAsync("s1", "L", "P");
    }

    [Fact]
    public async Task SendsInstructionIdInPathWhenReadingUpdatingAndDeletingInstruction() {
      ExpectNoBody(HttpMethod.Get, "v3/style_rules/s1/custom_instructions/i1").RespondJson(InstructionReply);
      ExpectJson(
                  HttpMethod.Put,
                  "v3/style_rules/s1/custom_instructions/i1",
                  "{\"label\":\"New\",\"prompt\":\"Newer\",\"source_language\":\"fr\"}")
            .RespondJson(InstructionReply);
      ExpectNoBody(HttpMethod.Delete, "v3/style_rules/s1/custom_instructions/i1").Respond(HttpStatusCode.NoContent);
      var client = CreateClient();
      Assert.Equal("L", (await client.GetStyleRuleCustomInstructionAsync("s1", "i1")).Label);
      await client.UpdateStyleRuleCustomInstructionAsync("s1", "i1", "New", "Newer", "fr");
      await client.DeleteStyleRuleCustomInstructionAsync("s1", "i1");
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWithResourceNameWhenStatusIs404() {
      ExpectNoBody(HttpMethod.Get, "v3/style_rules/x").RespondJson("{\"message\":\"gone\"}", HttpStatusCode.NotFound);
      var exception = await Assert.ThrowsAsync<DeepL.NotFoundException>(() => CreateClient().GetStyleRuleAsync("x"));
      Assert.Equal("Style rule not found, message: gone", exception.Message);
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenArgumentsAreEmpty() {
      var client = CreateClient();
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateStyleRuleAsync("", "en"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateStyleRuleAsync("n", ""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.GetStyleRuleAsync(""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateStyleRuleNameAsync("", "n"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateStyleRuleNameAsync("s1", ""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteStyleRuleAsync(""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateStyleRuleConfiguredRulesAsync("", Rules()));
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateStyleRuleCustomInstructionAsync("", "L", "P"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateStyleRuleCustomInstructionAsync("s1", "", "P"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateStyleRuleCustomInstructionAsync("s1", "L", ""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.GetStyleRuleCustomInstructionAsync("", "i1"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.GetStyleRuleCustomInstructionAsync("s1", ""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateStyleRuleCustomInstructionAsync("", "i1", "L", "P"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateStyleRuleCustomInstructionAsync("s1", "", "L", "P"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateStyleRuleCustomInstructionAsync("s1", "i1", "", "P"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateStyleRuleCustomInstructionAsync("s1", "i1", "L", ""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteStyleRuleCustomInstructionAsync("", "i1"));
      await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteStyleRuleCustomInstructionAsync("s1", ""));
    }

    [Fact]
    public async Task ThrowsArgumentNullExceptionWhenConfiguredRulesAreNull() =>
          await Assert.ThrowsAsync<ArgumentNullException>(
                () => CreateClient().UpdateStyleRuleConfiguredRulesAsync("s1", null!));
  }
}
