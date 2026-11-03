// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeepL;
using Xunit;

namespace DeepLTests.Offline {
  /// <summary>
  ///   Sends an HTTPS request through a local forward proxy that rejects every CONNECT request, so the server is never
  ///   contacted and nothing leaves the machine.
  /// </summary>
  public sealed class ProxyTest : IDisposable {
    private readonly TcpListener _listener = new TcpListener(IPAddress.Loopback, 0);
    private readonly object _lock = new object();
    private readonly List<List<string>> _connectRequests = new List<List<string>>();

    public ProxyTest() {
      _listener.Start();
      _ = Task.Run(AcceptLoopAsync);
    }

    public void Dispose() => _listener.Stop();

    [Fact]
    public async Task DoesNotRetryProxyTunnelRejection() {
      var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
      using var client = new DeepLClient(
            "test-key",
            new DeepLClientOptions {
              ServerUrl = "https://api.deepl.test",
              Proxy = new Uri($"http://proxy-user:secret@127.0.0.1:{port}"),
              MaximumNetworkRetries = 2,
              PerRetryConnectionTimeout = TimeSpan.FromSeconds(10)
            });

      var exception = await Assert.ThrowsAsync<ConnectionException>(() => client.GetUsageAsync());

      Assert.False(exception.ShouldRetry);
      lock (_lock) {
        // Each attempt sends a CONNECT without credentials, then repeats it with credentials after the 407 challenge.
        Assert.All(_connectRequests, request => Assert.Equal("CONNECT api.deepl.test:443 HTTP/1.1", request[0]));
        Assert.Single(
              _connectRequests,
              request => request.Exists(
                    line => line.StartsWith("Proxy-Authorization: Basic ", StringComparison.OrdinalIgnoreCase)));
      }
    }

    private async Task AcceptLoopAsync() {
      while (true) {
        TcpClient connection;
        try {
          connection = await _listener.AcceptTcpClientAsync();
        } catch (Exception) {
          return;
        }

        _ = Task.Run(() => HandleAsync(connection));
      }
    }

    /// <summary>Answers every request on the connection with 407 until the client closes it.</summary>
    private async Task HandleAsync(TcpClient connection) {
      using (connection) {
        var stream = connection.GetStream();
        List<string> lines;
        while ((lines = await ReadHeaderAsync(stream)).Count > 0) {
          lock (_lock) {
            _connectRequests.Add(lines);
          }

          var bytes = Encoding.ASCII.GetBytes(
                "HTTP/1.1 407 Proxy Authentication Required\r\nProxy-Authenticate: Basic realm=\"test\"\r\n" +
                "Content-Length: 0\r\n\r\n");
          await stream.WriteAsync(bytes, 0, bytes.Length);
        }
      }
    }

    private static async Task<List<string>> ReadHeaderAsync(NetworkStream stream) {
      var lines = new List<string>();
      var line = new StringBuilder();
      var buffer = new byte[1];
      while (await stream.ReadAsync(buffer, 0, 1) == 1) {
        if (buffer[0] != '\n') {
          line.Append((char)buffer[0]);
          continue;
        }

        var text = line.ToString().TrimEnd('\r');
        line.Clear();
        if (text.Length == 0) {
          break;
        }

        lines.Add(text);
      }

      return lines;
    }
  }
}
