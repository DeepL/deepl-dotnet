# DeepL.Extensions.DependencyInjection

`Microsoft.Extensions.DependencyInjection` integration for [DeepL.net](https://www.nuget.org/packages/DeepL.net).

## Install

```
dotnet add package DeepL.Extensions.DependencyInjection
```

Pulls in `DeepL.net` transitively.

## Usage

### Configure inline

```csharp
using DeepL;
using DeepL.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDeepLClient(options => {
    options.AuthKey   = builder.Configuration["DeepL:AuthKey"]!;
    options.ServerUrl = "https://api.deepl.com";          // optional
});
```

### Bind from configuration

```json
// appsettings.json
{
  "DeepL": {
    "AuthKey":   "your-key-here",
    "ServerUrl": "https://api.deepl.com"
  }
}
```

```csharp
// Binds from the "DeepL" section by default
builder.Services.AddDeepLClient(builder.Configuration);

// Or pass a specific section
builder.Services.AddDeepLClient(builder.Configuration.GetSection("Translation:DeepL"));
```

### Inject what you need

Register once, inject the narrowest interface:

```csharp
app.MapPost("/translate", async (ITranslator translator, string text, string target)
    => await translator.Translate(text).To(target));

// In services: constructor-inject IWriter, IGlossaryManager, IStyleRuleManager, IVoiceManager
// or the full DeepLClient if you need multiple surfaces.
```

## What the registration does

- Registers `DeepLClient` as a **singleton** (the client is documented thread-safe).
- Forwards `ITranslator`, `IWriter`, `IGlossaryManager`, `IStyleRuleManager`, `IVoiceManager` to the same singleton.
- Routes the underlying `HttpClient` through `IHttpClientFactory` with the named client `"DeepL"`, so you can layer on your own handlers:

```csharp
builder.Services.AddDeepLClient(o => o.AuthKey = key);

builder.Services.AddHttpClient(DeepLOptions.HttpClientName)
    .AddHttpMessageHandler<MyLoggingHandler>()
    .AddStandardResilienceHandler();
```

- Validates `AuthKey` via `IValidateOptions<>`, so a missing key surfaces at application start rather than on first translation.

## Versioning

Versions lockstep with `DeepL.net`. Upgrading the integration package always pulls in a matching main-library version.
