// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;

namespace DeepL {
  /// <summary>
  ///   Changes to the usage limits of a developer key, passed to
  ///   <see cref="IAdminManager.SetDeveloperKeyUsageLimitsAsync(string, DeveloperKeyUsageLimitsUpdate, System.Threading.CancellationToken)" />.
  ///   Only the limits you assign are changed; limits you do not assign keep their current value. Assigning
  ///   <c>null</c> removes a limit (unlimited usage) and assigning 0 blocks that usage. The property getters return
  ///   <c>null</c> both for a limit that was not assigned and for one assigned <c>null</c>.
  /// </summary>
  /// <example>
  ///   <code>new DeveloperKeyUsageLimitsUpdate { Characters = 500000, SpeechToTextMilliseconds = null }</code>
  ///   limits characters to 500000, removes the speech-to-text limit and keeps the speech-to-speech limit.
  /// </example>
  public sealed class DeveloperKeyUsageLimitsUpdate {
    private readonly Dictionary<string, long?> _limits = new Dictionary<string, long?>();

    /// <summary>
    ///   Maximum number of characters per usage period across text translation, document translation and text
    ///   improvement, or <c>null</c> for unlimited. Only sent if assigned.
    /// </summary>
    public long? Characters {
      get => Get("characters");
      set => _limits["characters"] = value;
    }

    /// <summary>
    ///   Maximum number of speech-to-text milliseconds per usage period, or <c>null</c> for unlimited. Only sent if
    ///   assigned.
    /// </summary>
    public long? SpeechToTextMilliseconds {
      get => Get("speech_to_text_milliseconds");
      set => _limits["speech_to_text_milliseconds"] = value;
    }

    /// <summary>
    ///   Maximum number of speech-to-speech milliseconds per usage period, or <c>null</c> for unlimited. Only sent if
    ///   assigned.
    /// </summary>
    public long? SpeechToSpeechMilliseconds {
      get => Get("speech_to_speech_milliseconds");
      set => _limits["speech_to_speech_milliseconds"] = value;
    }

    /// <summary>The assigned limits by API field name, including those assigned <c>null</c>.</summary>
    internal IReadOnlyDictionary<string, long?> AssignedLimits => _limits;

    private long? Get(string field) => _limits.TryGetValue(field, out var value) ? value : null;
  }
}
