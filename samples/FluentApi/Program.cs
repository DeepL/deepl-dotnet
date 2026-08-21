// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

// Demonstrates every fluent entry point the DeepL .NET client exposes:
//   - text translation (single + batch + options)
//   - text rephrasing
//   - document translation (one-shot + split upload/poll/download)
//   - glossary management (list / create / inspect / modify / delete)
//   - style rule management (list / create / inspect / instructions / delete)
//
// Run with:
//   set DEEPL_AUTH_KEY=your-key-here
//   dotnet run --project samples/FluentApi
//
// Each sample is self-contained — comment out the ones you don't want to run.

using DeepL;
using DeepL.Model;

var authKey = Environment.GetEnvironmentVariable("DEEPL_AUTH_KEY")
              ?? throw new InvalidOperationException(
                    "Set the DEEPL_AUTH_KEY environment variable to your DeepL API key.");

using var client = new DeepLClient(authKey);

await FluentTextExamples.RunAsync(client);
await FluentRephraseExamples.RunAsync(client);
await FluentDocumentExamples.RunAsync(client);
await FluentGlossaryExamples.RunAsync(client);
await FluentStyleRuleExamples.RunAsync(client);

Console.WriteLine();
Console.WriteLine("All samples completed.");


static class FluentTextExamples {
  public static async Task RunAsync(DeepLClient client) {
    Console.WriteLine("== Fluent text translation ==");

    // Simplest form: single text, target only (source auto-detected).
    var simple = await client.Translate("Hello, world!").To(LanguageCode.German);
    Console.WriteLine($"  simple       : {simple.Text}  [detected: {simple.DetectedSourceLanguageCode}]");

    // Explicit source, chain of option helpers.
    var styled = await client
          .Translate("Hello, team — quick reminder about tomorrow's meeting.")
          .From(LanguageCode.English)
          .To(LanguageCode.German)
          .WithFormality(Formality.More)
          .WithContext("Internal team chat message, friendly-but-professional tone.")
          .WithCustomInstructions("Keep it concise", "Do not translate proper names");
    Console.WriteLine($"  styled       : {styled.Text}");

    // Options-object overload — drop in a pre-built options instance.
    var prepared = new TextTranslateOptions {
      Formality = Formality.Less,
      PreserveFormatting = true,
    };
    var withOptions = await client.Translate("Hey!").To(LanguageCode.German).Using(prepared);
    Console.WriteLine($"  opts obj     : {withOptions.Text}");

    // Lambda overload — mutate options inline.
    var withLambda = await client
          .Translate("<p>Hello</p>")
          .To(LanguageCode.German)
          .Using(o => {
            o.TagHandling = "html";
            o.IgnoreTags.Add("code");
          });
    Console.WriteLine($"  lambda opts  : {withLambda.Text}");

    // Batch translation — returns TextResult[]
    var batch = await client.Translate("Good morning", "How are you?", "See you soon").To(LanguageCode.German);
    Console.WriteLine($"  batch        : [{string.Join(" | ", batch.Select(r => r.Text))}]");

    // Enumerable input + cancellation token.
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    var fromList = await client.Translate(new List<string> { "Yes", "No" })
          .From("en").To("de")
          .WithCancellation(cts.Token);
    Console.WriteLine($"  list+ct      : [{string.Join(" | ", fromList.Select(r => r.Text))}]");

    Console.WriteLine();
  }
}


static class FluentRephraseExamples {
  public static async Task RunAsync(DeepLClient client) {
    Console.WriteLine("== Fluent rephrase ==");

    var improved = await client
          .Rephrase("This text has some grammar mistake and stuff like that.")
          .To(LanguageCode.EnglishAmerican)
          .WithTone("friendly");
    Console.WriteLine($"  single       : {improved.Text}");

    var batch = await client
          .Rephrase(new[] { "i go store", "He don't like it" })
          .To(LanguageCode.EnglishBritish)
          .WithStyle("business");
    foreach (var r in batch) {
      Console.WriteLine($"  batch item   : {r.Text}");
    }

    Console.WriteLine();
  }
}


