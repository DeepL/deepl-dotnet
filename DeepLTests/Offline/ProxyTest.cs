// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
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

    /// <summary>Creates a style rule through a local proxy that handles each CONNECT with the given handler.</summary>
    private static async Task<(ConnectionException Exception, int Connects)> CreateThroughProxyAsync(
          Func<NetworkStream, Task> afterConnect) {
      var connects = 0;
      using var proxy = new LocalProxy(
            async stream => {
              if ((await LocalProxy.ReadHeaderAsync(stream)).Count > 0) {
                Interlocked.Increment(ref connects);
                await afterConnect(stream);
              }
            });
      using var client = new DeepLClient(
            "test-key",
            new DeepLClientOptions {
              ServerUrl = "https://api.deepl.test",
              Proxy = new Uri($"http://127.0.0.1:{proxy.Port}"),
              MaximumNetworkRetries = 1,
              PerRetryConnectionTimeout = TimeSpan.FromSeconds(1)
            });
      var exception = await Assert.ThrowsAsync<ConnectionException>(() => client.CreateStyleRuleAsync("n", "en"));
      return (exception, Volatile.Read(ref connects));
    }

#if NET5_0_OR_GREATER
    [Fact]
    public async Task RetriesCreateWhenProxyResetsBeforeAnsweringConnect() {
      var (exception, connects) = await CreateThroughProxyAsync(
            stream => {
              // Closed here with linger 0, a reset; the proxy's own close would shut the socket down first (FIN).
              stream.Socket.LingerState = new LingerOption(true, 0);
              stream.Socket.Close();
              return Task.CompletedTask;
            });

      Assert.True(exception.ShouldRetry);
      Assert.Equal(2, connects);
    }
#endif

    // The library's netstandard2.0 build has no separate connect timeout, so its retry waits for the same tunnel.
#if NET5_0_OR_GREATER && !DEEPL_NETSTANDARD2_0
    [Fact]
    public async Task RetriesCreateWhenProxyDoesNotAnswerConnect() {
      var (exception, connects) = await CreateThroughProxyAsync(_ => Task.Delay(TimeSpan.FromSeconds(5)));

      Assert.True(exception.ShouldRetry);
      Assert.Equal(2, connects);
    }
#endif

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
