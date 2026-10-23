// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>Information about a Spoken Terms collection, excluding the terms themselves.</summary>
  public sealed class SpokenTerms {
    /// <summary>Initializes a new <see cref="SpokenTerms" /> containing information about a Spoken Terms collection.</summary>
    /// <param name="spokenTermsId">ID of the Spoken Terms collection.</param>
    /// <param name="name">Name of the Spoken Terms collection.</param>
    /// <param name="creationTime"><see cref="DateTime" /> when the Spoken Terms collection was created.</param>
    /// <param name="termLists">The term lists of the collection, one per language.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public SpokenTerms(
          string spokenTermsId,
          string name,
          DateTime creationTime,
          SpokenTermsList[]? termLists) {
      (SpokenTermsId, Name, CreationTime, TermLists) =
            (spokenTermsId, name, creationTime, termLists ?? new SpokenTermsList[0]);
    }

    /// <summary>ID of the Spoken Terms collection.</summary>
    [JsonPropertyName("spoken_terms_id")]
    public string SpokenTermsId { get; }

    /// <summary>Name of the Spoken Terms collection.</summary>
    [JsonPropertyName("name")]
    public string Name { get; }

    /// <summary><see cref="DateTime" /> when the Spoken Terms collection was created.</summary>
    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; }

    /// <summary>The term lists of the collection, one per language; empty if the response contains none.</summary>
    [JsonPropertyName("term_lists")]
    public SpokenTermsList[] TermLists { get; }

    /// <summary>Creates a string containing the name and ID of the current <see cref="SpokenTerms" /> object.</summary>
    /// <returns>A string containing the name and ID of the <see cref="SpokenTerms" /> object.</returns>
    /// <remarks>
    ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
    ///   compatibility.
    /// </remarks>
    public override string ToString() => $"Spoken terms \"{Name}\" ({SpokenTermsId})";
  }
}