static class FluentDocumentExamples {
  public static async Task RunAsync(DeepLClient client) {
    Console.WriteLine("== Fluent document translation ==");

    // Create a tiny source "document" on disk so the sample is self-contained.
    var workDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "deepl-samples"));
    var input = new FileInfo(Path.Combine(workDir.FullName, "hello.txt"));
    var output = new FileInfo(Path.Combine(workDir.FullName, $"hello-{Guid.NewGuid():N}.txt"));
    await File.WriteAllTextAsync(input.FullName, "Hello, world. This is a sample document.");

    try {
      // One-shot: upload + poll + download, fluent options.
      await client
            .TranslateDocument(input)
            .From(LanguageCode.English)
            .To(LanguageCode.German)
            .WithFormality(Formality.More)
            .WithMinification()
            .SaveTo(output);

      Console.WriteLine($"  one-shot     : wrote {output.Length} bytes → {output.FullName}");

      // With progress callback: each status poll is reported via IProgress<DocumentStatus>.
      // Useful for UI progress bars, structured logging, webhook emissions.
      var inputP = new FileInfo(Path.Combine(workDir.FullName, "hello-progress.txt"));
      var outputP = new FileInfo(Path.Combine(workDir.FullName, $"hello-progress-{Guid.NewGuid():N}.txt"));
      await File.WriteAllTextAsync(inputP.FullName, "Third doc — monitored via IProgress.");

      var progress = new Progress<DocumentStatus>(status =>
            Console.WriteLine(
                  $"  progress     : {status.Status}  (remaining: {status.SecondsRemaining?.ToString() ?? "n/a"})"));

      await client
            .TranslateDocument(inputP)
            .To(LanguageCode.German)
            .WithProgress(progress)
            .SaveTo(outputP);
      Console.WriteLine($"  progress/done: wrote {outputP.Length} bytes → {outputP.FullName}");

      // Fluent cancellation: SaveTo() returns a DocumentTranslationJob that supports .Cancel()
      // without needing a pre-built CancellationTokenSource. The job is still awaitable.
      var inputC = new FileInfo(Path.Combine(workDir.FullName, "hello-cancel.txt"));
      var outputC = new FileInfo(Path.Combine(workDir.FullName, $"hello-cancel-{Guid.NewGuid():N}.txt"));
      await File.WriteAllTextAsync(inputC.FullName, "Fourth doc — will be cancelled mid-flight.");

      var job = client.TranslateDocument(inputC).To(LanguageCode.German).SaveTo(outputC);
      _ = Task.Delay(TimeSpan.FromMilliseconds(150)).ContinueWith(_ => {
        Console.WriteLine("  cancel       : requesting cancellation...");
        job.Cancel();
      });
      try {
        await job;
        Console.WriteLine("  cancel       : finished before cancel fired (race; may happen on small docs)");
      } catch (OperationCanceledException) {
        Console.WriteLine("  cancel       : job cancelled cleanly");
      }

      // Split flow: useful when you want to do work between upload and download
      // (e.g. queue a webhook, show a progress UI).
      var input2 = new FileInfo(Path.Combine(workDir.FullName, "hello2.txt"));
      var output2 = new FileInfo(Path.Combine(workDir.FullName, $"hello2-{Guid.NewGuid():N}.txt"));
      await File.WriteAllTextAsync(input2.FullName, "A second, independent document.");

      var handle = await client.TranslateDocument(input2).To(LanguageCode.German).UploadAsync();
      Console.WriteLine($"  split/upload : {handle.DocumentId}");

      // Poll status manually if you want progress output.
      var status = await client.Document(handle).GetStatusAsync();
      Console.WriteLine($"  split/status : {status.Status} (remaining: {status.SecondsRemaining?.ToString() ?? "n/a"})");

      // Or block until done (with optional progress reporter).
      await client.Document(handle).WaitUntilDoneAsync(progress);
      await client.Document(handle).DownloadToAsync(output2);
      Console.WriteLine($"  split/done   : wrote {output2.Length} bytes → {output2.FullName}");
    } finally {
      try { input.Delete(); } catch { /* ignored */ }
    }

    Console.WriteLine();
  }
}


