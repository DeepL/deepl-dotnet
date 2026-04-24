// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Threading;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using NSubstitute;
using Xunit;

namespace DeepLTests {
  /// <summary>
  ///   Unit tests for the fluent style-rule-management layer in <c>FluentStyleRule.cs</c>.
  /// </summary>
  public sealed class FluentStyleRuleTest {
    private const string StyleId = "style-abc";
    private const string InstructionId = "instr-123";

    private static StyleRuleInfo MakeStyleRule(string id = StyleId, string name = "test") =>
          new StyleRuleInfo(id, name, DateTime.UtcNow, DateTime.UtcNow, "en", 1, null, null);

    private static CustomInstruction MakeInstruction(string id = InstructionId) =>
          new CustomInstruction("label", "prompt", "en", id);

    // ---------- List ----------

    [Fact]
    public async Task ListStyleRulesAsync_ForwardsPagingArgs() {
      var manager = Substitute.For<IStyleRuleManager>();
      var expected = new[] { MakeStyleRule() };
      manager.GetAllStyleRulesAsync(2, 50, true, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.ListStyleRulesAsync(page: 2, pageSize: 50, detailed: true);

      Assert.Same(expected, result);
    }

    // ---------- Ref: Get / Rename / Delete ----------

    [Fact]
    public async Task StyleRuleRef_GetAsync_CallsWithCorrectId() {
      var manager = Substitute.For<IStyleRuleManager>();
      var expected = MakeStyleRule();
      manager.GetStyleRuleAsync(StyleId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.StyleRule(StyleId).GetAsync();

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task StyleRuleRef_RenameAsync_CallsUpdateName() {
      var manager = Substitute.For<IStyleRuleManager>();
      var expected = MakeStyleRule(name: "new");
      manager.UpdateStyleRuleNameAsync(StyleId, "new", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.StyleRule(StyleId).RenameAsync("new");

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task StyleRuleRef_DeleteAsync_Forwards() {
      var manager = Substitute.For<IStyleRuleManager>();

      await manager.StyleRule(StyleId).DeleteAsync();

      await manager.Received(1).DeleteStyleRuleAsync(StyleId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StyleRuleRef_SetConfiguredRulesAsync_Forwards() {
      var manager = Substitute.For<IStyleRuleManager>();
      var rules = new ConfiguredRules();
      var expected = MakeStyleRule();
      manager.UpdateStyleRuleConfiguredRulesAsync(StyleId, rules, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.StyleRule(StyleId).SetConfiguredRulesAsync(rules);

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task StyleRuleRef_AddInstructionAsync_Forwards() {
      var manager = Substitute.For<IStyleRuleManager>();
      var expected = MakeInstruction();
      manager.CreateStyleRuleCustomInstructionAsync(
                  StyleId, "label", "prompt", "en", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.StyleRule(StyleId).AddInstructionAsync("label", "prompt", "en");

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task StyleRuleRef_FromInfo_UsesItsId() {
      var manager = Substitute.For<IStyleRuleManager>();
      var info = MakeStyleRule("real");
      manager.GetStyleRuleAsync("real", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(info));

      await manager.StyleRule(info).GetAsync();

      await manager.Received(1).GetStyleRuleAsync("real", Arg.Any<CancellationToken>());
    }

    // ---------- CustomInstructionRef ----------

    [Fact]
    public async Task InstructionRef_GetAsync_Forwards() {
      var manager = Substitute.For<IStyleRuleManager>();
      var expected = MakeInstruction();
      manager.GetStyleRuleCustomInstructionAsync(StyleId, InstructionId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.StyleRule(StyleId).Instruction(InstructionId).GetAsync();

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task InstructionRef_UpdateAsync_Forwards() {
      var manager = Substitute.For<IStyleRuleManager>();
      var expected = MakeInstruction();
      manager.UpdateStyleRuleCustomInstructionAsync(
                  StyleId, InstructionId, "new-label", "new-prompt", null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.StyleRule(StyleId).Instruction(InstructionId)
            .UpdateAsync("new-label", "new-prompt");

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task InstructionRef_DeleteAsync_Forwards() {
      var manager = Substitute.For<IStyleRuleManager>();

      await manager.StyleRule(StyleId).Instruction(InstructionId).DeleteAsync();

      await manager.Received(1).DeleteStyleRuleCustomInstructionAsync(
            StyleId, InstructionId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void InstructionRef_FromInstance_RequiresId() {
      var manager = Substitute.For<IStyleRuleManager>();
      // An instruction with null ID (as returned before save) must be rejected
      var instrWithoutId = new CustomInstruction("l", "p", null, null);

      var styleRef = manager.StyleRule(StyleId);
      Assert.Throws<ArgumentException>(() => { _ = styleRef.Instruction(instrWithoutId); });
    }

    // ---------- Creation builder ----------

    [Fact]
    public async Task CreateStyleRule_Full_ForwardsAllFields() {
      var manager = Substitute.For<IStyleRuleManager>();
      var expected = MakeStyleRule();
      var rules = new ConfiguredRules();
      CustomInstruction[]? capturedInstructions = null;
      manager.CreateStyleRuleAsync(
                  "Marketing",
                  "en",
                  rules,
                  Arg.Do<CustomInstruction[]?>(xs => capturedInstructions = xs),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.CreateStyleRule("Marketing")
            .ForLanguage("en")
            .WithConfiguredRules(rules)
            .WithInstruction("Friendly", "Be playful")
            .WithInstruction("Short", "Keep it brief", "en");

      Assert.Same(expected, result);
      Assert.NotNull(capturedInstructions);
      Assert.Equal(2, capturedInstructions!.Length);
      Assert.Equal("Friendly", capturedInstructions[0].Label);
      Assert.Equal("Short", capturedInstructions[1].Label);
      Assert.Equal("en", capturedInstructions[1].SourceLanguage);
    }

    [Fact]
    public async Task CreateStyleRule_NoInstructions_PassesNull() {
      var manager = Substitute.For<IStyleRuleManager>();
      var expected = MakeStyleRule();
      manager.CreateStyleRuleAsync(
                  "Marketing", "en", null, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.CreateStyleRule("Marketing").ForLanguage("en");

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task CreateStyleRule_WithoutLanguage_Throws() {
      var manager = Substitute.For<IStyleRuleManager>();

      await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await manager.CreateStyleRule("Marketing"));
    }

    [Fact]
    public void CreateStyleRule_EmptyName_Throws() {
      var manager = Substitute.For<IStyleRuleManager>();
      Assert.Throws<ArgumentException>(() => { _ = manager.CreateStyleRule(""); });
    }

    [Fact]
    public void CreateStyleRule_EmptyLanguage_Throws() {
      var manager = Substitute.For<IStyleRuleManager>();
      var builder = manager.CreateStyleRule("x");
      Assert.Throws<ArgumentException>(() => { _ = builder.ForLanguage(""); });
    }

    [Fact]
    public void CreateStyleRule_EmptyInstruction_Throws() {
      var manager = Substitute.For<IStyleRuleManager>();
      var builder = manager.CreateStyleRule("x");
      Assert.Throws<ArgumentException>(() => { _ = builder.WithInstruction("", "p"); });
      Assert.Throws<ArgumentException>(() => { _ = builder.WithInstruction("l", ""); });
    }

    [Fact]
    public void StyleRule_EmptyId_Throws() {
      var manager = Substitute.For<IStyleRuleManager>();
      Assert.Throws<ArgumentException>(() => { _ = manager.StyleRule(""); });
    }

    [Fact]
    public void Instruction_EmptyId_Throws() {
      var manager = Substitute.For<IStyleRuleManager>();
      var styleRef = manager.StyleRule(StyleId);
      Assert.Throws<ArgumentException>(() => { _ = styleRef.Instruction(""); });
    }
  }
}
