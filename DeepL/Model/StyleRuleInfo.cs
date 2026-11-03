// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>Custom instruction for a style rule.</summary>
  public sealed class CustomInstruction {
    /// <summary>Initializes a new instance of <see cref="CustomInstruction" />.</summary>
    [JsonConstructor]
    public CustomInstruction(string label, string prompt, string? sourceLanguage = null, string? id = null) {
      Id = id;
      Label = label;
      Prompt = prompt;
      SourceLanguage = sourceLanguage;
    }

    /// <summary>Unique ID assigned to the custom instruction.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; }

    /// <summary>Label for the custom instruction.</summary>
    [JsonPropertyName("label")]
    public string Label { get; }

    /// <summary>Prompt text for the custom instruction.</summary>
    [JsonPropertyName("prompt")]
    public string Prompt { get; }

    /// <summary>Optional source language code for the custom instruction.</summary>
    [JsonPropertyName("source_language")]
    public string? SourceLanguage { get; }

    /// <summary>
    ///   Implicitly converts the <see cref="CustomInstruction" /> to its ID, so it can be passed wherever the
    ///   instruction ID is accepted.
    /// </summary>
    /// <param name="customInstruction"><see cref="CustomInstruction" /> object to convert.</param>
    /// <returns>The value of <see cref="Id" />, or <c>null</c> if <paramref name="customInstruction" /> is <c>null</c>.</returns>
    public static implicit operator string?(CustomInstruction? customInstruction) => customInstruction?.Id;
  }

  /// <summary>Information about a style rule list.</summary>
  public sealed class StyleRuleInfo {
    /// <summary>Initializes a new instance of <see cref="StyleRuleInfo" />.</summary>
    [JsonConstructor]
    public StyleRuleInfo(
          string styleId,
          string name,
          DateTime creationTime,
          DateTime updatedTime,
          string language,
          int version,
          Dictionary<string, Dictionary<string, string>>? configuredRules,
          CustomInstruction[]? customInstructions) {
      StyleId = styleId;
      Name = name;
      CreationTime = creationTime;
      UpdatedTime = updatedTime;
      Language = language;
      Version = version;
      ConfiguredRules = configuredRules;
      CustomInstructions = customInstructions;
    }

    /// <summary>Unique ID assigned to the style rule list.</summary>
    [JsonPropertyName("style_id")]
    public string StyleId { get; }

    /// <summary>User-defined name assigned to the style rule list.</summary>
    [JsonPropertyName("name")]
    public string Name { get; }

    /// <summary>Time when the style rule list was created.</summary>
    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; }

    /// <summary>Time when the style rule list was last updated.</summary>
    [JsonPropertyName("updated_time")]
    public DateTime UpdatedTime { get; }

    /// <summary>Language code for the style rule list.</summary>
    [JsonPropertyName("language")]
    public string Language { get; }

    /// <summary>Version number of the style rule list.</summary>
    [JsonPropertyName("version")]
    public int Version { get; }

    /// <summary>
    ///   The predefined rules that have been enabled, as the API returns them: category to rule to value, keyed by the
    ///   API names (for example <c>dates_and_times</c>), the same shape the create and update methods take.
    /// </summary>
    [JsonPropertyName("configured_rules")]
    public Dictionary<string, Dictionary<string, string>>? ConfiguredRules { get; }

    /// <summary>Optional list of custom instructions.</summary>
    [JsonPropertyName("custom_instructions")]
    public CustomInstruction[]? CustomInstructions { get; }

    /// <summary>Returns a string describing the style rule.</summary>
    public override string ToString() => $"StyleRule \"{Name}\" ({StyleId})";

    /// <summary>
    ///   Implicitly converts the <see cref="StyleRuleInfo" /> to its ID, so it can be passed wherever the ID is accepted, for
    ///   example to <see cref="TextTranslateOptions" /> properties.
    /// </summary>
    /// <param name="styleRule"><see cref="StyleRuleInfo" /> object to convert.</param>
    /// <returns>The value of <see cref="StyleId" />, or <c>null</c> if <paramref name="styleRule" /> is <c>null</c>.</returns>
    [return: NotNullIfNotNull("styleRule")]
    public static implicit operator string?(StyleRuleInfo? styleRule) => styleRule?.StyleId;
  }
}
