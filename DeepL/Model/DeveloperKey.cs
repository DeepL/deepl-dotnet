// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Text.Json.Serialization;
using DeepL.Internal;

namespace DeepL.Model {
  /// <summary>Information about a developer API key, as returned by the Admin API.</summary>
  public sealed class DeveloperKey {
    /// <summary>Initializes a new instance of <see cref="DeveloperKey" />.</summary>
    [JsonConstructor]
    public DeveloperKey(
          string? keyId,
          string? label,
          DateTime? creationTime,
          DateTime? deactivatedTime,
          bool isDeactivated,
          DeveloperKeyUsageLimits? usageLimits) {
      KeyId = keyId ?? "";
      Label = label ?? "";
      CreationTime = creationTime;
      DeactivatedTime = deactivatedTime;
      IsDeactivated = isDeactivated;
      UsageLimits = usageLimits ?? new DeveloperKeyUsageLimits(null, null);
    }

    /// <summary>Unique ID of the key.</summary>
    [JsonPropertyName("key_id")]
    [JsonConverter(typeof(LenientStringConverter))]
    public string KeyId { get; }

    /// <summary>Label assigned to the key, or an empty string if the API did not return one.</summary>
    [JsonPropertyName("label")]
    [JsonConverter(typeof(LenientStringConverter))]
    public string Label { get; }

    /// <summary>Time when the key was created, or null if unknown.</summary>
    [JsonPropertyName("creation_time")]
    [JsonConverter(typeof(LenientNullableDateTimeConverter))]
    public DateTime? CreationTime { get; }

    /// <summary>Time when the key was deactivated, or null if the key is active.</summary>
    [JsonPropertyName("deactivated_time")]
    [JsonConverter(typeof(LenientNullableDateTimeConverter))]
    public DateTime? DeactivatedTime { get; }

    /// <summary><c>true</c> if the key is deactivated, otherwise <c>false</c>.</summary>
    [JsonPropertyName("is_deactivated")]
    [JsonConverter(typeof(LenientBoolConverter))]
    public bool IsDeactivated { get; }

    /// <summary>Usage limits configured for the key; all limits are null if the API did not return them.</summary>
    [JsonPropertyName("usage_limits")]
    [JsonConverter(typeof(LenientObjectConverter<DeveloperKeyUsageLimits>))]
    public DeveloperKeyUsageLimits UsageLimits { get; }

    /// <summary>Returns a string describing the developer key.</summary>
    public override string ToString() => $"DeveloperKey \"{Label}\" ({KeyId})";
  }

  /// <summary>Usage limits of a developer key. A value of null means unlimited.</summary>
  public sealed class DeveloperKeyUsageLimits {
    /// <summary>Initializes a new instance of <see cref="DeveloperKeyUsageLimits" />.</summary>
    [JsonConstructor]
    public DeveloperKeyUsageLimits(
          long? characters,
          long? speechToTextMilliseconds,
          long? speechToSpeechMilliseconds = null) {
      Characters = characters;
      SpeechToTextMilliseconds = speechToTextMilliseconds;
      SpeechToSpeechMilliseconds = speechToSpeechMilliseconds;
    }

    /// <summary>Maximum number of characters per usage period, or null if unlimited.</summary>
    [JsonPropertyName("characters")]
    [JsonConverter(typeof(LenientNullableLongConverter))]
    public long? Characters { get; }

    /// <summary>Maximum number of speech-to-text milliseconds per usage period, or null if unlimited.</summary>
    [JsonPropertyName("speech_to_text_milliseconds")]
    [JsonConverter(typeof(LenientNullableLongConverter))]
    public long? SpeechToTextMilliseconds { get; }

    /// <summary>Maximum number of speech-to-speech milliseconds per usage period, or null if unlimited.</summary>
    [JsonPropertyName("speech_to_speech_milliseconds")]
    [JsonConverter(typeof(LenientNullableLongConverter))]
    public long? SpeechToSpeechMilliseconds { get; }
  }
}
