// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using DeepL.Model;

namespace DeepL {
  /// <summary>Options for <see cref="ISpokenTermsManager.CreateSpokenTermsAsync" />.</summary>
  public sealed class CreateSpokenTermsOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="CreateSpokenTermsOptions" /> object.</summary>
    public CreateSpokenTermsOptions() { }
  }

  /// <summary>
  ///   Options for <see cref="ISpokenTermsManager.UpdateSpokenTermsAsync(string, UpdateSpokenTermsOptions?, System.Threading.CancellationToken)" />.
  ///   A property left <c>null</c> is not sent; with none set, the request body is <c>{}</c>.
  /// </summary>
  public sealed class UpdateSpokenTermsOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="UpdateSpokenTermsOptions" /> object.</summary>
    public UpdateSpokenTermsOptions() { }

    /// <summary>New name for the collection.</summary>
    public string? Name { get; set; }

    /// <summary>Terms to merge into the existing term list of each language.</summary>
    public IEnumerable<SpokenTermsEntries>? TermLists { get; set; }
  }

  /// <summary>
  ///   Options for <see cref="ISpokenTermsManager.ReplaceSpokenTermsListAsync(string, SpokenTermsEntries, ReplaceSpokenTermsListOptions?, System.Threading.CancellationToken)" />.
  /// </summary>
  public sealed class ReplaceSpokenTermsListOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="ReplaceSpokenTermsListOptions" /> object.</summary>
    public ReplaceSpokenTermsListOptions() { }
  }
}
