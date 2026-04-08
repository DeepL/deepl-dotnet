// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Represents an active Voice API streaming session. Provides methods for sending audio data and receiving
  ///   real-time transcriptions and translations via events.
  /// </summary>
  /// <remarks>
  ///   Events fire on a background thread. Consumers are responsible for marshaling to the appropriate
  ///   synchronization context if needed. Dispose the session to close the WebSocket connection.
  /// </remarks>
  public interface IVoiceSession : IDisposable {
    /// <summary>Raised when a source transcript update is received from the server.</summary>
    event EventHandler<TranscriptUpdate>? SourceTranscriptUpdated;

    /// <summary>Raised when a target transcript update is received from the server.</summary>
    event EventHandler<TranscriptUpdate>? TargetTranscriptUpdated;

    /// <summary>
    ///   Raised when a target media audio chunk is received from the server. This feature is in closed beta.
    /// </summary>
    event EventHandler<TargetMediaChunk>? TargetMediaChunkReceived;

    /// <summary>Raised when an error message is received from the WebSocket connection.</summary>
    event EventHandler<VoiceStreamError>? ErrorReceived;

    /// <summary>Raised when the end-of-stream message is received, indicating all outputs are complete.</summary>
    event EventHandler? StreamEnded;

    /// <summary>The unique session identifier.</summary>
    string? SessionId { get; }

    /// <summary>Whether the WebSocket connection is currently open.</summary>
    bool IsConnected { get; }

    /// <summary>
    ///   Sends a chunk of audio data to the server. The audio encoding must match the
    ///   <see cref="VoiceSessionOptions.SourceMediaContentType" /> specified when creating the session.
    /// </summary>
    /// <param name="audioData">Audio data to send. Must not exceed 100 KB or 1 second duration.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <exception cref="DeepLException">If the session is not connected or sending fails.</exception>
    Task SendAudioAsync(byte[] audioData, CancellationToken cancellationToken = default);

    /// <summary>
    ///   Sends a chunk of audio data to the server using a memory-efficient overload.
    /// </summary>
    /// <param name="audioData">Audio data to send. Must not exceed 100 KB or 1 second duration.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <exception cref="DeepLException">If the session is not connected or sending fails.</exception>
    Task SendAudioAsync(ArraySegment<byte> audioData, CancellationToken cancellationToken = default);

    /// <summary>
    ///   Signals the end of the audio stream. Causes finalization of tentative transcript segments and
    ///   triggers emission of final transcript updates, end-of-transcript, and end-of-stream messages.
    ///   No more audio data can be sent after calling this method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <exception cref="DeepLException">If the session is not connected or sending fails.</exception>
    Task EndAudioAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///   Requests a reconnection token and establishes a new WebSocket connection, resuming the session.
    ///   This should be called when the WebSocket connection is lost unexpectedly.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <exception cref="DeepLException">If reconnection fails.</exception>
    Task ReconnectAsync(CancellationToken cancellationToken = default);
  }
}
