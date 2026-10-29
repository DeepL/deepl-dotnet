// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

namespace DeepL {
  /// <summary>
  ///   Values for <see cref="TextTranslateOptions.TagHandling" />. The property also accepts any other string, which is
  ///   sent unchanged.
  /// </summary>
  public static class TagHandlingMode {
    /// <summary>Parse the text as HTML.</summary>
    public const string Html = "html";

    /// <summary>Parse the text as XML.</summary>
    public const string Xml = "xml";
  }
}
