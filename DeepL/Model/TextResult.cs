// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>The result of a text translation.</summary>
  public sealed class TextResult {
    /// <summary>Initializes a new instance of <see cref="TextResult" />.</summary>
    /// <param name="text">Translated text.</param>
    /// <param name="detectedSourceLanguageCode">The detected language code of the input text.</param>
    /// <param name="billedCharacters">The number of characters billed for the text.</param>
    /// <param name="modelTypeUsed">Indicates the translation model used, but is null unless the modelType option is specified.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    /// <exception cref="DeepLException">If <paramref name="detectedSourceLanguageCode" /> is null.</exception>
    [JsonConstructor]
    public TextResult(string text, string detectedSourceLanguageCode, int billedCharacters, string? modelTypeUsed) {
      Text = text;
      // Raised here so that a response without the field fails with a DeepLException, not a NullReferenceException.
      DetectedSourceLanguageCode = LanguageCode.Standardize(
            detectedSourceLanguageCode ??
            throw new DeepLException("Text translation result is missing detected_source_language"));
      BilledCharacters = billedCharacters;
      ModelTypeUsed = modelTypeUsed;
    }

    /// <summary>The translated text.</summary>
    public string Text { get; }

    /// <summary>The language code of the source text detected by DeepL.</summary>
    [JsonPropertyName("detected_source_language")]
    public string DetectedSourceLanguageCode { get; }

    /// <summary>The number of characters billed for the text.</summary>
    [JsonPropertyName("billed_characters")]
    public int BilledCharacters { get; }

    /// <summary>Indicated translation model used, if available.</summary>
    [JsonPropertyName("model_type_used")]
    public string? ModelTypeUsed { get; }

    /// <summary>Returns the translated text.</summary>
    /// <returns>The translated text.</returns>
    public override string ToString() => Text;
  }
}
