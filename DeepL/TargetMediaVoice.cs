// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;

namespace DeepL {
  /// <summary>
  ///   Target audio voice selection for synthesized speech in Voice API sessions.
  ///   This feature is currently in closed beta.
  /// </summary>
  public enum TargetMediaVoice {
    /// <summary>Male voice.</summary>
    Male,

    /// <summary>Female voice.</summary>
    Female
  }

  /// <summary>Extension methods for <see cref="TargetMediaVoice" />.</summary>
  public static class TargetMediaVoiceExtensions {
    /// <summary>Retrieves the string representation used by the DeepL API.</summary>
    /// <exception cref="ArgumentOutOfRangeException">If an unknown enum value is passed.</exception>
    public static string ToApiValue(this TargetMediaVoice voice) {
      return voice switch {
        TargetMediaVoice.Male => "male",
        TargetMediaVoice.Female => "female",
        _ => throw new ArgumentOutOfRangeException(nameof(voice), voice, "Unrecognized target media voice value")
      };
    }
  }
}
