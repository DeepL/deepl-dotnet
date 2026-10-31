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
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Timeout;

// Lets the signed test assembly build clients on a fake inner handler.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DeepLTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100efa31804a684d88f767cd993a3984c13d81ae6c1a7ebac1d8b3ab34d874df688b9b7c2de8869bd649cef08edc02d9ec2214dcb0ea7559c7c8f7b5fbb01f71408f42e1a5bd831fc46a0db864ecc04476c6abbb73530a719247dbd17c592340ce1389b8b5bbc0bef9255bb0b96b9ed6e1721032be4b59398857de675ffb47eda96")]

namespace DeepL.Internal {
  /// <summary>Identifies the type of resource being accessed, used for contextual error messages.</summary>
  internal enum ResourceType {
    Glossary,
    StyleRule,
    TranslationMemory,
    SpokenTerms,
    StyleProfile
  }

  internal static class ResourceTypeExtensions {
    internal static string ToDisplayString(this ResourceType resourceType) =>
          resourceType switch {
            ResourceType.Glossary => "Glossary",
            ResourceType.StyleRule => "Style rule",
            ResourceType.TranslationMemory => "Translation memory",
            ResourceType.SpokenTerms => "Spoken terms",
            ResourceType.StyleProfile => "Style profile",
            _ => resourceType.ToString()
          };
  }

  /// <summary>Internal class implementing HTTP requests.</summary>
  internal class DeepLHttpClient : IDisposable {
    /// <summary>HTTP status code returned by DeepL API to indicate servers are currently under high load.</summary>
    private const HttpStatusCode HttpStatusCodeTooManyRequests = (HttpStatusCode)429;

    /// <summary>HTTP status code returned by DeepL API to indicate account translation quota has been exceeded.</summary>
    private const HttpStatusCode HttpStatusCodeQuotaExceeded = (HttpStatusCode)456;

    /// <summary>
    ///   Polly operation key marking requests that are unsafe to repeat: they are retried only after failures showing
    ///   that the server did not process them.
    /// </summary>
    private const string RetryOnlyUnprocessedOperationKey = "DeepL.RetryOnlyUnprocessed";

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

    /// <summary>Polly context key holding the index of the current attempt (0 = first attempt).</summary>
    private const string AttemptContextKey = "DeepL.Attempt";

    /// <summary>Polly context key holding the jittered backoff of the current attempt.</summary>
    private const string BackoffContextKey = "DeepL.Backoff";

    /// <summary>
    ///   Returns the backoff of the given attempt: 1 s without jitter for the first attempt; for each later attempt the
    ///   previous backoff times 1.6, at most 120 s, with ±23 % jitter.
    /// </summary>
    /// <param name="attempt">Zero-based number of the attempt.</param>
    /// <param name="rnd">Source of the jitter; locked while it is used.</param>
    /// <returns>The deadline of the attempt and the wait before the next one.</returns>
    internal static TimeSpan ComputeBackoff(int attempt, Random rnd) {
      const double backoffInitial = 1.0;
      const double backoffMaximum = 120.0;
      const double backoffJitter = 0.23;
      const double backoffMultiplier = 1.6;
      if (attempt == 0) {
        return TimeSpan.FromSeconds(backoffInitial);
      }

      var backoff = Math.Min(backoffInitial * Math.Pow(backoffMultiplier, attempt), backoffMaximum);
      lock (rnd) {
        backoff *= 1.0 + (backoffJitter * ((rnd.NextDouble() * 2.0) - 1.0));
      }

      return TimeSpan.FromSeconds(backoff);
    }

