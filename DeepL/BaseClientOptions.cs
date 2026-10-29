// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace DeepL {
  /// <summary>
  ///   Connection options shared by <see cref="TranslatorOptions" />, <see cref="DeepLClientOptions" /> and
  ///   <see cref="DeepLAdminClientOptions" />.
  /// </summary>
  public abstract class BaseClientOptions {
    internal BaseClientOptions() { }

    /// <summary>
    ///   HTTP headers attached to every request to the DeepL API. By default no extra headers are used. The client adds
    ///   a User-Agent header unless one is set here, and always sets the Authorization header from the authentication
    ///   key; an Authorization header set here is ignored.
    /// </summary>
    public Dictionary<string, string?> Headers { get; set; } = new Dictionary<string, string?>();

    /// <summary>
    ///   The maximum number of failed attempts that the client will retry, per request. By default 5 retries are made.
    ///   Note: only errors due to transient conditions are retried. Only used if <see cref="ClientFactory" /> is unset.
    /// </summary>
    public int MaximumNetworkRetries { get; set; } = 5;

    /// <summary>
    ///   Timeout for HTTP requests including all retries and the waits between them. By default there is no overall
    ///   timeout (<see cref="System.Threading.Timeout.InfiniteTimeSpan" />): each attempt is limited by
    ///   <see cref="PerRetryConnectionTimeout" />. Only used if <see cref="ClientFactory" /> is unset.
    /// </summary>
    public TimeSpan OverallConnectionTimeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    ///   Timeout for each attempt of an HTTP request, the default is 10 seconds. An attempt may run longer, until the
    ///   time the client would wait before retrying it (the backoff delay), when that is longer. Only used if
    ///   <see cref="ClientFactory" /> is unset.
    /// </summary>
    public TimeSpan PerRetryConnectionTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    ///   The base URL for DeepL's API that may be overridden for testing purposes. By default <see cref="Translator" />
    ///   and <see cref="DeepLClient" /> select the correct DeepL API URL based on the user account type (free or paid),
    ///   and <see cref="DeepLAdminClient" /> uses <c>https://api.deepl.com</c>.
    /// </summary>
    public string? ServerUrl { get; set; }

    /// <summary>
    ///   Factory function returning an <see cref="HttpClient" /> to be used by the client for HTTP requests, and a flag
    ///   whether to call <see cref="HttpClient.Dispose" /> when the client is disposed. Override this function to provide
    ///   your own <see cref="HttpClient" />. Note that overriding this function will disable the built-in retrying of
    ///   failed requests, so you must provide your own retry policy.
    /// </summary>
    public Func<HttpClientAndDisposeFlag>? ClientFactory { get; set; }

    /// <summary>
    ///   <c>true</c> to send basic platform information (operating system and .NET runtime version) in the User-Agent
    ///   header (default), <c>false</c> to only send the library version.
    /// </summary>
    public bool sendPlatformInfo { get; set; } = true;

    /// <summary>Name and version of the application using this library, added to the User-Agent header if set.</summary>
    public AppInfo? appInfo { get; set; }
  }
}
