// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using Xunit;

namespace DeepLTests.RealApi {
  /// <summary>
  ///   Sends a real request through a local forward proxy that requires Basic credentials and tunnels CONNECT
  ///   requests to the API.
  /// </summary>
  public sealed class ProxyTest : IDisposable {
    private const string ProxyAuthorizationHeader = "Proxy-Authorization:";
    private readonly TcpListener _listener = new TcpListener(IPAddress.Loopback, 0);
    private readonly object _lock = new object();
    private readonly List<(string Target, string Authorization)> _tunnels = new List<(string, string)>();

    public ProxyTest() {
      _listener.Start();
      _ = Task.Run(AcceptLoopAsync);
    }

    public void Dispose() => _listener.Stop();

    [Fact]
    public async Task TunnelsRequestsThroughProxyWithCredentials() {
      var authKey = Environment.GetEnvironmentVariable("DEEPL_AUTH_KEY");
      if (string.IsNullOrEmpty(authKey)) {
        throw new InvalidOperationException("The DEEPL_AUTH_KEY environment variable must be set to run the real-API tests.");
      }

      var serverUrl = Environment.GetEnvironmentVariable("DEEPL_SERVER_URL");
      if (string.IsNullOrEmpty(serverUrl)) {
        serverUrl = DeepLClient.IsFreeAccountAuthKey(authKey) ? "https://api-free.deepl.com" : "https://api.deepl.com";
      }

      var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
      using var client = new DeepLClient(
            authKey,
            new DeepLClientOptions {
              ServerUrl = serverUrl, Proxy = new Uri($"http://proxy-user:p%40s:s@127.0.0.1:{port}")
            });

      var usage = await client.GetUsageAsync();

      Assert.NotNull(usage);
      (string Target, string Authorization) tunnel;
      lock (_lock) {
        tunnel = Assert.Single(_tunnels);
      }

      Assert.Equal($"{new Uri(serverUrl).Host}:443", tunnel.Target);
      Assert.Equal(
            "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes("proxy-user:p@s:s")),
            tunnel.Authorization);
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

    /// <summary>
    ///   Answers a CONNECT without credentials with 407; otherwise records it, connects to the target and copies bytes
    ///   both ways until either side closes.
    /// </summary>
    private async Task HandleAsync(TcpClient connection) {
      using (connection) {
        var stream = connection.GetStream();
        var lines = await ReadHeaderAsync(stream);
        var requestLine = lines.Count > 0 ? lines[0].Split(' ') : new string[0];
        var authorization = lines.Find(
              line => line.StartsWith(ProxyAuthorizationHeader, StringComparison.OrdinalIgnoreCase));
        if (requestLine.Length != 3 || requestLine[0] != "CONNECT" || authorization == null) {
          await WriteAsync(
                stream,
                "HTTP/1.1 407 Proxy Authentication Required\r\nProxy-Authenticate: Basic realm=\"test\"\r\n" +
                "Content-Length: 0\r\nConnection: close\r\n\r\n");
          return;
        }

        var target = requestLine[1];
        lock (_lock) {
          _tunnels.Add((target, authorization.Substring(ProxyAuthorizationHeader.Length).Trim()));
        }

        var separator = target.LastIndexOf(':');
        using var upstream = new TcpClient();
        await upstream.ConnectAsync(target.Substring(0, separator), int.Parse(target.Substring(separator + 1)));
        await WriteAsync(stream, "HTTP/1.1 200 Connection established\r\n\r\n");
        var upstreamStream = upstream.GetStream();
        await Task.WhenAny(stream.CopyToAsync(upstreamStream), upstreamStream.CopyToAsync(stream));
      }
    }

    /// <summary>Reads the request header byte by byte, so that no tunnelled bytes are consumed.</summary>
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

    private static Task WriteAsync(NetworkStream stream, string response) {
      var bytes = Encoding.ASCII.GetBytes(response);
      return stream.WriteAsync(bytes, 0, bytes.Length);
    }
  }
}
