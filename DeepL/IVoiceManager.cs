// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeepL {
  /// <summary>Interface for creating Voice API streaming sessions.</summary>
  public interface IVoiceManager : IDisposable {
    /// <summary>
    ///   Creates a new Voice API streaming session for real-time speech transcription and translation.
    ///   This requests a session from the DeepL API and establishes a WebSocket connection.
    /// </summary>
    /// <param name="options">Options controlling session configuration including audio format, languages, etc.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>An <see cref="IVoiceSession" /> for streaming audio and receiving transcripts.</returns>
    /// <exception cref="ArgumentException">If any option is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<IVoiceSession> CreateVoiceSessionAsync(
          VoiceSessionOptions options,
          CancellationToken cancellationToken = default);
  }
}
