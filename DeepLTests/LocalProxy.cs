// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DeepLTests {
  /// <summary>
  ///   Minimal forward proxy on 127.0.0.1 for the proxy tests: accepts connections on a random port and passes each to
  ///   the given handler, closing it when the handler returns.
  /// </summary>
  internal sealed class LocalProxy : IDisposable {
    private readonly Func<NetworkStream, Task> _handleConnection;
    private readonly TcpListener _listener = new TcpListener(IPAddress.Loopback, 0);

    internal LocalProxy(Func<NetworkStream, Task> handleConnection) {
      _handleConnection = handleConnection;
      _listener.Start();
      _ = Task.Run(AcceptLoopAsync);
    }

    internal int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    public void Dispose() => _listener.Stop();

    /// <summary>A 407 response asking for Basic credentials, optionally closing the connection.</summary>
    internal static string ProxyAuthenticationRequired(bool closeConnection) =>
          "HTTP/1.1 407 Proxy Authentication Required\r\nProxy-Authenticate: Basic realm=\"test\"\r\n" +
          "Content-Length: 0\r\n" + (closeConnection ? "Connection: close\r\n" : "") + "\r\n";

    /// <summary>
    ///   Reads one request header byte by byte, so that no tunnelled bytes are consumed. Returns no lines when the
    ///   client closed the connection.
    /// </summary>
    internal static async Task<List<string>> ReadHeaderAsync(NetworkStream stream) {
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

    internal static Task WriteAsync(NetworkStream stream, string response) {
      var bytes = Encoding.ASCII.GetBytes(response);
      return stream.WriteAsync(bytes, 0, bytes.Length);
    }

    private async Task AcceptLoopAsync() {
      while (true) {
        TcpClient connection;
        try {
          connection = await _listener.AcceptTcpClientAsync();
        } catch (Exception) {
          return;
        }

        _ = Task.Run(
              async () => {
                using (connection) {
                  await _handleConnection(connection.GetStream());
                }
              });
      }
    }
  }
}
