// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;

namespace DeepL {
  /// <summary>Represents the style the improved text should be in in a rephrase request.</summary>
  public enum WritingStyle {
    Academic,
    Business,
    Casual,
    Default,
    PreferAcademic,
    PreferBusiness,
    PreferCasual,
    PreferSimple,
    Simple,
  }

  /// <summary>Extension methods for <see cref="WritingStyle" />.</summary>
  public static class WritingStyleExtensions {
    /// <summary>Retrieves the string representation of the enum value used by the DeepL API.</summary>
    /// <exception cref="ArgumentOutOfRangeException">If an unknown enum value is passed.</exception>
    public static string ToApiValue(this WritingStyle writingStyle) =>
          writingStyle switch {
            WritingStyle.Academic => "academic",
            WritingStyle.Business => "business",
            WritingStyle.Casual => "casual",
            WritingStyle.Default => "default",
            WritingStyle.PreferAcademic => "prefer_academic",
            WritingStyle.PreferBusiness => "prefer_business",
            WritingStyle.PreferCasual => "prefer_casual",
            WritingStyle.PreferSimple => "prefer_simple",
            WritingStyle.Simple => "simple",
            _ => throw new ArgumentOutOfRangeException(
                  nameof(writingStyle),
                  writingStyle,
                  "Unrecognized writing style value")
          };
  }
}
