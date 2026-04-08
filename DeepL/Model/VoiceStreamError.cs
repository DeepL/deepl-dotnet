// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>Represents an error message received from the Voice API WebSocket connection.</summary>
  public sealed class VoiceStreamError {
    /// <summary>Initializes a new instance of <see cref="VoiceStreamError" />.</summary>
    /// <param name="code">The error code.</param>
    /// <param name="reason">The reason code for the error.</param>
    /// <param name="message">A human-readable error message.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public VoiceStreamError(string? code, string? reason, string? message) {
      Code = code;
      Reason = reason;
      Message = message;
    }

    /// <summary>The error code.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; }

    /// <summary>The reason code for the error.</summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; }

    /// <summary>A human-readable error message.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; }

    /// <summary>Returns the error message.</summary>
    public override string ToString() => $"VoiceStreamError(code={Code}, reason={Reason}, message={Message})";
  }
}
