// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>Information about one term list of a Spoken Terms collection, excluding the terms themselves.</summary>
  public sealed class SpokenTermsList {
    /// <summary>Initializes a new <see cref="SpokenTermsList" /> containing information about a term list.</summary>
    /// <param name="lang">Language code (ISO 639-1) of the term list.</param>
    /// <param name="entryCount">The number of terms in the term list.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public SpokenTermsList(string lang, long entryCount) {
      (Lang, EntryCount) = (lang, entryCount);
    }

    /// <summary>Language code (ISO 639-1) of the term list.</summary>
    [JsonPropertyName("lang")]
    public string Lang { get; }

    /// <summary>The number of terms in the term list.</summary>
    [JsonPropertyName("entry_count")]
    public long EntryCount { get; }

    /// <summary>Creates a string containing the language and entry count of the current term list.</summary>
    /// <returns>A string containing the language and entry count of the term list.</returns>
    /// <remarks>
    ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
    ///   compatibility.
    /// </remarks>
    public override string ToString() => $"Spoken terms list \"{Lang}\" ({EntryCount} entries)";
  }
}
