// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Options for <see cref="DeepLClient.CreateMultilingualGlossaryAsync(string, MultilingualGlossaryDictionaryEntries[], CreateMultilingualGlossaryOptions?, System.Threading.CancellationToken)" />
  ///   and <see cref="DeepLClient.CreateMultilingualGlossaryFromCsvAsync(string, string, string, System.IO.Stream, CreateMultilingualGlossaryOptions?, System.Threading.CancellationToken)" />.
  /// </summary>
  public sealed class CreateMultilingualGlossaryOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="CreateMultilingualGlossaryOptions" /> object.</summary>
    public CreateMultilingualGlossaryOptions() { }
  }

  /// <summary>
  ///   Options for <see cref="DeepLClient.UpdateMultilingualGlossaryAsync(string, UpdateMultilingualGlossaryOptions?, System.Threading.CancellationToken)" />.
  ///   A property left <c>null</c> is not sent; with none set, the request body is <c>{}</c>.
  /// </summary>
  public sealed class UpdateMultilingualGlossaryOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="UpdateMultilingualGlossaryOptions" /> object.</summary>
    public UpdateMultilingualGlossaryOptions() { }

    /// <summary>New name for the glossary.</summary>
    public string? Name { get; set; }

    /// <summary>
    ///   Dictionaries to update: the entries of each are merged into the glossary's dictionary for its language pair,
    ///   which is created if it does not exist. The API currently accepts at most one dictionary per request.
    /// </summary>
    public IEnumerable<MultilingualGlossaryDictionaryEntries>? Dictionaries { get; set; }
  }

  /// <summary>
  ///   Options for the <c>DeepLClient.ReplaceMultilingualGlossaryDictionaryAsync()</c> and
  ///   <c>DeepLClient.ReplaceMultilingualGlossaryDictionaryFromCsvAsync()</c> overloads taking options.
  /// </summary>
  public sealed class ReplaceMultilingualGlossaryDictionaryOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="ReplaceMultilingualGlossaryDictionaryOptions" /> object.</summary>
    public ReplaceMultilingualGlossaryDictionaryOptions() { }
  }
}
