// Copyright 2022 DeepL SE (https://www.deepl.com)
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
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Timeout;
#if NET8_0_OR_GREATER
using System.Net.Http.Json;
#endif

namespace DeepL.Internal {
  /// <summary>Identifies the type of resource being accessed, used for contextual error messages.</summary>
  internal enum ResourceType {
    Glossary,
    StyleRule,
    TranslationMemory
  }

  internal static class ResourceTypeExtensions {
    internal static string ToDisplayString(this ResourceType resourceType) =>
          resourceType switch {
            ResourceType.Glossary => "Glossary",
            ResourceType.StyleRule => "Style rule",
            ResourceType.TranslationMemory => "Translation memory",
            _ => resourceType.ToString()
          };
  }

  /// <summary>Internal class implementing HTTP requests.</summary>
  internal class DeepLHttpClient : IDisposable {
    /// <summary>HTTP status code returned by DeepL API to indicate servers are currently under high load.</summary>
    private const HttpStatusCode HttpStatusCodeTooManyRequests = (HttpStatusCode)429;

    /// <summary>HTTP status code returned by DeepL API to indicate account translation quota has been exceeded.</summary>
    private const HttpStatusCode HttpStatusCodeQuotaExceeded = (HttpStatusCode)456;

    /// <summary>PATCH HTTP verb (<see cref="HttpMethod.Patch" /> on net5+, fallback to string constructor on ns2.0).</summary>
    private static readonly HttpMethod HttpMethodPatch =
#if NET5_0_OR_GREATER
          HttpMethod.Patch;
#else
          new HttpMethod("PATCH");
#endif

    /// <summary>
    ///   Creates a JSON-serialized request body. Uses <see cref="JsonContent" /> on net8+ (streams directly,
    ///   skips the intermediate string allocation); falls back to <see cref="StringContent" /> on ns2.0.
    /// </summary>
    private static HttpContent CreateJsonContent(object body, JsonSerializerOptions? jsonOptions) {
#if NET8_0_OR_GREATER
      return JsonContent.Create(body, body?.GetType() ?? typeof(object), options: jsonOptions);
#else
      var jsonBody = JsonSerializer.Serialize(body, jsonOptions);
      return new StringContent(jsonBody, Encoding.UTF8, "application/json");
#endif
    }