    /// <summary>
    ///   Creates a policy to retry failed HTTP requests wrapping the given handler.
    /// </summary>
    /// <param name="innerHandler"><see cref="HttpMessageHandler" /> on which requests should be retried.</param>
    /// <param name="perRetryConnectionTimeout">Minimum time allowed for each attempted request.</param>
    /// <param name="maximumNetworkRetries">Maximum number of retried requests.</param>
    /// <returns>An <see cref="HttpMessageHandler" /> comprising the inner handler wrapped with the retry policy.</returns>
    private static HttpMessageHandler CreateHttpMessageHandlerWithRetryPolicy(
          HttpMessageHandler innerHandler,
          TimeSpan perRetryConnectionTimeout,
          int maximumNetworkRetries) {
      if (perRetryConnectionTimeout <= TimeSpan.Zero &&
          perRetryConnectionTimeout != System.Threading.Timeout.InfiniteTimeSpan) {
        throw new ArgumentOutOfRangeException(
              nameof(BaseClientOptions.PerRetryConnectionTimeout),
              "must be positive or Timeout.InfiniteTimeSpan");
      }

      var rnd = new Random();
      var computeBackoff = new Func<int, TimeSpan>(attempt => ComputeBackoff(attempt, rnd));

      // The backoff of an attempt is both its deadline and the wait before the next attempt; the attempt may run
      // until that deadline even if it is longer than the configured timeout.
      var getBackoff = new Func<Context, TimeSpan>(
            context => {
              if (context.TryGetValue(BackoffContextKey, out var stored)) {
                return (TimeSpan)stored;
              }

              var attempt = context.TryGetValue(AttemptContextKey, out var value) ? (int)value : 0;
              var backoff = computeBackoff(attempt);
              context[BackoffContextKey] = backoff;
              return backoff;
            });
      var getSleepDuration = new Func<int, DelegateResult<HttpResponseMessage>, Context, TimeSpan>(
            (retryCount, _, context) => {
              var backoff = getBackoff(context);
              context.Remove(BackoffContextKey);
              context[AttemptContextKey] = retryCount;
              return backoff;
            });
      var onRetry = new Func<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context, Task>(
            (result, sleep, retryCount, context) => Task.CompletedTask);

      var timeout = Policy.TimeoutAsync<HttpResponseMessage>(
            context => {
              if (perRetryConnectionTimeout == System.Threading.Timeout.InfiniteTimeSpan) {
                return perRetryConnectionTimeout;
              }

              var backoff = getBackoff(context);
              return backoff > perRetryConnectionTimeout ? backoff : perRetryConnectionTimeout;
            });
      var waitAndRetry = Policy.Handle<TaskCanceledException>()
            .Or<TimeoutRejectedException>()
            .Or<HttpRequestException>(IsTransientNetworkFailure)
            .OrResult<HttpResponseMessage>(
                  responseMessage => responseMessage.StatusCode == HttpStatusCodeTooManyRequests ||
                                     responseMessage.StatusCode >= HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(maximumNetworkRetries, getSleepDuration, onRetry);
      IAsyncPolicy<HttpResponseMessage> policy = Policy.WrapAsync(waitAndRetry, timeout);
      var waitAndRetryUnprocessed = Policy.Handle<HttpRequestException>(IsCertainlyNotSent)
            .OrResult<HttpResponseMessage>(
                  responseMessage => responseMessage.StatusCode == HttpStatusCodeTooManyRequests)
            .WaitAndRetryAsync(maximumNetworkRetries, getSleepDuration, onRetry);
      IAsyncPolicy<HttpResponseMessage> unprocessedPolicy = Policy.WrapAsync(waitAndRetryUnprocessed, timeout);
      return new PolicyHttpMessageHandler(
            request => request.GetPolicyExecutionContext()?.OperationKey == RetryOnlyUnprocessedOperationKey
                  ? unprocessedPolicy
                  : policy) { InnerHandler = innerHandler };
    }

    /// <summary>
    ///   Whether the request failed because of a transient network failure that a retry could overcome: a failed
    ///   connect, a name resolution failure, a timeout, a reset or closed connection, an unreachable host or network.
    ///   TLS and certificate failures are not transient, and neither are failures without a socket or I/O cause,
    ///   such as a proxy rejecting the tunnel to the server.
    /// </summary>
    private static bool IsTransientNetworkFailure(HttpRequestException exception) {
      for (Exception? inner = exception.InnerException; inner != null; inner = inner.InnerException) {
        if (inner is AuthenticationException) {
          return false;
        }
      }

      for (Exception? inner = exception.InnerException; inner != null; inner = inner.InnerException) {
        switch (inner) {
          case SocketException _:
          case IOException _:
            return true;
          case WebException webException:
            switch (webException.Status) {
              case WebExceptionStatus.ConnectFailure:
              case WebExceptionStatus.NameResolutionFailure:
              case WebExceptionStatus.Timeout:
              case WebExceptionStatus.ConnectionClosed:
              case WebExceptionStatus.KeepAliveFailure:
              case WebExceptionStatus.ReceiveFailure:
              case WebExceptionStatus.SendFailure:
              case WebExceptionStatus.PipelineFailure:
                return true;
              case WebExceptionStatus.TrustFailure:
              case WebExceptionStatus.SecureChannelFailure:
              case WebExceptionStatus.ProxyNameResolutionFailure:
                return false;
            }

            break;
        }
      }

      return false;
    }

    /// <summary>
    ///   Whether the request failed before it was sent: the server name could not be resolved, the connection was
    ///   refused or connecting timed out.
    /// </summary>
    private static bool IsCertainlyNotSent(HttpRequestException exception) =>
          IsNameResolutionFailure(exception) || IsConnectionRefused(exception) || IsConnectTimeout(exception);

    /// <summary>
    ///   Whether the request failed because the server name could not be resolved, so it was never sent.
    /// </summary>
    private static bool IsNameResolutionFailure(HttpRequestException exception) {
      for (Exception? inner = exception; inner != null; inner = inner.InnerException) {
        switch (inner) {
          case SocketException socketException
                when socketException.SocketErrorCode == SocketError.HostNotFound ||
                     socketException.SocketErrorCode == SocketError.TryAgain ||
                     socketException.SocketErrorCode == SocketError.NoData:
          case WebException webException when webException.Status == WebExceptionStatus.NameResolutionFailure:
            return true;
        }
      }

      return false;
    }

    /// <summary>Whether the request failed because the server refused the connection, so it was never sent.</summary>
    private static bool IsConnectionRefused(HttpRequestException exception) {
      for (Exception? inner = exception; inner != null; inner = inner.InnerException) {
        if (inner is SocketException socketException &&
            socketException.SocketErrorCode == SocketError.ConnectionRefused) {
          return true;
        }
      }

      return false;
    }

    /// <summary>Whether connecting to the server timed out, so the request was never sent.</summary>
    private static bool IsConnectTimeout(HttpRequestException exception) {
      for (Exception? inner = exception; inner != null; inner = inner.InnerException) {
        switch (inner) {
          // Socket errors while sending or receiving are wrapped in an IOException; connect errors are not.
          case IOException _:
            return false;
          case SocketException socketException when socketException.SocketErrorCode == SocketError.TimedOut:
            return true;
        }
      }

      return false;
    }

    /// <summary>
    ///   Creates a default HttpClient with exponential-backoff policy for retrying failed requests, configured from the
    ///   given client options.
    /// </summary>
    /// <param name="options">Client options supplying retries, timeouts and proxy.</param>
    /// <param name="innerHandler">Handler sending the requests; by default an <see cref="HttpClientHandler" />.</param>
    /// <returns>Newly initialized <see cref="HttpClient" /> object.</returns>
    internal static HttpClientAndDisposeFlag CreateDefaultHttpClient(
          BaseClientOptions options,
          HttpMessageHandler? innerHandler = null) {
      var handler = CreateHttpMessageHandlerWithRetryPolicy(
            innerHandler ?? CreateHttpClientHandler(options.Proxy, options.PerRetryConnectionTimeout),
            options.PerRetryConnectionTimeout,
            options.MaximumNetworkRetries);
      return new HttpClientAndDisposeFlag {
        DisposeClient = true,
        HttpClient = new HttpClient(handler) { Timeout = options.OverallConnectionTimeout }
      };
    }

    /// <summary>
    ///   Creates the handler sending requests, using the given proxy if set. On .NET 5 and later, connecting is limited
    ///   to half of the per-attempt timeout, so that a hanging connect fails as a connect timeout before the attempt
    ///   times out: unlike a per-attempt timeout, a connect timeout shows that the request was not sent.
    /// </summary>
    /// <param name="proxy">URL of the proxy, or <c>null</c> to use the system proxy settings.</param>
    /// <param name="perRetryConnectionTimeout">Timeout of each attempt, or an infinite timeout.</param>
    private static HttpMessageHandler CreateHttpClientHandler(Uri? proxy, TimeSpan perRetryConnectionTimeout) {
#if NET5_0_OR_GREATER
      var handler = new SocketsHttpHandler();
      if (perRetryConnectionTimeout > TimeSpan.Zero) {
        var connectTimeout = TimeSpan.FromTicks(perRetryConnectionTimeout.Ticks / 2);
        handler.ConnectCallback = (context, cancellationToken) =>
              ConnectAsync(context.DnsEndPoint, connectTimeout, cancellationToken);
      }
#else
      var handler = new HttpClientHandler();
#endif
      if (proxy != null) {
        var webProxy = new WebProxy(proxy);
        if (!string.IsNullOrEmpty(proxy.UserInfo)) {
          var userInfo = proxy.UserInfo.Split(new[] { ':' }, 2);
          webProxy.Credentials = new NetworkCredential(
                Uri.UnescapeDataString(userInfo[0]),
                userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty);
        }

        handler.Proxy = webProxy;
        handler.UseProxy = true;
      }

      return handler;
    }

#if NET5_0_OR_GREATER
    /// <summary>
    ///   Opens a TCP connection to the given server or proxy like <see cref="SocketsHttpHandler" /> does by default, but
    ///   within the given time. A connect timeout is thrown as a <see cref="SocketException" /> with
    ///   <see cref="SocketError.TimedOut" />, which the handler wraps in an <see cref="HttpRequestException" />.
    /// </summary>
    private static async ValueTask<Stream> ConnectAsync(
          DnsEndPoint endPoint,
          TimeSpan connectTimeout,
          CancellationToken cancellationToken) {
      var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
      using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      timeoutSource.CancelAfter(connectTimeout);
      try {
        await socket.ConnectAsync(endPoint, timeoutSource.Token).ConfigureAwait(false);
        return new NetworkStream(socket, true);
      } catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
        socket.Dispose();
        throw new SocketException((int)SocketError.TimedOut);
      } catch {
        socket.Dispose();
        throw;
      }
    }
#endif

