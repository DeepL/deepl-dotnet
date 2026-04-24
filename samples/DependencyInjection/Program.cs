// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

// Demonstrates consuming DeepL.net via Microsoft.Extensions.DependencyInjection and the
// generic host. The setup pattern transfers directly to ASP.NET Core apps — swap
// Host.CreateApplicationBuilder for WebApplication.CreateBuilder and the service
// registration is identical.
//
// Uses the companion DeepL.Extensions.DependencyInjection package for AddDeepLClient.
//
// Run with:
//   set DEEPL_AUTH_KEY=your-key-here
//   dotnet run --project samples/DependencyInjection

using DeepL.Extensions.DependencyInjection;
using DeepL.Samples.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Option A: configure inline
builder.Services.AddDeepLClient(options => {
  options.AuthKey = Environment.GetEnvironmentVariable("DEEPL_AUTH_KEY")
                    ?? throw new InvalidOperationException(
                          "Set DEEPL_AUTH_KEY to run this sample.");
});

// Option B (commented out): bind from appsettings.json with a "DeepL" section
// builder.Services.AddDeepLClient(builder.Configuration);

// Register the consumer-side IHostedService that pulls DeepL interfaces out of DI.
builder.Services.AddHostedService<TranslationService>();

var host = builder.Build();

// Drive the hosted service once, then shut down — this is a console sample, not a daemon.
// In a real long-lived app you'd call host.RunAsync() instead.
await host.StartAsync();
await host.StopAsync();
