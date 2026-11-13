// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Internal;
using DeepL.Model;

namespace DeepL {
  public interface IWriter : IDisposable {
    /// <summary>Rephrase specified texts, improving them by fixing grammar and spelling errors.</summary>
    /// <param name="texts">Texts to improve; none may be <c>null</c> or empty.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">Extra <see cref="TextRephraseOptions" /> influencing rephrasing.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Texts without grammatical or spelling errors.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="texts" /> or one of its entries is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<WriteResult[]> RephraseTextAsync(
          IEnumerable<string> texts,
          string? targetLanguageCode = null,
          TextRephraseOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Rephrase specified text, improving them by fixing grammar and spelling errors.</summary>
    /// <param name="text">Text to improve; must not be <c>null</c> or empty.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">Extra <see cref="TextRephraseOptions" /> influencing rephrasing.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Texts without grammatical or spelling errors.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="text" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<WriteResult> RephraseTextAsync(
          string text,
          string? targetLanguageCode = null,
          TextRephraseOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Correct specified text, improving it by fixing grammar and spelling errors.</summary>
    /// <param name="text">Text to improve; must not be <c>null</c> or empty.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">Extra <see cref="CorrectTextOptions" /> influencing correction.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Text without grammatical or spelling errors.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="text" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<WriteResult> CorrectTextAsync(
          string text,
          string? targetLanguageCode = null,
          CorrectTextOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Correct specified texts, improving them by fixing grammar and spelling errors.</summary>
    /// <param name="texts">Texts to improve; none may be <c>null</c> or empty.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">Extra <see cref="CorrectTextOptions" /> influencing correction.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Texts without grammatical or spelling errors.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="texts" /> or one of its entries is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<WriteResult[]> CorrectTextAsync(
          IEnumerable<string> texts,
          string? targetLanguageCode = null,
          CorrectTextOptions? options = null,
          CancellationToken cancellationToken = default);
  }

  /// <summary>
  ///   Client for the DeepL API. To use the DeepL API, initialize an instance of this class using your DeepL
  ///   Authentication Key. All functions are thread-safe, aside from <see cref="DeepLClient.Dispose" />.
  /// </summary>
  public sealed class DeepLClient : IUsageProvider, ITextTranslator, IDocumentTranslator, ILanguageProvider, IWriter,
        IGlossaryManager, IStyleRuleManager, IStyleProfileManager,
        ITranslationMemoryManager, ISpokenTermsManager {
    /// <summary>Base URL for DeepL API Pro accounts.</summary>
    private const string DeepLServerUrl = "https://api.deepl.com";

    /// <summary>Base URL for DeepL API Free accounts.</summary>
    private const string DeepLServerUrlFree = "https://api-free.deepl.com";

    /// <summary>Internal class implementing HTTP requests.</summary>
    internal readonly DeepLHttpClient _client;

    /// <summary>Initializes a new <see cref="DeepLClient" /> object using your authentication key.</summary>
    /// <param name="authKey">
    ///   Authentication Key as found in your
    ///   <a href="https://www.deepl.com/pro-account/">DeepL API account</a>.
    /// </param>
    /// <param name="options">Additional options controlling client behaviour.</param>
    /// <exception cref="ArgumentNullException">If authKey argument is null.</exception>
    /// <exception cref="ArgumentException">
    ///   If authKey argument is empty, or the <see cref="BaseClientOptions.ServerUrl" /> option is not an absolute
    ///   http or https URL.
    /// </exception>
    /// <remarks>
    ///   This function does not establish a connection to the DeepL API. To check connectivity, use
    ///   <see cref="GetUsageAsync" />.
    /// </remarks>
    public DeepLClient(string authKey, DeepLClientOptions? options = null) {
      options ??= new DeepLClientOptions();

      if (authKey == null) {
        throw new ArgumentNullException(nameof(authKey));
      }

      authKey = authKey.Trim();

      if (authKey.Length == 0) {
        throw new ArgumentException($"{nameof(authKey)} is empty");
      }

      var serverUrl = DeepLHttpClient.ParseServerUrl(
            options.ServerUrl ?? (IsFreeAccountAuthKey(authKey) ? DeepLServerUrlFree : DeepLServerUrl));

      _client = DeepLHttpClient.Create(authKey, serverUrl, options);
    }

    /// <summary>Releases the unmanaged resources and disposes of the managed resources used by the <see cref="DeepLClient" />.</summary>
    public void Dispose() => _client.Dispose();

    /// <summary>Retrieves the version string, with format MAJOR.MINOR.BUGFIX.</summary>
    /// <returns>String containing the library version.</returns>
    public static string Version() {
      var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "";
      return version;
    }

    /// <summary>
    ///   Determines if the given DeepL Authentication Key belongs to an API Free account or an API Pro account.
    /// </summary>
    /// <param name="authKey">
    ///   DeepL Authentication Key as found in your
    ///   <a href="https://www.deepl.com/pro-account/">DeepL API account</a>.
    /// </param>
    /// <returns>
    ///   <c>true</c> if the Authentication Key belongs to an API Free account (ends with <c>:fx</c>), <c>false</c> if it
    ///   belongs to an API Pro account.
    /// </returns>
    public static bool IsFreeAccountAuthKey(string authKey) => authKey.TrimEnd().EndsWith(":fx");

    /// <inheritdoc />
    public async Task<Usage> GetUsageAsync(CancellationToken cancellationToken = default) {
      using var responseMessage = await _client.ApiGetAsync("v2/usage", cancellationToken).ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var usageFields = await JsonUtils.DeserializeAsync<Usage.ResponseFieldsStruct>(responseMessage)
            .ConfigureAwait(false);
      return new Usage(usageFields);
    }

    /// <inheritdoc />
    public async Task<TextResult[]> TranslateTextAsync(
          IEnumerable<string> texts,
          string? sourceLanguageCode,
          string targetLanguageCode,
          TextTranslateOptions? options = null,
          CancellationToken cancellationToken = default) {
      var requestData = CreateHttpParams(sourceLanguageCode, targetLanguageCode, options);
      var textList = CheckTexts(texts);
      requestData["text"] = textList;
      // Added after the texts, so that an extra "text" parameter replaces them like any other field.
      AddExtraBodyParameters(requestData, options);

      using var responseMessage = await _client
            .ApiPostJsonAsync("v2/translate", cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var translatedTexts =
            await JsonUtils.DeserializeAsync<TextTranslateResult>(responseMessage).ConfigureAwait(false);
      var translations = translatedTexts.Translations;
      // An extra "text" parameter may change the number of texts sent; require at least one translation then.
      var expectedCount = options?.ExtraBodyParameters?.ContainsKey("text") == true ? 1 : textList.Length;
      if (translations == null || translations.Length < Math.Max(expectedCount, 1)) {
        throw new DeepLException(
              $"Expected {expectedCount} translations in the response, received {translations?.Length ?? 0}");
      }

      return translations;
    }

    /// <inheritdoc />
    public async Task<TextResult> TranslateTextAsync(
          string text,
          string? sourceLanguageCode,
          string targetLanguageCode,
          TextTranslateOptions? options = null,
          CancellationToken cancellationToken = default)
      => (await TranslateTextAsync(
                  new[] { Argument.NotNull(text, nameof(text)) },
                  sourceLanguageCode,
                  targetLanguageCode,
                  options,
                  cancellationToken)
            .ConfigureAwait(false))[0];

    /// <inheritdoc />
    public async Task<DocumentStatus> TranslateDocumentAsync(
          FileInfo inputFileInfo,
          FileInfo outputFileInfo,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default) {
      CheckDocumentInputFile(inputFileInfo);
      Argument.NotNull(outputFileInfo, nameof(outputFileInfo));
      // Local file failures are wrapped like the failures of the translation itself. The output is opened with
      // CreateNew and outside the try below, so an occupied path is never deleted.
      using var inputFile = OpenLocalDocumentFile(inputFileInfo.OpenRead);
      using var outputFile = OpenLocalDocumentFile(() => outputFileInfo.Open(FileMode.CreateNew, FileAccess.Write));
      try {
        return await TranslateDocumentAsync(
              inputFile,
              options?.Filename ?? inputFileInfo.Name,
              outputFile,
              sourceLanguageCode,
              targetLanguageCode,
              options,
              cancellationToken).ConfigureAwait(false);
      } catch {
        try {
          outputFileInfo.Delete();
        } catch {
          // ignored
        }

        throw;
      }
    }

    /// <summary>
    ///   Throws <see cref="ArgumentNullException" /> if the document input file is <c>null</c>, and
    ///   <see cref="ArgumentException" /> if it does not exist or is not a file.
    /// </summary>
    private static void CheckDocumentInputFile(FileInfo inputFileInfo) {
      Argument.NotNull(inputFileInfo, nameof(inputFileInfo));
      // FileInfo caches Exists from its first read; the caller may have read it before creating the file.
      inputFileInfo.Refresh();
      if (!inputFileInfo.Exists) {
        throw new ArgumentException($"File does not exist: {inputFileInfo.FullName}", nameof(inputFileInfo));
      }
    }

    /// <summary>Opens a local file for TranslateDocumentAsync, wrapping a failure in DocumentTranslationException.</summary>
    private static FileStream OpenLocalDocumentFile(Func<FileStream> open) {
      try {
        return open();
      } catch (Exception exception) when (IsLocalFileException(exception)) {
        throw new DocumentTranslationException(
              $"Error occurred during document translation: {exception.Message}",
              exception,
              null);
      }
    }

    /// <inheritdoc />
    public async Task<DocumentStatus> TranslateDocumentAsync(
          Stream inputFile,
          string inputFileName,
          Stream outputFile,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default) {
      DocumentHandle? handle = null;
      try {
        Argument.NotNull(outputFile, nameof(outputFile));
        handle = await TranslateDocumentUploadAsync(
                    inputFile,
                    inputFileName,
                    sourceLanguageCode,
                    targetLanguageCode,
                    options,
                    cancellationToken)
              .ConfigureAwait(false);
        var status = await TranslateDocumentWaitUntilDoneAsync(handle.Value, options?.Timeout, cancellationToken)
              .ConfigureAwait(false);
        await TranslateDocumentDownloadAsync(handle.Value, outputFile, cancellationToken).ConfigureAwait(false);
        return status;
      } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        // Cancellation by the caller is not wrapped.
        throw;
      } catch (Exception exception) {
        throw new DocumentTranslationException(
              $"Error occurred during document translation: {exception.Message}",
              exception,
              handle);
      }
    }

    /// <inheritdoc />
    public Task<DocumentStatus> TranslateDocumentAsync(
          Stream inputFile,
          Stream outputFile,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default) =>
          TranslateDocumentAsync(
                inputFile,
                options?.Filename!,
                outputFile,
                sourceLanguageCode,
                targetLanguageCode,
                options,
                cancellationToken);

    /// <inheritdoc />
    public async Task<DocumentHandle> TranslateDocumentUploadAsync(
          FileInfo inputFileInfo,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default) {
      CheckDocumentInputFile(inputFileInfo);
      using var inputFileStream = inputFileInfo.OpenRead();
      return await TranslateDocumentUploadAsync(
            inputFileStream,
            options?.Filename ?? inputFileInfo.Name,
            sourceLanguageCode,
            targetLanguageCode,
            options,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<DocumentHandle> TranslateDocumentUploadAsync(
          Stream inputFile,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default) =>
          TranslateDocumentUploadAsync(
                inputFile,
                options?.Filename!,
                sourceLanguageCode,
                targetLanguageCode,
                options,
                cancellationToken);

    /// <inheritdoc />
    public async Task<DocumentHandle> TranslateDocumentUploadAsync(
          Stream inputFile,
          string inputFileName,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default) {
      Argument.NotNull(inputFile, nameof(inputFile));
      // null if passed so, or if the overload without it is used and DocumentTranslateOptions.Filename is unset.
      if (inputFileName == null) {
        throw new ArgumentNullException(
              nameof(inputFileName),
              $"{nameof(inputFileName)} or {nameof(DocumentTranslateOptions)}." +
              $"{nameof(DocumentTranslateOptions.Filename)} is required with stream input");
      }

      // Document translation cannot detect the source language when a glossary is used.
      if (sourceLanguageCode == null && (options?.Glossary != null || options?.GlossaryIds?.Count > 0)) {
        throw new ArgumentException($"{nameof(sourceLanguageCode)} is required if using a glossary");
      }

      // The multipart upload takes form fields: glossary_ids is comma-joined there.
      var bodyParams = CreateCommonHttpParams(
                  sourceLanguageCode,
                  targetLanguageCode,
                  options?.FormalityValue != null ? null : options?.Formality,
                  options?.Glossary,
                  options?.StyleRule,
                  options?.TranslationMemory,
                  options?.GlossaryIds,
                  options?.StyleProfile)
            .Select(pair => (pair.Key, pair.Value is string[] values ? string.Join(",", values) : (string)pair.Value!))
            .ToList();

      if (options?.FormalityValue != null) {
        bodyParams.Add(("formality", options.FormalityValue));
      }

      if (options?.TranslationMemoryThreshold != null) {
        if (options.TranslationMemory == null) {
          throw new ArgumentException("TranslationMemoryThreshold requires TranslationMemory");
        }

        if (options.TranslationMemoryThreshold.Value < 0 || options.TranslationMemoryThreshold.Value > 100) {
          throw new ArgumentOutOfRangeException(
                nameof(options.TranslationMemoryThreshold),
                options.TranslationMemoryThreshold.Value,
                $"{nameof(options.TranslationMemoryThreshold)} must be between 0 and 100");
        }

        bodyParams.Add(("translation_memory_threshold", options.TranslationMemoryThreshold.Value.ToString()));
      }

      if (options?.OutputFormat != null) {
        bodyParams.Add(("output_format", options?.OutputFormat!));
      }

      if (options?.EnableWatermark != null) {
        // The API ignores "1" for this parameter; it needs "true"/"false".
        bodyParams.Add(("enable_watermark", options.EnableWatermark.Value ? "true" : "false"));
      }

      AddExtraBodyParameters(bodyParams, options);

      using var responseMessage = await _client.ApiUploadAsync(
                  "v2/document",
                  cancellationToken,
                  bodyParams,
                  inputFile,
                  inputFileName,
                  retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, retryOnlyUnprocessed: true).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<DocumentHandle>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DocumentStatus> TranslateDocumentGetStatusAsync(
          DocumentHandle handle,
          CancellationToken cancellationToken = default) {
      var bodyParams = new (string Key, string Value)[] { ("document_key", handle.DocumentKey) };
      using var responseMessage =
            await _client.ApiPostAsync(
                        $"v2/document/{Uri.EscapeDataString(handle.DocumentId)}",
                        cancellationToken,
                        bodyParams)
                  .ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<DocumentStatus>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<DocumentStatus> TranslateDocumentWaitUntilDoneAsync(
          DocumentHandle handle,
          CancellationToken cancellationToken = default) =>
          TranslateDocumentWaitUntilDoneAsync(handle, null, cancellationToken);

    /// <inheritdoc />
    public async Task<DocumentStatus> TranslateDocumentWaitUntilDoneAsync(
          DocumentHandle handle,
          TimeSpan? timeout,
          CancellationToken cancellationToken = default) {
      var deadline = timeout == null ? (DateTime?)null : DateTime.UtcNow + timeout.Value;
      var status = await TranslateDocumentGetStatusAsync(handle, cancellationToken).ConfigureAwait(false);
      while (status.Ok && !status.Done) {
        var wait = CalculateDocumentWaitTime(status.SecondsRemaining);
        if (deadline != null) {
          var remaining = deadline.Value - DateTime.UtcNow;
          if (remaining <= TimeSpan.Zero) {
            throw new DeepLException($"Document translation did not finish within the timeout of {timeout}");
          }

          if (remaining < wait) {
            wait = remaining;
          }
        }

        await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
        status = await TranslateDocumentGetStatusAsync(handle, cancellationToken).ConfigureAwait(false);
      }

      if (!status.Ok) {
        throw new DeepLException(status.ErrorMessage ?? "Unknown error");
      }

      return status;
    }

    /// <inheritdoc />
    public async Task TranslateDocumentDownloadAsync(
          DocumentHandle handle,
          FileInfo outputFileInfo,
          CancellationToken cancellationToken = default) {
      Argument.NotNull(outputFileInfo, nameof(outputFileInfo));
      using var outputFileStream = outputFileInfo.Open(FileMode.CreateNew, FileAccess.Write);
      try {
        await TranslateDocumentDownloadAsync(handle, outputFileStream, cancellationToken).ConfigureAwait(false);
      } catch {
        try {
          outputFileInfo.Delete();
        } catch {
          // ignored
        }

        throw;
      }
    }

    /// <inheritdoc />
    public async Task TranslateDocumentDownloadAsync(
          DocumentHandle handle,
          Stream outputFile,
          CancellationToken cancellationToken = default) {
      Argument.NotNull(outputFile, nameof(outputFile));
      var bodyParams = new (string Key, string Value)[] { ("document_key", handle.DocumentKey) };
      using var responseMessage = await _client.ApiPostAsync(
                  $"v2/document/{Uri.EscapeDataString(handle.DocumentId)}/result",
                  cancellationToken,
                  bodyParams)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, downloadingDocument: true).ConfigureAwait(false);
      await responseMessage.Content.CopyToAsync(outputFile).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SourceLanguage[]> GetSourceLanguagesAsync(CancellationToken cancellationToken = default) =>
          await GetLanguagesAsync<SourceLanguage>(false, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TargetLanguage[]> GetTargetLanguagesAsync(CancellationToken cancellationToken = default) =>
          await GetLanguagesAsync<TargetLanguage>(true, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<GlossaryLanguagePair[]> GetGlossaryLanguagesAsync(CancellationToken cancellationToken = default) {
      var queryParams = new (string Key, string Value)[] { ("resource", "glossary") };
      using var responseMessage = await _client
            .ApiGetAsync("v3/languages", cancellationToken, queryParams).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var languages = await JsonUtils.DeserializeAsync<V3Language[]>(responseMessage).ConfigureAwait(false);
      var sourceLanguages = languages.Where(language => language.UsableAsSource).Select(language => language.Lang);
      var targetLanguages = languages.Where(language => language.UsableAsTarget).Select(language => language.Lang);
      return sourceLanguages
            .SelectMany(source => targetLanguages, (source, target) => new GlossaryLanguagePair(source, target))
            .Where(pair => pair.SourceLanguageCode != pair.TargetLanguageCode)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<GlossaryInfo> CreateGlossaryAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default) =>
          await CreateGlossaryInternalAsync(
                name,
                sourceLanguageCode,
                targetLanguageCode,
                "tsv",
                Argument.NotNull(entries, nameof(entries)).ToTsv(),
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<GlossaryInfo> CreateGlossaryFromCsvAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default) =>
          await CreateGlossaryInternalAsync(
                name,
                sourceLanguageCode,
                targetLanguageCode,
                "csv",
                await new StreamReader(Argument.NotNull(csvFile, nameof(csvFile))).ReadToEndAsync().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<GlossaryInfo> GetGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default) {
      Argument.NotNull(glossaryId, nameof(glossaryId));
      using var responseMessage =
            await _client.ApiGetAsync($"v2/glossaries/{glossaryId}", cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<GlossaryInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<GlossaryInfo> WaitUntilGlossaryReadyAsync(
          string glossaryId,
          CancellationToken cancellationToken = default) {
      var info = await GetGlossaryAsync(glossaryId, cancellationToken).ConfigureAwait(false);
      while (!info.Ready) {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
        info = await GetGlossaryAsync(glossaryId, cancellationToken).ConfigureAwait(false);
      }

      return info;
    }

    /// <inheritdoc />
    public async Task<GlossaryInfo[]> ListGlossariesAsync(CancellationToken cancellationToken = default) {
      using var responseMessage =
            await _client.ApiGetAsync("v2/glossaries", cancellationToken).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return (await JsonUtils.DeserializeAsync<GlossaryListResult>(responseMessage).ConfigureAwait(false)).Glossaries;
    }

    /// <inheritdoc />
    public async Task<GlossaryEntries> GetGlossaryEntriesAsync(
          string glossaryId,
          CancellationToken cancellationToken = default) {
      Argument.NotNull(glossaryId, nameof(glossaryId));
      using var responseMessage = await _client.ApiGetAsync(
            $"v2/glossaries/{glossaryId}/entries",
            cancellationToken,
            acceptHeader: "text/tab-separated-values").ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      var contentTsv = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
      return GlossaryEntries.FromTsv(contentTsv, true);
    }

    /// <inheritdoc />
    public async Task<GlossaryEntries> GetGlossaryEntriesAsync(
          GlossaryInfo glossary,
          CancellationToken cancellationToken = default) =>
          await GetGlossaryEntriesAsync(GlossaryId(glossary), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default) {
      Argument.NotNull(glossaryId, nameof(glossaryId));
      using var responseMessage =
            await _client.ApiDeleteAsync($"v2/glossaries/{glossaryId}", cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteGlossaryAsync(
          GlossaryInfo glossary,
          CancellationToken cancellationToken = default) =>
          await DeleteGlossaryAsync(GlossaryId(glossary), cancellationToken).ConfigureAwait(false);

    /// <summary>Internal function to retrieve available languages.</summary>
    /// <param name="target"><c>true</c> to retrieve target languages, <c>false</c> to retrieve source languages.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="Language" /> objects containing information about the available languages.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    private async Task<TValue[]> GetLanguagesAsync<TValue>(
          bool target,
          CancellationToken cancellationToken = default) {
      var queryParams = new (string Key, string Value)[] { ("resource", "translate_text") };
      using var responseMessage =
            await _client.ApiGetAsync("v3/languages", cancellationToken, queryParams)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var languages = await JsonUtils.DeserializeAsync<V3Language[]>(responseMessage).ConfigureAwait(false);
      return languages
            .Where(language => target ? language.UsableAsTarget : language.UsableAsSource)
            .Select(
                  language => target
                        ? (TValue)(object)new TargetLanguage(
                              language.Lang,
                              language.Name,
                              language.Features?.ContainsKey("formality") ?? false)
                        : (TValue)(object)new SourceLanguage(language.Lang, language.Name))
            .ToArray();
    }

    /// <summary>
    ///   Checks the specified languages and options are valid, and returns a list of tuples containing the parameters
    ///   to include in HTTP request, without the texts and the extra body parameters.
    /// </summary>
    /// <param name="sourceLanguageCode">
    ///   Language code of translation source language, or null if auto-detection should be
    ///   used.
    /// </param>
    /// <param name="targetLanguageCode">Language code of translation target language.</param>
    /// <param name="options">Extra <see cref="TextTranslateOptions" /> influencing translation.</param>
    /// <returns>Enumerable of tuples containing the parameters to include in HTTP request.</returns>
    /// <exception cref="ArgumentException">If the specified languages or options are invalid.</exception>
    private static Dictionary<string, object?> CreateHttpParams(
          string? sourceLanguageCode,
          string targetLanguageCode,
          TextTranslateOptions? options) {
      var requestData = CreateCommonHttpParams(
            sourceLanguageCode,
            targetLanguageCode,
            options?.FormalityValue != null ? null : options?.Formality,
            options?.Glossary,
            options?.StyleRule,
            options?.TranslationMemory,
            options?.GlossaryIds,
            options?.StyleProfile);

      if (options?.FormalityValue != null) {
        requestData["formality"] = options.FormalityValue;
      }

      // Always send show_billed_characters=true, remove when the API default is changed to true
      requestData["show_billed_characters"] = true;

      if (options == null) {
        return requestData;
      }

      if (options.TranslationMemoryThreshold != null) {
        if (options.TranslationMemory == null) {
          throw new ArgumentException("TranslationMemoryThreshold requires TranslationMemory");
        }

        if (options.TranslationMemoryThreshold.Value < 0 || options.TranslationMemoryThreshold.Value > 100) {
          throw new ArgumentOutOfRangeException(
                nameof(options.TranslationMemoryThreshold),
                options.TranslationMemoryThreshold.Value,
                $"{nameof(options.TranslationMemoryThreshold)} must be between 0 and 100");
        }

        requestData["translation_memory_threshold"] = options.TranslationMemoryThreshold.Value;
      }

      if (options.Context != null) {
        requestData["context"] = options.Context;
      }

      if (options.SplitSentences != null) {
        requestData["split_sentences"] = options.SplitSentences;
      } else if (options.SentenceSplittingMode != SentenceSplittingMode.All) {
        requestData["split_sentences"] = options.SentenceSplittingMode == SentenceSplittingMode.Off ? "0" : "nonewlines";
      }

      if (options.PreserveFormatting) {
        requestData["preserve_formatting"] = true;
      }

      if (options.TagHandling != null) {
        requestData["tag_handling"] = options.TagHandling;
      }

      if (options.TagHandlingVersion != null) {
        requestData["tag_handling_version"] = options.TagHandlingVersion;
      }

      if (!options.OutlineDetection) {
        requestData["outline_detection"] = false;
      }

      if (options.NonSplittingTags.Count > 0) {
        requestData["non_splitting_tags"] = SplitTags(options.NonSplittingTags, nameof(options.NonSplittingTags));
      }

      if (options.SplittingTags.Count > 0) {
        requestData["splitting_tags"] = SplitTags(options.SplittingTags, nameof(options.SplittingTags));
      }

      if (options.IgnoreTags.Count > 0) {
        requestData["ignore_tags"] = SplitTags(options.IgnoreTags, nameof(options.IgnoreTags));
      }

      if (options.ModelTypeValue != null) {
        requestData["model_type"] = options.ModelTypeValue;
      } else if (options.ModelType != null) {
        requestData["model_type"] = options.ModelType.Value.ToApiValue();
      }

      if (options.CustomInstructions.Count > 0) {
        requestData["custom_instructions"] =
              Argument.NoNullEntries(options.CustomInstructions, nameof(options.CustomInstructions));
      }

      return requestData;
    }

    /// <summary>Returns the tags of a tag list option as an array, splitting entries that hold comma-separated tags.</summary>
    private static string[] SplitTags(IEnumerable<string> tags, string optionName) =>
          Argument.NoNullEntries(tags, optionName).SelectMany(tag => tag.Split(','))
                .Select(tag => tag.Trim())
                .Where(tag => tag.Length > 0)
                .ToArray();

    /// <summary>
    ///   Checks the specified languages and options are valid, and returns the parameters common to text and document
    ///   translation; list values are string arrays.
    /// </summary>
    /// <param name="sourceLanguageCode">
    ///   Language code of translation source language, or null if auto-detection should be
    ///   used.
    /// </param>
    /// <param name="targetLanguageCode">Language code of translation target language.</param>
    /// <param name="formality">Formality option for translation.</param>
    /// <param name="glossaryId">Optional ID of glossary to use for translation.</param>
    /// <param name="styleId">Optional ID of style rule to use for translation.</param>
    /// <param name="translationMemoryId">Optional ID of translation memory to use for translation.</param>
    /// <param name="glossaryIds">
    ///   Optional list of glossary IDs to use for translation (maximum of 5). Cannot be combined with
    ///   <paramref name="glossaryId" />.
    /// </param>
    /// <param name="styleProfileId">Optional ID of style profile to use for translation.</param>
    /// <returns>Dictionary of the parameters, in insertion order.</returns>
    /// <exception cref="ArgumentException">If the specified languages or options are invalid.</exception>
    private static Dictionary<string, object?> CreateCommonHttpParams(
          string? sourceLanguageCode,
          string targetLanguageCode,
          Formality? formality,
          string? glossaryId,
          string? styleId = null,
          string? translationMemoryId = null,
          ICollection<string>? glossaryIds = null,
          string? styleProfileId = null) {
      CheckOptionalLanguage(sourceLanguageCode, nameof(sourceLanguageCode));
      CheckRequiredLanguage(targetLanguageCode, nameof(targetLanguageCode));
      targetLanguageCode = LanguageCode.Standardize(targetLanguageCode);
      sourceLanguageCode = sourceLanguageCode == null ? null : LanguageCode.Standardize(sourceLanguageCode);

      var bodyParams = new Dictionary<string, object?> { ["target_lang"] = targetLanguageCode };
      if (sourceLanguageCode != null) {
        bodyParams["source_lang"] = sourceLanguageCode;
      }

      CheckOptionalId(glossaryId, "Glossary");
      CheckOptionalId(styleId, "StyleRule");
      CheckOptionalId(styleProfileId, "StyleProfile");
      CheckOptionalId(translationMemoryId, "TranslationMemory");
      if (glossaryIds != null) Argument.NoNullEntries(glossaryIds, nameof(TextTranslateOptions.GlossaryIds));
      if (glossaryIds != null && glossaryIds.Any(id => id != null && string.IsNullOrWhiteSpace(id))) {
        throw new ArgumentException("GlossaryIds must not contain empty or whitespace-only IDs", "GlossaryIds");
      }

      var hasGlossaryIds = glossaryIds != null && glossaryIds.Count > 0;

      if (glossaryId != null && hasGlossaryIds) {
        throw new ArgumentException("Glossary and GlossaryIds cannot be used together");
      }

      if (glossaryId != null) {
        bodyParams["glossary_id"] = glossaryId;
      }

      if (hasGlossaryIds) {
        if (glossaryIds!.Count > 5) {
          throw new ArgumentException("GlossaryIds must not contain more than 5 glossary IDs");
        }

        bodyParams["glossary_ids"] = glossaryIds.ToArray();
      }

      if (styleId != null) {
        bodyParams["style_id"] = styleId;
      }

      if (styleProfileId != null) {
        bodyParams["style_profile_id"] = styleProfileId;
      }

      if (translationMemoryId != null) {
        bodyParams["translation_memory_id"] = translationMemoryId;
      }

      switch (formality) {
        case null:
        case Formality.Default:
          break;
        case Formality.Less:
          bodyParams["formality"] = "less";
          break;
        case Formality.More:
          bodyParams["formality"] = "more";
          break;
        case Formality.PreferLess:
          bodyParams["formality"] = "prefer_less";
          break;
        case Formality.PreferMore:
          bodyParams["formality"] = "prefer_more";
          break;
        default:
          throw new ArgumentException($"{nameof(formality)} value is out of range");
      }

      return bodyParams;
    }

    /// <summary>
    ///   Determines recommended time to wait before checking document translation again, using an optional hint of
    ///   seconds remaining.
    /// </summary>
    /// <param name="hintSecondsRemaining">Optional hint of the number of seconds remaining.</param>
    /// <returns><see cref="TimeSpan" /> to wait.</returns>
    private static TimeSpan CalculateDocumentWaitTime(int? hintSecondsRemaining) {
      // hintSecondsRemaining is currently unreliable, so just poll equidistantly
      const int POLLING_TIME_SECS = 5;
      return TimeSpan.FromSeconds(POLLING_TIME_SECS);
    }

    /// <summary>Creates a glossary with given details.</summary>
    private async Task<GlossaryInfo> CreateGlossaryInternalAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          string entriesFormat,
          string entries,
          CancellationToken cancellationToken) {
      if (Argument.NotNull(name, nameof(name)).Length == 0) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      Argument.NotNull(sourceLanguageCode, nameof(sourceLanguageCode));
      Argument.NotNull(targetLanguageCode, nameof(targetLanguageCode));

      sourceLanguageCode = LanguageCode.RemoveRegionalVariant(sourceLanguageCode);
      targetLanguageCode = LanguageCode.RemoveRegionalVariant(targetLanguageCode);

      var requestData = new Dictionary<string, object?> {
            ["name"] = name,
            ["source_lang"] = sourceLanguageCode,
            ["target_lang"] = targetLanguageCode,
            ["entries_format"] = entriesFormat,
            ["entries"] = entries
      };
      using var responseMessage = await _client
            .ApiPostJsonAsync("v2/glossaries", cancellationToken, requestData, SerializationOptions, true)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary, retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<GlossaryInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <summary>Class used for JSON-deserialization of text translate results.</summary>
    private readonly struct TextTranslateResult {
      /// <summary>Initializes a new instance of <see cref="TextTranslateResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TextTranslateResult(TextResult[] translations) {
        Translations = translations;
      }

      /// <summary>Array of <see cref="TextResult" /> objects holding text translation results.</summary>
      public TextResult[] Translations { get; }
    }

    /// <summary>Class used for JSON-deserialization of glossary list results.</summary>
    private readonly struct GlossaryListResult {
      /// <summary>Initializes a new instance of <see cref="GlossaryListResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public GlossaryListResult(GlossaryInfo[] glossaries) {
        Glossaries = glossaries;
      }

      /// <summary>Array of <see cref="GlossaryInfo" /> objects holding glossary information.</summary>
      public GlossaryInfo[] Glossaries { get; }
    }

    /// <summary>Class used for JSON-deserialization of entries returned by the <c>GET /v3/languages</c> endpoint.</summary>
    private readonly struct V3Language {
      /// <summary>Initializes a new instance of <see cref="V3Language" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public V3Language(
            string lang,
            string name,
            bool usableAsSource,
            bool usableAsTarget,
            Dictionary<string, JsonElement> features) {
        Lang = lang;
        Name = name;
        UsableAsSource = usableAsSource;
        UsableAsTarget = usableAsTarget;
        Features = features;
      }

      /// <summary>The BCP-47 language code, for example "de" or "en-US".</summary>
      public string Lang { get; }

      /// <summary>The name of the language in English.</summary>
      public string Name { get; }

      /// <summary><c>true</c> if the language may be used as a source language for the requested resource.</summary>
      public bool UsableAsSource { get; }

      /// <summary><c>true</c> if the language may be used as a target language for the requested resource.</summary>
      public bool UsableAsTarget { get; }

      /// <summary>Map of feature name to feature metadata supported by this language for the requested resource.</summary>
      public Dictionary<string, JsonElement> Features { get; }
    }

    /// <inheritdoc />
    public async Task<WriteResult> RephraseTextAsync(
          string text,
          string? targetLanguageCode = null,
          TextRephraseOptions? options = null,
          CancellationToken cancellationToken = default) => (await RephraseTextAsync(
                new[] { Argument.NotNull(text, nameof(text)) },
                targetLanguageCode,
                options,
                cancellationToken)
          .ConfigureAwait(false))[0];

    /// <inheritdoc />
    public async Task<WriteResult[]> RephraseTextAsync(
          IEnumerable<string> texts,
          string? targetLanguageCode = null,
          TextRephraseOptions? options = null,
          CancellationToken cancellationToken = default) {
      var requestData = new Dictionary<string, object?>();
      if (targetLanguageCode != null) {
        CheckOptionalLanguage(targetLanguageCode, nameof(targetLanguageCode));
        requestData["target_lang"] = targetLanguageCode;
      }

      var textList = CheckTexts(texts);
      if (options != null && options.WritingStyle != null) {
        requestData["writing_style"] = options.WritingStyle;
      }

      if (options != null && options.WritingTone != null) {
        requestData["tone"] = options.WritingTone;
      }
      // TODO add `show_billed_characters` once write API supports it.

      return await WriteAsync("v2/write/rephrase", requestData, textList, options, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WriteResult> CorrectTextAsync(
          string text,
          string? targetLanguageCode = null,
          CorrectTextOptions? options = null,
          CancellationToken cancellationToken = default) => (await CorrectTextAsync(
                new[] { Argument.NotNull(text, nameof(text)) },
                targetLanguageCode,
                options,
                cancellationToken)
          .ConfigureAwait(false))[0];

    /// <inheritdoc />
    public async Task<WriteResult[]> CorrectTextAsync(
          IEnumerable<string> texts,
          string? targetLanguageCode = null,
          CorrectTextOptions? options = null,
          CancellationToken cancellationToken = default) {
      var requestData = new Dictionary<string, object?>();
      if (targetLanguageCode != null) {
        CheckOptionalLanguage(targetLanguageCode, nameof(targetLanguageCode));
        requestData["target_lang"] = targetLanguageCode;
      }

      var textList = CheckTexts(texts);
      return await WriteAsync("v2/write/correct", requestData, textList, options, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Returns the texts to translate or improve, throwing if the list or a text is <c>null</c> or a text is empty.</summary>
    private static string[] CheckTexts(IEnumerable<string> texts) {
      var textList = Argument.NoNullEntries(texts, nameof(texts));
      if (textList.Any(text => text.Length == 0)) {
        throw new ArgumentException("text must not be empty");
      }

      return textList;
    }

    /// <summary>
    ///   Sends a Write API request with the given parameters, then the texts, then the extra body parameters of the
    ///   options, and returns one improvement per text.
    /// </summary>
    /// <exception cref="DeepLException">If the response holds fewer improvements than texts were sent.</exception>
    private async Task<WriteResult[]> WriteAsync(
          string path,
          Dictionary<string, object?> requestData,
          string[] texts,
          BaseRequestOptions? options,
          CancellationToken cancellationToken) {
      requestData["text"] = texts;
      AddExtraBodyParameters(requestData, options);

      using var responseMessage = await _client
            .ApiPostJsonAsync(path, cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var result = await JsonUtils.DeserializeAsync<TextRephraseResult>(responseMessage).ConfigureAwait(false);
      var improvements = result.Improvements;
      if (improvements == null || improvements.Length < texts.Length) {
        throw new DeepLException(
              $"Expected {texts.Length} improvements in the response, received {improvements?.Length ?? 0}");
      }

      return improvements;
    }

    /// <inheritdoc />
    public async Task<StyleRuleInfo[]> GetAllStyleRulesAsync(
          int? page = null,
          int? pageSize = null,
          bool? detailed = null,
          CancellationToken cancellationToken = default) {
      var queryParams = new List<(string Key, string Value)>();

      if (page != null) {
        queryParams.Add(("page", page.Value.ToString()));
      }

      if (pageSize != null) {
        queryParams.Add(("page_size", pageSize.Value.ToString()));
      }

      if (detailed != null) {
        queryParams.Add(("detailed", detailed.Value.ToString().ToLower()));
      }

      using var responseMessage = await _client
            .ApiGetAsync("v3/style_rules", cancellationToken, queryParams.ToArray()).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
      var styleRuleList = await JsonUtils.DeserializeAsync<StyleRuleListResult>(responseMessage)
            .ConfigureAwait(false);
      return styleRuleList.StyleRules;
    }

    /// <summary>Retrieves a list of available translation memories. The maximum number of translation memories returned is controlled by pageSize (max 25).</summary>
    /// <param name="page">Optional page number for pagination, 0-indexed.</param>
    /// <param name="pageSize">Optional number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="TranslationMemoryInfo" /> objects representing the available translation memories.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    public async Task<TranslationMemoryInfo[]> ListTranslationMemoriesAsync(
          int? page = null,
          int? pageSize = null,
          CancellationToken cancellationToken = default) {
      var queryParams = new List<(string Key, string Value)>();

      if (page != null) {
        queryParams.Add(("page", page.Value.ToString()));
      }

      if (pageSize != null) {
        queryParams.Add(("page_size", pageSize.Value.ToString()));
      }

      using var responseMessage = await _client
            .ApiGetAsync("v3/translation_memories", cancellationToken, queryParams.ToArray()).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var translationMemoryList = await JsonUtils.DeserializeAsync<TranslationMemoryListResult>(responseMessage)
            .ConfigureAwait(false);
      return translationMemoryList.TranslationMemories;
    }

    /// <inheritdoc />
    public async Task<TranslationMemoryInfo> GetTranslationMemoryAsync(
          string translationMemoryId,
          CancellationToken cancellationToken = default) {
      CheckTranslationMemoryId(translationMemoryId);

      using var responseMessage = await _client
            .ApiGetAsync($"v3/translation_memories/{Uri.EscapeDataString(translationMemoryId)}", cancellationToken)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.TranslationMemory)
            .ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<TranslationMemoryInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TranslationMemoryInfo> GetTranslationMemoryAsync(
          TranslationMemoryInfo translationMemory,
          CancellationToken cancellationToken = default) =>
          await GetTranslationMemoryAsync(TranslationMemoryId(translationMemory), cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemorySegments> ListTranslationMemorySegmentsAsync(
          string translationMemoryId,
          int? pageSize = null,
          string? pageCursor = null,
          string? filterText = null,
          bool? filterCaseSensitive = null,
          CancellationToken cancellationToken = default) =>
          await ListTranslationMemorySegmentsAsync(
                      translationMemoryId,
                      new TranslationMemorySegmentsOptions {
                        PageSize = pageSize,
                        PageCursor = pageCursor,
                        FilterText = filterText,
                        FilterCaseSensitive = filterCaseSensitive
                      },
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemorySegments> ListTranslationMemorySegmentsAsync(
          string translationMemoryId,
          TranslationMemorySegmentsOptions? options,
          CancellationToken cancellationToken = default) {
      CheckTranslationMemoryId(translationMemoryId);

      var queryParams = new List<(string Key, string Value)>();

      if (options?.PageSize != null) {
        queryParams.Add(("page_size", options.PageSize.Value.ToString()));
      }

      if (options?.PageCursor != null) {
        queryParams.Add(("page_cursor", options.PageCursor));
      }

      if (options?.FilterText != null) {
        queryParams.Add(("filter_text", options.FilterText));
      }

      if (options?.FilterCaseSensitive != null) {
        queryParams.Add(("filter_case_sensitive", options.FilterCaseSensitive.Value ? "true" : "false"));
      }

      using var responseMessage = await _client
            .ApiGetAsync(
                  $"v3/translation_memories/{Uri.EscapeDataString(translationMemoryId)}/segments",
                  cancellationToken,
                  queryParams.ToArray())
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.TranslationMemory)
            .ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<TranslationMemorySegments>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TranslationMemorySegments> ListTranslationMemorySegmentsAsync(
          TranslationMemoryInfo translationMemory,
          int? pageSize = null,
          string? pageCursor = null,
          string? filterText = null,
          bool? filterCaseSensitive = null,
          CancellationToken cancellationToken = default) =>
          await ListTranslationMemorySegmentsAsync(
                      TranslationMemoryId(translationMemory),
                      pageSize,
                      pageCursor,
                      filterText,
                      filterCaseSensitive,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemorySegments> ListTranslationMemorySegmentsAsync(
          TranslationMemoryInfo translationMemory,
          TranslationMemorySegmentsOptions? options,
          CancellationToken cancellationToken = default) =>
          await ListTranslationMemorySegmentsAsync(TranslationMemoryId(translationMemory), options, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteTranslationMemoryAsync(
          string translationMemoryId,
          CancellationToken cancellationToken = default) {
      CheckTranslationMemoryId(translationMemoryId);

      using var responseMessage = await _client
            .ApiDeleteAsync($"v3/translation_memories/{Uri.EscapeDataString(translationMemoryId)}", cancellationToken)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.TranslationMemory)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteTranslationMemoryAsync(
          TranslationMemoryInfo translationMemory,
          CancellationToken cancellationToken = default) =>
          await DeleteTranslationMemoryAsync(TranslationMemoryId(translationMemory), cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemoryImport> CreateTranslationMemoryImportAsync(
          string fileName,
          long contentLength,
          string? contentType = null,
          string? displayName = null,
          CancellationToken cancellationToken = default) =>
          await CreateTranslationMemoryImportAsync(
                      fileName,
                      contentLength,
                      new CreateTranslationMemoryImportOptions { ContentType = contentType, DisplayName = displayName },
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemoryImport> CreateTranslationMemoryImportAsync(
          string fileName,
          long contentLength,
          CreateTranslationMemoryImportOptions? options,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(fileName)) {
        throw new ArgumentException($"Parameter {nameof(fileName)} must not be empty");
      }

      if (contentLength <= 0) {
        throw new ArgumentException($"Parameter {nameof(contentLength)} must be greater than 0");
      }

      var sourceFile = new Dictionary<string, object?> {
        ["file_name"] = fileName,
        ["content_length"] = contentLength
      };
      if (options?.ContentType != null) sourceFile["content_type"] = options.ContentType;
      var requestData = new Dictionary<string, object?> { ["source_file"] = sourceFile };
      if (options?.DisplayName != null) {
        requestData["parameters"] = new Dictionary<string, object?> { ["display_name"] = options.DisplayName };
      }

      AddExtraBodyParameters(requestData, options);

      using var responseMessage = await _client
            .ApiPostJsonAsync(
                  "v3/translation_memories/import",
                  cancellationToken,
                  requestData,
                  SerializationOptions,
                  true)
            .ConfigureAwait(false);

      await DeepLHttpClient
            .CheckStatusCodeAsync(responseMessage, ResourceType.TranslationMemory, retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<TranslationMemoryImport>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UploadTranslationMemoryFileAsync(
          TranslationMemoryImport translationMemoryImport,
          Stream fileContent,
          string contentType = "application/xml",
          CancellationToken cancellationToken = default) =>
          await UploadTranslationMemoryFileAsync(
                      translationMemoryImport.UploadUrl,
                      fileContent,
                      contentType,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UploadTranslationMemoryFileAsync(
          string uploadUrl,
          Stream fileContent,
          string contentType = "application/xml",
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(uploadUrl)) {
        throw new ArgumentException($"Parameter {nameof(uploadUrl)} must not be empty");
      }

      if (!Uri.TryCreate(uploadUrl, UriKind.Absolute, out var parsedUploadUrl) ||
          (parsedUploadUrl.Scheme != Uri.UriSchemeHttp && parsedUploadUrl.Scheme != Uri.UriSchemeHttps)) {
        throw new ArgumentException($"Parameter {nameof(uploadUrl)} must be an absolute http or https URL");
      }

      // Buffer the file before sending it. Requests are retried on 429 and 5xx responses, and a
      // StreamContent body would already be at the end of the stream on the second attempt,
      // uploading nothing.
      using var buffer = new MemoryStream();
      await fileContent.CopyToAsync(buffer, 81920, cancellationToken).ConfigureAwait(false);

      using var content = new ByteArrayContent(buffer.ToArray());
      content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
      using var responseMessage = await _client
            .AssetCallAsync(HttpMethod.Put, uploadUrl, cancellationToken, content).ConfigureAwait(false);

      await DeepLHttpClient.CheckAssetStatusCodeAsync(responseMessage, "uploading translation memory file")
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TranslationMemoryExport> CreateTranslationMemoryExportAsync(
          string translationMemoryId,
          CancellationToken cancellationToken = default) {
      CheckTranslationMemoryId(translationMemoryId);

      using var responseMessage = await _client
            .ApiPostAsync(
                  $"v3/translation_memories/{Uri.EscapeDataString(translationMemoryId)}/export",
                  cancellationToken,
                  retryOnlyUnprocessed: true)
            .ConfigureAwait(false);

      await DeepLHttpClient
            .CheckStatusCodeAsync(responseMessage, ResourceType.TranslationMemory, retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      // 200 means the DeepL API reused a previously completed export, 202 that it started a new one.
      var reusedExisting = responseMessage.StatusCode == HttpStatusCode.OK;
      var exportResult = await JsonUtils.DeserializeAsync<TranslationMemoryExportResult>(responseMessage)
            .ConfigureAwait(false);
      return new TranslationMemoryExport(
            exportResult.JobId,
            exportResult.Parameters?.TranslationMemoryId,
            reusedExisting);
    }

    /// <inheritdoc />
    public async Task<TranslationMemoryExport> CreateTranslationMemoryExportAsync(
          TranslationMemoryInfo translationMemory,
          CancellationToken cancellationToken = default) =>
          await CreateTranslationMemoryExportAsync(TranslationMemoryId(translationMemory), cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemoryJob> GetTranslationMemoryJobAsync(
          string jobId,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(jobId)) {
        throw new ArgumentException($"Parameter {nameof(jobId)} must not be empty");
      }

      using var responseMessage = await _client
            .ApiGetAsync($"v3/translation_memories/jobs/{Uri.EscapeDataString(jobId)}", cancellationToken)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.TranslationMemory)
            .ConfigureAwait(false);
      var jobResult = await JsonUtils.DeserializeAsync<TranslationMemoryJobResponse>(responseMessage)
            .ConfigureAwait(false);
      return CreateTranslationMemoryJob(jobResult);
    }

    /// <inheritdoc />
    public async Task<TranslationMemoryJob> GetTranslationMemoryJobAsync(
          TranslationMemoryJob job,
          CancellationToken cancellationToken = default) =>
          await GetTranslationMemoryJobAsync(TranslationMemoryJobId(job), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          string jobId,
          CancellationToken cancellationToken = default) =>
          WaitUntilTranslationMemoryJobDoneAsync(jobId, null, cancellationToken);

    /// <inheritdoc />
    public Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          string jobId,
          TimeSpan? timeout,
          CancellationToken cancellationToken = default) =>
          WaitUntilTranslationMemoryJobDoneAsync(jobId, timeout, _ => { }, cancellationToken);

    /// <summary>
    ///   Polls the translation memory job like the public overloads, passing every retrieved job state to
    ///   <paramref name="onJob" /> so that the file helpers can report the last known state on failure.
    /// </summary>
    private async Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          string jobId,
          TimeSpan? timeout,
          Action<TranslationMemoryJob> onJob,
          CancellationToken cancellationToken) {
      var deadline = timeout == null ? (DateTime?)null : DateTime.UtcNow + timeout.Value;
      var job = await GetTranslationMemoryJobAsync(jobId, cancellationToken).ConfigureAwait(false);
      onJob(job);
      // Done means completed; a failed or expired job ends the wait through Ok. Any other status keeps polling.
      while (!job.Done && job.Ok) {
        if (job.Result == null) {
          throw new DeepLException("Translation memory job status contained no result");
        }

        var wait = TranslationMemoryJobPollInterval;
        if (deadline != null) {
          var remaining = deadline.Value - DateTime.UtcNow;
          if (remaining <= TimeSpan.Zero) {
            throw new DeepLException($"Translation memory job did not finish within the timeout of {timeout}");
          }

          if (remaining < wait) {
            wait = remaining;
          }
        }

        await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
        job = await GetTranslationMemoryJobAsync(jobId, cancellationToken).ConfigureAwait(false);
        onJob(job);
      }

      if (!job.Ok) {
        throw new DeepLException(job.Result?.ErrorMessage ?? $"Translation memory job {job.Status}");
      }

      return job;
    }

    /// <inheritdoc />
    public Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          TranslationMemoryJob job,
          CancellationToken cancellationToken = default) =>
          WaitUntilTranslationMemoryJobDoneAsync(TranslationMemoryJobId(job), null, cancellationToken);

    /// <inheritdoc />
    public Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          TranslationMemoryJob job,
          TimeSpan? timeout,
          CancellationToken cancellationToken = default) =>
          WaitUntilTranslationMemoryJobDoneAsync(TranslationMemoryJobId(job), timeout, cancellationToken);

    /// <inheritdoc />
    public async Task DownloadTranslationMemoryExportAsync(
          TranslationMemoryJob job,
          Stream outputFile,
          CancellationToken cancellationToken = default) {
      var downloadUrl = job.Result?.DownloadUrl;
      if (string.IsNullOrEmpty(downloadUrl)) {
        throw new DeepLException(
              "Translation memory export job has no download URL; it may not have completed yet");
      }

      using var responseMessage = await _client
            .AssetCallAsync(HttpMethod.Get, downloadUrl!, cancellationToken).ConfigureAwait(false);

      await DeepLHttpClient.CheckAssetStatusCodeAsync(responseMessage, "downloading translation memory export")
            .ConfigureAwait(false);
      // The body can be large, so honour cancellation during the copy where the framework
      // supports it; netstandard2.0 has no CancellationToken overload.
#if NET5_0_OR_GREATER
      await responseMessage.Content.CopyToAsync(outputFile, cancellationToken).ConfigureAwait(false);
#else
      await responseMessage.Content.CopyToAsync(outputFile).ConfigureAwait(false);
#endif
    }

    /// <inheritdoc />
    public async Task DownloadTranslationMemoryExportAsync(
          TranslationMemoryJob job,
          string outputFilePath,
          CancellationToken cancellationToken = default) {
      var outputFileInfo = new FileInfo(outputFilePath);
      // CreateNew rather than Create: the catch below deletes the file to clean up a partial
      // download, so truncating an existing one would let a failed export destroy the caller's
      // data. Matches TranslateDocumentDownloadAsync.
      using var outputFileStream = outputFileInfo.Open(FileMode.CreateNew, FileAccess.Write);
      try {
        await DownloadTranslationMemoryExportAsync(job, outputFileStream, cancellationToken).ConfigureAwait(false);
      } catch {
        try {
          outputFileStream.Dispose();
          outputFileInfo.Delete();
        } catch {
          // ignored
        }

        throw;
      }
    }

    /// <inheritdoc />
    public async Task<TranslationMemoryJob> ImportTranslationMemoryFromFilepathAsync(
          string inputFilePath,
          string? displayName = null,
          CancellationToken cancellationToken = default) =>
          await ImportTranslationMemoryFromFilepathAsync(
                      inputFilePath,
                      new ImportTranslationMemoryOptions { DisplayName = displayName },
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemoryJob> ImportTranslationMemoryFromFilepathAsync(
          string inputFilePath,
          ImportTranslationMemoryOptions? options,
          CancellationToken cancellationToken = default) {
      var inputFileInfo = new FileInfo(inputFilePath);
      if (!inputFileInfo.Exists) {
        throw new ArgumentException($"File does not exist: {inputFilePath}");
      }

      string? jobId = null;
      TranslationMemoryJob? job = null;
      try {
        // Read the file before creating the job, so that a file that cannot be read leaves no job awaiting input. The
        // upload buffers the whole file anyway.
        byte[] fileContent;
        using (var inputFileStream = inputFileInfo.OpenRead())
        using (var buffer = new MemoryStream()) {
          await inputFileStream.CopyToAsync(buffer, 81920, cancellationToken).ConfigureAwait(false);
          fileContent = buffer.ToArray();
        }

        if (fileContent.Length == 0) {
          throw new DeepLException($"Input file is empty: {inputFilePath}");
        }

        var createOptions = new CreateTranslationMemoryImportOptions {
          ContentType = options?.ContentType,
          DisplayName = options?.DisplayName,
          ExtraBodyParameters = options?.ExtraBodyParameters
        };
        var created = await CreateTranslationMemoryImportAsync(
                    inputFileInfo.Name,
                    fileContent.Length,
                    createOptions,
                    cancellationToken)
              .ConfigureAwait(false);
        jobId = created.JobId;

        using (var contentStream = new MemoryStream(fileContent, false)) {
          await UploadTranslationMemoryFileAsync(
                      created,
                      contentStream,
                      options?.ContentType ?? "application/xml",
                      cancellationToken)
                .ConfigureAwait(false);
        }

        return await WaitUntilTranslationMemoryJobDoneAsync(
                    created.JobId,
                    options?.Timeout,
                    polled => job = polled,
                    cancellationToken)
              .ConfigureAwait(false);
      } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        // Cancellation by the caller is not wrapped.
        throw;
      } catch (Exception exception) {
        throw new TranslationMemoryJobException(
              $"Error occurred during translation memory import: {exception.Message}",
              exception,
              jobId,
              job);
      }
    }

    /// <inheritdoc />
    public async Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          string translationMemoryId,
          string outputFilePath,
          CancellationToken cancellationToken = default) =>
          await ExportTranslationMemoryToFilepathAsync(translationMemoryId, outputFilePath, null, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          string translationMemoryId,
          string outputFilePath,
          TimeSpan? timeout,
          CancellationToken cancellationToken = default) {
      string? jobId = null;
      TranslationMemoryJob? job = null;
      try {
        return await ExportTranslationMemoryToFilepathCoreAsync(
                    translationMemoryId,
                    outputFilePath,
                    timeout,
                    created => jobId = created,
                    polled => job = polled,
                    cancellationToken)
              .ConfigureAwait(false);
      } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        // Cancellation by the caller is not wrapped.
        throw;
      } catch (Exception exception) {
        throw new TranslationMemoryJobException(
              $"Error occurred during translation memory export: {exception.Message}",
              exception,
              jobId,
              job);
      }
    }

    private async Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathCoreAsync(
          string translationMemoryId,
          string outputFilePath,
          TimeSpan? timeout,
          Action<string> onCreated,
          Action<TranslationMemoryJob> onJob,
          CancellationToken cancellationToken) {
      CheckTranslationMemoryId(translationMemoryId);
      // Create the output file before creating the job, so that an existing file or an unwritable path fails before
      // the export runs. CreateNew rather than Create: the file is deleted again if the export fails, so truncating
      // an existing one would destroy the caller's data.
      var outputFileInfo = new FileInfo(outputFilePath);
      var outputFileStream = outputFileInfo.Open(FileMode.CreateNew, FileAccess.Write);
      try {
        using (outputFileStream) {
          var created = await CreateTranslationMemoryExportAsync(translationMemoryId, cancellationToken)
                .ConfigureAwait(false);
          onCreated(created.JobId);
          var job = await WaitUntilTranslationMemoryJobDoneAsync(created.JobId, timeout, onJob, cancellationToken)
                .ConfigureAwait(false);
          if (string.IsNullOrEmpty(job.Result?.DownloadUrl)) {
            throw new DeepLException("Translation memory export job completed without a download URL");
          }

          await DownloadTranslationMemoryExportAsync(job, outputFileStream, cancellationToken).ConfigureAwait(false);

          return job;
        }
      } catch {
        try {
          outputFileInfo.Delete();
        } catch {
          // ignored
        }

        throw;
      }
    }

    /// <summary>
    ///   <c>true</c> for the exceptions of local file access that the document translation helpers wrap in a
    ///   <see cref="DocumentTranslationException" />.
    /// </summary>
    private static bool IsLocalFileException(Exception exception) =>
          exception is IOException || exception is UnauthorizedAccessException;

    /// <inheritdoc />
    public async Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          TranslationMemoryInfo translationMemory,
          string outputFilePath,
          CancellationToken cancellationToken = default) =>
          // A null object reaches the wrapped empty-ID check, like an empty ID.
          await ExportTranslationMemoryToFilepathAsync(
                      translationMemory?.TranslationMemoryId!,
                      outputFilePath,
                      null,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          TranslationMemoryInfo translationMemory,
          string outputFilePath,
          TimeSpan? timeout,
          CancellationToken cancellationToken = default) =>
          // A null object reaches the wrapped empty-ID check, like an empty ID.
          await ExportTranslationMemoryToFilepathAsync(
                      translationMemory?.TranslationMemoryId!,
                      outputFilePath,
                      timeout,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleRuleInfo> CreateStyleRuleAsync(
          string name,
          string language,
          Dictionary<string, Dictionary<string, string>>? configuredRules = null,
          CustomInstruction[]? customInstructions = null,
          CancellationToken cancellationToken = default) =>
          await CreateStyleRuleAsync(name, language, configuredRules, customInstructions, null, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleRuleInfo> CreateStyleRuleAsync(
          string name,
          string language,
          Dictionary<string, Dictionary<string, string>>? configuredRules,
          CustomInstruction[]? customInstructions,
          CreateStyleRuleOptions? options,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(name)) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(language)) {
        throw new ArgumentException($"Parameter {nameof(language)} must not be empty");
      }

      foreach (var instruction in customInstructions ?? Array.Empty<CustomInstruction>()) {
        CheckOptionalLanguage(instruction?.SourceLanguage, nameof(CustomInstruction.SourceLanguage));
      }

      var requestData = new Dictionary<string, object?> { ["name"] = name, ["language"] = language };
      if (configuredRules != null) requestData["configured_rules"] = configuredRules;
      if (customInstructions != null) requestData["custom_instructions"] = customInstructions;
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPostJsonAsync("v3/style_rules", cancellationToken, requestData, SerializationOptions, true)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule, retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<StyleRuleInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<StyleRuleInfo> GetStyleRuleAsync(
          string styleId,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty");
      }

      using var responseMessage =
            await _client.ApiGetAsync($"v3/style_rules/{Uri.EscapeDataString(styleId)}", cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<StyleRuleInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<StyleRuleInfo> GetStyleRuleAsync(
          StyleRuleInfo styleRule,
          CancellationToken cancellationToken = default) =>
          await GetStyleRuleAsync(StyleRuleId(styleRule), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleRuleInfo> UpdateStyleRuleNameAsync(
          string styleId,
          string name,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleNameAsync(styleId, name, null, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleRuleInfo> UpdateStyleRuleNameAsync(
          string styleId,
          string name,
          UpdateStyleRuleNameOptions? options,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(name)) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      var requestData = new Dictionary<string, object?> { ["name"] = name };
      AddExtraBodyParameters(requestData, options);
      using var responseMessage =
            await _client.ApiPatchJsonAsync($"v3/style_rules/{Uri.EscapeDataString(styleId)}", cancellationToken, requestData, SerializationOptions)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<StyleRuleInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<StyleRuleInfo> UpdateStyleRuleNameAsync(
          StyleRuleInfo styleRule,
          string name,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleNameAsync(StyleRuleId(styleRule), name, null, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleRuleInfo> UpdateStyleRuleNameAsync(
          StyleRuleInfo styleRule,
          string name,
          UpdateStyleRuleNameOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleNameAsync(StyleRuleId(styleRule), name, options, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteStyleRuleAsync(
          string styleId,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty");
      }

      using var responseMessage =
            await _client.ApiDeleteAsync($"v3/style_rules/{Uri.EscapeDataString(styleId)}", cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteStyleRuleAsync(
          StyleRuleInfo styleRule,
          CancellationToken cancellationToken = default) =>
          await DeleteStyleRuleAsync(StyleRuleId(styleRule), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleRuleInfo> UpdateStyleRuleConfiguredRulesAsync(
          string styleId,
          Dictionary<string, Dictionary<string, string>> configuredRules,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleConfiguredRulesAsync(styleId, configuredRules, null, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleRuleInfo> UpdateStyleRuleConfiguredRulesAsync(
          string styleId,
          Dictionary<string, Dictionary<string, string>> configuredRules,
          UpdateStyleRuleConfiguredRulesOptions? options,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty");
      }

      if (configuredRules == null) {
        throw new ArgumentNullException(nameof(configuredRules));
      }

      var requestData = new Dictionary<string, object?>();
      foreach (var category in configuredRules) {
        requestData[category.Key] = category.Value;
      }

      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPutJsonAsync($"v3/style_rules/{Uri.EscapeDataString(styleId)}/configured_rules", cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<StyleRuleInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<StyleRuleInfo> UpdateStyleRuleConfiguredRulesAsync(
          StyleRuleInfo styleRule,
          Dictionary<string, Dictionary<string, string>> configuredRules,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleConfiguredRulesAsync(StyleRuleId(styleRule), configuredRules, null, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleRuleInfo> UpdateStyleRuleConfiguredRulesAsync(
          StyleRuleInfo styleRule,
          Dictionary<string, Dictionary<string, string>> configuredRules,
          UpdateStyleRuleConfiguredRulesOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleConfiguredRulesAsync(StyleRuleId(styleRule), configuredRules, options, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> CreateStyleRuleCustomInstructionAsync(
          string styleId,
          string label,
          string prompt,
          string? sourceLanguage = null,
          CancellationToken cancellationToken = default) =>
          await CreateStyleRuleCustomInstructionAsync(styleId, label, prompt, sourceLanguage, null, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> CreateStyleRuleCustomInstructionAsync(
          string styleId,
          string label,
          string prompt,
          string? sourceLanguage,
          CreateStyleRuleCustomInstructionOptions? options,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(label)) {
        throw new ArgumentException($"Parameter {nameof(label)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(prompt)) {
        throw new ArgumentException($"Parameter {nameof(prompt)} must not be empty");
      }

      CheckOptionalLanguage(sourceLanguage, nameof(sourceLanguage));
      var requestData = new Dictionary<string, object?> { ["label"] = label, ["prompt"] = prompt };
      if (sourceLanguage != null) requestData["source_language"] = sourceLanguage;
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPostJsonAsync(
                  $"v3/style_rules/{Uri.EscapeDataString(styleId)}/custom_instructions",
                  cancellationToken,
                  requestData,
                  SerializationOptions,
                  true)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule, retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<CustomInstruction>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CustomInstruction> CreateStyleRuleCustomInstructionAsync(
          StyleRuleInfo styleRule,
          string label,
          string prompt,
          string? sourceLanguage = null,
          CancellationToken cancellationToken = default) =>
          await CreateStyleRuleCustomInstructionAsync(
                      StyleRuleId(styleRule),
                      label,
                      prompt,
                      sourceLanguage,
                      null,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> CreateStyleRuleCustomInstructionAsync(
          StyleRuleInfo styleRule,
          string label,
          string prompt,
          string? sourceLanguage,
          CreateStyleRuleCustomInstructionOptions? options,
          CancellationToken cancellationToken = default) =>
          await CreateStyleRuleCustomInstructionAsync(
                      StyleRuleId(styleRule),
                      label,
                      prompt,
                      sourceLanguage,
                      options,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> GetStyleRuleCustomInstructionAsync(
          string styleId,
          string instructionId,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(instructionId)) {
        throw new ArgumentException($"Parameter {nameof(instructionId)} must not be empty");
      }

      using var responseMessage =
            await _client.ApiGetAsync(
                  $"v3/style_rules/{Uri.EscapeDataString(styleId)}/custom_instructions/{Uri.EscapeDataString(instructionId)}", cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<CustomInstruction>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CustomInstruction> GetStyleRuleCustomInstructionAsync(
          StyleRuleInfo styleRule,
          string instructionId,
          CancellationToken cancellationToken = default) =>
          await GetStyleRuleCustomInstructionAsync(StyleRuleId(styleRule), instructionId, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> GetStyleRuleCustomInstructionAsync(
          string styleId,
          CustomInstruction instructionId,
          CancellationToken cancellationToken = default) =>
          await GetStyleRuleCustomInstructionAsync(styleId, InstructionId(instructionId), cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> GetStyleRuleCustomInstructionAsync(
          StyleRuleInfo styleRule,
          CustomInstruction instructionId,
          CancellationToken cancellationToken = default) =>
          await GetStyleRuleCustomInstructionAsync(StyleRuleId(styleRule), InstructionId(instructionId), cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> UpdateStyleRuleCustomInstructionAsync(
          string styleId,
          string instructionId,
          string label,
          string prompt,
          string? sourceLanguage = null,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleCustomInstructionAsync(
                      styleId,
                      instructionId,
                      label,
                      prompt,
                      sourceLanguage,
                      null,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> UpdateStyleRuleCustomInstructionAsync(
          string styleId,
          string instructionId,
          string label,
          string prompt,
          string? sourceLanguage,
          UpdateStyleRuleCustomInstructionOptions? options,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(instructionId)) {
        throw new ArgumentException($"Parameter {nameof(instructionId)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(label)) {
        throw new ArgumentException($"Parameter {nameof(label)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(prompt)) {
        throw new ArgumentException($"Parameter {nameof(prompt)} must not be empty");
      }

      CheckOptionalLanguage(sourceLanguage, nameof(sourceLanguage));
      var requestData = new Dictionary<string, object?> { ["label"] = label, ["prompt"] = prompt };
      if (sourceLanguage != null) requestData["source_language"] = sourceLanguage;
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPutJsonAsync(
                  $"v3/style_rules/{Uri.EscapeDataString(styleId)}/custom_instructions/{Uri.EscapeDataString(instructionId)}",
                  cancellationToken,
                  requestData,
                  SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<CustomInstruction>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CustomInstruction> UpdateStyleRuleCustomInstructionAsync(
          StyleRuleInfo styleRule,
          string instructionId,
          string label,
          string prompt,
          string? sourceLanguage = null,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleCustomInstructionAsync(
                      StyleRuleId(styleRule),
                      instructionId,
                      label,
                      prompt,
                      sourceLanguage,
                      null,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> UpdateStyleRuleCustomInstructionAsync(
          string styleId,
          CustomInstruction instructionId,
          string label,
          string prompt,
          string? sourceLanguage = null,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleCustomInstructionAsync(
                      styleId,
                      InstructionId(instructionId),
                      label,
                      prompt,
                      sourceLanguage,
                      null,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> UpdateStyleRuleCustomInstructionAsync(
          StyleRuleInfo styleRule,
          CustomInstruction instructionId,
          string label,
          string prompt,
          string? sourceLanguage = null,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleCustomInstructionAsync(
                      StyleRuleId(styleRule),
                      InstructionId(instructionId),
                      label,
                      prompt,
                      sourceLanguage,
                      null,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> UpdateStyleRuleCustomInstructionAsync(
          StyleRuleInfo styleRule,
          string instructionId,
          string label,
          string prompt,
          string? sourceLanguage,
          UpdateStyleRuleCustomInstructionOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleCustomInstructionAsync(
                      StyleRuleId(styleRule),
                      instructionId,
                      label,
                      prompt,
                      sourceLanguage,
                      options,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> UpdateStyleRuleCustomInstructionAsync(
          string styleId,
          CustomInstruction instructionId,
          string label,
          string prompt,
          string? sourceLanguage,
          UpdateStyleRuleCustomInstructionOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleCustomInstructionAsync(
                      styleId,
                      InstructionId(instructionId),
                      label,
                      prompt,
                      sourceLanguage,
                      options,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<CustomInstruction> UpdateStyleRuleCustomInstructionAsync(
          StyleRuleInfo styleRule,
          CustomInstruction instructionId,
          string label,
          string prompt,
          string? sourceLanguage,
          UpdateStyleRuleCustomInstructionOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleRuleCustomInstructionAsync(
                      StyleRuleId(styleRule),
                      InstructionId(instructionId),
                      label,
                      prompt,
                      sourceLanguage,
                      options,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteStyleRuleCustomInstructionAsync(
          string styleId,
          string instructionId,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(instructionId)) {
        throw new ArgumentException($"Parameter {nameof(instructionId)} must not be empty");
      }

      using var responseMessage =
            await _client.ApiDeleteAsync(
                  $"v3/style_rules/{Uri.EscapeDataString(styleId)}/custom_instructions/{Uri.EscapeDataString(instructionId)}", cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteStyleRuleCustomInstructionAsync(
          StyleRuleInfo styleRule,
          string instructionId,
          CancellationToken cancellationToken = default) =>
          await DeleteStyleRuleCustomInstructionAsync(StyleRuleId(styleRule), instructionId, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteStyleRuleCustomInstructionAsync(
          string styleId,
          CustomInstruction instructionId,
          CancellationToken cancellationToken = default) =>
          await DeleteStyleRuleCustomInstructionAsync(styleId, InstructionId(instructionId), cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteStyleRuleCustomInstructionAsync(
          StyleRuleInfo styleRule,
          CustomInstruction instructionId,
          CancellationToken cancellationToken = default) =>
          await DeleteStyleRuleCustomInstructionAsync(StyleRuleId(styleRule), InstructionId(instructionId), cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleProfile[]> ListStyleProfilesAsync(
          int? page = null,
          int? pageSize = null,
          CancellationToken cancellationToken = default) {
      var queryParams = new List<(string Key, string Value)>();
      if (page != null) queryParams.Add(("page", page.Value.ToString()));
      if (pageSize != null) queryParams.Add(("page_size", pageSize.Value.ToString()));

      using var responseMessage = await _client
            .ApiGetAsync("v3/style_profiles", cancellationToken, queryParams.ToArray()).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleProfile).ConfigureAwait(false);
      var result = await JsonUtils.DeserializeAsync<StyleProfileListResult>(responseMessage).ConfigureAwait(false);
      return result.StyleProfiles ?? new StyleProfile[0];
    }

    /// <inheritdoc />
    public async Task<StyleProfile> CreateStyleProfileAsync(
          string name,
          CreateStyleProfileOptions? options = null,
          CancellationToken cancellationToken = default) {
      using var responseMessage = await _client
            .ApiPostJsonAsync(
                  "v3/style_profiles",
                  cancellationToken,
                  CreateStyleProfileBody(name, options),
                  SerializationOptions,
                  true)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleProfile, retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<StyleProfile>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<StyleProfile> GetStyleProfileAsync(
          string styleProfileId,
          CancellationToken cancellationToken = default) {
      using var responseMessage = await _client
            .ApiGetAsync(StyleProfilePath(styleProfileId), cancellationToken).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleProfile).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<StyleProfile>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<StyleProfile> GetStyleProfileAsync(
          StyleProfile styleProfile,
          CancellationToken cancellationToken = default) =>
          await GetStyleProfileAsync(StyleProfileId(styleProfile), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleProfile> UpdateStyleProfileAsync(
          string styleProfileId,
          UpdateStyleProfileOptions? options = null,
          CancellationToken cancellationToken = default) {
      var path = StyleProfilePath(styleProfileId);
      // With nothing set the body is {}; the API returns the style profile unchanged.
      using var responseMessage = await _client
            .ApiPatchJsonAsync(
                  path,
                  cancellationToken,
                  CreateStyleProfileBody(options?.Name, options),
                  SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleProfile).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<StyleProfile>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<StyleProfile> UpdateStyleProfileAsync(
          StyleProfile styleProfile,
          UpdateStyleProfileOptions? options = null,
          CancellationToken cancellationToken = default) =>
          await UpdateStyleProfileAsync(StyleProfileId(styleProfile), options, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteStyleProfileAsync(
          string styleProfileId,
          CancellationToken cancellationToken = default) {
      using var responseMessage = await _client
            .ApiDeleteAsync(StyleProfilePath(styleProfileId), cancellationToken).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleProfile).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteStyleProfileAsync(
          StyleProfile styleProfile,
          CancellationToken cancellationToken = default) =>
          await DeleteStyleProfileAsync(StyleProfileId(styleProfile), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SpokenTerms> CreateSpokenTermsAsync(
          string name,
          IEnumerable<SpokenTermsEntries> termLists,
          CreateSpokenTermsOptions? options = null,
          CancellationToken cancellationToken = default) {
      var requestData = new Dictionary<string, object?> {
            ["name"] = name, ["term_lists"] = CreateSpokenTermsListsBody(termLists)
      };
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPostJsonAsync("v3/spoken-terms", cancellationToken, requestData, SerializationOptions, true)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.SpokenTerms, retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<SpokenTerms>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpokenTerms[]> ListSpokenTermsAsync(CancellationToken cancellationToken = default) {
      using var responseMessage =
            await _client.ApiGetAsync("v3/spoken-terms", cancellationToken).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.SpokenTerms).ConfigureAwait(false);
      var result = await JsonUtils.DeserializeAsync<SpokenTermsListResult>(responseMessage).ConfigureAwait(false);
      return result.SpokenTerms ?? new SpokenTerms[0];
    }

    /// <inheritdoc />
    public async Task<SpokenTerms> GetSpokenTermsAsync(
          string spokenTermsId,
          CancellationToken cancellationToken = default) {
      using var responseMessage = await _client
            .ApiGetAsync(SpokenTermsPath(spokenTermsId), cancellationToken).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.SpokenTerms).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<SpokenTerms>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpokenTerms> GetSpokenTermsAsync(
          SpokenTerms spokenTerms,
          CancellationToken cancellationToken = default) =>
          await GetSpokenTermsAsync(SpokenTermsId(spokenTerms), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SpokenTerms> UpdateSpokenTermsAsync(
          string spokenTermsId,
          UpdateSpokenTermsOptions? options = null,
          CancellationToken cancellationToken = default) {
      var path = SpokenTermsPath(spokenTermsId);
      // With neither field set the body is {} and the API reports the error.
      var requestData = new Dictionary<string, object?>();
      if (options?.Name != null) requestData["name"] = options.Name;
      if (options?.TermLists != null) requestData["term_lists"] = CreateSpokenTermsListsBody(options.TermLists);
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPatchJsonAsync(path, cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.SpokenTerms).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<SpokenTerms>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpokenTerms> UpdateSpokenTermsAsync(
          SpokenTerms spokenTerms,
          UpdateSpokenTermsOptions? options = null,
          CancellationToken cancellationToken = default) =>
          await UpdateSpokenTermsAsync(SpokenTermsId(spokenTerms), options, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SpokenTermsEntries> GetSpokenTermsEntriesAsync(
          string spokenTermsId,
          string lang,
          CancellationToken cancellationToken = default) {
      CheckRequiredLanguage(lang, nameof(lang));
      using var responseMessage = await _client
            .ApiGetAsync(SpokenTermsPath(spokenTermsId) + "/entries", cancellationToken, new[] { ("lang", lang) })
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.SpokenTerms).ConfigureAwait(false);
      var result = await JsonUtils.DeserializeAsync<SpokenTermsEntriesResult>(responseMessage).ConfigureAwait(false);
      return new SpokenTermsEntries(
            result.Lang ?? lang,
            (result.Entries ?? string.Empty).Split('\n').Where(entry => entry.Length > 0));
    }

    /// <inheritdoc />
    public async Task<SpokenTermsEntries> GetSpokenTermsEntriesAsync(
          SpokenTerms spokenTerms,
          string lang,
          CancellationToken cancellationToken = default) =>
          await GetSpokenTermsEntriesAsync(SpokenTermsId(spokenTerms), lang, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SpokenTermsList> ReplaceSpokenTermsListAsync(
          string spokenTermsId,
          SpokenTermsEntries entries,
          ReplaceSpokenTermsListOptions? options = null,
          CancellationToken cancellationToken = default) {
      var requestData = CreateSpokenTermsListBody(entries).ToDictionary(pair => pair.Key, pair => (object?)pair.Value);
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPutJsonAsync(
                  SpokenTermsPath(spokenTermsId) + "/term-lists",
                  cancellationToken,
                  requestData,
                  SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.SpokenTerms).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<SpokenTermsList>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpokenTermsList> ReplaceSpokenTermsListAsync(
          SpokenTerms spokenTerms,
          SpokenTermsEntries entries,
          ReplaceSpokenTermsListOptions? options = null,
          CancellationToken cancellationToken = default) =>
          await ReplaceSpokenTermsListAsync(SpokenTermsId(spokenTerms), entries, options, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteSpokenTermsAsync(
          string spokenTermsId,
          CancellationToken cancellationToken = default) {
      using var responseMessage = await _client
            .ApiDeleteAsync(SpokenTermsPath(spokenTermsId), cancellationToken).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.SpokenTerms).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteSpokenTermsAsync(
          SpokenTerms spokenTerms,
          CancellationToken cancellationToken = default) =>
          await DeleteSpokenTermsAsync(SpokenTermsId(spokenTerms), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteSpokenTermsListAsync(
          string spokenTermsId,
          string lang,
          CancellationToken cancellationToken = default) {
      CheckRequiredLanguage(lang, nameof(lang));
      using var responseMessage = await _client
            .ApiDeleteAsync(SpokenTermsPath(spokenTermsId) + "/term-lists", cancellationToken, new[] { ("lang", lang) })
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.SpokenTerms).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteSpokenTermsListAsync(
          SpokenTerms spokenTerms,
          string lang,
          CancellationToken cancellationToken = default) =>
          await DeleteSpokenTermsListAsync(SpokenTermsId(spokenTerms), lang, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryAsync(
          string name,
          MultilingualGlossaryDictionaryEntries[] glossaryDicts,
          CancellationToken cancellationToken = default) =>
          await CreateMultilingualGlossaryAsync(name, glossaryDicts, null, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryAsync(
          string name,
          MultilingualGlossaryDictionaryEntries[] glossaryDicts,
          CreateMultilingualGlossaryOptions? options,
          CancellationToken cancellationToken = default) {
      if (Argument.NotNull(name, nameof(name)).Length == 0) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      var dictionaries = Argument.NoNullEntries(glossaryDicts, nameof(glossaryDicts));
      if (dictionaries.Length == 0) throw new ArgumentException("Parameter dictionaries must not be empty");

      var requestData = new Dictionary<string, object?> {
            ["name"] = name,
            ["dictionaries"] = dictionaries
                  .Select(dictionary => CreateGlossaryDictionaryBody(
                        dictionary.SourceLanguageCode,
                        dictionary.TargetLanguageCode,
                        dictionary.Entries.ToTsv(),
                        "tsv"))
                  .ToArray()
      };
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPostJsonAsync("v3/glossaries", cancellationToken, requestData, SerializationOptions, true)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary, retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      var glossary =
            await JsonUtils.DeserializeAsync<MultilingualGlossaryInfo>(responseMessage).ConfigureAwait(false);
      return glossary;
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryFromCsvAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default) =>
          await CreateMultilingualGlossaryFromCsvAsync(
                name,
                sourceLanguageCode,
                targetLanguageCode,
                csvFile,
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryFromCsvAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CreateMultilingualGlossaryOptions? options,
          CancellationToken cancellationToken = default) {
      if (Argument.NotNull(name, nameof(name)).Length == 0) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      CheckRequiredLanguage(sourceLanguageCode, nameof(sourceLanguageCode));
      CheckRequiredLanguage(targetLanguageCode, nameof(targetLanguageCode));

      var csvString = await new StreamReader(Argument.NotNull(csvFile, nameof(csvFile))).ReadToEndAsync()
            .ConfigureAwait(false);
      var requestData = new Dictionary<string, object?> {
            ["name"] = name,
            ["dictionaries"] = new[] {
                  CreateGlossaryDictionaryBody(sourceLanguageCode, targetLanguageCode, csvString, "csv")
            }
      };
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPostJsonAsync("v3/glossaries", cancellationToken, requestData, SerializationOptions, true)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary, retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      var glossary =
            await JsonUtils.DeserializeAsync<MultilingualGlossaryInfo>(responseMessage).ConfigureAwait(false);
      return glossary;
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> GetMultilingualGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default) {
      Argument.NotEmpty(glossaryId, nameof(glossaryId));
      using var responseMessage =
            await _client.ApiGetAsync(MultilingualGlossaryPath(glossaryId), cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<MultilingualGlossaryInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> GetMultilingualGlossaryAsync(
          MultilingualGlossaryInfo glossary,
          CancellationToken cancellationToken = default) =>
          await GetMultilingualGlossaryAsync(MultilingualGlossaryId(glossary), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default) {
      Argument.NotEmpty(glossaryId, nameof(glossaryId));

      var queryParams = CreateLanguageQueryParams(sourceLanguageCode, targetLanguageCode);

      using var responseMessage =
            await _client.ApiGetAsync(
                  MultilingualGlossaryPath(glossaryId) + "/entries",
                  cancellationToken,
                  queryParams).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      var dictionaryEntriesList = await JsonUtils
            .DeserializeAsync<MultilingualGlossaryDictionaryEntriesListResult>(responseMessage)
            .ConfigureAwait(false);

      if (dictionaryEntriesList.Dictionaries.Length == 0) throw new NotFoundException("Glossary dictionary not found");

      // When the source and target language codes are specified, there should be at most one dictionary returned where
      // a NotFoundException would be thrown if no dictionary cannot be found for the given source and target language codes
      return new MultilingualGlossaryDictionaryEntries(dictionaryEntriesList.Dictionaries[0]);
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default) =>
          await GetMultilingualGlossaryDictionaryEntriesAsync(
                MultilingualGlossaryId(glossary),
                Argument.NotNull(glossaryDict, nameof(glossaryDict)).SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default) =>
          await GetMultilingualGlossaryDictionaryEntriesAsync(
                glossaryId,
                Argument.NotNull(glossaryDict, nameof(glossaryDict)).SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default) =>
          await GetMultilingualGlossaryDictionaryEntriesAsync(
                MultilingualGlossaryId(glossary),
                sourceLanguageCode,
                targetLanguageCode,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo[]> ListMultilingualGlossariesAsync(
          CancellationToken cancellationToken = default) {
      using var responseMessage =
            await _client.ApiGetAsync("v3/glossaries", cancellationToken).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return (await JsonUtils.DeserializeAsync<MultilingualGlossaryListResult>(responseMessage).ConfigureAwait(false))
            .Glossaries;
    }

    /// <inheritdoc />
    public async Task DeleteMultilingualGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default) {
      Argument.NotEmpty(glossaryId, nameof(glossaryId));

      using var responseMessage =
            await _client.ApiDeleteAsync(MultilingualGlossaryPath(glossaryId), cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteMultilingualGlossaryAsync(
          MultilingualGlossaryInfo glossary,
          CancellationToken cancellationToken = default) =>
          await DeleteMultilingualGlossaryAsync(MultilingualGlossaryId(glossary), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default) {
      Argument.NotEmpty(glossaryId, nameof(glossaryId));

      var queryParams = CreateLanguageQueryParams(sourceLanguageCode, targetLanguageCode);

      using var responseMessage =
            await _client.ApiDeleteAsync(MultilingualGlossaryPath(glossaryId) + "/dictionaries", cancellationToken, queryParams)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default) =>
          await DeleteMultilingualGlossaryDictionaryAsync(
                MultilingualGlossaryId(glossary),
                sourceLanguageCode,
                targetLanguageCode,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default) =>
          await DeleteMultilingualGlossaryDictionaryAsync(
                MultilingualGlossaryId(glossary),
                Argument.NotNull(glossaryDict, nameof(glossaryDict)).SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default) =>
          await DeleteMultilingualGlossaryDictionaryAsync(
                glossaryId,
                Argument.NotNull(glossaryDict, nameof(glossaryDict)).SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                Argument.NotNull(entries, nameof(entries)).ToTsv(),
                "tsv",
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                MultilingualGlossaryId(glossary),
                sourceLanguageCode,
                targetLanguageCode,
                Argument.NotNull(entries, nameof(entries)).ToTsv(),
                "tsv",
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                MultilingualGlossaryId(glossary),
                Argument.NotNull(glossaryDict, nameof(glossaryDict)).SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                glossaryDict.Entries.ToTsv(),
                "tsv",
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                Argument.NotNull(glossaryDict, nameof(glossaryDict)).SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                glossaryDict.Entries.ToTsv(),
                "tsv",
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                await ReadCsvAsync(csvFile).ConfigureAwait(false),
                "csv",
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                MultilingualGlossaryId(glossary),
                sourceLanguageCode,
                targetLanguageCode,
                await ReadCsvAsync(csvFile).ConfigureAwait(false),
                "csv",
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                Argument.NotNull(entries, nameof(entries)).ToTsv(),
                "tsv",
                options,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryAsync(
                MultilingualGlossaryId(glossary),
                sourceLanguageCode,
                targetLanguageCode,
                entries,
                options,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryAsync(
                glossaryId,
                Argument.NotNull(glossaryDict, nameof(glossaryDict)).SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                glossaryDict.Entries,
                options,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryAsync(
                MultilingualGlossaryId(glossary),
                Argument.NotNull(glossaryDict, nameof(glossaryDict)).SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                glossaryDict.Entries,
                options,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                await ReadCsvAsync(csvFile).ConfigureAwait(false),
                "csv",
                options,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
                MultilingualGlossaryId(glossary),
                sourceLanguageCode,
                targetLanguageCode,
                csvFile,
                options,
                cancellationToken).ConfigureAwait(false);

    private async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryInternalAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          string entries,
          string entriesFormat,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default) {
      Argument.NotEmpty(glossaryId, nameof(glossaryId));
      CheckRequiredLanguage(sourceLanguageCode, nameof(sourceLanguageCode));
      CheckRequiredLanguage(targetLanguageCode, nameof(targetLanguageCode));

      var requestData = new Dictionary<string, object?> {
            ["source_lang"] = sourceLanguageCode,
            ["target_lang"] = targetLanguageCode,
            ["entries"] = entries,
            ["entries_format"] = entriesFormat
      };
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPutJsonAsync(
                  MultilingualGlossaryPath(glossaryId) + "/dictionaries",
                  cancellationToken,
                  requestData,
                  SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<MultilingualGlossaryDictionaryInfo>(responseMessage)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryNameAsync(
          string glossaryId,
          string name,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryNameAsync(glossaryId, name, null, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryNameAsync(
          string glossaryId,
          string name,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default) {
      Argument.NotEmpty(glossaryId, nameof(glossaryId));
      Argument.NotEmpty(name, nameof(name));

      var requestData = new Dictionary<string, object?> { ["name"] = name };
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPatchJsonAsync(MultilingualGlossaryPath(glossaryId), cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<MultilingualGlossaryInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryNameAsync(
          MultilingualGlossaryInfo glossary,
          string name,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryNameAsync(MultilingualGlossaryId(glossary), name, null, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryNameAsync(
          MultilingualGlossaryInfo glossary,
          string name,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryNameAsync(MultilingualGlossaryId(glossary), name, options, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryAsync(
          string glossaryId,
          UpdateMultilingualGlossaryOptions? options = null,
          CancellationToken cancellationToken = default) {
      Argument.NotEmpty(glossaryId, nameof(glossaryId));

      var requestData = new Dictionary<string, object?>();
      if (options?.Name != null) requestData["name"] = options.Name;
      if (options?.Dictionaries != null) {
        requestData["dictionaries"] = Argument.NoNullEntries(options.Dictionaries, nameof(options.Dictionaries))
              .Select(dictionary => CreateGlossaryDictionaryBody(
                    dictionary.SourceLanguageCode,
                    dictionary.TargetLanguageCode,
                    dictionary.Entries.ToTsv(),
                    "tsv"))
              .ToList();
      }

      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPatchJsonAsync(MultilingualGlossaryPath(glossaryId), cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<MultilingualGlossaryInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryAsync(
          MultilingualGlossaryInfo glossary,
          UpdateMultilingualGlossaryOptions? options = null,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryAsync(MultilingualGlossaryId(glossary), options, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryAsync(
                glossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                entries,
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                Argument.NotNull(entries, nameof(entries)).ToTsv(),
                "tsv",
                options,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryAsync(
                glossary,
                sourceLanguageCode,
                targetLanguageCode,
                entries,
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                MultilingualGlossaryId(glossary),
                sourceLanguageCode,
                targetLanguageCode,
                Argument.NotNull(entries, nameof(entries)).ToTsv(),
                "tsv",
                options,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryAsync(
                glossary,
                glossaryDict,
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                MultilingualGlossaryId(glossary),
                Argument.NotNull(glossaryDict, nameof(glossaryDict)).SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                glossaryDict.Entries.ToTsv(),
                "tsv",
                options,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryAsync(
                glossaryId,
                glossaryDict,
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                Argument.NotNull(glossaryDict, nameof(glossaryDict)).SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                glossaryDict.Entries.ToTsv(),
                "tsv",
                options,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryFromCsvAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryFromCsvAsync(
                glossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                csvFile,
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryFromCsvAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                await ReadCsvAsync(csvFile).ConfigureAwait(false),
                "csv",
                options,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryFromCsvAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryFromCsvAsync(
                glossary,
                sourceLanguageCode,
                targetLanguageCode,
                csvFile,
                null,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryFromCsvAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                MultilingualGlossaryId(glossary),
                sourceLanguageCode,
                targetLanguageCode,
                await ReadCsvAsync(csvFile).ConfigureAwait(false),
                "csv",
                options,
                cancellationToken).ConfigureAwait(false);

    private async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryInternalAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          string entries,
          string entriesFormat,
          BaseRequestOptions? options,
          CancellationToken cancellationToken) {
      Argument.NotEmpty(glossaryId, nameof(glossaryId));
      CheckRequiredLanguage(sourceLanguageCode, nameof(sourceLanguageCode));
      CheckRequiredLanguage(targetLanguageCode, nameof(targetLanguageCode));

      var requestData = new Dictionary<string, object?> {
            ["dictionaries"] = new[] {
                  CreateGlossaryDictionaryBody(sourceLanguageCode, targetLanguageCode, entries, entriesFormat)
            }
      };
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPatchJsonAsync(MultilingualGlossaryPath(glossaryId), cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<MultilingualGlossaryInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <summary>
    ///   Retrieves the languages supported by the given DeepL API resource, including whether each language can be
    ///   used as source or target and the features it supports for that resource.
    /// </summary>
    /// <param name="resource">
    ///   Resource to retrieve languages for, for example <see cref="LanguageResource.TranslateText" /> or
    ///   <see cref="LanguageResource.Glossary" />.
    /// </param>
    /// <param name="include">
    ///   Optional values controlling which languages and features are returned, for example
    ///   <see cref="LanguageSupport.IncludeBeta" /> or <see cref="LanguageSupport.IncludeExternal" />. By default only
    ///   stable languages and features are returned.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="LanguageSupport" /> objects, one per supported language.</returns>
    /// <exception cref="ArgumentException">If <paramref name="resource" /> is empty.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    public async Task<LanguageSupport[]> GetLanguagesForResourceAsync(
          string resource,
          IEnumerable<string>? include = null,
          CancellationToken cancellationToken = default) {
      var options = new GetLanguagesForResourceOptions();
      if (include != null) {
        options.Include.AddRange(include);
      }

      return await GetLanguagesForResourceAsync(resource, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///   Retrieves the languages supported by the given DeepL API resource, including whether each language can be
    ///   used as source or target and the features it supports for that resource.
    /// </summary>
    /// <param name="resource">
    ///   Resource to retrieve languages for, for example <see cref="LanguageResource.TranslateText" /> or
    ///   <see cref="LanguageResource.Glossary" />.
    /// </param>
    /// <param name="options">
    ///   Optional <see cref="GetLanguagesForResourceOptions" />, for example
    ///   <see cref="GetLanguagesForResourceOptions.Include" /> to also return beta or external languages and features.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="LanguageSupport" /> objects, one per supported language.</returns>
    /// <exception cref="ArgumentException">If <paramref name="resource" /> is empty.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    public async Task<LanguageSupport[]> GetLanguagesForResourceAsync(
          string resource,
          GetLanguagesForResourceOptions? options,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrEmpty(resource)) {
        throw new ArgumentException($"Parameter {nameof(resource)} must not be empty");
      }

      var queryParams = new List<(string Key, string Value)> { ("resource", resource) };
      if (options != null) {
        queryParams.AddRange(options.Include.Select(value => ("include", value)));
      }

      using var responseMessage = await _client
            .ApiGetAsync("v3/languages", cancellationToken, queryParams).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var languages = await JsonUtils.DeserializeAsync<LanguageSupportResult[]>(responseMessage)
            .ConfigureAwait(false);
      return languages.Select(
                  language => new LanguageSupport(
                        language.Lang,
                        language.Name,
                        language.Status,
                        language.UsableAsSource,
                        language.UsableAsTarget,
                        (language.Features ?? new Dictionary<string, LanguageFeatureResult>()).ToDictionary(
                              feature => feature.Key,
                              feature => new LanguageFeature(feature.Value.Status))))
            .ToArray();
    }

    /// <summary>
    ///   Retrieves the resources supported by the DeepL API, along with their features and whether each feature
    ///   requires support from the source and/or target language.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="LanguageResource" /> objects, one per supported resource.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    public async Task<LanguageResource[]> GetLanguageResourcesAsync(CancellationToken cancellationToken = default) {
      using var responseMessage = await _client
            .ApiGetAsync("v3/languages/resources", cancellationToken).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var resources = await JsonUtils.DeserializeAsync<LanguageResourceResult[]>(responseMessage)
            .ConfigureAwait(false);
      return resources.Select(
                  resource => new LanguageResource(
                        resource.Name,
                        (resource.Features ?? Array.Empty<ResourceFeatureResult>()).ToDictionary(
                              feature => feature.Name,
                              feature => new ResourceFeature(
                                    feature.Name,
                                    feature.NeedsSourceSupport ?? false,
                                    feature.NeedsTargetSupport ?? false))))
            .ToArray();
    }

    /// <summary>Class used for JSON-deserialization of entries returned by <c>GET /v3/languages</c>.</summary>
    private readonly struct LanguageSupportResult {
      /// <summary>Initializes a new instance of <see cref="LanguageSupportResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public LanguageSupportResult(
            string lang,
            string name,
            string status,
            bool usableAsSource,
            bool usableAsTarget,
            Dictionary<string, LanguageFeatureResult>? features) {
        Lang = lang;
        Name = name;
        Status = status;
        UsableAsSource = usableAsSource;
        UsableAsTarget = usableAsTarget;
        Features = features;
      }

      public string Lang { get; }
      public string Name { get; }
      public string Status { get; }
      public bool UsableAsSource { get; }
      public bool UsableAsTarget { get; }
      public Dictionary<string, LanguageFeatureResult>? Features { get; }
    }

    /// <summary>Class used for JSON-deserialization of per-language feature entries of <c>GET /v3/languages</c>.</summary>
    private readonly struct LanguageFeatureResult {
      /// <summary>Initializes a new instance of <see cref="LanguageFeatureResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public LanguageFeatureResult(string status) {
        Status = status;
      }

      public string Status { get; }
    }

    /// <summary>Class used for JSON-deserialization of entries returned by <c>GET /v3/languages/resources</c>.</summary>
    private readonly struct LanguageResourceResult {
      /// <summary>Initializes a new instance of <see cref="LanguageResourceResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public LanguageResourceResult(string name, ResourceFeatureResult[]? features) {
        Name = name;
        Features = features;
      }

      public string Name { get; }
      public ResourceFeatureResult[]? Features { get; }
    }

    /// <summary>Class used for JSON-deserialization of resource feature entries of <c>GET /v3/languages/resources</c>.</summary>
    private readonly struct ResourceFeatureResult {
      /// <summary>Initializes a new instance of <see cref="ResourceFeatureResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public ResourceFeatureResult(string name, bool? needsSourceSupport, bool? needsTargetSupport) {
        Name = name;
        NeedsSourceSupport = needsSourceSupport;
        NeedsTargetSupport = needsTargetSupport;
      }

      public string Name { get; }
      public bool? NeedsSourceSupport { get; }
      public bool? NeedsTargetSupport { get; }
    }

    /// <summary>Class used for JSON-deserialization of text rephrase results.</summary>
    private readonly struct TextRephraseResult {
      /// <summary>Initializes a new instance of <see cref="TextRephraseResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TextRephraseResult(WriteResult[]? improvements) {
        Improvements = improvements;
      }

      /// <summary>Array of <see cref="WriteResult" /> objects holding text rephrase results.</summary>
      public WriteResult[]? Improvements { get; }
    }

    /// <summary>Class used for JSON-deserialization of glossary dictionary entries list results.</summary>
    private readonly struct MultilingualGlossaryDictionaryEntriesListResult {
      /// <summary>
      ///   Initializes a new instance of <see cref="MultilingualGlossaryDictionaryEntriesListResult" />, used for JSON
      ///   deserialization.
      /// </summary>
      [JsonConstructor]
      public MultilingualGlossaryDictionaryEntriesListResult(
            MultilingualGlossaryDictionaryEntriesResult[] dictionaries) {
        Dictionaries = dictionaries;
      }

      /// <summary>
      ///   Array of <see cref="MultilingualGlossaryDictionaryEntriesResult" /> objects holding glossary dictionary information
      ///   including their entries.
      /// </summary>
      public MultilingualGlossaryDictionaryEntriesResult[] Dictionaries { get; }
    }

    /// <summary>Class used for JSON-deserialization of glossary list results.</summary>
    private readonly struct MultilingualGlossaryListResult {
      /// <summary>
      ///   Initializes a new instance of <see cref="MultilingualGlossaryListResult" />, used for JSON
      ///   deserialization.
      /// </summary>
      [JsonConstructor]
      public MultilingualGlossaryListResult(MultilingualGlossaryInfo[] glossaries) {
        Glossaries = glossaries;
      }

      /// <summary>
      ///   Array of <see cref="MultilingualGlossaryInfo" /> objects holding glossary dictionary information
      ///   including their entries.
      /// </summary>
      public MultilingualGlossaryInfo[] Glossaries { get; }
    }

    /// <summary>
    ///   Returns an array containing the query parameters to include in HTTP request.
    /// </summary>
    /// <param name="sourceLanguageCode"> The source language code of the glossary dictionary </param>
    /// <param name="targetLanguageCode"> The target language code of the glossary dictionary </param>
    /// <returns>An array of key value pairs containing the query parameters to include in HTTP request.</returns>
    /// <exception cref="ArgumentException">If the specified languages or options are invalid.</exception>
    private static (string Key, string Value)[] CreateLanguageQueryParams(
          string sourceLanguageCode,
          string targetLanguageCode) {
      CheckRequiredLanguage(sourceLanguageCode, nameof(sourceLanguageCode));
      CheckRequiredLanguage(targetLanguageCode, nameof(targetLanguageCode));

      return new (string Key, string Value)[] {
            ("source_lang", sourceLanguageCode), ("target_lang", targetLanguageCode)
      };
    }

    /// <summary>Returns the JSON object of one glossary dictionary.</summary>
    /// <param name="sourceLanguageCode">Language code of the source terms.</param>
    /// <param name="targetLanguageCode">Language code of the target terms.</param>
    /// <param name="entries">The entries as TSV or CSV text.</param>
    /// <param name="entriesFormat">The format of the entries, <c>tsv</c> or <c>csv</c>.</param>
    /// <returns>Dictionary to serialize as one entry of the JSON <c>dictionaries</c> array.</returns>
    private static Dictionary<string, string> CreateGlossaryDictionaryBody(
          string sourceLanguageCode,
          string targetLanguageCode,
          string entries,
          string entriesFormat) {
      CheckRequiredLanguage(sourceLanguageCode, nameof(sourceLanguageCode));
      CheckRequiredLanguage(targetLanguageCode, nameof(targetLanguageCode));
      return new Dictionary<string, string> {
            ["source_lang"] = sourceLanguageCode,
            ["target_lang"] = targetLanguageCode,
            ["entries"] = entries,
            ["entries_format"] = entriesFormat
      };
    }

    /// <summary>Returns the API path of the given v3 glossary, with the ID URL-encoded.</summary>
    /// <param name="glossaryId">ID of the glossary.</param>
    /// <returns>The relative path of the glossary.</returns>
    private static string MultilingualGlossaryPath(string glossaryId) =>
          $"v3/glossaries/{Uri.EscapeDataString(glossaryId)}";

    /// <summary>Returns the API path of the given style profile.</summary>
    /// <param name="styleProfileId">ID of the style profile.</param>
    /// <returns>The relative path of the style profile.</returns>
    /// <exception cref="ArgumentException">If the ID is empty.</exception>
    private static string StyleProfilePath(string styleProfileId) {
      if (string.IsNullOrWhiteSpace(styleProfileId)) {
        throw new ArgumentException($"Parameter {nameof(styleProfileId)} must not be empty");
      }

      return $"v3/style_profiles/{Uri.EscapeDataString(styleProfileId)}";
    }

    /// <summary>
    ///   Returns the JSON body of a style profile create or update request, containing only the set fields, followed by
    ///   the extra body parameters of the options.
    /// </summary>
    /// <param name="name">Name of the style profile, or <c>null</c> to leave it out.</param>
    /// <param name="options">Optional fields of the style profile.</param>
    /// <returns>Dictionary to serialize as the JSON body.</returns>
    private static Dictionary<string, object?> CreateStyleProfileBody(
          string? name,
          CreateStyleProfileOptions? options) {
      var body = new Dictionary<string, object?>();
      if (name != null) body["name"] = name;
      if (options == null) return body;
      if (options.Description != null) body["description"] = options.Description;
      if (options.Industry != null) body["industry"] = options.Industry;
      if (options.GlossaryIds != null) body["glossary_ids"] = options.GlossaryIds.ToArray();
      if (options.StyleRuleIds != null) body["style_rule_ids"] = options.StyleRuleIds.ToArray();
      if (options.TranslationMemoryIds != null) body["translation_memory_ids"] = options.TranslationMemoryIds.ToArray();
      if (options.GlossaryPriorityOrderEnabled != null) {
        body["glossary_priority_order_enabled"] = options.GlossaryPriorityOrderEnabled.Value;
      }

      AddExtraBodyParameters(body, options);
      return body;
    }

    /// <summary>Class used for JSON-deserialization of the style profile list result.</summary>
    private readonly struct StyleProfileListResult {
      /// <summary>Initializes a new instance of <see cref="StyleProfileListResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public StyleProfileListResult(StyleProfile[]? styleProfiles) {
        StyleProfiles = styleProfiles;
      }

      /// <summary>Array of <see cref="StyleProfile" /> objects; entries that are not objects are skipped.</summary>
      [JsonConverter(typeof(LenientArrayConverter<StyleProfile>))]
      public StyleProfile[]? StyleProfiles { get; }
    }

    /// <summary>Returns the API path of the given Spoken Terms collection.</summary>
    /// <param name="spokenTermsId">ID of the Spoken Terms collection.</param>
    /// <returns>The relative path of the collection.</returns>
    /// <exception cref="ArgumentException">If the ID is empty.</exception>
    private static string SpokenTermsPath(string spokenTermsId) {
      if (string.IsNullOrWhiteSpace(spokenTermsId)) {
        throw new ArgumentException($"Parameter {nameof(spokenTermsId)} must not be empty");
      }

      return $"v3/spoken-terms/{Uri.EscapeDataString(spokenTermsId)}";
    }

    /// <summary>Returns the JSON body of one term list; the API takes the terms as one newline-separated string.</summary>
    /// <param name="entries">The language and terms of the term list.</param>
    /// <returns>Dictionary to serialize as the JSON body.</returns>
    /// <exception cref="ArgumentException">If any term is invalid.</exception>
    private static Dictionary<string, string> CreateSpokenTermsListBody(SpokenTermsEntries entries) {
      CheckRequiredLanguage(entries.Lang, nameof(entries.Lang));
      // Trimmed like glossary terms; a newline inside a term would silently split it into several terms on the wire.
      var terms = entries.Entries.Select(term => term.Trim()).ToArray();
      foreach (var term in terms) {
        GlossaryEntries.ValidateGlossaryTerm(term);
      }

      return new Dictionary<string, string> { ["lang"] = entries.Lang, ["entries"] = string.Join("\n", terms) };
    }

    /// <summary>Reads the CSV content of a dictionary update or replace from the stream.</summary>
    /// <param name="csvFile">Stream containing the CSV content.</param>
    /// <returns>The CSV content.</returns>
    /// <exception cref="ArgumentException">If the content is empty or contains only whitespace.</exception>
    private static async Task<string> ReadCsvAsync(Stream csvFile) {
      var csv = await new StreamReader(Argument.NotNull(csvFile, nameof(csvFile))).ReadToEndAsync().ConfigureAwait(false);
      if (string.IsNullOrWhiteSpace(csv)) {
        throw new ArgumentException($"Parameter {nameof(csvFile)} must not be empty", nameof(csvFile));
      }

      return csv;
    }

    /// <summary>
    ///   Adds the extra body parameters of the options last, replacing fields with the same key; a <c>null</c> value
    ///   is sent as JSON <c>null</c>.
    /// </summary>
    private static void AddExtraBodyParameters(Dictionary<string, object?> requestData, BaseRequestOptions? options) {
      if (options?.ExtraBodyParameters == null) return;
      foreach (var pair in options.ExtraBodyParameters) {
        requestData[pair.Key] = pair.Value;
      }
    }

    /// <summary>Returns the ID of the given style rule, or an empty string if it is <c>null</c>.</summary>
    private static string StyleRuleId(StyleRuleInfo? styleRule) => styleRule?.StyleId ?? string.Empty;

    /// <summary>Returns the ID of the given custom instruction, or an empty string if it or its ID is <c>null</c>.</summary>
    private static string InstructionId(CustomInstruction? instruction) => instruction?.Id ?? string.Empty;

    /// <summary>Throws if the required language code is <c>null</c>, empty or whitespace-only.</summary>
    private static void CheckRequiredLanguage(string? languageCode, string paramName) =>
          Argument.NotEmpty(languageCode, paramName);

    /// <summary>Throws if the optional language code is given but empty or whitespace-only.</summary>
    private static void CheckOptionalLanguage(string? languageCode, string paramName) {
      if (languageCode != null && string.IsNullOrWhiteSpace(languageCode)) {
        throw new ArgumentException($"Parameter {paramName} must not be empty", paramName);
      }
    }

    /// <summary>Throws if the optional resource ID is given but empty or whitespace-only.</summary>
    private static void CheckOptionalId(string? id, string optionName) {
      if (id != null && string.IsNullOrWhiteSpace(id)) {
        throw new ArgumentException($"{optionName} must not be empty", optionName);
      }
    }

    /// <summary>Returns the ID of the given v2 glossary, throwing if it is <c>null</c>.</summary>
    private static string GlossaryId(GlossaryInfo glossary) => Argument.NotNull(glossary, nameof(glossary)).GlossaryId;

    /// <summary>Returns the ID of the given multilingual glossary, throwing if it is <c>null</c>.</summary>
    private static string MultilingualGlossaryId(MultilingualGlossaryInfo glossary) =>
          Argument.NotNull(glossary, nameof(glossary)).GlossaryId;

    /// <summary>Returns the ID of the given translation memory, throwing if it is <c>null</c>.</summary>
    private static string TranslationMemoryId(TranslationMemoryInfo translationMemory) =>
          Argument.NotNull(translationMemory, nameof(translationMemory)).TranslationMemoryId;

    /// <summary>Returns the ID of the given translation memory job, throwing if it is <c>null</c>.</summary>
    private static string TranslationMemoryJobId(TranslationMemoryJob job) =>
          Argument.NotNull(job, nameof(job)).JobId;

    /// <summary>Returns the ID of the given style profile, throwing if it is <c>null</c>.</summary>
    private static string StyleProfileId(StyleProfile styleProfile) =>
          Argument.NotNull(styleProfile, nameof(styleProfile)).StyleProfileId;

    /// <summary>Returns the ID of the given spoken terms collection, throwing if it is <c>null</c>.</summary>
    private static string SpokenTermsId(SpokenTerms spokenTerms) =>
          Argument.NotNull(spokenTerms, nameof(spokenTerms)).SpokenTermsId;

    /// <summary>
    ///   Adds the extra body parameters of the options last to form fields, replacing fields with the same key; a
    ///   <c>null</c> value removes the field.
    /// </summary>
    private static void AddExtraBodyParameters(List<(string Key, string Value)> bodyParams, BaseRequestOptions? options) {
      if (options?.ExtraBodyParameters == null) return;
      foreach (var pair in options.ExtraBodyParameters) {
        bodyParams.RemoveAll(param => param.Key == pair.Key);
        if (pair.Value != null) {
          bodyParams.Add((pair.Key, FormValue(pair.Value)));
        }
      }
    }

    /// <summary>
    ///   Returns the form field text of an extra body parameter: strings as given, collections comma-joined, booleans as
    ///   <c>true</c>/<c>false</c>, numbers in the invariant culture, anything else as its JSON text.
    /// </summary>
    private static string FormValue(object value) {
      switch (value) {
        case string text:
          return text;
        case bool flag:
          return flag ? "true" : "false";
        case IEnumerable collection:
          return string.Join(",", collection.Cast<object?>().Select(item => item == null ? "" : FormValue(item)));
        case IFormattable number when IsNumber(number):
          return number.ToString(null, CultureInfo.InvariantCulture);
        default:
          return JsonSerializer.Serialize(value, value.GetType(), SerializationOptions);
      }
    }

    private static bool IsNumber(object value) =>
          value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint ||
          value is long || value is ulong || value is float || value is double || value is decimal;

    /// <summary>Returns the JSON <c>term_lists</c> array for the given term lists.</summary>
    /// <param name="termLists">The language and terms of each term list.</param>
    /// <returns>List to serialize as the JSON <c>term_lists</c> array.</returns>
    private static List<Dictionary<string, string>> CreateSpokenTermsListsBody(
          IEnumerable<SpokenTermsEntries> termLists) =>
          termLists.Select(CreateSpokenTermsListBody).ToList();

    /// <summary>Class used for JSON-deserialization of the Spoken Terms list result.</summary>
    private readonly struct SpokenTermsListResult {
      /// <summary>Initializes a new instance of <see cref="SpokenTermsListResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public SpokenTermsListResult(SpokenTerms[]? spokenTerms) {
        SpokenTerms = spokenTerms;
      }

      /// <summary>Array of <see cref="DeepL.Model.SpokenTerms" /> objects.</summary>
      public SpokenTerms[]? SpokenTerms { get; }
    }

    /// <summary>Class used for JSON-deserialization of Spoken Terms entries.</summary>
    private readonly struct SpokenTermsEntriesResult {
      /// <summary>Initializes a new instance of <see cref="SpokenTermsEntriesResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public SpokenTermsEntriesResult(string? lang, string? entries) {
        (Lang, Entries) = (lang, entries);
      }

      /// <summary>Language code of the term list.</summary>
      public string? Lang { get; }

      /// <summary>The terms as one newline-separated string.</summary>
      public string? Entries { get; }
    }

    /// <summary>JSON serializer options for JSON-encoded request bodies.</summary>
    private static readonly JsonSerializerOptions SerializationOptions = new JsonSerializerOptions {
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Interval between polls of a translation memory job in <see cref="WaitUntilTranslationMemoryJobDoneAsync" />.</summary>
    private static readonly TimeSpan TranslationMemoryJobPollInterval = TimeSpan.FromSeconds(5);

    /// <summary>Checks the given translation memory ID is usable, otherwise throws an exception.</summary>
    /// <param name="translationMemoryId">The translation memory ID to check.</param>
    /// <exception cref="ArgumentException">If the translation memory ID is empty.</exception>
    private static void CheckTranslationMemoryId(string translationMemoryId) {
      if (string.IsNullOrWhiteSpace(translationMemoryId)) {
        throw new ArgumentException($"Parameter {nameof(translationMemoryId)} must not be empty");
      }
    }

    /// <summary>Creates a <see cref="TranslationMemoryJob" /> from the JSON-deserialized DeepL API response.</summary>
    /// <param name="response">The deserialized job response.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object holding the flattened job status.</returns>
    private static TranslationMemoryJob CreateTranslationMemoryJob(TranslationMemoryJobResponse response) {
      var results = (response.Results ?? new TranslationMemoryJobResultResponse[0])
            .Select(
                  result => new TranslationMemoryJobResult(
                        result.Status ?? string.Empty,
                        result.StatusMetadata?.RequiredAction,
                        result.DownloadUrl,
                        result.ExpiresAt,
                        result.Error?.Message,
                        result.TranslationMemoryId,
                        result.SkippedSegmentCount))
            .ToArray();

      return new TranslationMemoryJob(
            response.JobId,
            response.Operation,
            results,
            response.Product ?? throw MissingTranslationMemoryJobField("product"),
            response.CreationTime ?? throw MissingTranslationMemoryJobField("creation_time"),
            response.UpdatedTime ?? throw MissingTranslationMemoryJobField("updated_time"),
            response.Parameters?.TranslationMemoryId,
            response.Parameters?.DisplayName,
            response.SourceFile?.ContentType,
            response.SourceFile?.ContentLength);
    }

    /// <summary>Returns the exception for a required field missing from a translation memory job response.</summary>
    private static DeepLException MissingTranslationMemoryJobField(string fieldName) =>
          new DeepLException(
                $"Failed to deserialize JSON in received response: translation memory job without {fieldName}");

    /// <summary>Class used for JSON-deserialization of translation memory export creation results.</summary>
    private readonly struct TranslationMemoryExportResult {
      /// <summary>Initializes a new instance of <see cref="TranslationMemoryExportResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TranslationMemoryExportResult(string jobId, TranslationMemoryJobParameters? parameters) {
        JobId = jobId;
        Parameters = parameters;
      }

      /// <summary>Unique ID assigned to the export job.</summary>
      [JsonPropertyName("job_id")]
      public string JobId { get; }

      /// <summary>Parameters of the export job, holding the translation memory ID.</summary>
      [JsonPropertyName("parameters")]
      public TranslationMemoryJobParameters? Parameters { get; }
    }

    // The nested job objects are typed rather than string dictionaries, so that fields the API adds later, of any
    // JSON type, are ignored instead of failing the call.
    /// <summary>Class used for JSON-deserialization of the parameters of a translation memory job.</summary>
    private readonly struct TranslationMemoryJobParameters {
      /// <summary>Initializes a new instance of <see cref="TranslationMemoryJobParameters" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TranslationMemoryJobParameters(string? translationMemoryId, string? displayName) {
        TranslationMemoryId = translationMemoryId;
        DisplayName = displayName;
      }

      /// <summary>ID of the translation memory an export job reads from.</summary>
      [JsonPropertyName("translation_memory_id")]
      public string? TranslationMemoryId { get; }

      /// <summary>Name an import job assigns to the new translation memory.</summary>
      [JsonPropertyName("display_name")]
      public string? DisplayName { get; }
    }

    /// <summary>Class used for JSON-deserialization of the status metadata of a translation memory job result.</summary>
    private readonly struct TranslationMemoryJobStatusMetadata {
      /// <summary>Initializes a new instance of <see cref="TranslationMemoryJobStatusMetadata" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TranslationMemoryJobStatusMetadata(string? requiredAction) {
        RequiredAction = requiredAction;
      }

      /// <summary>Description of the action the caller must take.</summary>
      [JsonPropertyName("required_action")]
      public string? RequiredAction { get; }
    }

    /// <summary>Class used for JSON-deserialization of the error of a translation memory job result.</summary>
    private readonly struct TranslationMemoryJobError {
      /// <summary>Initializes a new instance of <see cref="TranslationMemoryJobError" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TranslationMemoryJobError(string? message) {
        Message = message;
      }

      /// <summary>Short description of the error.</summary>
      [JsonPropertyName("message")]
      public string? Message { get; }
    }

    /// <summary>Class used for JSON-deserialization of the source file of a translation memory import job.</summary>
    private readonly struct TranslationMemoryJobSourceFile {
      /// <summary>Initializes a new instance of <see cref="TranslationMemoryJobSourceFile" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TranslationMemoryJobSourceFile(string? contentType, long? contentLength) {
        ContentType = contentType;
        ContentLength = contentLength;
      }

      /// <summary>MIME type declared for the file.</summary>
      [JsonPropertyName("content_type")]
      public string? ContentType { get; }

      /// <summary>Size in bytes declared for the file.</summary>
      [JsonPropertyName("content_length")]
      public long? ContentLength { get; }
    }

    /// <summary>Class used for JSON-deserialization of the results of a translation memory job.</summary>
    private readonly struct TranslationMemoryJobResultResponse {
      /// <summary>Initializes a new instance of <see cref="TranslationMemoryJobResultResponse" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TranslationMemoryJobResultResponse(
            string? status,
            TranslationMemoryJobStatusMetadata? statusMetadata,
            string? downloadUrl,
            DateTime? expiresAt,
            TranslationMemoryJobError? error,
            string? translationMemoryId,
            long? skippedSegmentCount) {
        Status = status;
        StatusMetadata = statusMetadata;
        DownloadUrl = downloadUrl;
        ExpiresAt = expiresAt;
        Error = error;
        TranslationMemoryId = translationMemoryId;
        SkippedSegmentCount = skippedSegmentCount;
      }

      /// <summary>Status of the job as returned by the API, kept as a string so unknown statuses do not fail.</summary>
      [JsonPropertyName("status")]
      public string? Status { get; }

      /// <summary>Metadata describing the action the caller must take, if any.</summary>
      [JsonPropertyName("status_metadata")]
      public TranslationMemoryJobStatusMetadata? StatusMetadata { get; }

      /// <summary>Download URL of the exported TMX file, set once an export completes.</summary>
      [JsonPropertyName("download_url")]
      public string? DownloadUrl { get; }

      /// <summary>Time after which the download URL is no longer valid.</summary>
      [JsonPropertyName("expires_at")]
      public DateTime? ExpiresAt { get; }

      /// <summary>Error information, set when the job failed.</summary>
      [JsonPropertyName("error")]
      public TranslationMemoryJobError? Error { get; }

      /// <summary>ID of the translation memory created by a completed import.</summary>
      [JsonPropertyName("translation_memory_id")]
      public string? TranslationMemoryId { get; }

      /// <summary>Number of segments an import skipped.</summary>
      [JsonPropertyName("skipped_segment_count")]
      public long? SkippedSegmentCount { get; }
    }

    /// <summary>Class used for JSON-deserialization of translation memory job status results.</summary>
    private readonly struct TranslationMemoryJobResponse {
      /// <summary>Initializes a new instance of <see cref="TranslationMemoryJobResponse" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TranslationMemoryJobResponse(
            string jobId,
            string operation,
            TranslationMemoryJobResultResponse[]? results,
            string? product,
            DateTime? creationTime,
            DateTime? updatedTime,
            TranslationMemoryJobParameters? parameters,
            TranslationMemoryJobSourceFile? sourceFile) {
        JobId = jobId;
        Operation = operation;
        Results = results;
        Product = product;
        CreationTime = creationTime;
        UpdatedTime = updatedTime;
        Parameters = parameters;
        SourceFile = sourceFile;
      }

      /// <summary>Unique ID assigned to the job.</summary>
      [JsonPropertyName("job_id")]
      public string JobId { get; }

      /// <summary>Operation the job performs, either "import" or "export".</summary>
      [JsonPropertyName("operation")]
      public string Operation { get; }

      /// <summary>Results of the job.</summary>
      [JsonPropertyName("results")]
      public TranslationMemoryJobResultResponse[]? Results { get; }

      /// <summary>Product the job belongs to.</summary>
      [JsonPropertyName("product")]
      public string? Product { get; }

      /// <summary>Time when the job was created.</summary>
      [JsonPropertyName("creation_time")]
      public DateTime? CreationTime { get; }

      /// <summary>Time when the job was last updated.</summary>
      [JsonPropertyName("updated_time")]
      public DateTime? UpdatedTime { get; }

      /// <summary>Parameters of the job, holding the translation memory ID or display name.</summary>
      [JsonPropertyName("parameters")]
      public TranslationMemoryJobParameters? Parameters { get; }

      /// <summary>Source file declared by an import job.</summary>
      [JsonPropertyName("source_file")]
      public TranslationMemoryJobSourceFile? SourceFile { get; }
    }

    /// <summary>Class used for JSON-deserialization of translation memory list results.</summary>
    private readonly struct TranslationMemoryListResult {
      /// <summary>Initializes a new instance of <see cref="TranslationMemoryListResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TranslationMemoryListResult(TranslationMemoryInfo[] translationMemories) {
        TranslationMemories = translationMemories;
      }

      /// <summary>Array of <see cref="TranslationMemoryInfo" /> objects holding translation memory information.</summary>
      [JsonPropertyName("translation_memories")]
      public TranslationMemoryInfo[] TranslationMemories { get; }
    }

    /// <summary>Class used for JSON-deserialization of style rule list results.</summary>
    private readonly struct StyleRuleListResult {
      /// <summary>Initializes a new instance of <see cref="StyleRuleListResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public StyleRuleListResult(StyleRuleInfo[] styleRules) {
        StyleRules = styleRules;
      }

      /// <summary>Array of <see cref="StyleRuleInfo" /> objects holding style rule information.</summary>
      [JsonPropertyName("style_rules")]
      public StyleRuleInfo[] StyleRules { get; }
    }
  }
}
