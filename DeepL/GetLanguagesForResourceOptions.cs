// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Options for
  ///   <see
  ///     cref="DeepLClient.GetLanguagesForResourceAsync(string, GetLanguagesForResourceOptions?, System.Threading.CancellationToken)" />
  ///   .
  /// </summary>
  public sealed class GetLanguagesForResourceOptions {
    /// <summary>Initializes a new <see cref="GetLanguagesForResourceOptions" /> object.</summary>
    public GetLanguagesForResourceOptions() { }

    /// <summary>
    ///   Values controlling which languages and features are returned, for example
    ///   <see cref="LanguageSupport.IncludeBeta" /> or <see cref="LanguageSupport.IncludeExternal" />; other strings are
    ///   sent unchanged. Each value is sent as an <c>include</c> query parameter. By default the list is empty and only
    ///   stable languages and features are returned.
    /// </summary>
    public List<string> Include { get; } = new List<string>();
  }
}
