// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>Configuration rules for a style rule list.</summary>
  public sealed class ConfiguredRules {
    /// <summary>Initializes a new instance of <see cref="ConfiguredRules" />.</summary>
    [JsonConstructor]
    public ConfiguredRules(
          Dictionary<string, string>? datesAndTimes = null,
          Dictionary<string, string>? formatting = null,
          Dictionary<string, string>? numbers = null,
          Dictionary<string, string>? punctuation = null,
          Dictionary<string, string>? spellingAndGrammar = null,
          Dictionary<string, string>? styleAndTone = null,
          Dictionary<string, string>? vocabulary = null) {
      DatesAndTimes = datesAndTimes;
      Formatting = formatting;
      Numbers = numbers;
      Punctuation = punctuation;
      SpellingAndGrammar = spellingAndGrammar;
      StyleAndTone = styleAndTone;
      Vocabulary = vocabulary;
    }

    /// <summary>Date and time formatting rules.</summary>
    [JsonPropertyName("dates_and_times")]
    public Dictionary<string, string>? DatesAndTimes { get; }

    /// <summary>Text formatting rules.</summary>
    [JsonPropertyName("formatting")]
    public Dictionary<string, string>? Formatting { get; }

    /// <summary>Number formatting rules.</summary>
    [JsonPropertyName("numbers")]
    public Dictionary<string, string>? Numbers { get; }

    /// <summary>Punctuation rules.</summary>
    [JsonPropertyName("punctuation")]
    public Dictionary<string, string>? Punctuation { get; }

    /// <summary>Spelling and grammar rules.</summary>
    [JsonPropertyName("spelling_and_grammar")]
    public Dictionary<string, string>? SpellingAndGrammar { get; }

    /// <summary>Style and tone rules.</summary>
    [JsonPropertyName("style_and_tone")]
    public Dictionary<string, string>? StyleAndTone { get; }

    /// <summary>Vocabulary rules.</summary>
    [JsonPropertyName("vocabulary")]
    public Dictionary<string, string>? Vocabulary { get; }
  }

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
          ConfiguredRules? configuredRules,
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

    /// <summary>The predefined rules that have been enabled.</summary>
    [JsonPropertyName("configured_rules")]
    public ConfiguredRules? ConfiguredRules { get; }

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
    /// <returns>The value of <see cref="StyleId" />.</returns>
    public static implicit operator string(StyleRuleInfo styleRule) => styleRule.StyleId;
  }
}
