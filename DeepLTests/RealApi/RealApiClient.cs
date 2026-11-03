// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DeepL;

namespace DeepLTests.RealApi {
  internal static class RealApiClient {
    /// <summary>
    ///   Creates a client for the API at <c>DEEPL_SERVER_URL</c> (the default server if unset), authenticating with
    ///   <c>DEEPL_AUTH_KEY</c>.
    /// </summary>
    /// <param name="options">Further client options; <c>ServerUrl</c> is set from <c>DEEPL_SERVER_URL</c>.</param>
    internal static DeepLClient CreateClient(DeepLClientOptions? options = null) {
      var authKey = Environment.GetEnvironmentVariable("DEEPL_AUTH_KEY");
      if (string.IsNullOrEmpty(authKey)) {
        throw new InvalidOperationException("The DEEPL_AUTH_KEY environment variable must be set to run the real-API tests.");
      }

      options ??= new DeepLClientOptions();
      var serverUrl = Environment.GetEnvironmentVariable("DEEPL_SERVER_URL");
      options.ServerUrl = string.IsNullOrEmpty(serverUrl) ? null : serverUrl;
      return new DeepLClient(authKey, options);
    }

    /// <summary>
    ///   The URL of the request that a client from <see cref="CreateClient" /> sends for <c>GetUsageAsync()</c>, found
    ///   without contacting the API.
    /// </summary>
    internal static async Task<Uri> GetUsageRequestUrlAsync() {
      var handler = new RecordingHandler();
      using var client = CreateClient(
            new DeepLClientOptions {
              ClientFactory = () => new HttpClientAndDisposeFlag {
                HttpClient = new HttpClient(handler), DisposeClient = true
              }
            });
      await client.GetUsageAsync();
      return handler.RequestUri!;
    }

    /// <summary>
    ///   Creates an admin client for the API at <c>DEEPL_SERVER_URL</c> (the default server if unset), authenticating
    ///   with <c>DEEPL_ADMIN_KEY</c>.
    /// </summary>
    internal static DeepLAdminClient CreateAdminClient() {
      var adminKey = Environment.GetEnvironmentVariable("DEEPL_ADMIN_KEY");
      if (string.IsNullOrEmpty(adminKey)) {
        throw new InvalidOperationException(
              "The DEEPL_ADMIN_KEY environment variable must be set to run the real-API admin tests.");
      }

      var serverUrl = Environment.GetEnvironmentVariable("DEEPL_SERVER_URL");
      return new DeepLAdminClient(
            adminKey,
            new DeepLAdminClientOptions { ServerUrl = string.IsNullOrEmpty(serverUrl) ? null : serverUrl });
    }

    /// <summary>Records the request URL and answers with an empty usage.</summary>
    private sealed class RecordingHandler : HttpMessageHandler {
      internal Uri? RequestUri { get; private set; }

      protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
        RequestUri = request.RequestUri;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
      }
    }
  }
}
