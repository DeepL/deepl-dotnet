// Copyright 2025 DeepL SE (https://www.deepl.com)
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
  public sealed class StyleRulesTest : OfflineTest {
    private const string RuleReply =
          "{\"style_id\":\"s1\",\"name\":\"My rule\",\"creation_time\":\"2025-01-02T03:04:05Z\"," +
          "\"updated_time\":\"2025-01-03T03:04:05Z\",\"language\":\"en\",\"version\":3," +
          "\"configured_rules\":{\"dates_and_times\":{\"calendar_era\":\"use_bc_and_ad\"}}," +
          "\"custom_instructions\":[{\"id\":\"i1\",\"label\":\"Tone\",\"prompt\":\"Be formal\",\"source_language\":\"de\"}]}";

    private const string InstructionReply = "{\"id\":\"i1\",\"label\":\"L\",\"prompt\":\"P\",\"source_language\":\"de\"}";

    private static ConfiguredRules Rules() =>
          new ConfiguredRules(datesAndTimes: new Dictionary<string, string> { { "calendar_era", "use_bc_and_ad" } });

    private static StyleRuleInfo Info() =>
          new StyleRuleInfo("s1", "My rule", DateTime.UtcNow, DateTime.UtcNow, "en", 3, null, null);

    private static CustomInstruction Instruction() => new CustomInstruction("L", "P", "de", "i1");

    private static Dictionary<string, string> Extra() => new Dictionary<string, string> { ["extra"] = "1" };

    [Fact]
    public async Task AcceptsStyleRuleObjectInAllEightMethods() {
      ExpectNoBody(HttpMethod.Get, "v3/style_rules/s1").RespondJson(RuleReply);
      ExpectJson(Patch, "v3/style_rules/s1", "{\"name\":\"New\"}").RespondJson(RuleReply);
      ExpectJson(HttpMethod.Put, "v3/style_rules/s1/configured_rules", "{\"numbers\":{\"a\":\"b\"}}")
            .RespondJson(RuleReply);
      ExpectNoBody(HttpMethod.Delete, "v3/style_rules/s1").Respond(HttpStatusCode.NoContent);
      ExpectJson(HttpMethod.Post, "v3/style_rules/s1/custom_instructions", "{\"label\":\"L\",\"prompt\":\"P\"}")
            .RespondJson(InstructionReply);
      ExpectNoBody(HttpMethod.Get, "v3/style_rules/s1/custom_instructions/i1").RespondJson(InstructionReply);
      ExpectJson(HttpMethod.Put, "v3/style_rules/s1/custom_instructions/i1", "{\"label\":\"L\",\"prompt\":\"P\"}")
            .RespondJson(InstructionReply);
      ExpectNoBody(HttpMethod.Delete, "v3/style_rules/s1/custom_instructions/i1").Respond(HttpStatusCode.NoContent);
      var client = CreateClient();
      Assert.Equal("s1", (await client.GetStyleRuleAsync(Info())).StyleId);
      await client.UpdateStyleRuleNameAsync(Info(), "New");
      await client.UpdateStyleRuleConfiguredRulesAsync(
            Info(),
            new Dictionary<string, Dictionary<string, string>> { ["numbers"] = new Dictionary<string, string> { ["a"] = "b" } });
      await client.DeleteStyleRuleAsync(Info());
      await client.CreateStyleRuleCustomInstructionAsync(Info(), "L", "P");
      await client.GetStyleRuleCustomInstructionAsync(Info(), "i1");
      await client.UpdateStyleRuleCustomInstructionAsync(Info(), "i1", "L", "P");
      await client.DeleteStyleRuleCustomInstructionAsync(Info(), "i1");
    }

    [Fact]
    public async Task AcceptsCustomInstructionObjectForTheInstructionId() {
      const string path = "v3/style_rules/s1/custom_instructions/i1";
      ExpectNoBody(HttpMethod.Get, path).RespondJson(InstructionReply);
      ExpectNoBody(HttpMethod.Get, path).RespondJson(InstructionReply);
      ExpectJson(HttpMethod.Put, path, "{\"label\":\"New\",\"prompt\":\"Newer\"}").RespondJson(InstructionReply);
      ExpectJson(HttpMethod.Put, path, "{\"label\":\"New\",\"prompt\":\"Newer\"}").RespondJson(InstructionReply);
      ExpectNoBody(HttpMethod.Delete, path).Respond(HttpStatusCode.NoContent);
      ExpectNoBody(HttpMethod.Delete, path).Respond(HttpStatusCode.NoContent);
      var client = CreateClient();
      Assert.Equal("i1", (await client.GetStyleRuleCustomInstructionAsync("s1", Instruction())).Id);
      await client.GetStyleRuleCustomInstructionAsync(Info(), Instruction());
      await client.UpdateStyleRuleCustomInstructionAsync("s1", Instruction(), "New", "Newer");
      await client.UpdateStyleRuleCustomInstructionAsync(Info(), Instruction(), "New", "Newer");
      await client.DeleteStyleRuleCustomInstructionAsync("s1", Instruction());
      await client.DeleteStyleRuleCustomInstructionAsync(Info(), Instruction());
    }

    [Fact]
    public async Task ThrowsArgumentExceptionForCustomInstructionWithoutId() =>
          await Assert.ThrowsAsync<ArgumentException>(
                () => CreateClient().GetStyleRuleCustomInstructionAsync("s1", new CustomInstruction("L", "P")));

    [Fact]
    public void ConvertsCustomInstructionToItsId() {
      string? id = Instruction();
      Assert.Equal("i1", id);
    }

    [Fact]
    public async Task SendsConfiguredRulesMapAsGiven() {
      const string rules = "{\"new_category\":{\"some_rule\":\"some_value\"},\"numbers\":{}}";
      ExpectJson(HttpMethod.Post, "v3/style_rules", "{\"name\":\"My rule\",\"language\":\"en\",\"configured_rules\":" + rules + "}")
            .RespondJson(RuleReply);
      ExpectJson(HttpMethod.Put, "v3/style_rules/s1/configured_rules", rules).RespondJson(RuleReply);
      var map = new Dictionary<string, Dictionary<string, string>> {
        ["new_category"] = new Dictionary<string, string> { ["some_rule"] = "some_value" },
        ["numbers"] = new Dictionary<string, string>()
      };
      var client = CreateClient();
      await client.CreateStyleRuleAsync("My rule", "en", map);
      await client.UpdateStyleRuleConfiguredRulesAsync("s1", map);
    }

    [Fact]
    public void ConvertsConfiguredRulesToTheMapOfSetCategories() {
      Dictionary<string, Dictionary<string, string>>? map = Rules();
      Assert.NotNull(map);
      var category = Assert.Single(map!);
      Assert.Equal("dates_and_times", category.Key);
      Assert.Equal("use_bc_and_ad", category.Value["calendar_era"]);
    }

    [Fact]
    public async Task SendsExtraBodyParametersLastWhenCreatingAndRenaming() {
      ExpectJson(
                  HttpMethod.Post,
                  "v3/style_rules",
                  "{\"name\":\"Override\",\"language\":\"en\",\"custom_instructions\":" +
                  "[{\"label\":\"L\",\"prompt\":\"P\"}],\"extra\":\"1\"}")
            .RespondJson(RuleReply);
      ExpectJson(Patch, "v3/style_rules/s1", "{\"name\":\"New\",\"extra\":\"1\"}").RespondJson(RuleReply);
      ExpectJson(Patch, "v3/style_rules/s1", "{\"name\":\"New\",\"extra\":\"1\"}").RespondJson(RuleReply);
      var client = CreateClient();
      await client.CreateStyleRuleAsync(
            "My rule",
            "en",
            null,
            new[] { new CustomInstruction("L", "P") },
            new CreateStyleRuleOptions {
              ExtraBodyParameters = new Dictionary<string, string> { ["name"] = "Override", ["extra"] = "1" }
            });
      await client.UpdateStyleRuleNameAsync("s1", "New", new UpdateStyleRuleNameOptions { ExtraBodyParameters = Extra() });
      await client.UpdateStyleRuleNameAsync(Info(), "New", new UpdateStyleRuleNameOptions { ExtraBodyParameters = Extra() });
    }

    [Fact]
    public async Task MergesExtraBodyParametersIntoConfiguredRulesMap() {
      const string body = "{\"numbers\":{\"a\":\"b\"},\"extra\":\"1\"}";
      ExpectJson(HttpMethod.Put, "v3/style_rules/s1/configured_rules", body).RespondJson(RuleReply);
      ExpectJson(HttpMethod.Put, "v3/style_rules/s1/configured_rules", body).RespondJson(RuleReply);
      var map = new Dictionary<string, Dictionary<string, string>> {
        ["numbers"] = new Dictionary<string, string> { ["a"] = "b" }
      };
      var options = new UpdateStyleRuleConfiguredRulesOptions { ExtraBodyParameters = Extra() };
      var client = CreateClient();
      await client.UpdateStyleRuleConfiguredRulesAsync("s1", map, options);
      await client.UpdateStyleRuleConfiguredRulesAsync(Info(), map, options);
      Assert.Single(map);
    }

    [Fact]
    public async Task SendsExtraBodyParametersLastWhenCreatingAndUpdatingCustomInstructions() {
      const string createBody = "{\"label\":\"L\",\"prompt\":\"P\",\"source_language\":\"de\",\"extra\":\"1\"}";
      const string updateBody = "{\"label\":\"L\",\"prompt\":\"P\",\"extra\":\"1\"}";
      const string path = "v3/style_rules/s1/custom_instructions";
      ExpectJson(HttpMethod.Post, path, createBody).RespondJson(InstructionReply);
      ExpectJson(HttpMethod.Post, path, createBody).RespondJson(InstructionReply);
      for (var i = 0; i < 4; i++) {
        ExpectJson(HttpMethod.Put, path + "/i1", updateBody).RespondJson(InstructionReply);
      }

      var createOptions = new CreateStyleRuleCustomInstructionOptions { ExtraBodyParameters = Extra() };
      var updateOptions = new UpdateStyleRuleCustomInstructionOptions { ExtraBodyParameters = Extra() };
      var client = CreateClient();
      await client.CreateStyleRuleCustomInstructionAsync("s1", "L", "P", "de", createOptions);
      await client.CreateStyleRuleCustomInstructionAsync(Info(), "L", "P", "de", createOptions);
      await client.UpdateStyleRuleCustomInstructionAsync("s1", "i1", "L", "P", null, updateOptions);
      await client.UpdateStyleRuleCustomInstructionAsync(Info(), "i1", "L", "P", null, updateOptions);
      await client.UpdateStyleRuleCustomInstructionAsync("s1", Instruction(), "L", "P", null, updateOptions);
      await client.UpdateStyleRuleCustomInstructionAsync(Info(), Instruction(), "L", "P", null, updateOptions);
    }

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
      Assert.Equal(new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc), rule.CreationTime.ToUniversalTime());
      Assert.Equal(new DateTime(2025, 1, 3, 3, 4, 5, DateTimeKind.Utc), rule.UpdatedTime.ToUniversalTime());
      Assert.Equal("use_bc_and_ad", rule.ConfiguredRules!.DatesAndTimes!["calendar_era"]);
      Assert.Null(rule.ConfiguredRules.Numbers);
      var instruction = Assert.Single(rule.CustomInstructions!);
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
#pragma warning disable CS0618 // The deprecated ConfiguredRules overloads stay covered by the existing tests.
      await CreateClient().CreateStyleRuleAsync(
            "My rule",
            "en",
            Rules(),
            new[] { new CustomInstruction("Tone", "Be formal", "de") });
#pragma warning restore CS0618
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
#pragma warning disable CS0618
      await CreateClient().UpdateStyleRuleConfiguredRulesAsync("s1", Rules());
#pragma warning restore CS0618
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
      await client.CreateStyleRuleCustomInstructionAsync("s1", "L", "P", "de");
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
      await client.GetStyleRuleCustomInstructionAsync("s1", "i1");
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
#pragma warning disable CS0618
      await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateStyleRuleConfiguredRulesAsync("", Rules()));
#pragma warning restore CS0618
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
#pragma warning disable CS0618
          await Assert.ThrowsAsync<ArgumentNullException>(
                () => CreateClient().UpdateStyleRuleConfiguredRulesAsync("s1", null!));
#pragma warning restore CS0618
  }
}
