# DeepL.net samples

Runnable samples that demonstrate how to use DeepL.net in .NET 8+ apps.

The samples are in their own solution (`DeepL.Samples.slnx`) so they don't affect the main library CI scope or the NuGet package. They reference the library by **project reference** — `dotnet build` against the sibling source — so any changes you make to the library are picked up automatically.

## Prerequisites

- .NET 8 SDK (or newer)
- A DeepL API auth key — free or pro. Set it in the environment before running:

  ```bash
  # bash / zsh
  export DEEPL_AUTH_KEY=your-key-here

  # PowerShell
  $env:DEEPL_AUTH_KEY = "your-key-here"

  # cmd
  set DEEPL_AUTH_KEY=your-key-here
  ```

## Samples

### 1. `FluentApi` — every fluent entry point

End-to-end console demo of the fluent API surface:

- text translation (single, batch, params, `IEnumerable`) with every option helper
- text rephrasing (style + tone)
- document translation — both one-shot `SaveTo` and the split upload / poll / download flow
- glossary management (create → inspect → merge → rename → delete, plus using a glossary in translation)
- style rule management (create with instructions → add/update instruction → rename → delete)

```bash
dotnet run --project samples/FluentApi
```

The sample creates temporary glossaries, style rules, and files, and cleans them all up in `finally` blocks. If a run is interrupted, any leftover `sample-*` glossaries on your account can be safely deleted manually.

### 2. `DependencyInjection` — idiomatic DI wire-up

Shows how to register `DeepLClient` into `Microsoft.Extensions.DependencyInjection` so consumers can inject the narrowest interface they need (`ITranslator`, `IWriter`, `IGlossaryManager`, `IStyleRuleManager`):

- `AddDeepLClient(options => ...)` / `AddDeepLClient(IConfiguration)` — from the `DeepL.Extensions.DependencyInjection` companion package
- Routes the underlying `HttpClient` through `IHttpClientFactory` so apps can layer on their own handlers / resilience / logging
- Registers the client as a singleton (it is thread-safe by design) and exposes every surface interface
- `TranslationService` — example `IHostedService` that pulls `ITranslator` / `IWriter` / `IGlossaryManager` out of DI and uses the same fluent extensions

```bash
dotnet run --project samples/DependencyInjection
```

This sample depends on the companion package `DeepL.Extensions.DependencyInjection`, which lives in its own project in this repo and ships as a separate NuGet package. It keeps the main `DeepL.net` package dependency-free for consumers who don't need DI.

### Adapting to ASP.NET Core

The DI sample uses the generic host (`Host.CreateApplicationBuilder`), but the `AddDeepLClient` registration is identical in an ASP.NET Core app:

```csharp
using DeepL;
using DeepL.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Bind from the "DeepL" configuration section
builder.Services.AddDeepLClient(builder.Configuration);

var app = builder.Build();

app.MapPost("/translate", async (ITranslator translator, string text, string target)
    => await translator.Translate(text).To(target));

app.Run();
```

## Building only the samples

```bash
dotnet build samples/DeepL.Samples.slnx
```

This also builds the library as a transitive dependency. To build the library alone, use the top-level `DeepL.net.sln`.