    /// <summary>Headers describing the request the client builds, which configured headers cannot replace.</summary>
    private static readonly string[] TechnicalHeaders = { "Content-Type", "Content-Length", "Transfer-Encoding", "Host" };

    /// <summary>
    ///   Creates a <see cref="DeepLHttpClient" /> configured from the given client options: the HTTP client from
    ///   <see cref="BaseClientOptions.ClientFactory" /> or a default client with retries, and the configured headers plus
    ///   the User-Agent and Authorization (carrying the given key) headers unless the configured headers already contain
    ///   them. Configured Content-Type, Content-Length, Transfer-Encoding and Host headers are ignored: they describe the
    ///   request the client builds.
    /// </summary>
    /// <param name="authKey">Authentication key sent in the Authorization header.</param>
    /// <param name="serverUrl">Base server URL to apply to all relative URLs in requests.</param>
    /// <param name="options">Client options.</param>
    /// <returns>Newly initialized <see cref="DeepLHttpClient" /> object.</returns>
    internal static DeepLHttpClient Create(string authKey, Uri serverUrl, BaseClientOptions options) {
      var headers = new Dictionary<string, string?>(options.Headers, StringComparer.OrdinalIgnoreCase);
      foreach (var technicalHeader in TechnicalHeaders) {
        headers.Remove(technicalHeader);
      }

      if (!headers.ContainsKey("User-Agent")) {
        headers.Add("User-Agent", ConstructUserAgentString(options.sendPlatformInfo, options.appInfo));
      }

      if (!headers.ContainsKey("Authorization")) {
        headers.Add("Authorization", $"DeepL-Auth-Key {authKey}");
      }

      var clientFactory = options.ClientFactory ?? (() => CreateDefaultHttpClient(options));

      return new DeepLHttpClient(serverUrl, clientFactory, headers);
    }

