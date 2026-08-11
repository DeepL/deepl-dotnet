// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>Information about a translation memory.</summary>
  public sealed class TranslationMemoryInfo {
    /// <summary>Initializes a new instance of <see cref="TranslationMemoryInfo" />.</summary>
    [JsonConstructor]
    public TranslationMemoryInfo(
          string translationMemoryId,
          string name,
          string sourceLanguage,
          string[] targetLanguages,
          int segmentCount,
          DateTime? creationTime = null,
          DateTime? updatedTime = null) {
      TranslationMemoryId = translationMemoryId;
      Name = name;
      SourceLanguage = sourceLanguage;
      TargetLanguages = targetLanguages;
      SegmentCount = segmentCount;
      CreationTime = creationTime;
      UpdatedTime = updatedTime;
    }

    /// <summary>Unique ID assigned to the translation memory.</summary>
    [JsonPropertyName("translation_memory_id")]
    public string TranslationMemoryId { get; }

    /// <summary>User-defined name assigned to the translation memory.</summary>
    [JsonPropertyName("name")]
    public string Name { get; }

    /// <summary>Source language code for the translation memory.</summary>
    [JsonPropertyName("source_language")]
    public string SourceLanguage { get; }

    /// <summary>Target language codes for the translation memory.</summary>
    [JsonPropertyName("target_languages")]
    public string[] TargetLanguages { get; }

    /// <summary>Number of segments in the translation memory.</summary>
    [JsonPropertyName("segment_count")]
    public int SegmentCount { get; }

    /// <summary>Time when the translation memory was created, if provided by the API.</summary>
    [JsonPropertyName("creation_time")]
    public DateTime? CreationTime { get; }

    /// <summary>Time when the translation memory was last updated, if provided by the API.</summary>
    [JsonPropertyName("updated_time")]
    public DateTime? UpdatedTime { get; }

    /// <summary>Returns a string describing the translation memory.</summary>
    public override string ToString() => $"TranslationMemory \"{Name}\" ({TranslationMemoryId})";
  }
}