static class FluentGlossaryExamples {
  public static async Task RunAsync(DeepLClient client) {
    Console.WriteLine("== Fluent glossary management ==");

    // List existing glossaries.
    var existing = await client.ListGlossariesAsync();
    Console.WriteLine($"  existing     : {existing.Length} glossary/ies on account");

    // Create a new glossary with two dictionaries (EN->DE and DE->EN).
    var enDe = new GlossaryEntries(new[] {
      ("hello", "hallo"),
      ("team", "Mannschaft"),
    });
    var deEn = new GlossaryEntries(new[] {
      ("hallo", "hello"),
      ("Mannschaft", "team"),
    });

    var glossaryName = $"sample-{Guid.NewGuid():N}";
    var created = await client
          .CreateGlossary(glossaryName)
          .WithDictionary("en", "de", enDe)
          .WithDictionary("de", "en", deEn);
    Console.WriteLine($"  created      : {created.Name} ({created.GlossaryId})");

    try {
      // Inspect the freshly created glossary.
      var info = await client.Glossary(created.GlossaryId).GetAsync();
      Console.WriteLine($"  inspected    : {info.Dictionaries.Length} dict(s)");

      // Pull the entries for a specific dictionary.
      var entries = await client.Glossary(created.GlossaryId).Dictionary("en", "de").GetEntriesAsync();
      Console.WriteLine($"  entries      : {entries.Entries.ToDictionary().Count} pair(s) in EN→DE");

      // Merge additional entries into an existing dictionary.
      var moreEntries = new GlossaryEntries(new[] { ("goodbye", "auf Wiedersehen") });
      await client.Glossary(created.GlossaryId).Dictionary("en", "de").MergeAsync(moreEntries);
      Console.WriteLine("  merged       : added 'goodbye' → 'auf Wiedersehen'");

      // Use the glossary in a translation (fluent WithGlossary).
      var translated = await client
            .Translate("Hello team, goodbye team!")
            .From("en").To("de")
            .WithGlossary(created);
      Console.WriteLine($"  applied      : {translated.Text}");

      // Rename.
      await client.Glossary(created.GlossaryId).RenameAsync(glossaryName + "-v2");
      Console.WriteLine("  renamed      : appended -v2");
    } finally {
      // Always clean up sample resources.
      await client.Glossary(created.GlossaryId).DeleteAsync();
      Console.WriteLine("  deleted      : sample glossary removed");
    }

    Console.WriteLine();
  }
}


static class FluentStyleRuleExamples {
  public static async Task RunAsync(DeepLClient client) {
    Console.WriteLine("== Fluent style-rule management ==");

    var existing = await client.ListStyleRulesAsync(detailed: false);
    Console.WriteLine($"  existing     : {existing.Length} style rule(s) on account");

    var ruleName = $"sample-style-{Guid.NewGuid():N}";
    var rule = await client
          .CreateStyleRule(ruleName)
          .ForLanguage("en")
          .WithInstruction("Friendly", "Write in a warm, friendly voice.")
          .WithInstruction("No jargon", "Avoid technical buzzwords.");
    Console.WriteLine($"  created      : {rule.Name} ({rule.StyleId})");

    try {
      // Add another instruction after creation.
      var added = await client.StyleRule(rule.StyleId)
            .AddInstructionAsync("Short", "Keep responses under 50 words.");
      Console.WriteLine($"  added instr  : {added.Label} ({added.Id})");

      // Update the instruction.
      if (added.Id is { } instrId) {
        await client.StyleRule(rule.StyleId).Instruction(instrId)
              .UpdateAsync("Short-and-snappy", "One sentence or less.");
        Console.WriteLine("  updated      : renamed + reworded instruction");
      }

      // Apply the style rule in a translation.
      var translated = await client
            .Translate("We are pleased to announce the imminent deployment of our newest SaaS offering.")
            .From("en").To("en-US")
            .WithStyle(rule);
      Console.WriteLine($"  applied      : {translated.Text}");

      // Rename.
      await client.StyleRule(rule.StyleId).RenameAsync(ruleName + "-v2");
      Console.WriteLine("  renamed      : appended -v2");
    } finally {
      await client.StyleRule(rule.StyleId).DeleteAsync();
      Console.WriteLine("  deleted      : sample style rule removed");
    }

    Console.WriteLine();
  }
}
