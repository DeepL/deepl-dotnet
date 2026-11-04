// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DeepL;
using RichardSzalay.MockHttp;
using Xunit;

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

    /// <summary>
    ///   Asserts that every public <see cref="DeepLClient" /> overload of a method that <typeparamref name="T" /> declares
    ///   is a member of <typeparamref name="T" />.
    /// </summary>
    protected static void AssertClientOverloadsDeclaredOn<T>() {
      var map = typeof(DeepLClient).GetInterfaceMap(typeof(T));
      var names = new HashSet<string>(map.InterfaceMethods.Select(method => method.Name));
      var overloads = typeof(DeepLClient).GetMethods().Where(method => names.Contains(method.Name));
      Assert.All(overloads, method => Assert.Contains(method, map.TargetMethods));
    }

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
    protected DeepLClient CreateClientWithRetries(int maximumNetworkRetries, TimeSpan perRetryConnectionTimeout) =>
          CreateClient(
                options => options.ClientFactory = () => DeepL.Internal.DeepLHttpClient.CreateDefaultHttpClient(
                      new DeepLClientOptions {
                        MaximumNetworkRetries = maximumNetworkRetries,
                        PerRetryConnectionTimeout = perRetryConnectionTimeout
                      },
                      Http));

    /// <summary>Declares a request to the DeepL API that carries the authorization header.</summary>
    protected MockedRequest Expect(HttpMethod method, string path) =>
          Http.Expect(method, ApiUrl + "/" + path).WithHeaders("Authorization", "DeepL-Auth-Key " + AuthKey);

    /// <summary>Declares a request whose body is exactly the given form fields, in any order.</summary>
    protected MockedRequest ExpectForm(HttpMethod method, string path, params (string Key, string Value)[] fields) =>
          Expect(method, path)
                .WithExactFormData(fields.Select(field => new KeyValuePair<string, string>(field.Key, field.Value)));

    /// <summary>
    ///   Declares a request whose JSON body is an object with exactly the given fields, in any order. Each value must
    ///   serialize to the same JSON as the field in the body, so <c>true</c> does not match <c>"true"</c>.
    /// </summary>
    protected MockedRequest ExpectJsonFields(HttpMethod method, string path, params (string Key, object Value)[] fields) {
      var expected = fields.Select(field => (field.Key, JsonSerializer.Serialize(field.Value)))
            .OrderBy(field => field.Key, StringComparer.Ordinal)
            .ToList();
      return Expect(method, path)
            .WithHeaders("Content-Type", "application/json; charset=utf-8")
            .With(request => JsonFields(request.Content!.ReadAsStringAsync().Result).SequenceEqual(expected));
    }

    /// <summary>Returns the top-level fields of a JSON object as (name, raw JSON value) pairs, ordered by name.</summary>
    private static List<(string Key, string Value)> JsonFields(string json) {
      using var document = JsonDocument.Parse(json);
      return document.RootElement.EnumerateObject()
            .Select(property => (property.Name, property.Value.GetRawText()))
            .OrderBy(field => field.Name, StringComparer.Ordinal)
            .ToList();
    }

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

    /// <summary>Replies 200 with a JSON body that breaks off with an <see cref="IOException" /> after the given text.</summary>
    protected static Func<Task<HttpResponseMessage>> CutOffReply(string prefix) =>
          () => {
            var content = new StreamContent(new CutOffStream(Encoding.UTF8.GetBytes(prefix)));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
          };

    /// <summary>Stream returning the given bytes, then throwing an <see cref="IOException" /> like a reset connection.</summary>
    private sealed class CutOffStream : Stream {
      private readonly byte[] _prefix;
      private bool _prefixRead;

      public CutOffStream(byte[] prefix) => _prefix = prefix;

      public override bool CanRead => true;
      public override bool CanSeek => false;
      public override bool CanWrite => false;
      public override long Length => throw new NotSupportedException();

      public override long Position {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
      }

      public override int Read(byte[] buffer, int offset, int count) {
        if (_prefixRead) {
          throw new IOException("Connection reset by peer");
        }

        _prefixRead = true;
        var length = Math.Min(count, _prefix.Length);
        Array.Copy(_prefix, 0, buffer, offset, length);
        return length;
      }

      public override void Flush() {
      }

      public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
      public override void SetLength(long value) => throw new NotSupportedException();
      public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

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
