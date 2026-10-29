// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>The result of a text translation.</summary>
  public sealed class WriteResult {
    /// <summary>Initializes a new instance of <see cref="WriteResult" />.</summary>
    /// <param name="text">improved text.</param>
    /// <param name="detectedSourceLanguageCode">The detected language code of the input text.</param>
    /// <param name="targetLanguageCode">The selected target language in the request.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public WriteResult(string text, string detectedSourceLanguageCode, string targetLanguageCode) {
      Text = text;
      // Raised here so that a response without the field fails with a DeepLException, not a NullReferenceException.
      DetectedSourceLanguageCode = LanguageCode.Standardize(
            detectedSourceLanguageCode ??
            throw new DeepLException("Write result is missing detected_source_language"));
      TargetLanguageCode = targetLanguageCode;
    }

    /// <summary>The improved text.</summary>
    public string Text { get; }

    /// <summary>The language code of the source text detected by DeepL.</summary>
    [JsonPropertyName("detected_source_language")]
    public string DetectedSourceLanguageCode { get; }

    /// <summary>The number of characters billed for the text.</summary>
    [JsonPropertyName("target_language")]
    public string TargetLanguageCode { get; }

    /// <summary>Returns the improved text.</summary>
    /// <returns>The improved text.</returns>
    public override string ToString() => Text;
  }
}
