// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;

namespace DeepL.Model {
  /// <summary>
  ///   The terms of one language in a Spoken Terms collection. Used as input when creating or updating a collection or
  ///   replacing a term list, and as the result of reading a term list's entries.
  /// </summary>
  public sealed class SpokenTermsEntries {
    /// <summary>Initializes a new <see cref="SpokenTermsEntries" /> object.</summary>
    /// <param name="lang">Language code (ISO 639-1) of the terms, for example "en".</param>
    /// <param name="entries">The terms, one string per term.</param>
    public SpokenTermsEntries(string lang, IEnumerable<string> entries) {
      Lang = lang;
      Entries = entries.ToArray();
    }

    /// <summary>Language code (ISO 639-1) of the terms.</summary>
    public string Lang { get; }

    /// <summary>The terms, one string per term.</summary>
    public string[] Entries { get; }

    /// <summary>Creates a string containing the language and number of terms of the current object.</summary>
    /// <returns>A string containing the language and number of terms.</returns>
    /// <remarks>
    ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
    ///   compatibility.
    /// </remarks>
    public override string ToString() => $"Spoken terms entries \"{Lang}\" ({Entries.Length} entries)";
  }
}
