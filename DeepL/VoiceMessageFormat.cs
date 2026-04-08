// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;

namespace DeepL {
  /// <summary>Message encoding format for Voice API WebSocket communication.</summary>
  public enum VoiceMessageFormat {
    /// <summary>JSON-encoded messages sent as TEXT WebSocket frames. Binary fields are base64-encoded.</summary>
    Json,

    /// <summary>MessagePack-encoded messages sent as BINARY WebSocket frames. Binary fields are raw binary.</summary>
    MessagePack
  }

  /// <summary>Extension methods for <see cref="VoiceMessageFormat" />.</summary>
  public static class VoiceMessageFormatExtensions {
    /// <summary>Retrieves the string representation used by the DeepL API.</summary>
    /// <exception cref="ArgumentOutOfRangeException">If an unknown enum value is passed.</exception>
    public static string ToApiValue(this VoiceMessageFormat format) {
      return format switch {
        VoiceMessageFormat.Json => "json",
        VoiceMessageFormat.MessagePack => "msgpack",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unrecognized message format value")
      };
    }
  }
}