    /// <summary>
    ///   Builds the user-agent string we want to send to the API with every request. This string contains
    ///   basic information about the client environment, such as the deepl client library version, operating
    ///   system and language runtime version.
    /// </summary>
    /// <param name="sendPlatformInfo">
    ///   <c>true</c> to send platform information with every API request (default),
    ///   <c>false</c> to only send the library version.
    /// </param>
    /// <param name="appInfo">
    ///   Name and version of the application using this library. Ignored if null.
    /// </param>
    /// <returns>The user-agent string.</returns>
    private static string ConstructUserAgentString(bool sendPlatformInfo, AppInfo? appInfo) {
      var platformInfoString = $"deepl-dotnet/{DeepLClient.Version()}";
      if (sendPlatformInfo) {
        var osDescription = RuntimeInformation.OSDescription;
        var clrVersion = Environment.Version.ToString();
        platformInfoString += $" ({osDescription}) dotnet-clr/{clrVersion}";
      }

      if (appInfo != null) {
        platformInfoString += $" {appInfo?.AppName}/{appInfo?.AppVersion}";
      }

      return platformInfoString;
    }

    /// <summary>Checks the response HTTP status is 2xx, otherwise throws corresponding exception.</summary>
    /// <param name="responseMessage"><see cref="HttpResponseMessage" /> received from DeepL API.</param>
    /// <param name="usingGlossary"><c>true</c> if a glossary function is used, otherwise <c>false</c>.</param>
    /// <param name="downloadingDocument"><c>true</c> if document download function is used, otherwise <c>false</c>.</param>
    /// <exception cref="AuthorizationException">If authorization failed.</exception>
    /// <exception cref="QuotaExceededException">If the translation quota has been exceeded.</exception>
    /// <exception cref="NotFoundException">
    ///   If the specified resource was not found; on glossary endpoints the deprecated subclass
    ///   <see cref="GlossaryNotFoundException" />.
    /// </exception>
    /// <exception cref="TooManyRequestsException">If the DeepL servers are currently receiving too many requests.</exception>
    /// <exception cref="DocumentNotReadyException">If a document download returns HTTP 503.</exception>
    /// <exception cref="DeepLException">
    ///   If any other status than 2xx was returned, including a 3xx redirect the HTTP client did not follow.
    /// </exception>
    internal static async Task CheckStatusCodeAsync(
          HttpResponseMessage responseMessage,
          ResourceType? resourceType = null,
          bool downloadingDocument = false,
          bool retryOnlyUnprocessed = false) {
      if (responseMessage.IsSuccessStatusCode) {
        return;
      }

      var statusCode = responseMessage.StatusCode;

      string message;
      try {
        var errorResult = await JsonUtils.DeserializeAsync<ErrorResult>(responseMessage).ConfigureAwait(false);
        message = (errorResult.Message != null ? $", message: {errorResult.Message}" : "") +
                  (errorResult.Detail != null ? $", detail: {errorResult.Detail}" : "");
      } catch (DeepLException) {
        message = string.Empty;
      }

      var exception = CreateStatusException(statusCode, message, resourceType, downloadingDocument);
      exception.HttpStatusCode = (int)statusCode;
      // A request that is unsafe to repeat may have been processed despite a 5xx; only a 429 shows it was not.
      exception.ShouldRetry = statusCode == HttpStatusCodeTooManyRequests ||
                              (!retryOnlyUnprocessed && statusCode >= HttpStatusCode.InternalServerError);
      throw exception;
    }

