// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using DeepL;
using RichardSzalay.MockHttp;

namespace DeepLTests.Offline {
  /// <summary>
  ///   Base class of the offline tests. xUnit creates one instance per test, so the constructor and
  ///   <see cref="Dispose" /> run around every test: the network is blocked because the only HTTP handler is
  ///   <see cref="Http" /> and a request without a matching declaration fails, and a declared request that was never
  ///   sent fails the test.
  /// </summary>
  public abstract class OfflineTest : IDisposable {
    protected const string ApiUrl = "https://api.deepl.com";
    protected const string AuthKey = "test-key";

    protected OfflineTest() {
      Http.Fallback.Throw(new InvalidOperationException("No declared request matches"));
    }

    protected static readonly HttpMethod Patch = new HttpMethod("PATCH");

    protected MockHttpMessageHandler Http { get; } = new MockHttpMessageHandler();

    public void Dispose() => Http.VerifyNoOutstandingExpectation();

    /// <summary>Creates a client whose only HTTP handler is <see cref="Http" />.</summary>
    protected DeepLClient CreateClient(Action<DeepLClientOptions>? configure = null) {
      var options = new DeepLClientOptions {
        ServerUrl = ApiUrl,
        ClientFactory = () => new HttpClientAndDisposeFlag { HttpClient = Http.ToHttpClient(), DisposeClient = true }
      };
      configure?.Invoke(options);
      return new DeepLClient(AuthKey, options);
    }

    /// <summary>
    ///   Creates a client that retries like the default client does, with <see cref="Http" /> as the innermost handler.
    /// </summary>
    protected DeepLClient CreateClientWithRetries(
          int maximumNetworkRetries,
          TimeSpan perRetryConnectionTimeout,
          TimeSpan? overallConnectionTimeout = null) =>
          CreateClient(
                options => options.ClientFactory = () => DeepL.Internal.DeepLHttpClient.CreateDefaultHttpClient(
                      perRetryConnectionTimeout,
                      overallConnectionTimeout ?? TimeSpan.FromSeconds(60),
                      maximumNetworkRetries,
                      Http));

    /// <summary>Declares a request to the DeepL API that carries the authorization header.</summary>
    protected MockedRequest Expect(HttpMethod method, string path) =>
          Http.Expect(method, ApiUrl + "/" + path).WithHeaders("Authorization", "DeepL-Auth-Key " + AuthKey);

    /// <summary>Declares a request whose body is exactly the given form fields, in any order.</summary>
    protected MockedRequest ExpectForm(HttpMethod method, string path, params (string Key, string Value)[] fields) =>
          Expect(method, path)
                .WithExactFormData(fields.Select(field => new KeyValuePair<string, string>(field.Key, field.Value)));

    /// <summary>Declares a request whose body is exactly the given JSON text.</summary>
    protected MockedRequest ExpectJson(HttpMethod method, string path, string json) =>
          Expect(method, path)
                .WithHeaders("Content-Type", "application/json; charset=utf-8")
                .WithContent(json);

    /// <summary>
    ///   Declares a multipart upload whose parts are exactly the given (name, file name, text) triples, in order.
    /// </summary>
    protected MockedRequest ExpectMultipart(string path, params (string Name, string? FileName, string Value)[] parts) =>
          Expect(HttpMethod.Post, path)
                .WithHeaders("Accept", "application/json")
                .With(request => MultipartParts(request).SequenceEqual(parts));

    /// <summary>Reads the parts of a multipart request through the framework's own part objects.</summary>
    protected static (string Name, string? FileName, string Value)[] MultipartParts(HttpRequestMessage request) =>
          ((MultipartFormDataContent)request.Content!)
          .Select<HttpContent, (string Name, string? FileName, string Value)>(
                part => (part.Headers.ContentDisposition!.Name!, part.Headers.ContentDisposition.FileName,
                      part.ReadAsStringAsync().Result))
          .ToArray();

    /// <summary>Declares a request without a body.</summary>
    protected MockedRequest ExpectNoBody(HttpMethod method, string path) =>
          Expect(method, path).With(request => request.Content == null);
  }

  internal static class MockedRequestExtensions {
    /// <summary>Replies 200 with the given JSON body. Replies always carry a body, as real ones do.</summary>
    internal static void RespondJson(this MockedRequest request, string json, HttpStatusCode status = HttpStatusCode.OK) =>
          request.Respond(status, "application/json", json);
  }
}
