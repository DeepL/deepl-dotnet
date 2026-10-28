// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>Information about a glossary dictionary, including the entry list. </summary>
  public sealed class MultilingualGlossaryDictionaryEntries {
    /// <summary>
    ///   Initializes a new <see cref="MultilingualGlossaryDictionaryEntries" /> containing information about a glossary
    ///   dictionary.
    /// </summary>
    /// <param name="sourceLanguageCode">Language code of the source terms in the glossary dictionary.</param>
    /// <param name="targetLanguageCode">Language code of the target terms in the glossary dictionary.</param>
    /// <param name="entries"><see cref="GlossaryEntries" />The source-target entry pairs in the glossary dictionary.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    public MultilingualGlossaryDictionaryEntries(
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries) {
      (SourceLanguageCode, TargetLanguageCode, Entries) =
            (sourceLanguageCode, targetLanguageCode, entries);
    }

    [Obsolete("Internal type; will be made internal in the next major version.")]
    public MultilingualGlossaryDictionaryEntries(MultilingualGlossaryDictionaryEntriesResult dictionaryEntriesResult) {
      (SourceLanguageCode, TargetLanguageCode, Entries) =
            (dictionaryEntriesResult.SourceLanguageCode, dictionaryEntriesResult.TargetLanguageCode,
                  GlossaryEntries.FromTsv(dictionaryEntriesResult.Entries));
    }

    /// <summary>Language code of the source terms in the glossary.</summary>
    public string SourceLanguageCode { get; }

    /// <summary>Language code of the target terms in the glossary.</summary>
    public string TargetLanguageCode { get; }

    /// <summary>The source-target entry pairs in the glossary dictionary.</summary>
    public GlossaryEntries Entries { get; }

    /// <summary>
    ///   Creates a string containing the name and ID of the current
    ///   <see cref="MultilingualGlossaryDictionaryEntries" /> object.
    /// </summary>
    /// <returns>
    ///   A string containing the source and target language pair of the <see cref="MultilingualGlossaryDictionaryEntries" />
    ///   object.
    /// </returns>
    /// <remarks>
    ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
    ///   compatibility.
    /// </remarks>
    public override string ToString() =>
          $"Glossary dictionary \"{SourceLanguageCode}\" ({TargetLanguageCode}): {Entries})";
  }

  /// <summary>Class used for JSON-deserialization of glossary dictionary entries results.</summary>
  [Obsolete("Internal type; will be made internal in the next major version.")]
  public readonly struct MultilingualGlossaryDictionaryEntriesResult {
    /// <summary>
    ///   Initializes a new instance of <see cref="MultilingualGlossaryDictionaryEntriesResult" />, used for JSON
    ///   deserialization.
    /// </summary>
    [JsonConstructor]
    public MultilingualGlossaryDictionaryEntriesResult(
          string sourceLanguageCode,
          string targetLanguageCode,
          string entries,
          string entriesFormat) {
      (SourceLanguageCode, TargetLanguageCode, Entries, EntriesFormat) =
            (sourceLanguageCode, targetLanguageCode, entries, entriesFormat);
    }

    /// <summary>Language code of the source terms in the glossary dictionary.</summary>
    [JsonPropertyName("source_lang")]
    public string SourceLanguageCode { get; }

    /// <summary>Language code of the target terms in the glossary dictionary.</summary>
    [JsonPropertyName("target_lang")]
    public string TargetLanguageCode { get; }

    /// <summary>The source-target entry pairs in the glossary dictionary representing as a string.</summary>
    [JsonPropertyName("entries")]
    public string Entries { get; }

    /// <summary>The format of the entries (should be TSV if deserializing JSON from the v3 glossary APIs)</summary>
    [JsonPropertyName("entries_format")]
    public string EntriesFormat { get; }
  }
}
