// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>
  ///   Information about a glossary, excluding the entry list. <see cref="MultilingualGlossaryInfo" /> is compatible with
  ///   the
  ///   /v3 glossary endpoints and supports multi-lingual glossaries compared to the v2 version. Glossaries now have
  ///   multiple glossary dictionaries each with their own source language, target language and entries.
  /// </summary>
  public sealed class MultilingualGlossaryInfo {
    /// <summary>Initializes a new <see cref="MultilingualGlossaryInfo" /> containing information about a glossary.</summary>
    /// <param name="glossaryId">ID of the associated glossary.</param>
    /// <param name="name">Name of the glossary chosen during creation.</param>
    /// <param name="dictionaries"><see cref="MultilingualGlossaryDictionaryInfo" />The dictionaries of the glossary</param>
    /// <param name="creationTime"><see cref="DateTime" /> when the glossary was created.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public MultilingualGlossaryInfo(
          string glossaryId,
          string name,
          MultilingualGlossaryDictionaryInfo[] dictionaries,
          DateTime creationTime) {
      (GlossaryId, Name, Dictionaries, CreationTime) =
            (glossaryId, name, dictionaries, creationTime);
    }

    /// <summary>ID of the associated glossary.</summary>
    [JsonPropertyName("glossary_id")]
    public string GlossaryId { get; }

    /// <summary>Name of the glossary chosen during creation.</summary>
    [JsonPropertyName("name")]
    public string Name { get; }

    /// <summary>The dictionaries of the glossary.</summary>
    [JsonPropertyName("dictionaries")]
    public MultilingualGlossaryDictionaryInfo[] Dictionaries { get; }

    /// <summary><see cref="DateTime" /> when the glossary was created.</summary>
    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; }

    /// <summary>Creates a string containing the name and ID of the current <see cref="MultilingualGlossaryInfo" /> object.</summary>
    /// <returns>A string containing the name and ID of the <see cref="MultilingualGlossaryInfo" /> object.</returns>
    /// <remarks>
    ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
    ///   compatibility.
    /// </remarks>
    public override string ToString() => $"Glossary \"{Name}\" ({GlossaryId})";

    /// <summary>
    ///   Implicitly converts the <see cref="MultilingualGlossaryInfo" /> to its ID, so it can be passed wherever the ID is accepted, for
    ///   example to <see cref="TextTranslateOptions" /> properties.
    /// </summary>
    /// <param name="glossary"><see cref="MultilingualGlossaryInfo" /> object to convert.</param>
    /// <returns>The value of <see cref="GlossaryId" />, or <c>null</c> if <paramref name="glossary" /> is <c>null</c>.</returns>
    [return: NotNullIfNotNull("glossary")]
    public static implicit operator string?(MultilingualGlossaryInfo? glossary) => glossary?.GlossaryId;
  }
}
