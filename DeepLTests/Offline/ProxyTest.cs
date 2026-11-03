// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using DeepL;
using Xunit;

namespace DeepLTests.Offline {
  /// <summary>
  ///   Sends an HTTPS request through a local forward proxy that rejects every CONNECT request, so the server is never
  ///   contacted and nothing leaves the machine.
  /// </summary>
  public sealed class ProxyTest : IDisposable {
    private readonly object _lock = new object();
    private readonly List<List<string>> _connectRequests = new List<List<string>>();
    private readonly LocalProxy _proxy;

    public ProxyTest() {
      _proxy = new LocalProxy(HandleAsync);
    }

    public void Dispose() => _proxy.Dispose();

    [Fact]
    public async Task DoesNotRetryProxyTunnelRejection() {
      using var client = new DeepLClient(
            "test-key",
            new DeepLClientOptions {
              ServerUrl = "https://api.deepl.test",
              Proxy = new Uri($"http://proxy-user:secret@127.0.0.1:{_proxy.Port}"),
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

    /// <summary>Answers every request on the connection with 407 until the client closes it.</summary>
    private async Task HandleAsync(NetworkStream stream) {
      List<string> lines;
      while ((lines = await LocalProxy.ReadHeaderAsync(stream)).Count > 0) {
        lock (_lock) {
          _connectRequests.Add(lines);
        }

        await LocalProxy.WriteAsync(stream, LocalProxy.ProxyAuthenticationRequired(closeConnection: false));
      }
    }
  }
}
