// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using DeepL.Internal;

namespace DeepL.Model {
  /// <summary>
  ///   Information about a style profile: a named set of glossaries, style rule lists and translation memories.
  /// </summary>
  public sealed class StyleProfile {
    /// <summary>Initializes a new <see cref="StyleProfile" /> containing information about a style profile.</summary>
    /// <param name="styleProfileId">ID of the style profile.</param>
    /// <param name="name">Name of the style profile.</param>
    /// <param name="description">Description of the style profile, or <c>null</c> if it has none.</param>
    /// <param name="industry">Industry of the style profile, or <c>null</c> if it has none.</param>
    /// <param name="glossaryIds">IDs of the linked glossaries, highest priority first.</param>
    /// <param name="styleRuleIds">IDs of the linked style rule lists.</param>
    /// <param name="translationMemoryIds">IDs of the linked translation memories.</param>
    /// <param name="glossaryPriorityOrderEnabled">Whether the order of the glossary IDs sets their priority.</param>
    /// <param name="creationTime"><see cref="DateTime" /> when the style profile was created.</param>
    /// <param name="updatedTime"><see cref="DateTime" /> when the style profile was last updated.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public StyleProfile(
          string styleProfileId,
          string name,
          string? description,
          string? industry,
          string[]? glossaryIds,
          string[]? styleRuleIds,
          string[]? translationMemoryIds,
          bool glossaryPriorityOrderEnabled,
          DateTime? creationTime,
          DateTime? updatedTime) {
      (StyleProfileId, Name, Description, Industry) = (styleProfileId, name, description, industry);
      GlossaryIds = glossaryIds ?? new string[0];
      StyleRuleIds = styleRuleIds ?? new string[0];
      TranslationMemoryIds = translationMemoryIds ?? new string[0];
      GlossaryPriorityOrderEnabled = glossaryPriorityOrderEnabled;
      (CreationTime, UpdatedTime) = (creationTime, updatedTime);
    }

    /// <summary>ID of the style profile.</summary>
    [JsonPropertyName("style_profile_id")]
    public string StyleProfileId { get; }

    /// <summary>Name of the style profile.</summary>
    [JsonPropertyName("name")]
    public string Name { get; }

    /// <summary>Description of the style profile, or <c>null</c> if it has none.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; }

    /// <summary>The industry the style profile is written for, or <c>null</c> if it has none.</summary>
    [JsonPropertyName("industry")]
    public string? Industry { get; }

    /// <summary>
    ///   IDs of the glossaries linked to the style profile, highest priority first; empty if none are linked. The order
    ///   only takes effect if <see cref="GlossaryPriorityOrderEnabled" /> is <c>true</c>.
    /// </summary>
    [JsonPropertyName("glossary_ids")]
    [JsonConverter(typeof(LenientStringArrayConverter))]
    public string[] GlossaryIds { get; }

    /// <summary>IDs of the style rule lists linked to the style profile; empty if none are linked.</summary>
    [JsonPropertyName("style_rule_ids")]
    [JsonConverter(typeof(LenientStringArrayConverter))]
    public string[] StyleRuleIds { get; }

    /// <summary>IDs of the translation memories linked to the style profile; empty if none are linked.</summary>
    [JsonPropertyName("translation_memory_ids")]
    [JsonConverter(typeof(LenientStringArrayConverter))]
    public string[] TranslationMemoryIds { get; }

    /// <summary>
    ///   Whether the order of <see cref="GlossaryIds" /> sets priority: when two linked glossaries define the same source
    ///   term, the one listed first wins.
    /// </summary>
    [JsonPropertyName("glossary_priority_order_enabled")]
    public bool GlossaryPriorityOrderEnabled { get; }

    /// <summary><see cref="DateTime" /> when the style profile was created, or <c>null</c> if it could not be read.</summary>
    [JsonPropertyName("creation_time")]
    [JsonConverter(typeof(LenientNullableDateTimeConverter))]
    public DateTime? CreationTime { get; }

    /// <summary>
    ///   <see cref="DateTime" /> when the style profile was last updated, or <c>null</c> if it could not be read.
    /// </summary>
    [JsonPropertyName("updated_time")]
    [JsonConverter(typeof(LenientNullableDateTimeConverter))]
    public DateTime? UpdatedTime { get; }

    /// <summary>Creates a string containing the name and ID of the current <see cref="StyleProfile" /> object.</summary>
    /// <returns>A string containing the name and ID of the <see cref="StyleProfile" /> object.</returns>
    /// <remarks>
    ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
    ///   compatibility.
    /// </remarks>
    public override string ToString() => $"Style profile \"{Name}\" ({StyleProfileId})";

    /// <summary>
    ///   Implicitly converts the <see cref="StyleProfile" /> to its ID, so it can be passed wherever the ID is accepted, for
    ///   example to <see cref="TextTranslateOptions" /> properties.
    /// </summary>
    /// <param name="styleProfile"><see cref="StyleProfile" /> object to convert.</param>
    /// <returns>The value of <see cref="StyleProfileId" />, or <c>null</c> if <paramref name="styleProfile" /> is <c>null</c>.</returns>
    [return: NotNullIfNotNull("styleProfile")]
    public static implicit operator string?(StyleProfile? styleProfile) => styleProfile?.StyleProfileId;
  }
}
