// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Text.Json.Serialization;
using DeepL.Internal;

namespace DeepL.Model {
  /// <summary>Custom tag usage analytics report for a date range, as returned by the Admin API.</summary>
  public sealed class CustomTagUsageReport {
    /// <summary>Initializes a new instance of <see cref="CustomTagUsageReport" />.</summary>
    [JsonConstructor]
    public CustomTagUsageReport(
          string? aggregateBy,
          DateTime? startDate,
          DateTime? endDate,
          int? nextPage,
          CustomTagUsage[]? usage) {
      AggregateBy = aggregateBy;
      StartDate = startDate;
      EndDate = endDate;
      NextPage = nextPage;
      Usage = usage ?? Array.Empty<CustomTagUsage>();
    }

    /// <summary>Aggregation used for the report as returned by the API, or null if missing.</summary>
    [JsonPropertyName("aggregate_by")]
    [JsonConverter(typeof(LenientStringConverter))]
    public string? AggregateBy { get; }

    /// <summary>Start of the report period, in UTC, or null if the API did not return it.</summary>
    [JsonPropertyName("start_date")]
    [JsonConverter(typeof(LenientNullableDateTimeConverter))]
    public DateTime? StartDate { get; }

    /// <summary>End of the report period, in UTC, or null if the API did not return it.</summary>
    [JsonPropertyName("end_date")]
    [JsonConverter(typeof(LenientNullableDateTimeConverter))]
    public DateTime? EndDate { get; }

    /// <summary>Page number to request the next page, or null if this is the last page.</summary>
    [JsonPropertyName("next_page")]
    [JsonConverter(typeof(LenientNullableIntConverter))]
    public int? NextPage { get; }

    /// <summary>Usage entries per custom tag, empty if there are none.</summary>
    [JsonPropertyName("usage")]
    [JsonConverter(typeof(LenientArrayConverter<CustomTagUsage>))]
    public CustomTagUsage[] Usage { get; }
  }

  /// <summary>Usage of a single custom tag in a <see cref="CustomTagUsageReport" />.</summary>
  public sealed class CustomTagUsage {
    /// <summary>Initializes a new instance of <see cref="CustomTagUsage" />.</summary>
    [JsonConstructor]
    public CustomTagUsage(string? customTag, CustomTagUsageBreakdown? breakdown, DateTime? usageDate = null) {
      CustomTag = customTag ?? "";
      Breakdown = breakdown ?? new CustomTagUsageBreakdown(0, 0, 0, 0, 0);
      UsageDate = usageDate;
    }

    /// <summary>The custom tag.</summary>
    [JsonPropertyName("custom_tag")]
    [JsonConverter(typeof(LenientStringConverter))]
    public string CustomTag { get; }

    /// <summary>Usage of the custom tag.</summary>
    [JsonPropertyName("breakdown")]
    [JsonConverter(typeof(LenientObjectConverter<CustomTagUsageBreakdown>))]
    public CustomTagUsageBreakdown Breakdown { get; }

    /// <summary>Day of the usage, present when aggregated by <see cref="CustomTagUsageAggregateBy.Day" />.</summary>
    [JsonPropertyName("usage_date")]
    [JsonConverter(typeof(LenientNullableDateTimeConverter))]
    public DateTime? UsageDate { get; }
  }

  /// <summary>Usage totals of a custom tag.</summary>
  public sealed class CustomTagUsageBreakdown {
    /// <summary>Initializes a new instance of <see cref="CustomTagUsageBreakdown" />.</summary>
    [JsonConstructor]
    public CustomTagUsageBreakdown(
          long totalCharacters,
          long textTranslationCharacters,
          long textImprovementCharacters,
          double speechToTextMinutes,
          double speechToSpeechMinutes) {
      TotalCharacters = totalCharacters;
      TextTranslationCharacters = textTranslationCharacters;
      TextImprovementCharacters = textImprovementCharacters;
      SpeechToTextMinutes = speechToTextMinutes;
      SpeechToSpeechMinutes = speechToSpeechMinutes;
    }

    /// <summary>Total number of characters used.</summary>
    [JsonPropertyName("total_characters")]
    [JsonConverter(typeof(LenientLongConverter))]
    public long TotalCharacters { get; }

    /// <summary>Number of characters used for text translation.</summary>
    [JsonPropertyName("text_translation_characters")]
    [JsonConverter(typeof(LenientLongConverter))]
    public long TextTranslationCharacters { get; }

    /// <summary>Number of characters used for text improvement.</summary>
    [JsonPropertyName("text_improvement_characters")]
    [JsonConverter(typeof(LenientLongConverter))]
    public long TextImprovementCharacters { get; }

    /// <summary>Number of speech-to-text minutes used.</summary>
    [JsonPropertyName("speech_to_text_minutes")]
    [JsonConverter(typeof(LenientDoubleConverter))]
    public double SpeechToTextMinutes { get; }

    /// <summary>Number of speech-to-speech minutes used.</summary>
    [JsonPropertyName("speech_to_speech_minutes")]
    [JsonConverter(typeof(LenientDoubleConverter))]
    public double SpeechToSpeechMinutes { get; }
  }
}