    /// <summary>
    ///   Creates a form-URL-encoded request body. On net5+ uses the built-in <see cref="FormUrlEncodedContent" />
    ///   (the size-limit bug that originally required <see cref="LargeFormUrlEncodedContent" /> was fixed in .NET 5).
    /// </summary>
    private static HttpContent CreateFormContent(IEnumerable<(string Key, string Value)> bodyParams) {
      var pairs = bodyParams.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value));
#if NET5_0_OR_GREATER
      return new FormUrlEncodedContent(pairs);
#else
      return new LargeFormUrlEncodedContent(pairs);
#endif
    }

    /// <summary><c>true</c> if <see cref="_httpClient" /> should be disposed, otherwise <c>false</c>.</summary>
    private readonly bool _disposeClient;

    /// <summary>HTTP headers attached to every request.</summary>
    private readonly KeyValuePair<string, string?>[] _headers;

    /// <summary>
    ///   Headers for requests to pre-signed storage URLs, which are served by the Asset Store
    ///   rather than the DeepL API. Built from scratch rather than by removing
    ///   <c>Authorization</c> from <see cref="_headers" />, so neither the auth key nor any
    ///   caller-configured header can reach a third-party host by mistake.
    /// </summary>
    private readonly KeyValuePair<string, string?>[] _storageHeaders;

    /// <summary><see cref="HttpClient" /> used for requests to DeepL API.</summary>
    private readonly HttpClient _httpClient;

    /// <summary>The base URL for DeepL's API.</summary>
    private readonly Uri _serverUrl;

    /// <summary>Initializes a new <see cref="DeepLHttpClient" />.</summary>
    /// <param name="serverUrl">Base server URL to apply to all relative URLs in requests.</param>
    /// <param name="clientFactory">Factory function to obtain <see cref="HttpClient" /> used for requests.</param>
    /// <param name="headers">HTTP headers applied to all requests.</param>
    /// <exception cref="ArgumentNullException">If any argument is null.</exception>
    internal DeepLHttpClient(
          Uri serverUrl,
          Func<HttpClientAndDisposeFlag> clientFactory,
          IEnumerable<KeyValuePair<string, string?>> headers) {
      if (serverUrl == null) {
        throw new ArgumentNullException($"{nameof(serverUrl)}");
      }

      // Ensure the server URL ends with a trailing slash so that relative URI resolution
      // (RFC 3986 §5.2.2) appends path segments rather than replacing the last segment.
      // This is important when ServerUrl contains a path prefix such as a reverse-proxy base path.
      var serverUrlStr = serverUrl.ToString();
      _serverUrl = serverUrlStr.EndsWith("/") ? serverUrl : new Uri(serverUrlStr + "/");
      var clientAndDisposeFlag = clientFactory();
      _httpClient = clientAndDisposeFlag.HttpClient;
      _disposeClient = clientAndDisposeFlag.DisposeClient;

      if (_httpClient == null) {
        throw new ArgumentNullException(
              $"{nameof(clientAndDisposeFlag.HttpClient)}",
              $"HttpClient returned by {nameof(clientFactory)} was null");
      }

      _headers = headers.ToArray();
      _storageHeaders = _headers
            .Where(header => string.Equals(header.Key, "User-Agent", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    /// <summary>
    ///   Releases the unmanaged resources and disposes of the managed resources used by the
    ///   <see cref="DeepLHttpClient" />.
    /// </summary>
    public void Dispose() {
      if (_disposeClient) {
        _httpClient.Dispose();
      }
    }

    /// <summary>
    ///   Creates a policy to retry failed HTTP requests wrapping the given handler.
    /// </summary>
    /// <param name="innerHandler"><see cref="HttpMessageHandler" /> on which requests should be retried.</param>
    /// <param name="perRetryConnectionTimeout">Maximum time for each attempted request.</param>
    /// <param name="maximumNetworkRetries">Maximum number of retried requests.</param>
    /// <returns>An <see cref="HttpMessageHandler" /> comprising the inner handler wrapped with the retry policy.</returns>
    private static HttpMessageHandler CreateHttpMessageHandlerWithRetryPolicy(
          HttpMessageHandler innerHandler,
          TimeSpan perRetryConnectionTimeout,
          int maximumNetworkRetries) {
      var rnd = new Random();
      var getSleepDuration = new Func<int, TimeSpan>(
            retryCount => {
              const double backoffInitial = 1.0;
              const double backoffMaximum = 120.0;
              const double backoffJitter = 0.23;
              const double backoffMultiplier = 1.6;
              var backoff = backoffInitial * Math.Pow(backoffMultiplier, retryCount - 1);
              backoff = Math.Min(backoff, backoffMaximum);
              lock (rnd) {
                backoff *= 1.0 + (backoffJitter * ((rnd.NextDouble() * 2.0) - 1.0));
              }

              return TimeSpan.FromSeconds(backoff);
            });

      var timeout = Policy.TimeoutAsync<HttpResponseMessage>(perRetryConnectionTimeout);
      var waitAndRetry = Policy.Handle<TaskCanceledException>()
            .Or<TimeoutRejectedException>()
            .Or<HttpRequestException>(_ => false)
            .Or<Exception>()
            .OrResult<HttpResponseMessage>(
                  responseMessage => responseMessage.StatusCode == HttpStatusCodeTooManyRequests ||
                                     responseMessage.StatusCode >= HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(maximumNetworkRetries, getSleepDuration);
      var policy = Policy.WrapAsync(waitAndRetry, timeout);
      return new PolicyHttpMessageHandler(policy) { InnerHandler = innerHandler };
    }

    /// <summary>Creates a default HttpClient with exponential-backoff policy for retrying failed requests.</summary>
    /// <param name="perRetryConnectionTimeout">Connection timeout for each HTTP request.</param>
    /// <param name="overallConnectionTimeout">Timeout including all request-retries.</param>
    /// <param name="maximumNetworkRetries">Maximum number of failed requests that may be retried.</param>
    /// <returns>Newly initialized <see cref="HttpClient" /> object.</returns>
    public static HttpClientAndDisposeFlag CreateDefaultHttpClient(
          TimeSpan perRetryConnectionTimeout,
          TimeSpan overallConnectionTimeout,
          int maximumNetworkRetries) {
      var handler = CreateHttpMessageHandlerWithRetryPolicy(
            CreateInnerHandler(),
            perRetryConnectionTimeout,
            maximumNetworkRetries);
      var httpClient = new HttpClient(handler) { Timeout = overallConnectionTimeout };
#if NET8_0_OR_GREATER
      // Prefer HTTP/2 (the DeepL API supports it) and allow upgrade to HTTP/3 where available.
      // Gives proper request multiplexing for high-throughput batch translation.
      httpClient.DefaultRequestVersion = System.Net.HttpVersion.Version20;
      httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
#endif
      return new HttpClientAndDisposeFlag {
        DisposeClient = true,
        HttpClient = httpClient
      };
    }

    private static HttpMessageHandler CreateInnerHandler() {
#if NET8_0_OR_GREATER
      // SocketsHttpHandler is the modern managed handler; PooledConnectionLifetime forces periodic
      // socket recreation so DNS changes are picked up on long-lived HttpClient instances.
      return new SocketsHttpHandler {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
      };
#else
      return new HttpClientHandler();
#endif
    }

    /// <summary>Checks the response HTTP status is OK, otherwise throws corresponding exception.</summary>
    /// <param name="responseMessage"><see cref="HttpResponseMessage" /> received from DeepL API.</param>
    /// <param name="usingGlossary"><c>true</c> if a glossary function is used, otherwise <c>false</c>.</param>
    /// <param name="downloadingDocument"><c>true</c> if document download function is used, otherwise <c>false</c>.</param>
    /// <exception cref="AuthorizationException">If authorization failed.</exception>
    /// <exception cref="QuotaExceededException">If the translation quota has been exceeded.</exception>
    /// <exception cref="GlossaryNotFoundException">If the specified glossary was not found.</exception>
    /// <exception cref="GlossaryDictionaryNotFoundException">If the specified glossary dictionary was not found.</exception>
    /// <exception cref="TooManyRequestsException">If the DeepL servers are currently receiving too many requests.</exception>
    /// <exception cref="DeepLException">If some other error occurred.</exception>
    internal static async Task CheckStatusCodeAsync(
          HttpResponseMessage responseMessage,
          ResourceType? resourceType = null,
          bool downloadingDocument = false) {
      var statusCode = responseMessage.StatusCode;
      if (statusCode >= HttpStatusCode.OK && statusCode < HttpStatusCode.BadRequest) {
        return;
      }

      string message;
      try {
        var errorResult = await JsonUtils.DeserializeAsync<ErrorResult>(responseMessage).ConfigureAwait(false);
        message = (errorResult.Message != null ? $", message: {errorResult.Message}" : "") +
                  (errorResult.Detail != null ? $", detail: {errorResult.Detail}" : "");
      } catch (JsonException) {
        message = string.Empty;
      }

      switch (statusCode) {
        case HttpStatusCode.Forbidden:
          throw new AuthorizationException("Authorization failure, check AuthKey" + message);
        case HttpStatusCodeQuotaExceeded:
          throw new QuotaExceededException("Quota for this billing period has been exceeded" + message);
        case HttpStatusCode.NotFound:
          var notFoundMessage = resourceType != null
                ? $"{resourceType.Value.ToDisplayString()} not found" + message
                : "Not found" + message;
          if (resourceType == ResourceType.Glossary) {
            throw new GlossaryNotFoundException(notFoundMessage);
          }
          throw new NotFoundException(notFoundMessage);
        case HttpStatusCode.BadRequest:
          throw new DeepLException("Bad request" + message);
        case HttpStatusCodeTooManyRequests:
          throw new TooManyRequestsException(
                "Too many requests, DeepL servers are currently experiencing high load" + message);
        case HttpStatusCode.ServiceUnavailable:
          if (downloadingDocument) {
            throw new DocumentNotReadyException("Document not ready" + message);
          } else {
            throw new DeepLException("Service unavailable" + message);
          }
        default:
          throw new DeepLException("Unexpected status code: " + statusCode + message);
      }
    }

    /// <summary>Internal function to perform HTTP GET requests.</summary>
    /// <param name="relativeUri">Endpoint URL relative to server base URL.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="queryParams">Parameters to embed in the HTTP request query string.</param>
    /// <param name="acceptHeader">String to use as Accept header.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> ApiGetAsync(
          string relativeUri,
          CancellationToken cancellationToken,
          IEnumerable<(string Key, string Value)>? queryParams = null,
          string? acceptHeader = null) {
      var queryString = queryParams == null
            ? string.Empty
            : "?" + string.Join(
                  "&",
                  queryParams.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));

      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri + queryString),
        Method = HttpMethod.Get,
        Headers = { Accept = { new MediaTypeWithQualityHeaderValue(acceptHeader ?? "application/json") } }
      };
      return await ApiCallAsync(requestMessage, cancellationToken);
    }

    /// <summary>Internal function to perform HTTP DELETE requests.</summary>
    /// <param name="relativeUri">Endpoint URL relative to server base URL.</param>
    /// <param name="queryParams">Parameters to embed in the HTTP request query string.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> ApiDeleteAsync(
          string relativeUri,
          CancellationToken cancellationToken,
          IEnumerable<(string Key, string Value)>? queryParams = null) {
      var queryString = queryParams == null
            ? string.Empty
            : "?" + string.Join(
                  "&",
                  queryParams.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri + queryString),
        Method = HttpMethod.Delete
      };
      return await ApiCallAsync(requestMessage, cancellationToken);
    }

    /// <summary>Internal function to perform HTTP POST requests with form-encoded body.</summary>
    /// <param name="relativeUri">Endpoint URL relative to server base URL.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="bodyParams">Parameters to embed in the HTTP request body.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> ApiPostAsync(
          string relativeUri,
          CancellationToken cancellationToken,
          IEnumerable<(string Key, string Value)>? bodyParams = null) {
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri),
        Method = HttpMethod.Post,
        Content = bodyParams != null ? CreateFormContent(bodyParams) : null
      };
      return await ApiCallAsync(requestMessage, cancellationToken);
    }

    /// <summary>Internal function to perform HTTP POST requests with JSON body.</summary>
    /// <param name="relativeUri">Endpoint URL relative to server base URL.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="body">Object to serialize as JSON for the request body.</param>
    /// <param name="jsonOptions">Optional JSON serializer options.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> ApiPostJsonAsync(
          string relativeUri,
          CancellationToken cancellationToken,
          object body,
          JsonSerializerOptions? jsonOptions = null) {
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri),
        Method = HttpMethod.Post,
        Content = CreateJsonContent(body, jsonOptions)
      };
      return await ApiCallAsync(requestMessage, cancellationToken);
    }

    /// <summary>Internal function to perform HTTP PUT requests with form-encoded body.</summary>
    /// <param name="relativeUri">Endpoint URL relative to server base URL.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="bodyParams">Parameters to embed in the HTTP request body.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> ApiPutAsync(
          string relativeUri,
          CancellationToken cancellationToken,
          IEnumerable<(string Key, string Value)>? bodyParams = null) {
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri),
        Method = HttpMethod.Put,
        Content = bodyParams != null ? CreateFormContent(bodyParams) : null
      };
      return await ApiCallAsync(requestMessage, cancellationToken);
    }

    /// <summary>Internal function to perform HTTP PUT requests with JSON body.</summary>
    /// <param name="relativeUri">Endpoint URL relative to server base URL.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="body">Object to serialize as JSON for the request body.</param>
    /// <param name="jsonOptions">Optional JSON serializer options.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> ApiPutJsonAsync(
          string relativeUri,
          CancellationToken cancellationToken,
          object body,
          JsonSerializerOptions? jsonOptions = null) {
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri),
        Method = HttpMethod.Put,
        Content = CreateJsonContent(body, jsonOptions)
      };
      return await ApiCallAsync(requestMessage, cancellationToken);
    }

    /// <summary>Internal function to perform HTTP PATCH requests with form-encoded body.</summary>
    /// <param name="relativeUri">Endpoint URL relative to server base URL.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="bodyParams">Parameters to embed in the HTTP request body.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> ApiPatchAsync(
          string relativeUri,
          CancellationToken cancellationToken,
          IEnumerable<(string Key, string Value)>? bodyParams = null) {
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri),
        Method = HttpMethodPatch,
        Content = bodyParams != null ? CreateFormContent(bodyParams) : null
      };
      return await ApiCallAsync(requestMessage, cancellationToken);
    }

    /// <summary>Internal function to perform HTTP PATCH requests with JSON body.</summary>
    /// <param name="relativeUri">Endpoint URL relative to server base URL.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="body">Object to serialize as JSON for the request body.</param>
    /// <param name="jsonOptions">Optional JSON serializer options.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> ApiPatchJsonAsync(
          string relativeUri,
          CancellationToken cancellationToken,
          object body,
          JsonSerializerOptions? jsonOptions = null) {
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri),
        Method = HttpMethodPatch,
        Content = CreateJsonContent(body, jsonOptions)
      };
      return await ApiCallAsync(requestMessage, cancellationToken);
    }

    /// <summary>Internal function to upload files using an HTTP POST request.</summary>
    /// <param name="relativeUri">Endpoint URL relative to server base URL.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="bodyParams">Parameters to embed in the HTTP request body.</param>
    /// <param name="file">Optional file content to upload in request.</param>
    /// <param name="fileName">If <see cref="file" /> is used, the name of file.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> ApiUploadAsync(
          string relativeUri,
          CancellationToken cancellationToken,
          IEnumerable<(string Key, string Value)> bodyParams,
          Stream file,
          string fileName) {
      var content = new MultipartFormDataContent();
      foreach (var (key, value) in bodyParams) {
        content.Add(new StringContent(value), key);
      }

      content.Add(new StreamContent(file), "file", fileName);

      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri),
        Method = HttpMethod.Post,
        Content = content,
        Headers = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
      };
      return await ApiCallAsync(requestMessage, cancellationToken);
    }

    /// <summary>Internal function to perform HTTP requests against storage URLs handed out by the DeepL API.</summary>
    /// <param name="method">HTTP method to use for the request.</param>
    /// <param name="url">Absolute storage URL, for example a translation memory upload or download URL.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="content">Optional content to send in the request body.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from the storage server.</returns>
    /// <remarks>
    ///   These URLs are pre-signed and point outside the DeepL API, so the DeepL <c>Authorization</c> header is
    ///   deliberately not sent.
    /// </remarks>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> AssetCallAsync(
          HttpMethod method,
          string url,
          CancellationToken cancellationToken,
          HttpContent? content = null) {
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(url),
        Method = method,
        Content = content
      };
      return await ApiCallAsync(requestMessage, cancellationToken, _storageHeaders);
    }

    /// <summary>Checks the response HTTP status of a storage request is OK, otherwise throws an exception.</summary>
    /// <param name="responseMessage"><see cref="HttpResponseMessage" /> received from the storage server.</param>
    /// <param name="action">Description of the attempted action, included in the error message.</param>
    /// <exception cref="DeepLException">If the request was not successful.</exception>
    internal static async Task CheckAssetStatusCodeAsync(HttpResponseMessage responseMessage, string action) {
      if (responseMessage.IsSuccessStatusCode) {
        return;
      }

      var detail = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
      throw new DeepLException(
            $"Error {action}, HTTP status: {(int)responseMessage.StatusCode}" +
            (string.IsNullOrEmpty(detail) ? "" : $", detail: {detail}"));
    }

    /// <summary>Sends given HTTP request, ensuring message uses HTTP 2.0 and includes configured HTTP headers.</summary>
    /// <param name="requestMessage"><see cref="HttpRequestMessage" /> to send to the DeepL API.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="headers">Headers to attach; defaults to the configured DeepL API headers.</param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    private async Task<HttpResponseMessage> ApiCallAsync(
          HttpRequestMessage requestMessage,
          CancellationToken cancellationToken,
          KeyValuePair<string, string?>[]? headers = null) {
      try {
        foreach (var header in headers ?? _headers) {
          requestMessage.Headers.Add(header.Key, header.Value);
        }

        return await _httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
        // Distinguish cancellation due to user-provided token or request time-out
      } catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested) {
        throw;
      } catch (TaskCanceledException ex) {
        throw new ConnectionException($"Request timed out: {ex.Message}", ex);
      } catch (HttpRequestException ex) {
        throw new ConnectionException($"Request failed: {ex.Message}", ex);
      } catch (Exception ex) {
        throw new ConnectionException($"Unexpected request failure: {ex.Message}", ex);
      }
    }

    /// <summary>Class used for JSON-deserialization of error results.</summary>
    private readonly struct ErrorResult {
      /// <summary>Initializes a new instance of <see cref="ErrorResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public ErrorResult(string? message, string? detail) {
        Message = message;
        Detail = detail;
      }

      /// <summary>Message describing the error, if it was included in response.</summary>
      public string? Message { get; }

      /// <summary>String explaining more detail the error, if it was included in response.</summary>
      public string? Detail { get; }
    }
  }
}
