// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

namespace DeepL {
  /// <summary>
  ///   Values for <see cref="TextTranslateOptions.TagHandlingVersion" /> and
  ///   <see cref="Model.TextResult.TagHandlingVersion" />. The option also accepts any other string, which is sent
  ///   unchanged, and the result returns any value the API sends.
  /// </summary>
  public static class TagHandlingVersion {
    /// <summary>The original tag handling algorithm.</summary>
    public const string V1 = "v1";

    /// <summary>The improved tag handling algorithm.</summary>
    public const string V2 = "v2";
  }
}
