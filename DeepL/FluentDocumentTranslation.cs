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
    private IProgress<DocumentStatus>? _progress;

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

    /// <summary>
    ///   Associates a cancellation token with the eventual request(s).
    ///   For ad-hoc cancellation without a pre-built <see cref="CancellationTokenSource" />,
    ///   prefer calling <see cref="DocumentTranslationJob.Cancel" /> on the handle returned from
    ///   <see cref="SaveTo(FileInfo)" /> / <see cref="SaveTo(Stream)" />.
    /// </summary>
    public DocumentTranslationBuilder WithCancellation(CancellationToken cancellationToken) {
      _cancellationToken = cancellationToken;
      return this;
    }

    /// <summary>
    ///   Attaches a progress callback that is invoked each time the document status is polled
    ///   during the wait phase (between upload and download). Useful for UI progress indicators,
    ///   structured logging, or webhook emissions.
    /// </summary>
    /// <remarks>
    ///   When a progress callback is attached, the fluent builder takes its own orchestration
    ///   path (upload → poll → download) instead of delegating to
    ///   <see cref="ITranslator.TranslateDocumentAsync(Stream,string,Stream,string?,string,DocumentTranslateOptions?,CancellationToken)" />.
    ///   Document minification is not supported on the progress path;
    ///   if both are required, fall back to configuring a <see cref="CancellationToken" /> and
    ///   awaiting <see cref="SaveTo(Stream)" /> without progress.
    /// </remarks>
    public DocumentTranslationBuilder WithProgress(IProgress<DocumentStatus> progress) {
      _progress = progress ?? throw new ArgumentNullException(nameof(progress));
      return this;
    }

    /// <summary>
    ///   Uploads, waits, and downloads the translated document to <paramref name="outputFileInfo" />.
    ///   Returns a <see cref="DocumentTranslationJob" /> that is directly awaitable AND supports
    ///   <see cref="DocumentTranslationJob.Cancel" /> for fluent, ad-hoc cancellation.
    /// </summary>
    public DocumentTranslationJob SaveTo(FileInfo outputFileInfo) {
      if (outputFileInfo == null) throw new ArgumentNullException(nameof(outputFileInfo));
      EnsureTargetLanguage();
      return Start(outputFileInfo, outputStream: null);
    }

    /// <summary>
    ///   Uploads, waits, and downloads the translated document into <paramref name="outputStream" />.
    ///   Returns a <see cref="DocumentTranslationJob" /> that is directly awaitable AND supports
    ///   <see cref="DocumentTranslationJob.Cancel" />.
    /// </summary>
    public DocumentTranslationJob SaveTo(Stream outputStream) {
      if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));
      EnsureTargetLanguage();
      return Start(outputFile: null, outputStream);
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

    private DocumentTranslationJob Start(FileInfo? outputFile, Stream? outputStream) {
      var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
      var task = RunAsync(outputFile, outputStream, linkedCts.Token);
      // Dispose the CTS when the job completes, regardless of outcome.
      _ = task.ContinueWith(
            _ => linkedCts.Dispose(),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
      return new DocumentTranslationJob(task, linkedCts);
    }

    private Task RunAsync(FileInfo? outputFile, Stream? outputStream, CancellationToken ct) {
      // Without progress: delegate to the library's existing orchestration so we inherit its
      // DocumentTranslationException wrapping AND document-minification support.
      if (_progress == null) {
        return outputFile != null
              ? RunViaLibraryToFileAsync(outputFile, ct)
              : RunViaLibraryToStreamAsync(outputStream!, ct);
      }
      // With progress: run upload → poll-with-callbacks → download in this layer.
      return RunWithProgressAsync(outputFile, outputStream, ct);
    }

    private async Task RunViaLibraryToFileAsync(FileInfo outputFileInfo, CancellationToken ct) {
      if (_inputFileInfo != null) {
        await _translator.TranslateDocumentAsync(
                    _inputFileInfo,
                    outputFileInfo,
                    _sourceLanguageCode,
                    _targetLanguageCode!,
                    _options,
                    ct)
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
                    ct)
              .ConfigureAwait(false);
      } catch {
        try { outputFileInfo.Delete(); } catch { /* ignored */ }
        throw;
      }
    }

    private Task RunViaLibraryToStreamAsync(Stream outputStream, CancellationToken ct) {
      if (_inputFileInfo != null) {
        return RunFromFileToStreamViaLibraryAsync(outputStream, ct);
      }

      return _translator.TranslateDocumentAsync(
            _inputStream!,
            _inputFileName!,
            outputStream,
            _sourceLanguageCode,
            _targetLanguageCode!,
            _options,
            ct);
    }

    private async Task RunFromFileToStreamViaLibraryAsync(Stream outputStream, CancellationToken ct) {
      using var inputFile = _inputFileInfo!.OpenRead();
      await _translator.TranslateDocumentAsync(
                  inputFile,
                  _inputFileInfo.Name,
                  outputStream,
                  _sourceLanguageCode,
                  _targetLanguageCode!,
                  _options,
                  ct)
            .ConfigureAwait(false);
    }

    private async Task RunWithProgressAsync(
          FileInfo? outputFile, Stream? outputStream, CancellationToken ct) {
      FileStream? openedOutputFile = null;
      try {
        // Upload
        var handle = await UploadCoreAsync(ct).ConfigureAwait(false);

        // Wait (with progress)
        await DocumentPolling.WaitAsync(_translator, handle, _progress, ct).ConfigureAwait(false);

        // Download
        if (outputFile != null) {
          openedOutputFile = outputFile.Open(FileMode.CreateNew, FileAccess.Write);
          await _translator.TranslateDocumentDownloadAsync(handle, openedOutputFile, ct)
                .ConfigureAwait(false);
        } else {
          await _translator.TranslateDocumentDownloadAsync(handle, outputStream!, ct)
                .ConfigureAwait(false);
        }
      } catch {
        // Mirror the library's cleanup behavior: remove the half-written output file on error.
        if (outputFile != null) {
          openedOutputFile?.Dispose();
          try { outputFile.Refresh(); if (outputFile.Exists) outputFile.Delete(); } catch { /* ignored */ }
        }
        throw;
      } finally {
        openedOutputFile?.Dispose();
      }
    }

    private Task<DocumentHandle> UploadCoreAsync(CancellationToken ct) {
      if (_inputFileInfo != null) {
        return _translator.TranslateDocumentUploadAsync(
              _inputFileInfo, _sourceLanguageCode, _targetLanguageCode!, _options, ct);
      }
      return _translator.TranslateDocumentUploadAsync(
            _inputStream!, _inputFileName!, _sourceLanguageCode, _targetLanguageCode!, _options, ct);
    }

    private void EnsureTargetLanguage() {
      if (_targetLanguageCode == null) {
        throw new InvalidOperationException(
              "Target language is required. Call .To(targetLanguageCode) before uploading / saving.");
      }
    }
  }

  /// <summary>
  ///   Handle to a running document-translation operation. Directly awaitable, and supports
  ///   <see cref="Cancel" /> so callers can keep the fluent style instead of plumbing a
  ///   <see cref="CancellationTokenSource" /> through by hand.
  /// </summary>
  /// <example>
  ///   <code>
  ///     var job = translator.TranslateDocument(input).To("de").SaveTo(output);
  ///     // ...time passes, user clicks Cancel in UI...
  ///     job.Cancel();
  ///     try { await job; } catch (OperationCanceledException) { /* handled */ }
  ///   </code>
  /// </example>
  public sealed class DocumentTranslationJob {
    private readonly Task _task;
    private readonly CancellationTokenSource _cts;

    internal DocumentTranslationJob(Task task, CancellationTokenSource cts) {
      _task = task;
      _cts = cts;
    }

    /// <summary>The underlying <see cref="Task" /> representing the upload → poll → download flow.</summary>
    public Task Task => _task;

    /// <summary><c>true</c> once the job has completed (successfully, failed, or cancelled).</summary>
    public bool IsCompleted => _task.IsCompleted;

    /// <summary>
    ///   Signals cancellation to the in-flight job. Safe to call after completion (no-op).
    ///   Awaiting the job afterwards will typically surface an <see cref="OperationCanceledException" />.
    /// </summary>
    public void Cancel() {
      try { _cts.Cancel(); } catch (ObjectDisposedException) { /* already finished */ }
    }

    /// <summary>Enables <c>await job</c> — waits until upload/poll/download finishes or is cancelled.</summary>
    public TaskAwaiter GetAwaiter() => _task.GetAwaiter();

    /// <summary>Implicit conversion so the job can be passed wherever a <see cref="Task" /> is expected.</summary>
    public static implicit operator Task(DocumentTranslationJob job) =>
          job?._task ?? throw new ArgumentNullException(nameof(job));
  }

  /// <summary>Shared poll loop used by <see cref="DocumentTranslationBuilder" /> and <see cref="DocumentRef" />.</summary>
  internal static class DocumentPolling {
    internal static async Task WaitAsync(
          ITranslator translator,
          DocumentHandle handle,
          IProgress<DocumentStatus>? progress,
          CancellationToken cancellationToken) {
      var status = await translator.TranslateDocumentStatusAsync(handle, cancellationToken)
            .ConfigureAwait(false);
      progress?.Report(status);
      while (status.Ok && !status.Done) {
        await Task.Delay(CalculatePollDelay(status.SecondsRemaining), cancellationToken)
              .ConfigureAwait(false);
        status = await translator.TranslateDocumentStatusAsync(handle, cancellationToken)
              .ConfigureAwait(false);
        progress?.Report(status);
      }
      if (!status.Ok) {
        throw new DeepLException(status.ErrorMessage ?? "Unknown error");
      }
    }

    // Mirrors the library's internal CalculateDocumentWaitTime heuristic without reaching into it:
    // fall back to a 5-second floor when the server gives no estimate, clamp to [1, 60] seconds.
    private static TimeSpan CalculatePollDelay(int? secondsRemaining) {
      var seconds = secondsRemaining.GetValueOrDefault(5);
      if (seconds < 1) seconds = 1;
      if (seconds > 60) seconds = 60;
      return TimeSpan.FromSeconds(seconds);
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

    /// <summary>
    ///   Polls until the translation is done or fails. Delegates to the library's built-in
    ///   <see cref="ITranslator.TranslateDocumentWaitUntilDoneAsync" />.
    /// </summary>
    public Task WaitUntilDoneAsync(CancellationToken cancellationToken = default) =>
          _translator.TranslateDocumentWaitUntilDoneAsync(Handle, cancellationToken);

    /// <summary>
    ///   Polls until the translation is done or fails, reporting each status tick through
    ///   <paramref name="progress" />. Useful for UI progress indicators, structured logging,
    ///   or webhook emissions during the wait phase.
    /// </summary>
    public Task WaitUntilDoneAsync(
          IProgress<DocumentStatus> progress,
          CancellationToken cancellationToken = default) {
      if (progress == null) throw new ArgumentNullException(nameof(progress));
      return DocumentPolling.WaitAsync(_translator, Handle, progress, cancellationToken);
    }

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
