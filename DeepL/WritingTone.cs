// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;

namespace DeepL {
  /// <summary>Represents the tone the improved text should be in in a rephrase request.</summary>
  public enum WritingTone {
    Confident,
    Default,
    Diplomatic,
    Enthusiastic,
    Friendly,
    PreferConfident,
    PreferDiplomatic,
    PreferEnthusiastic,
    PreferFriendly,
  }

  /// <summary>Extension methods for <see cref="WritingTone" />.</summary>
  public static class WritingToneExtensions {
    /// <summary>Retrieves the string representation of the enum value used by the DeepL API.</summary>
    /// <exception cref="ArgumentOutOfRangeException">If an unknown enum value is passed.</exception>
    public static string ToApiValue(this WritingTone writingTone) =>
          writingTone switch {
            WritingTone.Confident => "confident",
            WritingTone.Default => "default",
            WritingTone.Diplomatic => "diplomatic",
            WritingTone.Enthusiastic => "enthusiastic",
            WritingTone.Friendly => "friendly",
            WritingTone.PreferConfident => "prefer_confident",
            WritingTone.PreferDiplomatic => "prefer_diplomatic",
            WritingTone.PreferEnthusiastic => "prefer_enthusiastic",
            WritingTone.PreferFriendly => "prefer_friendly",
            _ => throw new ArgumentOutOfRangeException(nameof(writingTone), writingTone, "Unrecognized writing tone value")
          };
  }
}
