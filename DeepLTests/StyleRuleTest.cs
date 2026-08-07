// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using Xunit;

namespace DeepLTests {
  public sealed class StyleRuleTest : BaseDeepLTest {
    private const string DefaultStyleId = "dca2e053-8ae5-45e6-a0d2-881156e7f4e4";

    [MockServerOnlyFact]
    public async Task TestGetAllStyleRules() {
      var client = CreateTestClient();
      var styleRules = await client.GetAllStyleRulesAsync(0, 10, true);
      Assert.NotNull(styleRules);
      Assert.True(styleRules.Length > 0);
      Assert.Equal(DefaultStyleId, styleRules[0].StyleId);
      Assert.Equal("Default Style Rule", styleRules[0].Name);
      Assert.Equal("en", styleRules[0].Language);
      Assert.Equal(1, styleRules[0].Version);
      Assert.NotNull(styleRules[0].ConfiguredRules);
      Assert.NotNull(styleRules[0].CustomInstructions);
    }

    [MockServerOnlyFact]
    public async Task TestGetAllStyleRulesWithoutDetailed() {
      var client = CreateTestClient();
      var styleRules = await client.GetAllStyleRulesAsync();

      Assert.NotNull(styleRules);
      Assert.True(styleRules.Length > 0);
      Assert.Equal(DefaultStyleId, styleRules[0].StyleId);
      Assert.Null(styleRules[0].ConfiguredRules);
      Assert.Null(styleRules[0].CustomInstructions);
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTextWithStyleId() {
      // Note: this test may use the mock server that will not translate the text,
      // therefore we do not check the translated result.
      var client = CreateTestClient();
      const string exampleText = "Hallo, Welt!";

      var result = await client.TranslateTextAsync(
            exampleText,
            "de",
            "en-US",
            new TextTranslateOptions { StyleId = DefaultStyleId });

      Assert.NotNull(result);
    }

    [Fact]
    public async Task TestStyleRuleCrud() {
      var client = CreateTestClient();

      // Create a style rule with configuredRules and customInstructions
      var configuredRules = new ConfiguredRules(
            datesAndTimes: new Dictionary<string, string> { { "calendar_era", "use_bc_and_ad" } });
      var customInstructions = new[] {
            new CustomInstruction("Tone", "Use formal tone")
      };
      var rule = await client.CreateStyleRuleAsync(
            "Test Rule", "en", configuredRules, customInstructions);
      Assert.NotNull(rule.StyleId);
      Assert.Equal("Test Rule", rule.Name);

      // Get the style rule
      var retrieved = await client.GetStyleRuleAsync(rule.StyleId);
      Assert.Equal(rule.StyleId, retrieved.StyleId);

      // Update the style rule name
      var updated = await client.UpdateStyleRuleNameAsync(rule.StyleId, "Updated");
      Assert.Equal("Updated", updated.Name);

      // Update configured rules
      var newConfiguredRules = new ConfiguredRules(
            datesAndTimes: new Dictionary<string, string> { { "calendar_era", "use_bc_and_ad" } });
      var configuredResult = await client.UpdateStyleRuleConfiguredRulesAsync(rule.StyleId, newConfiguredRules);
      Assert.Equal(rule.StyleId, configuredResult.StyleId);

      // Create a custom instruction with sourceLanguage
      var instruction = await client.CreateStyleRuleCustomInstructionAsync(
            rule.StyleId, "Label", "Prompt", "de");
      Assert.NotNull(instruction.Id);

      // Get the custom instruction
      var gotInstruction = await client.GetStyleRuleCustomInstructionAsync(
            rule.StyleId, instruction.Id!);
      Assert.Equal("Label", gotInstruction.Label);

      // Update the custom instruction with sourceLanguage
      var updatedInstruction = await client.UpdateStyleRuleCustomInstructionAsync(
            rule.StyleId, instruction.Id!, "New Label", "New Prompt", "fr");
      Assert.Equal("New Label", updatedInstruction.Label);

      // Delete the custom instruction and style rule
      await client.DeleteStyleRuleCustomInstructionAsync(rule.StyleId, instruction.Id!);
      await client.DeleteStyleRuleAsync(rule.StyleId);
    }

    [Fact]
    public async Task TestStyleRuleNotFound() {
      var client = CreateTestClient();
      var nonexistentId = "96ab91fd-e715-41a1-adeb-5d701f84a483";

      var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => client.GetStyleRuleAsync(nonexistentId));
      Assert.Contains("Style rule not found", ex.Message);

      ex = await Assert.ThrowsAsync<NotFoundException>(
            () => client.DeleteStyleRuleAsync(nonexistentId));
      Assert.Contains("Style rule not found", ex.Message);

      ex = await Assert.ThrowsAsync<NotFoundException>(
            () => client.UpdateStyleRuleNameAsync(nonexistentId, "New Name"));
      Assert.Contains("Style rule not found", ex.Message);
    }

