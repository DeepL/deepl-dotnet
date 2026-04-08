// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;

namespace DeepL {
  /// <summary>Controls how the source language value is used in Voice API sessions.</summary>
  public enum SourceLanguageMode {
    /// <summary>Treats source language as a hint; server can override.</summary>
    Auto,

    /// <summary>Treats source language as mandatory; server must use this language.</summary>
    Fixed
  }

  /// <summary>Extension methods for <see cref="SourceLanguageMode" />.</summary>
  public static class SourceLanguageModeExtensions {
    /// <summary>Retrieves the string representation used by the DeepL API.</summary>
    /// <exception cref="ArgumentOutOfRangeException">If an unknown enum value is passed.</exception>
    public static string ToApiValue(this SourceLanguageMode mode) {
      return mode switch {
        SourceLanguageMode.Auto => "auto",
        SourceLanguageMode.Fixed => "fixed",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unrecognized source language mode value")
      };
    }
  }
}
