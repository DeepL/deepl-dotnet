// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using DeepL;
using DeepL.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeepL.Samples.DependencyInjection;

/// <summary>
///   A sample hosted service that demonstrates consuming DeepL via constructor injection.
///   Real applications would inject only the interface(s) they actually use
///   (e.g. <see cref="ITranslator" /> alone) rather than pulling in the full client.
/// </summary>
public sealed class TranslationService(
      ITranslator translator,
      IWriter writer,
      IGlossaryManager glossaryManager,
      ILogger<TranslationService> logger) : IHostedService {
  public async Task StartAsync(CancellationToken cancellationToken) {
    logger.LogInformation("Demo: translating via injected ITranslator");

    // The fluent extension methods are plain extensions over ITranslator / IWriter / etc.,
    // so they work the same with a DI-resolved instance as with a manually-constructed one.
    var greeting = await translator
          .Translate("Hello from dependency injection!")
          .From(LanguageCode.English)
          .To(LanguageCode.German)
          .WithCancellation(cancellationToken);

    logger.LogInformation("Translated: {Text}", greeting.Text);

    var improved = await writer
          .Rephrase("i maked an example of DI")
          .To(LanguageCode.EnglishAmerican)
          .WithTone("friendly")
          .WithCancellation(cancellationToken);

    logger.LogInformation("Rephrased:  {Text}", improved.Text);

    var glossaries = await glossaryManager.ListGlossariesAsync(cancellationToken);
    logger.LogInformation("Account has {Count} glossary/ies", glossaries.Length);
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