    [Fact]
    public async Task TestStyleRuleValidation() {
      var client = CreateTestClient();

      // CreateStyleRuleAsync
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateStyleRuleAsync("", "en"));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateStyleRuleAsync("Test", ""));

      // GetStyleRuleAsync
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.GetStyleRuleAsync(""));

      // UpdateStyleRuleNameAsync
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.UpdateStyleRuleNameAsync("", "New Name"));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.UpdateStyleRuleNameAsync("some-id", ""));

      // DeleteStyleRuleAsync
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.DeleteStyleRuleAsync(""));

      // UpdateStyleRuleConfiguredRulesAsync
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.UpdateStyleRuleConfiguredRulesAsync(
                  "", new ConfiguredRules()));

      // CreateStyleRuleCustomInstructionAsync
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateStyleRuleCustomInstructionAsync("", "L", "P"));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateStyleRuleCustomInstructionAsync("some-id", "", "P"));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateStyleRuleCustomInstructionAsync("some-id", "L", ""));

      // GetStyleRuleCustomInstructionAsync
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.GetStyleRuleCustomInstructionAsync("", "instr-id"));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.GetStyleRuleCustomInstructionAsync("some-id", ""));

      // UpdateStyleRuleCustomInstructionAsync
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.UpdateStyleRuleCustomInstructionAsync("", "instr-id", "L", "P"));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.UpdateStyleRuleCustomInstructionAsync("some-id", "", "L", "P"));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.UpdateStyleRuleCustomInstructionAsync("some-id", "instr-id", "", "P"));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.UpdateStyleRuleCustomInstructionAsync("some-id", "instr-id", "L", ""));

      // DeleteStyleRuleCustomInstructionAsync
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.DeleteStyleRuleCustomInstructionAsync("", "instr-id"));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.DeleteStyleRuleCustomInstructionAsync("some-id", ""));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTextWithStyleRuleInfo() {
      var client = CreateTestClient();
      var styleRules = await client.GetAllStyleRulesAsync();
      var styleRule = styleRules[0];
      const string exampleText = "Hallo, Welt!";

      var result = await client.TranslateTextAsync(
            exampleText,
            "de",
            "en-US",
            new TextTranslateOptions(styleRule));

      Assert.NotNull(result);
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithStyleId() {
      // Note: this test may use the mock server that will not translate the text,
      // therefore we do not check the translated result.
      var translator = CreateTestTranslator();
      var tempDir = TempDir();
      var inputFilePath = Path.Combine(tempDir, "example_document.txt");
      var inputFileInfo = new FileInfo(inputFilePath);
      File.Delete(inputFilePath);
      File.WriteAllText(inputFilePath, "Hallo, Welt!");
      var outputFilePath = Path.Combine(tempDir, "output_document.txt");
      var outputFileInfo = new FileInfo(outputFilePath);
      File.Delete(outputFilePath);

      await translator.TranslateDocumentAsync(
            inputFileInfo,
            outputFileInfo,
            "de",
            "en-US",
            new DocumentTranslateOptions { StyleId = DefaultStyleId });

      Assert.True(File.Exists(outputFilePath));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithStyleRuleInfo() {
      var client = CreateTestClient();
      var styleRules = await client.GetAllStyleRulesAsync();
      var styleRule = styleRules[0];
      var tempDir = TempDir();
      var inputFilePath = Path.Combine(tempDir, "example_document.txt");
      var inputFileInfo = new FileInfo(inputFilePath);
      File.Delete(inputFilePath);
      File.WriteAllText(inputFilePath, "Hallo, Welt!");
      var outputFilePath = Path.Combine(tempDir, "output_document.txt");
      var outputFileInfo = new FileInfo(outputFilePath);
      File.Delete(outputFilePath);

      await client.TranslateDocumentAsync(
            inputFileInfo,
            outputFileInfo,
            "de",
            "en-US",
            new DocumentTranslateOptions(styleRule));

      Assert.True(File.Exists(outputFilePath));
    }
  }
}
