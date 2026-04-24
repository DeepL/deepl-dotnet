// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Fluent entry points for style-rule management on <see cref="IStyleRuleManager" />.
  /// </summary>
  /// <example>
  ///   <code>
  ///     var rule = await client
  ///       .CreateStyleRule("Marketing")
  ///       .ForLanguage("en")
  ///       .WithConfiguredRules(rules)
  ///       .WithInstruction("Friendly",  "Be playful")
  ///       .WithInstruction("No jargon", "Avoid buzzwords")
  ///       .CreateAsync();
  ///
  ///     var rules = await client.ListStyleRulesAsync();
  ///
  ///     await client.StyleRule(id).RenameAsync("Marketing v2");
  ///     await client.StyleRule(id).AddInstructionAsync("label", "prompt");
  ///     await client.StyleRule(id).DeleteAsync();
  ///   </code>
  /// </example>
  public static class FluentStyleRuleExtensions {
    /// <summary>Lists style rules (page / pageSize optional, detailed toggles full configured-rules payload).</summary>
    public static Task<StyleRuleInfo[]> ListStyleRulesAsync(
          this IStyleRuleManager manager,
          int? page = null,
          int? pageSize = null,
          bool? detailed = null,
          CancellationToken cancellationToken = default) {
      if (manager == null) throw new ArgumentNullException(nameof(manager));
      return manager.GetAllStyleRulesAsync(page, pageSize, detailed, cancellationToken);
    }

    /// <summary>Returns a fluent reference to the style rule with the given ID.</summary>
    public static StyleRuleRef StyleRule(this IStyleRuleManager manager, string styleId) {
      if (manager == null) throw new ArgumentNullException(nameof(manager));
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty", nameof(styleId));
      }

      return new StyleRuleRef(manager, styleId);
    }

    /// <summary>Returns a fluent reference for the supplied style rule.</summary>
    public static StyleRuleRef StyleRule(this IStyleRuleManager manager, StyleRuleInfo styleRule) {
      if (manager == null) throw new ArgumentNullException(nameof(manager));
      if (styleRule == null) throw new ArgumentNullException(nameof(styleRule));
      return new StyleRuleRef(manager, styleRule.StyleId);
    }

    /// <summary>Begins a fluent style-rule-creation builder.</summary>
    public static StyleRuleCreateBuilder CreateStyleRule(this IStyleRuleManager manager, string name) {
      if (manager == null) throw new ArgumentNullException(nameof(manager));
      if (string.IsNullOrWhiteSpace(name)) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty", nameof(name));
      }

      return new StyleRuleCreateBuilder(manager, name);
    }
  }

  /// <summary>Fluent reference for an existing style rule.</summary>
  public sealed class StyleRuleRef {
    private readonly IStyleRuleManager _manager;

    internal StyleRuleRef(IStyleRuleManager manager, string styleId) {
      _manager = manager;
      StyleId = styleId;
    }

    public string StyleId { get; }

    /// <summary>Retrieves the style rule.</summary>
    public Task<StyleRuleInfo> GetAsync(CancellationToken cancellationToken = default) =>
          _manager.GetStyleRuleAsync(StyleId, cancellationToken);

    /// <summary>Renames the style rule.</summary>
    public Task<StyleRuleInfo> RenameAsync(string name, CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(name)) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty", nameof(name));
      }

      return _manager.UpdateStyleRuleNameAsync(StyleId, name, cancellationToken);
    }

    /// <summary>Deletes the style rule.</summary>
    public Task DeleteAsync(CancellationToken cancellationToken = default) =>
          _manager.DeleteStyleRuleAsync(StyleId, cancellationToken);

    /// <summary>Replaces the configured rules of this style rule.</summary>
    public Task<StyleRuleInfo> SetConfiguredRulesAsync(
          ConfiguredRules configuredRules,
          CancellationToken cancellationToken = default) {
      if (configuredRules == null) throw new ArgumentNullException(nameof(configuredRules));
      return _manager.UpdateStyleRuleConfiguredRulesAsync(StyleId, configuredRules, cancellationToken);
    }

    /// <summary>Adds a custom instruction to this style rule.</summary>
    public Task<CustomInstruction> AddInstructionAsync(
          string label,
          string prompt,
          string? sourceLanguage = null,
          CancellationToken cancellationToken = default) =>
          _manager.CreateStyleRuleCustomInstructionAsync(StyleId, label, prompt, sourceLanguage, cancellationToken);

    /// <summary>Returns a fluent reference to a custom instruction on this style rule.</summary>
    public CustomInstructionRef Instruction(string instructionId) {
      if (string.IsNullOrWhiteSpace(instructionId)) {
        throw new ArgumentException(
              $"Parameter {nameof(instructionId)} must not be empty", nameof(instructionId));
      }

      return new CustomInstructionRef(_manager, StyleId, instructionId);
    }

    /// <summary>Returns a fluent reference to a custom instruction on this style rule.</summary>
    public CustomInstructionRef Instruction(CustomInstruction instruction) {
      if (instruction == null) throw new ArgumentNullException(nameof(instruction));
      if (string.IsNullOrEmpty(instruction.Id)) {
        throw new ArgumentException(
              "The supplied instruction has no ID (was it deserialized from a create response?)", nameof(instruction));
      }

      return new CustomInstructionRef(_manager, StyleId, instruction.Id!);
    }
  }

  /// <summary>Fluent reference for a single custom instruction inside a style rule.</summary>
  public sealed class CustomInstructionRef {
    private readonly IStyleRuleManager _manager;

    internal CustomInstructionRef(IStyleRuleManager manager, string styleId, string instructionId) {
      _manager = manager;
      StyleId = styleId;
      InstructionId = instructionId;
    }

    public string StyleId { get; }
    public string InstructionId { get; }

    /// <summary>Retrieves the custom instruction.</summary>
    public Task<CustomInstruction> GetAsync(CancellationToken cancellationToken = default) =>
          _manager.GetStyleRuleCustomInstructionAsync(StyleId, InstructionId, cancellationToken);

    /// <summary>Replaces the custom instruction with the given label/prompt.</summary>
    public Task<CustomInstruction> UpdateAsync(
          string label,
          string prompt,
          string? sourceLanguage = null,
          CancellationToken cancellationToken = default) =>
          _manager.UpdateStyleRuleCustomInstructionAsync(
                StyleId,
                InstructionId,
                label,
                prompt,
                sourceLanguage,
                cancellationToken);

    /// <summary>Deletes the custom instruction.</summary>
    public Task DeleteAsync(CancellationToken cancellationToken = default) =>
          _manager.DeleteStyleRuleCustomInstructionAsync(StyleId, InstructionId, cancellationToken);
  }

  /// <summary>Fluent builder for creating a new style rule.</summary>
  public sealed class StyleRuleCreateBuilder {
    private readonly IStyleRuleManager _manager;
    private readonly string _name;
    private readonly List<CustomInstruction> _instructions = new List<CustomInstruction>();
    private string? _language;
    private ConfiguredRules? _configuredRules;
    private CancellationToken _cancellationToken;

    internal StyleRuleCreateBuilder(IStyleRuleManager manager, string name) {
      _manager = manager;
      _name = name;
    }

    /// <summary>Sets the language code for the style rule (required).</summary>
    public StyleRuleCreateBuilder ForLanguage(string language) {
      if (string.IsNullOrWhiteSpace(language)) {
        throw new ArgumentException($"Parameter {nameof(language)} must not be empty", nameof(language));
      }

      _language = language;
      return this;
    }

    /// <summary>Supplies configured rules for the style rule.</summary>
    public StyleRuleCreateBuilder WithConfiguredRules(ConfiguredRules configuredRules) {
      _configuredRules = configuredRules ?? throw new ArgumentNullException(nameof(configuredRules));
      return this;
    }

    /// <summary>Adds a custom instruction to the style rule being created.</summary>
    public StyleRuleCreateBuilder WithInstruction(
          string label, string prompt, string? sourceLanguage = null) {
      if (string.IsNullOrWhiteSpace(label)) {
        throw new ArgumentException($"Parameter {nameof(label)} must not be empty", nameof(label));
      }

      if (string.IsNullOrWhiteSpace(prompt)) {
        throw new ArgumentException($"Parameter {nameof(prompt)} must not be empty", nameof(prompt));
      }

      _instructions.Add(new CustomInstruction(label, prompt, sourceLanguage));
      return this;
    }

    /// <summary>Adds a prepared custom instruction to the style rule being created.</summary>
    public StyleRuleCreateBuilder WithInstruction(CustomInstruction instruction) {
      if (instruction == null) throw new ArgumentNullException(nameof(instruction));
      _instructions.Add(instruction);
      return this;
    }

    /// <summary>Associates a cancellation token with the create request.</summary>
    public StyleRuleCreateBuilder WithCancellation(CancellationToken cancellationToken) {
      _cancellationToken = cancellationToken;
      return this;
    }

    /// <summary>Executes the style-rule creation request.</summary>
    public Task<StyleRuleInfo> CreateAsync() {
      if (_language == null) {
        throw new InvalidOperationException(
              "Language is required. Call .ForLanguage(languageCode) before awaiting.");
      }

      return _manager.CreateStyleRuleAsync(
            _name,
            _language,
            _configuredRules,
            _instructions.Count > 0 ? _instructions.ToArray() : null,
            _cancellationToken);
    }

    /// <summary>Enables direct <c>await</c> on the builder.</summary>
    public TaskAwaiter<StyleRuleInfo> GetAwaiter() => CreateAsync().GetAwaiter();

    public static implicit operator Task<StyleRuleInfo>(StyleRuleCreateBuilder builder) =>
          builder?.CreateAsync() ?? throw new ArgumentNullException(nameof(builder));
  }
}
