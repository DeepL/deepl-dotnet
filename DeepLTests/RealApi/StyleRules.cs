// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeepL.Model;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class StyleRules {
    [Fact]
    public async Task SendsTheStyleRuleRequestsCreateGetListRenameConfigureInstructionsAndDelete() {
      var client = RealApiClient.CreateClient();
      var name = RealApiHelpers.GetUniqueName("style-rule");
      var rules = new ConfiguredRules(
            datesAndTimes: new Dictionary<string, string> { { "calendar_era", "use_bc_and_ad" } });

      var styleRule = await client.CreateStyleRuleAsync(
            name,
            "en",
            rules,
            new[] { new CustomInstruction("Init", "Be formal", "de") });
      try {
        Assert.NotEmpty(styleRule.StyleId);
        Assert.Equal(name, styleRule.Name);
        Assert.Equal("en", styleRule.Language);

        var fetched = await client.GetStyleRuleAsync(styleRule.StyleId);
        Assert.Equal(styleRule.StyleId, fetched.StyleId);
        Assert.Equal(name, fetched.Name);

        var listed = false;
        for (var page = 0; !listed && page < 20; page++) {
          var styleRules = await client.GetAllStyleRulesAsync(page, 100, true);
          if (styleRules.Length == 0) {
            break;
          }

          listed = styleRules.Any(rule => rule.StyleId == styleRule.StyleId);
        }

        Assert.True(listed);

        var renamed = await client.UpdateStyleRuleNameAsync(styleRule.StyleId, name + "-renamed");
        Assert.Equal(name + "-renamed", renamed.Name);

        var configured = await client.UpdateStyleRuleConfiguredRulesAsync(styleRule.StyleId, rules);
        Assert.Equal(styleRule.StyleId, configured.StyleId);

        var instruction = await client.CreateStyleRuleCustomInstructionAsync(styleRule.StyleId, "Label", "Prompt", "de");
        Assert.NotEmpty(instruction.Id!);
        Assert.Equal("Label", instruction.Label);

        var fetchedInstruction = await client.GetStyleRuleCustomInstructionAsync(styleRule.StyleId, instruction.Id!);
        Assert.Equal("Prompt", fetchedInstruction.Prompt);

        var updatedInstruction = await client.UpdateStyleRuleCustomInstructionAsync(
              styleRule.StyleId,
              instruction.Id!,
              "New Label",
              "New Prompt",
              "en");
        Assert.Equal("New Label", updatedInstruction.Label);

        await client.DeleteStyleRuleCustomInstructionAsync(styleRule.StyleId, instruction.Id!);
      } finally {
        await client.DeleteStyleRuleAsync(styleRule.StyleId);
      }
    }
  }
}
