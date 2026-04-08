// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>A single text segment within a Voice API transcript update.</summary>
  public sealed class TranscriptSegment {
    /// <summary>Initializes a new instance of <see cref="TranscriptSegment" />.</summary>
    /// <param name="text">The text content of this segment.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public TranscriptSegment(string text) {
      Text = text;
    }

    /// <summary>The text content of this segment.</summary>
    [JsonPropertyName("text")]
    public string Text { get; }

    /// <summary>Returns the text content of this segment.</summary>
    public override string ToString() => Text;
  }
}
