// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Fluent entry points for glossary management on <see cref="IGlossaryManager" />.
  /// </summary>
  /// <example>
  ///   <code>
  ///     // Create
  ///     var glossary = await client
  ///       .CreateGlossary("My glossary")
  ///       .WithDictionary("en", "de", entries)
  ///       .WithDictionary("de", "en", reverseEntries)
  ///       .CreateAsync();
  ///
  ///     // Inspect / modify
  ///     var info    = await client.Glossary(id).GetAsync();
  ///     await client.Glossary(id).RenameAsync("new name");
  ///     await client.Glossary(id).DeleteAsync();
  ///
  ///     // Dictionary-level operations
  ///     var entries = await client.Glossary(id).Dictionary("en", "de").GetEntriesAsync();
  ///     await client.Glossary(id).Dictionary("en", "de").ReplaceAsync(newEntries);
  ///     await client.Glossary(id).Dictionary("en", "de").MergeAsync(extraEntries);
  ///     await client.Glossary(id).Dictionary("en", "de").DeleteAsync();
  ///   </code>
  /// </example>
  public static class FluentGlossaryExtensions {
    /// <summary>Lists all glossaries on the account.</summary>
    public static Task<MultilingualGlossaryInfo[]> ListGlossariesAsync(
          this IGlossaryManager manager,
          CancellationToken cancellationToken = default) {
      if (manager == null) throw new ArgumentNullException(nameof(manager));
      return manager.ListMultilingualGlossariesAsync(cancellationToken);
    }

    /// <summary>Returns a fluent reference to the glossary with the given ID.</summary>
    public static GlossaryRef Glossary(this IGlossaryManager manager, string glossaryId) {
      if (manager == null) throw new ArgumentNullException(nameof(manager));
      if (string.IsNullOrWhiteSpace(glossaryId)) {
        throw new ArgumentException($"Parameter {nameof(glossaryId)} must not be empty", nameof(glossaryId));
      }

      return new GlossaryRef(manager, glossaryId);
    }

    /// <summary>Returns a fluent reference for the supplied glossary.</summary>
    public static GlossaryRef Glossary(this IGlossaryManager manager, MultilingualGlossaryInfo glossary) {
      if (manager == null) throw new ArgumentNullException(nameof(manager));
      if (glossary == null) throw new ArgumentNullException(nameof(glossary));
      return new GlossaryRef(manager, glossary.GlossaryId);
    }

    /// <summary>Begins a fluent glossary-creation builder.</summary>
    public static GlossaryCreateBuilder CreateGlossary(this IGlossaryManager manager, string name) {
      if (manager == null) throw new ArgumentNullException(nameof(manager));
      if (string.IsNullOrWhiteSpace(name)) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty", nameof(name));
      }

      return new GlossaryCreateBuilder(manager, name);
    }
  }

  /// <summary>Fluent reference for an existing glossary. Operations execute when awaited.</summary>
  public sealed class GlossaryRef {
    private readonly IGlossaryManager _manager;

    internal GlossaryRef(IGlossaryManager manager, string glossaryId) {
      _manager = manager;
      GlossaryId = glossaryId;
    }

    /// <summary>ID of the glossary this reference targets.</summary>
    public string GlossaryId { get; }

    /// <summary>Retrieves glossary metadata.</summary>
    public Task<MultilingualGlossaryInfo> GetAsync(CancellationToken cancellationToken = default) =>
          _manager.GetMultilingualGlossaryAsync(GlossaryId, cancellationToken);

    /// <summary>Renames the glossary.</summary>
    public Task<MultilingualGlossaryInfo> RenameAsync(string name, CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(name)) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty", nameof(name));
      }

      return _manager.UpdateMultilingualGlossaryNameAsync(GlossaryId, name, cancellationToken);
    }

    /// <summary>Deletes the glossary.</summary>
    public Task DeleteAsync(CancellationToken cancellationToken = default) =>
          _manager.DeleteMultilingualGlossaryAsync(GlossaryId, cancellationToken);

    /// <summary>Returns a fluent reference to a dictionary inside this glossary.</summary>
    public GlossaryDictionaryRef Dictionary(string sourceLanguageCode, string targetLanguageCode) {
      if (string.IsNullOrWhiteSpace(sourceLanguageCode)) {
        throw new ArgumentException(
              $"Parameter {nameof(sourceLanguageCode)} must not be empty", nameof(sourceLanguageCode));
      }

      if (string.IsNullOrWhiteSpace(targetLanguageCode)) {
        throw new ArgumentException(
              $"Parameter {nameof(targetLanguageCode)} must not be empty", nameof(targetLanguageCode));
      }

      return new GlossaryDictionaryRef(_manager, GlossaryId, sourceLanguageCode, targetLanguageCode);
    }

    /// <summary>Returns a fluent reference to a dictionary inside this glossary.</summary>
    public GlossaryDictionaryRef Dictionary(MultilingualGlossaryDictionaryInfo glossaryDict) {
      if (glossaryDict == null) throw new ArgumentNullException(nameof(glossaryDict));
      return Dictionary(glossaryDict.SourceLanguageCode, glossaryDict.TargetLanguageCode);
    }
  }

  /// <summary>Fluent reference for a single (source, target) dictionary inside a glossary.</summary>
  public sealed class GlossaryDictionaryRef {
    private readonly IGlossaryManager _manager;

    internal GlossaryDictionaryRef(
          IGlossaryManager manager,
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode) {
      _manager = manager;
      GlossaryId = glossaryId;
      SourceLanguageCode = sourceLanguageCode;
      TargetLanguageCode = targetLanguageCode;
    }

    public string GlossaryId { get; }
    public string SourceLanguageCode { get; }
    public string TargetLanguageCode { get; }

    /// <summary>Retrieves the dictionary entries.</summary>
    public Task<MultilingualGlossaryDictionaryEntries> GetEntriesAsync(
          CancellationToken cancellationToken = default) =>
          _manager.GetMultilingualGlossaryDictionaryEntriesAsync(
                GlossaryId,
                SourceLanguageCode,
                TargetLanguageCode,
                cancellationToken);

    /// <summary>Replaces the dictionary with the supplied entries (creates it if missing).</summary>
    public Task<MultilingualGlossaryDictionaryInfo> ReplaceAsync(
          GlossaryEntries entries,
          CancellationToken cancellationToken = default) {
      if (entries == null) throw new ArgumentNullException(nameof(entries));
      return _manager.ReplaceMultilingualGlossaryDictionaryAsync(
            GlossaryId,
            SourceLanguageCode,
            TargetLanguageCode,
            entries,
            cancellationToken);
    }

    /// <summary>Replaces the dictionary with CSV content (creates it if missing).</summary>
    public Task<MultilingualGlossaryDictionaryInfo> ReplaceFromCsvAsync(
          Stream csvFile,
          CancellationToken cancellationToken = default) {
      if (csvFile == null) throw new ArgumentNullException(nameof(csvFile));
      return _manager.ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
            GlossaryId,
            SourceLanguageCode,
            TargetLanguageCode,
            csvFile,
            cancellationToken);
    }

    /// <summary>Merges the supplied entries into the existing dictionary (creates it if missing).</summary>
    public Task<MultilingualGlossaryInfo> MergeAsync(
          GlossaryEntries entries,
          CancellationToken cancellationToken = default) {
      if (entries == null) throw new ArgumentNullException(nameof(entries));
      return _manager.UpdateMultilingualGlossaryDictionaryAsync(
            GlossaryId,
            SourceLanguageCode,
            TargetLanguageCode,
            entries,
            cancellationToken);
    }

    /// <summary>Merges the supplied CSV content into the existing dictionary.</summary>
    public Task<MultilingualGlossaryInfo> MergeFromCsvAsync(
          Stream csvFile,
          CancellationToken cancellationToken = default) {
      if (csvFile == null) throw new ArgumentNullException(nameof(csvFile));
      return _manager.UpdateMultilingualGlossaryDictionaryFromCsvAsync(
            GlossaryId,
            SourceLanguageCode,
            TargetLanguageCode,
            csvFile,
            cancellationToken);
    }

    /// <summary>Deletes the dictionary from the glossary.</summary>
    public Task DeleteAsync(CancellationToken cancellationToken = default) =>
          _manager.DeleteMultilingualGlossaryDictionaryAsync(
                GlossaryId,
                SourceLanguageCode,
                TargetLanguageCode,
                cancellationToken);
  }

  /// <summary>
  ///   Fluent builder for creating a glossary with one or more dictionaries.
  ///   Call <see cref="CreateAsync" /> (or <c>await</c> directly) once dictionaries have been added.
  /// </summary>
  public sealed class GlossaryCreateBuilder {
    private readonly IGlossaryManager _manager;
    private readonly string _name;
    private readonly List<MultilingualGlossaryDictionaryEntries> _dictionaries =
          new List<MultilingualGlossaryDictionaryEntries>();
    private Stream? _csvStream;
    private string? _csvSourceLanguage;
    private string? _csvTargetLanguage;
    private CancellationToken _cancellationToken;

    internal GlossaryCreateBuilder(IGlossaryManager manager, string name) {
      _manager = manager;
      _name = name;
    }

    /// <summary>Adds a dictionary to the glossary being created.</summary>
    public GlossaryCreateBuilder WithDictionary(
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries) {
      if (entries == null) throw new ArgumentNullException(nameof(entries));
      EnsureNoCsv();
      _dictionaries.Add(
            new MultilingualGlossaryDictionaryEntries(sourceLanguageCode, targetLanguageCode, entries));
      return this;
    }

    /// <summary>Adds a pre-built dictionary to the glossary being created.</summary>
    public GlossaryCreateBuilder WithDictionary(MultilingualGlossaryDictionaryEntries dictionary) {
      if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
      EnsureNoCsv();
      _dictionaries.Add(dictionary);
      return this;
    }

    /// <summary>
    ///   Creates the glossary from a CSV stream. Mutually exclusive with <see cref="WithDictionary" />; the resulting
    ///   glossary will contain a single dictionary.
    /// </summary>
    public GlossaryCreateBuilder FromCsv(
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile) {
      if (csvFile == null) throw new ArgumentNullException(nameof(csvFile));
      if (_dictionaries.Count > 0) {
        throw new InvalidOperationException(
              "FromCsv cannot be combined with WithDictionary. Pick one way of providing entries.");
      }

      _csvStream = csvFile;
      _csvSourceLanguage = sourceLanguageCode;
      _csvTargetLanguage = targetLanguageCode;
      return this;
    }

    /// <summary>Associates a cancellation token with the create request.</summary>
    public GlossaryCreateBuilder WithCancellation(CancellationToken cancellationToken) {
      _cancellationToken = cancellationToken;
      return this;
    }

    /// <summary>Executes the glossary creation request.</summary>
    public Task<MultilingualGlossaryInfo> CreateAsync() {
      if (_csvStream != null) {
        return _manager.CreateMultilingualGlossaryFromCsvAsync(
              _name,
              _csvSourceLanguage!,
              _csvTargetLanguage!,
              _csvStream,
              _cancellationToken);
      }

      if (_dictionaries.Count == 0) {
        throw new InvalidOperationException(
              "At least one dictionary is required. Call WithDictionary(...) or FromCsv(...) before awaiting.");
      }

      return _manager.CreateMultilingualGlossaryAsync(_name, _dictionaries.ToArray(), _cancellationToken);
    }

    /// <summary>Enables direct <c>await</c> on the builder.</summary>
    public TaskAwaiter<MultilingualGlossaryInfo> GetAwaiter() => CreateAsync().GetAwaiter();

    public static implicit operator Task<MultilingualGlossaryInfo>(GlossaryCreateBuilder builder) =>
          builder?.CreateAsync() ?? throw new ArgumentNullException(nameof(builder));

    private void EnsureNoCsv() {
      if (_csvStream != null) {
        throw new InvalidOperationException(
              "WithDictionary cannot be combined with FromCsv. Pick one way of providing entries.");
      }
    }
  }
}
