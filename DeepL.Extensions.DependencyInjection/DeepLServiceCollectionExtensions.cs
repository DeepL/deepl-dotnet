// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DeepL.Extensions.DependencyInjection {
  /// <summary>
  ///   Extension methods on <see cref="IServiceCollection" /> for registering <see cref="DeepLClient" />
  ///   (and its surface interfaces) into a dependency injection container.
  /// </summary>
  /// <example>
  ///   <code>
  ///     // Bind from configuration section "DeepL"
  ///     builder.Services.AddDeepLClient(builder.Configuration);
  ///
  ///     // Configure inline
  ///     builder.Services.AddDeepLClient(o => {
  ///       o.AuthKey   = "your-key-here";
  ///       o.ServerUrl = "https://api.deepl.com";
  ///     });
  ///
  ///     // Consume via constructor injection
  ///     public class TranslationHandler(ITranslator translator) { ... }
  ///   </code>
  /// </example>
  public static class DeepLServiceCollectionExtensions {
    /// <summary>
    ///   Registers <see cref="DeepLClient" /> as a singleton, routed through <see cref="IHttpClientFactory" />.
    ///   Consumers can then inject the narrowest interface they need
    ///   (<see cref="ITranslator" />, <see cref="IWriter" />, <see cref="IGlossaryManager" />,
    ///   <see cref="IStyleRuleManager" />).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Delegate to configure <see cref="DeepLOptions" />.</param>
    /// <returns>The original <paramref name="services" /> for chaining.</returns>
    public static IServiceCollection AddDeepLClient(
          this IServiceCollection services,
          Action<DeepLOptions> configure) {
      if (services == null) throw new ArgumentNullException(nameof(services));
      if (configure == null) throw new ArgumentNullException(nameof(configure));

      services.AddOptions<DeepLOptions>()
            .Configure(configure)
            .Validate(o => !string.IsNullOrWhiteSpace(o.AuthKey), "DeepLOptions.AuthKey must be set.");

      RegisterCore(services);
      return services;
    }

    /// <summary>
    ///   Registers <see cref="DeepLClient" /> as a singleton, binding <see cref="DeepLOptions" /> from the
    ///   supplied configuration. Defaults to the <see cref="DeepLOptions.DefaultSectionName" /> section
    ///   (<c>"DeepL"</c>) unless an explicit section is passed in.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">
    ///   Either a root <see cref="IConfiguration" /> (the <c>"DeepL"</c> section is read) or a specific
    ///   <see cref="IConfigurationSection" /> containing the options.
    /// </param>
    /// <returns>The original <paramref name="services" /> for chaining.</returns>
    public static IServiceCollection AddDeepLClient(
          this IServiceCollection services,
          IConfiguration configuration) {
      if (services == null) throw new ArgumentNullException(nameof(services));
      if (configuration == null) throw new ArgumentNullException(nameof(configuration));

      var section = configuration is IConfigurationSection s
            ? s
            : configuration.GetSection(DeepLOptions.DefaultSectionName);

      services.AddOptions<DeepLOptions>()
            .Bind(section)
            .Validate(o => !string.IsNullOrWhiteSpace(o.AuthKey), "DeepLOptions.AuthKey must be set.");

      RegisterCore(services);
      return services;
    }

    /// <summary>
    ///   Common registration shared by both <c>AddDeepLClient</c> overloads. Registers:
    ///   the named <see cref="HttpClient" />, the <see cref="DeepLClient" /> singleton,
    ///   and forwarders for every surface interface <see cref="DeepLClient" /> implements.
    /// </summary>
    private static void RegisterCore(IServiceCollection services) {
      services.AddHttpClient(DeepLOptions.HttpClientName);

      // DeepLClient is documented as thread-safe; singleton is the correct lifetime.
      services.TryAddSingleton<DeepLClient>(sp => {
        var opts = sp.GetRequiredService<IOptions<DeepLOptions>>().Value;
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

        var clientOptions = new DeepLClientOptions {
          ServerUrl = opts.ServerUrl,
          ClientFactory = () => new HttpClientAndDisposeFlag {
            HttpClient = httpClientFactory.CreateClient(DeepLOptions.HttpClientName),
            // IHttpClientFactory owns the HttpClient lifetime, not DeepLClient.
            DisposeClient = false,
          },
        };

        return new DeepLClient(opts.AuthKey, clientOptions);
      });

      // Expose every DeepLClient-implemented interface as resolvable against the same singleton.
      services.TryAddSingleton<ITranslator>(sp => sp.GetRequiredService<DeepLClient>());
      services.TryAddSingleton<IWriter>(sp => sp.GetRequiredService<DeepLClient>());
      services.TryAddSingleton<IGlossaryManager>(sp => sp.GetRequiredService<DeepLClient>());
      services.TryAddSingleton<IStyleRuleManager>(sp => sp.GetRequiredService<DeepLClient>());
    }
  }
}
