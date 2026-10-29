// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>
  ///   Information about a glossary, excluding the entry list. <see cref="GlossaryInfo" /> is compatible with the
  ///   /v2 glossary endpoints and can only support mono-lingual glossaries (e.g. a glossary with only one source and
  ///   target language defined).
  /// </summary>
  public sealed class GlossaryInfo {
    /// <summary>Initializes a new <see cref="GlossaryInfo" /> containing information about a glossary.</summary>
    /// <param name="glossaryId">ID of the associated glossary.</param>
    /// <param name="name">Name of the glossary chosen during creation.</param>
    /// <param name="ready"><c>true</c> if the glossary may be used for translations, otherwise <c>false</c>.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms in the glossary.</param>
    /// <param name="targetLanguageCode">Language code of the target terms in the glossary.</param>
    /// <param name="creationTime"><see cref="DateTime" /> when the glossary was created.</param>
    /// <param name="entryCount">The number of source-target entry pairs in the glossary.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public GlossaryInfo(
          string glossaryId,
          string name,
          bool ready,
          string sourceLanguageCode,
          string targetLanguageCode,
          DateTime creationTime,
          int entryCount) {
      (GlossaryId, Name, Ready, SourceLanguageCode, TargetLanguageCode, CreationTime, EntryCount) =
            (glossaryId, name, ready, sourceLanguageCode, targetLanguageCode, creationTime, entryCount);
    }

    /// <summary>ID of the associated glossary.</summary>
    public string GlossaryId { get; }

    /// <summary>Name of the glossary chosen during creation.</summary>
    public string Name { get; }

    /// <summary><c>true</c> if the glossary may be used for translations, otherwise <c>false</c>.</summary>
    public bool Ready { get; }

    /// <summary>Language code of the source terms in the glossary.</summary>
    [JsonPropertyName("source_lang")]
    public string SourceLanguageCode { get; }

    /// <summary>Language code of the target terms in the glossary.</summary>
    [JsonPropertyName("target_lang")]
    public string TargetLanguageCode { get; }

    /// <summary><see cref="DateTime" /> when the glossary was created.</summary>
    public DateTime CreationTime { get; }

    /// <summary>The number of source-target entry pairs in the glossary.</summary>
    public int EntryCount { get; }

    /// <summary>Creates a string containing the name and ID of the current <see cref="GlossaryInfo" /> object.</summary>
    /// <returns>A string containing the name and ID of the <see cref="GlossaryInfo" /> object.</returns>
    /// <remarks>
    ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
    ///   compatibility.
    /// </remarks>
    public override string ToString() => $"Glossary \"{Name}\" ({GlossaryId})";

    /// <summary>
    ///   Implicitly converts the <see cref="GlossaryInfo" /> to its ID, so it can be passed wherever the ID is accepted, for
    ///   example to <see cref="TextTranslateOptions" /> properties.
    /// </summary>
    /// <param name="glossary"><see cref="GlossaryInfo" /> object to convert.</param>
    /// <returns>The value of <see cref="GlossaryId" />.</returns>
    public static implicit operator string(GlossaryInfo glossary) => glossary.GlossaryId;
  }
}
