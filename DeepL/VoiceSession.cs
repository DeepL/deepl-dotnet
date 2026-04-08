// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Internal;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Internal implementation of <see cref="IVoiceSession" /> that manages a WebSocket connection
  ///   to the DeepL Voice API for real-time speech transcription and translation.
  /// </summary>
  internal sealed class VoiceSession : IVoiceSession {
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly DeepLHttpClient _httpClient;
    private readonly object _lock = new object();
    private ClientWebSocket _webSocket;
    private CancellationTokenSource _receiveCts;
    private Task? _receiveTask;
    private string _lastToken;
    private bool _disposed;

    /// <inheritdoc />
    public event EventHandler<TranscriptUpdate>? SourceTranscriptUpdated;

    /// <inheritdoc />
    public event EventHandler<TranscriptUpdate>? TargetTranscriptUpdated;

    /// <inheritdoc />
    public event EventHandler<TargetMediaChunk>? TargetMediaChunkReceived;

    /// <inheritdoc />
    public event EventHandler<VoiceStreamError>? ErrorReceived;

    /// <inheritdoc />
    public event EventHandler? StreamEnded;

    /// <inheritdoc />
    public string? SessionId { get; private set; }

    /// <inheritdoc />
    public bool IsConnected {
      get {
        lock (_lock) {
          return !_disposed && _webSocket.State == WebSocketState.Open;
        }
      }
    }

    internal VoiceSession(
          DeepLHttpClient httpClient,
          ClientWebSocket webSocket,
          VoiceSessionInfo sessionInfo) {
      _httpClient = httpClient;
      _webSocket = webSocket;
      _lastToken = sessionInfo.Token;
      SessionId = sessionInfo.SessionId;
      _receiveCts = new CancellationTokenSource();
      _receiveTask = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token));
    }

    /// <inheritdoc />
    public async Task SendAudioAsync(byte[] audioData, CancellationToken cancellationToken = default) {
      await SendAudioAsync(new ArraySegment<byte>(audioData), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SendAudioAsync(ArraySegment<byte> audioData, CancellationToken cancellationToken = default) {
      EnsureConnected();

      var base64Data = Convert.ToBase64String(
            audioData.Array ?? throw new ArgumentException("Audio data array is null"),
            audioData.Offset,
            audioData.Count);
      var message = $"{{\"source_media_chunk\":{{\"data\":\"{base64Data}\"}}}}";
      var bytes = Encoding.UTF8.GetBytes(message);

      await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task EndAudioAsync(CancellationToken cancellationToken = default) {
      EnsureConnected();

      var message = "{\"end_of_source_media\":{}}";
      var bytes = Encoding.UTF8.GetBytes(message);

      await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ReconnectAsync(CancellationToken cancellationToken = default) {
      // Stop current receive loop
      _receiveCts.Cancel();
      if (_receiveTask != null) {
        try {
          await _receiveTask.ConfigureAwait(false);
        } catch (OperationCanceledException) {
          // Expected
        }
      }

      // Close existing WebSocket if still open
      if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseReceived) {
        try {
          await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnecting", CancellationToken.None)
                .ConfigureAwait(false);
        } catch (WebSocketException) {
          // Ignore close errors during reconnection
        }
      }

      _webSocket.Dispose();

      // Request new token via GET v3/voice/realtime?token=<lastToken>
      var queryParams = new[] { ("token", _lastToken) };
      using var responseMessage = await _httpClient.ApiGetAsync("v3/voice/realtime", cancellationToken, queryParams)
            .ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var sessionInfo = await JsonUtils.DeserializeAsync<VoiceSessionInfo>(responseMessage).ConfigureAwait(false);

      _lastToken = sessionInfo.Token;
      SessionId = sessionInfo.SessionId;

      // Establish new WebSocket connection
      var wsUri = new Uri($"{sessionInfo.StreamingUrl}?token={Uri.EscapeDataString(sessionInfo.Token)}");
      _webSocket = new ClientWebSocket();
      await _webSocket.ConnectAsync(wsUri, cancellationToken).ConfigureAwait(false);

      // Restart receive loop
      _receiveCts = new CancellationTokenSource();
      _receiveTask = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token));
    }

    /// <summary>Background loop that receives and dispatches WebSocket messages.</summary>
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken) {
      var buffer = new byte[64 * 1024]; // 64 KB buffer
      var messageBuilder = new StringBuilder();

      try {
        while (!cancellationToken.IsCancellationRequested &&
               _webSocket.State == WebSocketState.Open) {
          messageBuilder.Clear();
          WebSocketReceiveResult result;
          do {
            result = await _webSocket.ReceiveAsync(
                  new ArraySegment<byte>(buffer), cancellationToken).ConfigureAwait(false);

            if (result.MessageType == WebSocketMessageType.Close) {
              return;
            }

            if (result.MessageType == WebSocketMessageType.Text) {
              messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            }
          } while (!result.EndOfMessage);

          if (messageBuilder.Length > 0) {
            DispatchMessage(messageBuilder.ToString());
          }
        }
      } catch (OperationCanceledException) {
        // Normal cancellation
      } catch (WebSocketException) {
        // Connection lost — consumer should call ReconnectAsync
      }
    }

    /// <summary>Parses a JSON message from the WebSocket and dispatches it to the appropriate event.</summary>
    private void DispatchMessage(string json) {
      try {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.TryGetProperty("source_transcript_update", out var sourceUpdate)) {
          var update = JsonSerializer.Deserialize<TranscriptUpdate>(sourceUpdate.GetRawText(), JsonOptions);
          if (update != null) {
            SourceTranscriptUpdated?.Invoke(this, update);
          }
        } else if (root.TryGetProperty("target_transcript_update", out var targetUpdate)) {
          var update = JsonSerializer.Deserialize<TranscriptUpdate>(targetUpdate.GetRawText(), JsonOptions);
          if (update != null) {
            TargetTranscriptUpdated?.Invoke(this, update);
          }
        } else if (root.TryGetProperty("target_media_chunk", out var mediaChunk)) {
          var chunk = JsonSerializer.Deserialize<TargetMediaChunk>(mediaChunk.GetRawText(), JsonOptions);
          if (chunk != null) {
            TargetMediaChunkReceived?.Invoke(this, chunk);
          }
        } else if (root.TryGetProperty("end_of_source_transcript", out _)) {
          // Source transcript complete — no special event needed, handled via StreamEnded
        } else if (root.TryGetProperty("end_of_target_transcript", out _)) {
          // Target transcript complete — no special event needed, handled via StreamEnded
        } else if (root.TryGetProperty("end_of_target_media", out _)) {
          // Target media complete — no special event needed, handled via StreamEnded
        } else if (root.TryGetProperty("end_of_stream", out _)) {
          StreamEnded?.Invoke(this, EventArgs.Empty);
        } else if (root.TryGetProperty("error", out var errorElement)) {
          var error = JsonSerializer.Deserialize<VoiceStreamError>(errorElement.GetRawText(), JsonOptions);
          if (error != null) {
            ErrorReceived?.Invoke(this, error);
          }
        }
      } catch (JsonException) {
        // Ignore malformed messages
      }
    }

    private void EnsureConnected() {
      if (_disposed) {
        throw new ObjectDisposedException(nameof(VoiceSession));
      }

      if (_webSocket.State != WebSocketState.Open) {
        throw new DeepLException("Voice session WebSocket is not connected");
      }
    }

    /// <summary>Releases the WebSocket connection and stops the receive loop.</summary>
    public void Dispose() {
      lock (_lock) {
        if (_disposed) return;
        _disposed = true;
      }

      _receiveCts.Cancel();

      try {
        if (_webSocket.State == WebSocketState.Open) {
          _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None)
                .GetAwaiter().GetResult();
        }
      } catch (WebSocketException) {
        // Ignore errors during disposal
      }

      _webSocket.Dispose();
      _receiveCts.Dispose();
    }
  }
}
