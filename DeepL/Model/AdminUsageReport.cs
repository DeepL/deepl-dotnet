// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Text.Json.Serialization;
using DeepL.Internal;

namespace DeepL.Model {
  /// <summary>Usage analytics report of an organization for a date range, as returned by the Admin API.</summary>
  public sealed class AdminUsageReport {
    /// <summary>Initializes a new instance of <see cref="AdminUsageReport" />.</summary>
    [JsonConstructor]
    public AdminUsageReport(
          AdminUsageBreakdown? totalUsage,
          DateTime? startDate,
          DateTime? endDate,
          string? groupBy = null,
          KeyUsage[]? keyUsages = null,
          KeyUsage[]? keyAndDayUsages = null) {
      TotalUsage = totalUsage;
      StartDate = startDate;
      EndDate = endDate;
      GroupBy = groupBy;
      KeyUsages = keyUsages ?? Array.Empty<KeyUsage>();
      KeyAndDayUsages = keyAndDayUsages ?? Array.Empty<KeyUsage>();
    }

    /// <summary>Usage totals over the whole date range, or null if the API did not return them.</summary>
    [JsonPropertyName("total_usage")]
    [JsonConverter(typeof(LenientObjectConverter<AdminUsageBreakdown>))]
    public AdminUsageBreakdown? TotalUsage { get; }

    /// <summary>Start of the report period, in UTC, or null if the API did not return it.</summary>
    [JsonPropertyName("start_date")]
    [JsonConverter(typeof(LenientNullableDateTimeConverter))]
    public DateTime? StartDate { get; }

    /// <summary>End of the report period, in UTC, or null if the API did not return it.</summary>
    [JsonPropertyName("end_date")]
    [JsonConverter(typeof(LenientNullableDateTimeConverter))]
    public DateTime? EndDate { get; }

    /// <summary>
    ///   Grouping used for the report, as the raw string the API returns: <c>key</c> or <c>keyAndDay</c> at the time
    ///   of writing, or null if not grouped.
    /// </summary>
    [JsonPropertyName("group_by")]
    [JsonConverter(typeof(LenientStringConverter))]
    public string? GroupBy { get; }

    /// <summary>Usage per key when grouped by <see cref="AdminUsageGroupBy.Key" />, otherwise empty.</summary>
    [JsonPropertyName("key_usages")]
    [JsonConverter(typeof(LenientArrayConverter<KeyUsage>))]
    public KeyUsage[] KeyUsages { get; }

    /// <summary>
    ///   Usage per key and day when grouped by <see cref="AdminUsageGroupBy.KeyAndDay" />, otherwise empty. The
    ///   entries have a <see cref="KeyUsage.UsageDate" />.
    /// </summary>
    [JsonPropertyName("key_and_day_usages")]
    [JsonConverter(typeof(LenientArrayConverter<KeyUsage>))]
    public KeyUsage[] KeyAndDayUsages { get; }
  }

  /// <summary>Usage totals in an <see cref="AdminUsageReport" />. Values missing in the response are 0.</summary>
  public sealed class AdminUsageBreakdown {
    /// <summary>Initializes a new instance of <see cref="AdminUsageBreakdown" />.</summary>
    [JsonConstructor]
    public AdminUsageBreakdown(
          long totalCharacters,
          long textTranslationCharacters,
          long documentTranslationCharacters,
          long textImprovementCharacters,
          double speechToTextMinutes,
          double speechToSpeechMinutes = 0) {
      TotalCharacters = totalCharacters;
      TextTranslationCharacters = textTranslationCharacters;
      DocumentTranslationCharacters = documentTranslationCharacters;
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

    /// <summary>Number of characters used for document translation.</summary>
    [JsonPropertyName("document_translation_characters")]
    [JsonConverter(typeof(LenientLongConverter))]
    public long DocumentTranslationCharacters { get; }

    /// <summary>Number of characters used for text improvement.</summary>
    [JsonPropertyName("text_improvement_characters")]
    [JsonConverter(typeof(LenientLongConverter))]
    public long TextImprovementCharacters { get; }

    /// <summary>Number of speech-to-text minutes used.</summary>
    [JsonPropertyName("speech_to_text_minutes")]
    [JsonConverter(typeof(LenientDoubleConverter))]
    public double SpeechToTextMinutes { get; }

    /// <summary>
    ///   Number of speech-to-speech minutes used. The API returns this field although the API reference does not
    ///   list it yet.
    /// </summary>
    [JsonPropertyName("speech_to_speech_minutes")]
    [JsonConverter(typeof(LenientDoubleConverter))]
    public double SpeechToSpeechMinutes { get; }
  }

  /// <summary>
  ///   Usage of a single API key in an <see cref="AdminUsageReport" />, either for the whole period or, when grouped
  ///   by key and day, for a single day.
  /// </summary>
  public sealed class KeyUsage {
    /// <summary>Initializes a new instance of <see cref="KeyUsage" />.</summary>
    [JsonConstructor]
    public KeyUsage(string? apiKey, string? apiKeyLabel, DateTime? usageDate, AdminUsageBreakdown? usage) {
      ApiKey = apiKey ?? "";
      ApiKeyLabel = apiKeyLabel;
      UsageDate = usageDate;
      Usage = usage ?? new AdminUsageBreakdown(0, 0, 0, 0, 0);
    }

    /// <summary>Masked API key.</summary>
    [JsonPropertyName("api_key")]
    [JsonConverter(typeof(LenientStringConverter))]
    public string ApiKey { get; }

    /// <summary>Label of the API key, or null if the API did not return one.</summary>
    [JsonPropertyName("api_key_label")]
    [JsonConverter(typeof(LenientStringConverter))]
    public string? ApiKeyLabel { get; }

    /// <summary>Day of the usage, in UTC. Null unless the report is grouped by key and day.</summary>
    [JsonPropertyName("usage_date")]
    [JsonConverter(typeof(LenientNullableDateTimeConverter))]
    public DateTime? UsageDate { get; }

    /// <summary>Usage of the API key.</summary>
    [JsonPropertyName("usage")]
    [JsonConverter(typeof(LenientObjectConverter<AdminUsageBreakdown>))]
    public AdminUsageBreakdown Usage { get; }
  }
}
