// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
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
    private readonly object _lock = new object();
    private readonly List<(string Target, string Authorization)> _tunnels = new List<(string, string)>();
    private readonly LocalProxy _proxy;

    public ProxyTest() {
      _proxy = new LocalProxy(HandleAsync);
    }

    public void Dispose() => _proxy.Dispose();

    [Fact]
    public async Task TunnelsRequestsThroughProxyWithCredentials() {
      using var client = RealApiClient.CreateClient(
            new DeepLClientOptions { Proxy = new Uri($"http://proxy-user:p%40s:s@127.0.0.1:{_proxy.Port}") });

      var usage = await client.GetUsageAsync();

      Assert.NotNull(usage);
      (string Target, string Authorization) tunnel;
      lock (_lock) {
        tunnel = Assert.Single(_tunnels);
      }

      Assert.EndsWith(":443", tunnel.Target);
      if (RealApiClient.ServerUrl != null) {
        Assert.Equal($"{new Uri(RealApiClient.ServerUrl).Host}:443", tunnel.Target);
      }

      Assert.Equal(
            "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes("proxy-user:p@s:s")),
            tunnel.Authorization);
    }

    /// <summary>
    ///   Answers a CONNECT without credentials with 407; otherwise records it, connects to the target and copies bytes
    ///   both ways until either side closes.
    /// </summary>
    private async Task HandleAsync(NetworkStream stream) {
      var lines = await LocalProxy.ReadHeaderAsync(stream);
      var requestLine = lines.Count > 0 ? lines[0].Split(' ') : new string[0];
      var authorization = lines.Find(
            line => line.StartsWith(ProxyAuthorizationHeader, StringComparison.OrdinalIgnoreCase));
      if (requestLine.Length != 3 || requestLine[0] != "CONNECT" || authorization == null) {
        await LocalProxy.WriteAsync(stream, LocalProxy.ProxyAuthenticationRequired(closeConnection: true));
        return;
      }

      var target = requestLine[1];
      lock (_lock) {
        _tunnels.Add((target, authorization.Substring(ProxyAuthorizationHeader.Length).Trim()));
      }

      var separator = target.LastIndexOf(':');
      using var upstream = new TcpClient();
      await upstream.ConnectAsync(target.Substring(0, separator), int.Parse(target.Substring(separator + 1)));
      await LocalProxy.WriteAsync(stream, "HTTP/1.1 200 Connection established\r\n\r\n");
      var upstreamStream = upstream.GetStream();
      await Task.WhenAny(stream.CopyToAsync(upstreamStream), upstreamStream.CopyToAsync(stream));
    }
  }
}
