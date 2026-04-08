// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>
  ///   Represents a translated audio chunk from the Voice API. This feature is currently in closed beta.
  ///   Audio data is provided as an array of base64-encoded indivisible chunks.
  /// </summary>
  public sealed class TargetMediaChunk {
    /// <summary>Initializes a new instance of <see cref="TargetMediaChunk" />.</summary>
    /// <param name="contentType">The content type of the audio data. Present in the first message.</param>
    /// <param name="headers">Number of header packets at the start of the data array, or null if all are audio.</param>
    /// <param name="data">Array of base64-encoded audio data packets.</param>
    /// <param name="text">Text corresponding to this audio chunk, for subtitle synchronization.</param>
    /// <param name="language">The target language of this audio chunk.</param>
    /// <param name="duration">Duration of this audio chunk in seconds.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public TargetMediaChunk(
          string? contentType,
          int? headers,
          string[] data,
          string? text,
          string? language,
          double? duration) {
      ContentType = contentType;
      Headers = headers;
      Data = data;
      Text = text;
      Language = language;
      Duration = duration;
    }

    /// <summary>The content type of the audio data. Present in the first message of a sequence.</summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; }

    /// <summary>
    ///   Number of packets at the start of <see cref="Data" /> that contain initialization/header data.
    ///   Null or absent when all packets are audio data.
    /// </summary>
    [JsonPropertyName("headers")]
    public int? Headers { get; }

    /// <summary>Array of base64-encoded indivisible audio data packets.</summary>
    [JsonPropertyName("data")]
    public string[] Data { get; }

    /// <summary>Text corresponding to this audio chunk, for subtitle synchronization.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; }

    /// <summary>The target language of this audio chunk.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; }

    /// <summary>Duration of this audio chunk in seconds.</summary>
    [JsonPropertyName("duration")]
    public double? Duration { get; }
  }
}