    /// <summary>Creates the exception for an unsuccessful HTTP status.</summary>
    private static DeepLException CreateStatusException(
          HttpStatusCode statusCode,
          string message,
          ResourceType? resourceType,
          bool downloadingDocument) {
      switch (statusCode) {
        case HttpStatusCode.Unauthorized:
        case HttpStatusCode.Forbidden:
          return new AuthorizationException("Authorization failure, check AuthKey" + message);
        case HttpStatusCodeQuotaExceeded:
          return new QuotaExceededException("Quota for this billing period has been exceeded" + message);
        case HttpStatusCode.NotFound:
          var notFoundMessage = resourceType != null
                ? $"{resourceType.Value.ToDisplayString()} not found" + message
                : "Not found" + message;
#pragma warning disable CS0618 // glossary endpoints keep throwing the deprecated subclass until the next major
          return resourceType == ResourceType.Glossary
                ? new GlossaryNotFoundException(notFoundMessage)
                : new NotFoundException(notFoundMessage);
#pragma warning restore CS0618
        case HttpStatusCode.BadRequest:
          return new DeepLException("Bad request" + message);
        case HttpStatusCodeTooManyRequests:
          return new TooManyRequestsException(
                "Too many requests, DeepL servers are currently experiencing high load" + message);
        case HttpStatusCode.ServiceUnavailable:
          return downloadingDocument
                ? new DocumentNotReadyException("Document not ready" + message)
                : new DeepLException("Service unavailable" + message);
        default:
          return new DeepLException("Unexpected status code: " + statusCode + message);
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
        Content = bodyParams != null
#pragma warning disable CS0618 // internal use of a type obsolete only for library users
                  ? new LargeFormUrlEncodedContent(
                        bodyParams.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value)))
#pragma warning restore CS0618
                  : null
      };
      return await ApiCallAsync(requestMessage, cancellationToken);
    }

    /// <summary>Internal function to perform HTTP POST requests with JSON body.</summary>
    /// <param name="relativeUri">Endpoint URL relative to server base URL.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="body">Object to serialize as JSON for the request body.</param>
    /// <param name="jsonOptions">Optional JSON serializer options.</param>
    /// <param name="retryOnlyUnprocessed">
    ///   <c>true</c> for requests that are unsafe to repeat: they are retried only after HTTP 429, a name resolution
    ///   failure, a refused connection or a connect timeout, not after server errors or other timeouts.
    /// </param>
    /// <returns><see cref="HttpResponseMessage" /> received from DeepL API.</returns>
    /// <exception cref="ConnectionException">If any failure occurs while sending the request.</exception>
    public async Task<HttpResponseMessage> ApiPostJsonAsync(
          string relativeUri,
          CancellationToken cancellationToken,
          object body,
          JsonSerializerOptions? jsonOptions = null,
          bool retryOnlyUnprocessed = false) {
      var jsonBody = JsonSerializer.Serialize(body, jsonOptions);
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri),
        Method = HttpMethod.Post,
        Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
      };
      if (retryOnlyUnprocessed) {
        requestMessage.SetPolicyExecutionContext(new Context(RetryOnlyUnprocessedOperationKey));
      }

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
        Content = bodyParams != null
