// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

namespace DeepL {
  /// <summary>
  ///   Options to control text correction behaviour. These options may be provided to the
  ///   <see cref="IWriter.CorrectTextAsync(string, string?, CorrectTextOptions?, System.Threading.CancellationToken)" />
  ///   functions.
  /// </summary>
  public sealed class CorrectTextOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="CorrectTextOptions" /> object.</summary>
    public CorrectTextOptions() { }
  }
}
