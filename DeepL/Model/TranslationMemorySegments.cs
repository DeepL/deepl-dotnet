// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>A target-language translation attached to a source segment of a translation memory.</summary>
  public sealed class TranslationMemoryTargetSegment {
    /// <summary>Initializes a new instance of <see cref="TranslationMemoryTargetSegment" />.</summary>
    [JsonConstructor]
    public TranslationMemoryTargetSegment(
          string targetSegmentId,
          string targetLanguage,
          string targetText,
          DateTime? creationTime = null,
          DateTime? updatedTime = null,
          DateTime? lastUsedTime = null) {
      TargetSegmentId = targetSegmentId;
      TargetLanguage = targetLanguage;
      TargetText = targetText;
      CreationTime = creationTime;
      UpdatedTime = updatedTime;
      LastUsedTime = lastUsedTime;
    }

    /// <summary>Unique ID assigned to the target segment.</summary>
    [JsonPropertyName("target_segment_id")]
    public string TargetSegmentId { get; }

    /// <summary>Target language code of the translation.</summary>
    [JsonPropertyName("target_language")]
    public string TargetLanguage { get; }

    /// <summary>The translated text.</summary>
    [JsonPropertyName("target_text")]
    public string TargetText { get; }

    /// <summary>Time when the target segment was created, if provided by the API.</summary>
    [JsonPropertyName("creation_time")]
    public DateTime? CreationTime { get; }

    /// <summary>Time when the target segment was last updated, if provided by the API.</summary>
    [JsonPropertyName("updated_time")]
    public DateTime? UpdatedTime { get; }

    /// <summary>Time when the target segment was last used, if provided by the API.</summary>
    [JsonPropertyName("last_used_time")]
    public DateTime? LastUsedTime { get; }

    /// <summary>Returns a string describing the target segment.</summary>
    public override string ToString() => $"{TargetLanguage}: {TargetText}";
  }

  /// <summary>A source segment of a translation memory together with its translations.</summary>
  public sealed class TranslationMemorySegment {
    /// <summary>Initializes a new instance of <see cref="TranslationMemorySegment" />.</summary>
    [JsonConstructor]
    public TranslationMemorySegment(
          string sourceSegmentId,
          string sourceText,
          TranslationMemoryTargetSegment[] targets,
          DateTime? creationTime = null,
          DateTime? updatedTime = null,
          DateTime? lastUsedTime = null) {
      SourceSegmentId = sourceSegmentId;
      SourceText = sourceText;
      Targets = targets;
      CreationTime = creationTime;
      UpdatedTime = updatedTime;
      LastUsedTime = lastUsedTime;
    }

    /// <summary>Unique ID assigned to the source segment.</summary>
    [JsonPropertyName("source_segment_id")]
    public string SourceSegmentId { get; }

    /// <summary>The source text.</summary>
    [JsonPropertyName("source_text")]
    public string SourceText { get; }

    /// <summary>Translations of the source text, one per target language.</summary>
    [JsonPropertyName("targets")]
    public TranslationMemoryTargetSegment[] Targets { get; }

    /// <summary>Time when the source segment was created, if provided by the API.</summary>
    [JsonPropertyName("creation_time")]
    public DateTime? CreationTime { get; }

    /// <summary>Time when the source segment was last updated, if provided by the API.</summary>
    [JsonPropertyName("updated_time")]
    public DateTime? UpdatedTime { get; }

    /// <summary>Time when the source segment was last used, if provided by the API.</summary>
    [JsonPropertyName("last_used_time")]
    public DateTime? LastUsedTime { get; }

    /// <summary>Returns a string describing the segment.</summary>
    public override string ToString() => $"TranslationMemorySegment ({SourceSegmentId})";
  }

  /// <summary>One page of the segments stored in a translation memory.</summary>
  public sealed class TranslationMemorySegments {
    /// <summary>Initializes a new instance of <see cref="TranslationMemorySegments" />.</summary>
    [JsonConstructor]
    public TranslationMemorySegments(
          TranslationMemorySegment[] segments,
          int segmentCount,
          string? nextPageCursor = null) {
      Segments = segments;
      SegmentCount = segmentCount;
      NextPageCursor = nextPageCursor;
    }

    /// <summary>The segments contained in this page.</summary>
    [JsonPropertyName("segments")]
    public TranslationMemorySegment[] Segments { get; }

    /// <summary>
    ///   Total number of segments stored in the translation memory. This is translation-memory-level metadata and
    ///   is not reduced by a text filter.
    /// </summary>
    [JsonPropertyName("segment_count")]
    public int SegmentCount { get; }

    /// <summary>
    ///   Opaque cursor to pass as the page cursor to retrieve the next page, or <c>null</c> if this is the last
    ///   page.
    /// </summary>
    [JsonPropertyName("next_page_cursor")]
    public string? NextPageCursor { get; }
  }
}