#pragma warning disable CS0618 // internal use of a type obsolete only for library users
                  ? new LargeFormUrlEncodedContent(
                        bodyParams.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value)))
#pragma warning restore CS0618
                  : null
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
      var jsonBody = JsonSerializer.Serialize(body, jsonOptions);
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri),
        Method = HttpMethod.Put,
        Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
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
        Method = new HttpMethod("PATCH"),
        Content = bodyParams != null
#pragma warning disable CS0618 // internal use of a type obsolete only for library users
                  ? new LargeFormUrlEncodedContent(
                        bodyParams.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value)))
#pragma warning restore CS0618
                  : null
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
      var jsonBody = JsonSerializer.Serialize(body, jsonOptions);
      using var requestMessage = new HttpRequestMessage {
        RequestUri = new Uri(_serverUrl, relativeUri),
        Method = new HttpMethod("PATCH"),
        Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
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

      if (file.CanSeek) {
        // StreamContent seeks back to the start position when the retry handler resends the request.
        content.Add(new StreamContent(file), "file", fileName);
      } else {
        // A non-seekable stream cannot be read again, so a retried request would fail; buffer it first.
        var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, 81920, cancellationToken).ConfigureAwait(false);
        content.Add(new ByteArrayContent(buffer.GetBuffer(), 0, (int)buffer.Length), "file", fileName);
      }

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

      var statusCode = responseMessage.StatusCode;
      var detail = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
      throw new DeepLException(
            $"Error {action}, HTTP status: {(int)statusCode}" +
            (string.IsNullOrEmpty(detail) ? "" : $", detail: {detail}")) {
        HttpStatusCode = (int)statusCode,
        ShouldRetry = statusCode == HttpStatusCodeTooManyRequests || statusCode >= HttpStatusCode.InternalServerError
      };
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
      var retryOnlyUnprocessed =
            requestMessage.GetPolicyExecutionContext()?.OperationKey == RetryOnlyUnprocessedOperationKey;
      try {
        foreach (var header in headers ?? _headers) {
          requestMessage.Headers.Add(header.Key, header.Value);
        }

        return await _httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
        // Distinguish cancellation due to user-provided token or request time-out
      } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        throw;
      } catch (TaskCanceledException ex) {
        throw new ConnectionException($"Request timed out: {ex.Message}", ex) { ShouldRetry = !retryOnlyUnprocessed };
      } catch (TimeoutRejectedException ex) {
        throw new ConnectionException($"Request timed out: {ex.Message}", ex) { ShouldRetry = !retryOnlyUnprocessed };
      } catch (HttpRequestException ex) {
        throw new ConnectionException($"Request failed: {ex.Message}", ex) {
          ShouldRetry = retryOnlyUnprocessed ? IsCertainlyNotSent(ex) : IsTransientNetworkFailure(ex)
        };
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
