// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Fluent entry points for document translation on <see cref="ITranslator" />.
  /// </summary>
  /// <example>
  ///   <code>
  ///     // One-shot: upload, wait, download
  ///     await translator
  ///       .TranslateDocument(new FileInfo("input.docx"))
  ///       .To("de")
  ///       .From("en")
  ///       .WithFormality(Formality.More)
  ///       .WithGlossary(glossary)
  ///       .SaveTo(new FileInfo("output.docx"));
  ///
  ///     // Split flow
  ///     var handle = await translator.TranslateDocument(fileInfo).To("de").UploadAsync();
  ///     await translator.Document(handle).WaitUntilDoneAsync();
  ///     await translator.Document(handle).DownloadToAsync(new FileInfo("output.docx"));
  ///   </code>
  /// </example>
  public static class FluentDocumentTranslationExtensions {
    /// <summary>Starts a fluent document translation from a <see cref="FileInfo" /> input.</summary>
    public static DocumentTranslationBuilder TranslateDocument(
          this ITranslator translator, FileInfo inputFileInfo) {
      if (translator == null) throw new ArgumentNullException(nameof(translator));
      if (inputFileInfo == null) throw new ArgumentNullException(nameof(inputFileInfo));
      return new DocumentTranslationBuilder(translator, inputFileInfo);
    }

    /// <summary>Starts a fluent document translation from a <see cref="Stream" /> input.</summary>
    public static DocumentTranslationBuilder TranslateDocument(
          this ITranslator translator, Stream inputStream, string inputFileName) {
      if (translator == null) throw new ArgumentNullException(nameof(translator));
      if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
      if (string.IsNullOrWhiteSpace(inputFileName)) {
        throw new ArgumentException($"Parameter {nameof(inputFileName)} must not be empty", nameof(inputFileName));
      }

      return new DocumentTranslationBuilder(translator, inputStream, inputFileName);
    }

    /// <summary>Returns a fluent reference for an in-progress document translation.</summary>
    public static DocumentRef Document(this ITranslator translator, DocumentHandle handle) {
      if (translator == null) throw new ArgumentNullException(nameof(translator));
      return new DocumentRef(translator, handle);
    }
  }

  /// <summary>
  ///   Fluent builder for a document translation. Supports both the one-shot flow
  ///   (<see cref="SaveTo(FileInfo)" /> / <see cref="SaveTo(Stream)" />) and the split
  ///   upload/status/download flow (<see cref="UploadAsync" />).
  /// </summary>
  public sealed class DocumentTranslationBuilder {
    private readonly ITranslator _translator;
    private readonly FileInfo? _inputFileInfo;
    private readonly Stream? _inputStream;
    private readonly string? _inputFileName;
    private readonly DocumentTranslateOptions _options = new DocumentTranslateOptions();
    private string? _sourceLanguageCode;
    private string? _targetLanguageCode;
    private CancellationToken _cancellationToken;

    internal DocumentTranslationBuilder(ITranslator translator, FileInfo inputFileInfo) {
      _translator = translator;
      _inputFileInfo = inputFileInfo;
    }

    internal DocumentTranslationBuilder(ITranslator translator, Stream inputStream, string inputFileName) {
      _translator = translator;
      _inputStream = inputStream;
      _inputFileName = inputFileName;
    }

    /// <summary>Sets the target language code.</summary>
    public DocumentTranslationBuilder To(string targetLanguageCode) {
      _targetLanguageCode = targetLanguageCode ?? throw new ArgumentNullException(nameof(targetLanguageCode));
      return this;
    }

    /// <summary>Sets the source language code. Pass <c>null</c> to rely on auto-detection.</summary>
    public DocumentTranslationBuilder From(string? sourceLanguageCode) {
      _sourceLanguageCode = sourceLanguageCode;
      return this;
    }

    /// <summary>Copies fields from the supplied options object onto this builder.</summary>
    public DocumentTranslationBuilder Using(DocumentTranslateOptions options) {
      if (options == null) throw new ArgumentNullException(nameof(options));
      _options.Formality = options.Formality;
      _options.GlossaryId = options.GlossaryId;
      _options.EnableDocumentMinification = options.EnableDocumentMinification;
      _options.OutputFormat = options.OutputFormat;
      return this;
    }

    /// <summary>Mutates the options via the supplied delegate.</summary>
    public DocumentTranslationBuilder Using(Action<DocumentTranslateOptions> configure) {
      if (configure == null) throw new ArgumentNullException(nameof(configure));
      configure(_options);
      return this;
    }

    /// <summary>Sets the formality level.</summary>
    public DocumentTranslationBuilder WithFormality(Formality formality) {
      _options.Formality = formality;
      return this;
    }

    /// <summary>Uses the supplied glossary.</summary>
    public DocumentTranslationBuilder WithGlossary(GlossaryInfo glossary) {
      if (glossary == null) throw new ArgumentNullException(nameof(glossary));
      _options.GlossaryId = glossary.GlossaryId;
      return this;
    }

    /// <summary>Uses the supplied multilingual glossary.</summary>
    public DocumentTranslationBuilder WithGlossary(MultilingualGlossaryInfo glossary) {
      if (glossary == null) throw new ArgumentNullException(nameof(glossary));
      _options.GlossaryId = glossary.GlossaryId;
      return this;
    }

    /// <summary>Uses the glossary identified by <paramref name="glossaryId" />.</summary>
    public DocumentTranslationBuilder WithGlossaryId(string glossaryId) {
      _options.GlossaryId = glossaryId ?? throw new ArgumentNullException(nameof(glossaryId));
      return this;
    }

    /// <summary>Enables document minification for supported formats.</summary>
    public DocumentTranslationBuilder WithMinification(bool enable = true) {
      _options.EnableDocumentMinification = enable;
      return this;
    }

    /// <summary>Requests a specific output format (e.g. "docx"). Defaults to the input file format.</summary>
    public DocumentTranslationBuilder WithOutputFormat(string outputFormat) {
      _options.OutputFormat = outputFormat ?? throw new ArgumentNullException(nameof(outputFormat));
      return this;
    }

    /// <summary>Associates a cancellation token with the eventual request(s).</summary>
    public DocumentTranslationBuilder WithCancellation(CancellationToken cancellationToken) {
      _cancellationToken = cancellationToken;
      return this;
    }

    /// <summary>
    ///   Uploads, waits, and downloads the translated document to <paramref name="outputFileInfo" />.
    ///   Returns a <see cref="Task" /> awaitable result.
    /// </summary>
    public Task SaveTo(FileInfo outputFileInfo) {
      if (outputFileInfo == null) throw new ArgumentNullException(nameof(outputFileInfo));
      EnsureTargetLanguage();
      return RunWithFileOutputAsync(outputFileInfo);
    }

    /// <summary>
    ///   Uploads, waits, and downloads the translated document into <paramref name="outputStream" />.
    /// </summary>
    public Task SaveTo(Stream outputStream) {
      if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));
      EnsureTargetLanguage();
      return RunWithStreamOutputAsync(outputStream);
    }

    /// <summary>
    ///   Uploads the document and returns a <see cref="DocumentHandle" /> without waiting for completion.
    ///   Use <see cref="FluentDocumentTranslationExtensions.Document" /> to track and download the result.
    /// </summary>
    public Task<DocumentHandle> UploadAsync() {
      EnsureTargetLanguage();
      if (_inputFileInfo != null) {
        return _translator.TranslateDocumentUploadAsync(
              _inputFileInfo, _sourceLanguageCode, _targetLanguageCode!, _options, _cancellationToken);
      }

      return _translator.TranslateDocumentUploadAsync(
            _inputStream!,
            _inputFileName!,
            _sourceLanguageCode,
            _targetLanguageCode!,
            _options,
            _cancellationToken);
    }

    private async Task RunWithFileOutputAsync(FileInfo outputFileInfo) {
      if (_inputFileInfo != null) {
        await _translator.TranslateDocumentAsync(
                    _inputFileInfo,
                    outputFileInfo,
                    _sourceLanguageCode,
                    _targetLanguageCode!,
                    _options,
                    _cancellationToken)
              .ConfigureAwait(false);
        return;
      }

      using var outputFile = outputFileInfo.Open(FileMode.CreateNew, FileAccess.Write);
      try {
        await _translator.TranslateDocumentAsync(
                    _inputStream!,
                    _inputFileName!,
                    outputFile,
                    _sourceLanguageCode,
                    _targetLanguageCode!,
                    _options,
                    _cancellationToken)
              .ConfigureAwait(false);
      } catch {
        try { outputFileInfo.Delete(); } catch { /* ignored */ }
        throw;
      }
    }

    private Task RunWithStreamOutputAsync(Stream outputStream) {
      if (_inputFileInfo != null) {
        return RunFromFileToStreamAsync(outputStream);
      }

      return _translator.TranslateDocumentAsync(
            _inputStream!,
            _inputFileName!,
            outputStream,
            _sourceLanguageCode,
            _targetLanguageCode!,
            _options,
            _cancellationToken);
    }

    private async Task RunFromFileToStreamAsync(Stream outputStream) {
      using var inputFile = _inputFileInfo!.OpenRead();
      await _translator.TranslateDocumentAsync(
                  inputFile,
                  _inputFileInfo.Name,
                  outputStream,
                  _sourceLanguageCode,
                  _targetLanguageCode!,
                  _options,
                  _cancellationToken)
            .ConfigureAwait(false);
    }

    private void EnsureTargetLanguage() {
      if (_targetLanguageCode == null) {
        throw new InvalidOperationException(
              "Target language is required. Call .To(targetLanguageCode) before uploading / saving.");
      }
    }
  }

  /// <summary>Fluent reference for an in-progress document translation identified by a <see cref="DocumentHandle" />.</summary>
  public sealed class DocumentRef {
    private readonly ITranslator _translator;

    internal DocumentRef(ITranslator translator, DocumentHandle handle) {
      _translator = translator;
      Handle = handle;
    }

    public DocumentHandle Handle { get; }

    /// <summary>Retrieves the current status of the translation.</summary>
    public Task<DocumentStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
          _translator.TranslateDocumentStatusAsync(Handle, cancellationToken);

    /// <summary>Polls until the translation is done or fails.</summary>
    public Task WaitUntilDoneAsync(CancellationToken cancellationToken = default) =>
          _translator.TranslateDocumentWaitUntilDoneAsync(Handle, cancellationToken);

    /// <summary>Downloads the translated document to a file.</summary>
    public Task DownloadToAsync(FileInfo outputFileInfo, CancellationToken cancellationToken = default) {
      if (outputFileInfo == null) throw new ArgumentNullException(nameof(outputFileInfo));
      return _translator.TranslateDocumentDownloadAsync(Handle, outputFileInfo, cancellationToken);
    }

    /// <summary>Downloads the translated document to a stream.</summary>
    public Task DownloadToAsync(Stream outputStream, CancellationToken cancellationToken = default) {
      if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));
      return _translator.TranslateDocumentDownloadAsync(Handle, outputStream, cancellationToken);
    }
  }
}
