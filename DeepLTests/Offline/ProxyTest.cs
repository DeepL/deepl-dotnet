// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using Xunit;

namespace DeepLTests.Offline {
  /// <summary>
  ///   Sends requests through a local forward proxy. The server URL is plain HTTP, so the proxy receives the request
  ///   itself and answers it; nothing leaves the machine.
  /// </summary>
  public sealed class ProxyTest : IDisposable {
    private const string UsageReply = "{\"character_count\":1,\"character_limit\":2}";
    private readonly TcpListener _listener = new TcpListener(IPAddress.Loopback, 0);
    private readonly ConcurrentQueue<List<string>> _requests = new ConcurrentQueue<List<string>>();

    public ProxyTest() {
      _listener.Start();
      _ = Task.Run(AcceptLoopAsync);
    }

    public void Dispose() => _listener.Stop();

    private int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    [Fact]
    public async Task SendsRequestsThroughProxyWithCredentials() {
      using var client = new DeepLClient(
            "test-key",
            new DeepLClientOptions {
              ServerUrl = "http://api.deepl.test",
              Proxy = new Uri($"http://proxy-user:p%40ss@127.0.0.1:{Port}"),
              MaximumNetworkRetries = 0
            });

      var usage = await client.GetUsageAsync();

      Assert.Equal(1, usage.Character!.Count);
      var lastRequest = Assert.Single(_requests.ToArray(), request => request.Exists(IsProxyAuthorization));
      Assert.Equal("GET http://api.deepl.test/v2/usage HTTP/1.1", lastRequest[0]);
      Assert.Contains("Authorization: DeepL-Auth-Key test-key", lastRequest);
      Assert.Contains(
            "Proxy-Authorization: Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes("proxy-user:p@ss")),
            lastRequest);
    }

    private static bool IsProxyAuthorization(string line) =>
          line.StartsWith("Proxy-Authorization:", StringComparison.OrdinalIgnoreCase);

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

    /// <summary>Reads one request; asks for proxy credentials if it has none, otherwise answers with usage.</summary>
    private async Task HandleAsync(TcpClient connection) {
      using (connection) {
        var stream = connection.GetStream();
        var reader = new StreamReader(stream, Encoding.ASCII);
        var lines = new List<string>();
        string? line;
        while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync())) {
          lines.Add(line);
        }

        _requests.Enqueue(lines);
        var response = lines.Exists(IsProxyAuthorization)
              ? "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n" +
                $"Content-Length: {UsageReply.Length}\r\nConnection: close\r\n\r\n{UsageReply}"
              : "HTTP/1.1 407 Proxy Authentication Required\r\nProxy-Authenticate: Basic realm=\"test\"\r\n" +
                "Content-Length: 0\r\nConnection: close\r\n\r\n";
        var bytes = Encoding.ASCII.GetBytes(response);
        await stream.WriteAsync(bytes, 0, bytes.Length);
      }
    }
  }
}
