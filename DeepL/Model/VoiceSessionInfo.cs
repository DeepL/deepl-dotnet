// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>Information about a Voice API session, received from the session request endpoint.</summary>
  public sealed class VoiceSessionInfo {
    /// <summary>Initializes a new instance of <see cref="VoiceSessionInfo" />.</summary>
    /// <param name="streamingUrl">The WebSocket URL for establishing the stream connection.</param>
    /// <param name="token">Ephemeral authentication token for the streaming endpoint.</param>
    /// <param name="sessionId">Unique identifier for the session.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public VoiceSessionInfo(string streamingUrl, string token, string? sessionId) {
      StreamingUrl = streamingUrl;
      Token = token;
      SessionId = sessionId;
    }

    /// <summary>The WebSocket URL to use for establishing the stream connection.</summary>
    [JsonPropertyName("streaming_url")]
    public string StreamingUrl { get; }

    /// <summary>
    ///   Ephemeral authentication token for the streaming endpoint. Valid for one-time use only.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; }

    /// <summary>Unique identifier for the session.</summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; }
  }
}
