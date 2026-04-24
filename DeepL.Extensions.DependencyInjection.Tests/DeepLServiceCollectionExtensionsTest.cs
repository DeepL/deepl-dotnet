// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net.Http;
using DeepL.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DeepL.Extensions.DependencyInjection.Tests {
  /// <summary>
  ///   Tests for <see cref="DeepLServiceCollectionExtensions" />.
  ///   These are pure DI-container tests — no DeepL API is called, the configured auth key
  ///   is only used to construct the <see cref="DeepLClient" /> (construction is lazy-safe).
  /// </summary>
  public sealed class DeepLServiceCollectionExtensionsTest {
    private const string FakeKey = "00000000-0000-0000-0000-000000000000:fx";

    private static ServiceProvider BuildProvider(Action<IServiceCollection> configureServices) {
      var services = new ServiceCollection();
      configureServices(services);
      return services.BuildServiceProvider();
    }

    // ---------- Configure overload ----------

    [Fact]
    public void AddDeepLClient_ConfigureOverload_RegistersClient() {
      using var sp = BuildProvider(s => s.AddDeepLClient(o => o.AuthKey = FakeKey));

      var client = sp.GetService<DeepLClient>();

      Assert.NotNull(client);
    }

    [Fact]
    public void AddDeepLClient_RegistersAllSurfaceInterfaces() {
      using var sp = BuildProvider(s => s.AddDeepLClient(o => o.AuthKey = FakeKey));

      Assert.NotNull(sp.GetService<ITranslator>());
      Assert.NotNull(sp.GetService<IWriter>());
      Assert.NotNull(sp.GetService<IGlossaryManager>());
      Assert.NotNull(sp.GetService<IStyleRuleManager>());
      Assert.NotNull(sp.GetService<IVoiceManager>());
    }

    [Fact]
    public void AddDeepLClient_AllInterfacesResolveToSameSingleton() {
      using var sp = BuildProvider(s => s.AddDeepLClient(o => o.AuthKey = FakeKey));

      var client = sp.GetRequiredService<DeepLClient>();
      var translator = sp.GetRequiredService<ITranslator>();
      var writer = sp.GetRequiredService<IWriter>();
      var glossary = sp.GetRequiredService<IGlossaryManager>();
      var styleRule = sp.GetRequiredService<IStyleRuleManager>();
      var voice = sp.GetRequiredService<IVoiceManager>();

      Assert.Same(client, translator);
      Assert.Same(client, writer);
      Assert.Same(client, glossary);
      Assert.Same(client, styleRule);
      Assert.Same(client, voice);
    }

    [Fact]
    public void AddDeepLClient_SingletonLifetime_ReturnsSameInstance() {
      using var sp = BuildProvider(s => s.AddDeepLClient(o => o.AuthKey = FakeKey));

      var first = sp.GetRequiredService<DeepLClient>();
      var second = sp.GetRequiredService<DeepLClient>();

      Assert.Same(first, second);
    }

    [Fact]
    public void AddDeepLClient_MissingAuthKey_ThrowsOnResolve() {
      using var sp = BuildProvider(s => s.AddDeepLClient(o => o.AuthKey = ""));

      var ex = Assert.Throws<OptionsValidationException>(() => sp.GetRequiredService<DeepLClient>());
      Assert.Contains("AuthKey", ex.Message);
    }

    [Fact]
    public void AddDeepLClient_WhitespaceAuthKey_ThrowsOnResolve() {
      using var sp = BuildProvider(s => s.AddDeepLClient(o => o.AuthKey = "   "));

      Assert.Throws<OptionsValidationException>(() => sp.GetRequiredService<DeepLClient>());
    }

    [Fact]
    public void AddDeepLClient_UsesNamedHttpClientFromFactory() {
      using var sp = BuildProvider(s => s.AddDeepLClient(o => o.AuthKey = FakeKey));

      // Resolving DeepLClient invokes IHttpClientFactory.CreateClient("DeepL") during construction;
      // if the named client wasn't registered, the resolution would throw.
      var factory = sp.GetRequiredService<IHttpClientFactory>();
      using var namedClient = factory.CreateClient(DeepLOptions.HttpClientName);

      Assert.NotNull(namedClient);

      // And actually resolving DeepLClient (which goes through the factory) must succeed.
      var client = sp.GetRequiredService<DeepLClient>();
      Assert.NotNull(client);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddDeepLClient_ServerUrl_PropagatesToClient() {
      // Construct with a custom server URL and a captured HttpClient we can interrogate
      // via an ordinary HttpMessageHandler that simply records the request URI.
      var captured = new UriCapturingHandler();

      var services = new ServiceCollection();
      services.AddDeepLClient(o => {
        o.AuthKey = FakeKey;
        o.ServerUrl = "https://example.invalid/deepl/";
      });

      // Replace the named HttpClient's primary handler with the capturing one.
      services.AddHttpClient(DeepLOptions.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => captured);

      using var sp = services.BuildServiceProvider();
      var client = sp.GetRequiredService<DeepLClient>();

      // Fire a request and swallow the resulting exception. We only care that the request
      // hit the configured ServerUrl.
      try {
        await client.GetUsageAsync();
      } catch {
        /* expected — the fake handler returns an empty response the client can't parse */
      }

      Assert.NotNull(captured.LastRequestUri);
      Assert.StartsWith("https://example.invalid/deepl/", captured.LastRequestUri!.ToString());
    }

    [Fact]
    public void AddDeepLClient_Idempotent_SecondCallDoesNotDuplicateRegistrations() {
      // Consumers sometimes call AddDeepLClient in library extensions plus app startup.
      // TryAdd* semantics should make the second call a no-op.
      var services = new ServiceCollection();
      services.AddDeepLClient(o => o.AuthKey = FakeKey);
      services.AddDeepLClient(o => o.AuthKey = FakeKey);

      using var sp = services.BuildServiceProvider();
      var clients = sp.GetServices<DeepLClient>();

      Assert.Single(clients);
    }

    [Fact]
    public void AddDeepLClient_NullServices_Throws() {
      IServiceCollection? services = null;
      Assert.Throws<ArgumentNullException>(
            () => services!.AddDeepLClient(o => o.AuthKey = FakeKey));
    }

    [Fact]
    public void AddDeepLClient_NullConfigureDelegate_Throws() {
      var services = new ServiceCollection();
      Assert.Throws<ArgumentNullException>(
            () => services.AddDeepLClient((Action<DeepLOptions>)null!));
    }

    // ---------- Configuration overload ----------

    [Fact]
    public void AddDeepLClient_ConfigurationOverload_BindsFromDefaultSection() {
      var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
              ["DeepL:AuthKey"] = FakeKey,
              ["DeepL:ServerUrl"] = "https://api.deepl.com/"
            })
            .Build();

      using var sp = BuildProvider(s => s.AddDeepLClient(config));

      var opts = sp.GetRequiredService<IOptions<DeepLOptions>>().Value;
      Assert.Equal(FakeKey, opts.AuthKey);
      Assert.Equal("https://api.deepl.com/", opts.ServerUrl);
    }

    [Fact]
    public void AddDeepLClient_ConfigurationOverload_AcceptsExplicitSection() {
      var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
              ["Translation:DeepL:AuthKey"] = FakeKey,
            })
            .Build();

      using var sp = BuildProvider(s => s.AddDeepLClient(config.GetSection("Translation:DeepL")));

      var opts = sp.GetRequiredService<IOptions<DeepLOptions>>().Value;
      Assert.Equal(FakeKey, opts.AuthKey);
    }

    [Fact]
    public void AddDeepLClient_ConfigurationOverload_MissingKey_Throws() {
      var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

      using var sp = BuildProvider(s => s.AddDeepLClient(config));

      Assert.Throws<OptionsValidationException>(() => sp.GetRequiredService<DeepLClient>());
    }

    [Fact]
    public void AddDeepLClient_ConfigurationOverload_NullConfig_Throws() {
      var services = new ServiceCollection();
      Assert.Throws<ArgumentNullException>(
            () => services.AddDeepLClient((IConfiguration)null!));
    }

    // ---------- Test helpers ----------

    private sealed class UriCapturingHandler : HttpMessageHandler {
      public Uri? LastRequestUri { get; private set; }

      protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, System.Threading.CancellationToken cancellationToken) {
        LastRequestUri = request.RequestUri;
        return System.Threading.Tasks.Task.FromResult(
              new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("{}") });
      }
    }
  }
}
