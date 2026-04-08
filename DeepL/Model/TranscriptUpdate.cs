// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>
  ///   Represents a transcript update from the Voice API, containing concluded (finalized) and tentative
  ///   (in-progress) text segments. Used for both source and target transcript updates.
  /// </summary>
  public sealed class TranscriptUpdate {
    /// <summary>Initializes a new instance of <see cref="TranscriptUpdate" />.</summary>
    /// <param name="concluded">Finalized text segments that will not change.</param>
    /// <param name="tentative">Preliminary text segments that may be refined.</param>
    /// <param name="language">The language code of this transcript update. Only present on target updates.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public TranscriptUpdate(TranscriptSegment[] concluded, TranscriptSegment[] tentative, string? language) {
      Concluded = concluded;
      Tentative = tentative;
      Language = language;
    }

    /// <summary>Finalized text segments that will not change. These segments are sent once and remain fixed.</summary>
    [JsonPropertyName("concluded")]
    public TranscriptSegment[] Concluded { get; }

    /// <summary>Preliminary text segments that may be refined as more audio context becomes available.</summary>
    [JsonPropertyName("tentative")]
    public TranscriptSegment[] Tentative { get; }

    /// <summary>The language code of this transcript update. Only present on target transcript updates.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; }
  }
}
