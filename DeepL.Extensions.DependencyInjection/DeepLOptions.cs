// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

namespace DeepL.Extensions.DependencyInjection {
  /// <summary>
  ///   Configuration contract for
  ///   <see cref="DeepLServiceCollectionExtensions.AddDeepLClient(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{DeepLOptions})" />
  ///   (and its overloads).
  ///   Typically populated from configuration:
  ///   <code>
  ///     services.AddDeepLClient(builder.Configuration.GetSection("DeepL"));
  ///   </code>
  ///   with a matching <c>appsettings.json</c> section:
  ///   <code>
  ///     "DeepL": {
  ///       "AuthKey": "...",
  ///       "ServerUrl": "https://api.deepl.com"
  ///     }
  ///   </code>
  /// </summary>
  public sealed class DeepLOptions {
    /// <summary>Default configuration section name (<c>"DeepL"</c>) used by the <c>IConfiguration</c> overload.</summary>
    public const string DefaultSectionName = "DeepL";

    /// <summary>
    ///   Name used when resolving the underlying <see cref="System.Net.Http.HttpClient" /> via
    ///   <see cref="System.Net.Http.IHttpClientFactory" />. Consumers can call
    ///   <see cref="Microsoft.Extensions.DependencyInjection.HttpClientBuilderExtensions" />
    ///   against this name to layer on additional handlers or policies.
    /// </summary>
    public const string HttpClientName = "DeepL";

    /// <summary>DeepL API auth key. Required.</summary>
    public string AuthKey { get; set; } = string.Empty;

    /// <summary>Optional override for the DeepL API server URL (for testing / proxying).</summary>
    public string? ServerUrl { get; set; }
  }
}
