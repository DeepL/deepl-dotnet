// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Internal;
using DeepL.Model;

namespace DeepL {
  public interface IWriter : IDisposable {
    /// <summary>Rephrase specified texts, improving them by fixing grammar and spelling errors.</summary>
    /// <param name="texts">Texts to improve; must not be empty.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">Extra <see cref="TextRephraseOptions" /> influencing rephrasing.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Texts without grammatical or spelling errors.</returns>
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
    /// <param name="text">Text to improve; must not be empty.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">Extra <see cref="TextRephraseOptions" /> influencing rephrasing.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Texts without grammatical or spelling errors.</returns>
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

    /// <summary>Correct specified texts, improving them by fixing grammar and spelling errors.</summary>
    /// <param name="texts">Texts to improve; must not be empty.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">Extra <see cref="CorrectTextOptions" /> influencing correction.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Texts without grammatical or spelling errors.</returns>
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

    /// <summary>Correct specified text, improving it by fixing grammar and spelling errors.</summary>
    /// <param name="text">Text to improve; must not be empty.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">Extra <see cref="CorrectTextOptions" /> influencing correction.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Text without grammatical or spelling errors.</returns>
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
  }

  /// <summary>
  ///   Client for the DeepL API. To use the DeepL API, initialize an instance of this class using your DeepL
  ///   Authentication Key. All functions are thread-safe, aside from <see cref="DeepLClient.Dispose" />.
  /// </summary>
#pragma warning disable CS0618 // derives from the deprecated Translator until the next major version
  public sealed class DeepLClient : Translator, IWriter, IGlossaryManager, IStyleRuleManager, IStyleProfileManager,
        ITranslationMemoryManager, ISpokenTermsManager {
#pragma warning restore CS0618
    /// <summary>Initializes a new instance of the <see cref="AuthorizationException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public DeepLClient(string authKey, DeepLClientOptions? options = null) : base(authKey, options) { }

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
    public static bool IsFreeAccountAuthKey(string authKey) => IsFreeAccountKey(authKey);

    /// <inheritdoc />
    public async Task<WriteResult> RephraseTextAsync(
          string text,
          string? targetLanguageCode = null,
          TextRephraseOptions? options = null,
          CancellationToken cancellationToken = default) => (await RephraseTextAsync(
                new[] { text },
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
      var bodyParams = new List<(string Key, string Value)>();
      if (targetLanguageCode != null) {
        CheckValidLanguages(null, targetLanguageCode);
        bodyParams.Add(("target_lang", targetLanguageCode));
      }

      var textList = CheckWriteTexts(texts);
      if (options != null && options.WritingStyle != null) {
        bodyParams.Add(("writing_style", options.WritingStyle));
      }

      if (options != null && options.WritingTone != null) {
        bodyParams.Add(("tone", options.WritingTone));
      }
      // TODO add `show_billed_characters` once write API supports it.

      return await WriteAsync("v2/write/rephrase", bodyParams, textList, options, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WriteResult> CorrectTextAsync(
          string text,
          string? targetLanguageCode = null,
          CorrectTextOptions? options = null,
          CancellationToken cancellationToken = default) => (await CorrectTextAsync(
                new[] { text },
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
      var bodyParams = new List<(string Key, string Value)>();
      if (targetLanguageCode != null) {
        CheckValidLanguages(null, targetLanguageCode);
        bodyParams.Add(("target_lang", targetLanguageCode));
      }

      var textList = CheckWriteTexts(texts);
      return await WriteAsync("v2/write/correct", bodyParams, textList, options, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Returns the texts of a Write API request as a list, checking that none is empty.</summary>
    /// <exception cref="ArgumentException">If any text is empty.</exception>
    private static List<string> CheckWriteTexts(IEnumerable<string> texts) =>
          texts.Select(text => text.Length > 0 ? text : throw new ArgumentException("text must not be empty"))
                .ToList();

    /// <summary>
    ///   Sends a Write API request with the given parameters, then the texts, then the extra body parameters of the
    ///   options, and returns the improvements.
    /// </summary>
    private async Task<WriteResult[]> WriteAsync(
          string path,
          List<(string Key, string Value)> bodyParams,
          List<string> texts,
          BaseRequestOptions? options,
          CancellationToken cancellationToken) {
      bodyParams.AddRange(texts.Select(text => ("text", text)));
      if (options?.ExtraBodyParameters != null) {
        foreach (var pair in options.ExtraBodyParameters) {
          bodyParams.RemoveAll(param => param.Key == pair.Key);
          bodyParams.Add((pair.Key, pair.Value));
        }
      }

      using var responseMessage = await _client
            .ApiPostAsync(path, cancellationToken, bodyParams)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var result = await JsonUtils.DeserializeAsync<TextRephraseResult>(responseMessage).ConfigureAwait(false);
      return result.Improvements;
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
          await GetTranslationMemoryAsync(translationMemory.TranslationMemoryId, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemorySegments> ListTranslationMemorySegmentsAsync(
          string translationMemoryId,
          int? pageSize = null,
          string? pageCursor = null,
          string? filterText = null,
          bool? filterCaseSensitive = null,
          CancellationToken cancellationToken = default) {
      CheckTranslationMemoryId(translationMemoryId);

      var queryParams = new List<(string Key, string Value)>();

      if (pageSize != null) {
        queryParams.Add(("page_size", pageSize.Value.ToString()));
      }

      if (pageCursor != null) {
        queryParams.Add(("page_cursor", pageCursor));
      }

      if (filterText != null) {
        queryParams.Add(("filter_text", filterText));
      }

      if (filterCaseSensitive != null) {
        queryParams.Add(("filter_case_sensitive", filterCaseSensitive.Value.ToString().ToLower()));
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
                      translationMemory.TranslationMemoryId,
                      pageSize,
                      pageCursor,
                      filterText,
                      filterCaseSensitive,
                      cancellationToken)
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
          await DeleteTranslationMemoryAsync(translationMemory.TranslationMemoryId, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TranslationMemoryImport> CreateTranslationMemoryImportAsync(
          string fileName,
          long contentLength,
          string? contentType = null,
          string? displayName = null,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(fileName)) {
        throw new ArgumentException($"Parameter {nameof(fileName)} must not be empty");
      }

      if (contentLength <= 0) {
        throw new ArgumentException($"Parameter {nameof(contentLength)} must be greater than 0");
      }

      var sourceFile = new Dictionary<string, object> {
        ["file_name"] = fileName,
        ["content_length"] = contentLength
      };
      if (contentType != null) sourceFile["content_type"] = contentType;
      var requestData = new Dictionary<string, object> { ["source_file"] = sourceFile };
      if (displayName != null) {
        requestData["parameters"] = new Dictionary<string, object> { ["display_name"] = displayName };
      }

      using var responseMessage = await _client
            .ApiPostJsonAsync("v3/translation_memories/import", cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.TranslationMemory)
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
                  cancellationToken)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.TranslationMemory)
            .ConfigureAwait(false);
      // 200 means the DeepL API reused a previously completed export, 202 that it started a new one.
      var reusedExisting = responseMessage.StatusCode == HttpStatusCode.OK;
      var exportResult = await JsonUtils.DeserializeAsync<TranslationMemoryExportResult>(responseMessage)
            .ConfigureAwait(false);
      return new TranslationMemoryExport(
            exportResult.JobId,
            GetParameter(exportResult.Parameters, "translation_memory_id"),
            reusedExisting);
    }

    /// <inheritdoc />
    public async Task<TranslationMemoryExport> CreateTranslationMemoryExportAsync(
          TranslationMemoryInfo translationMemory,
          CancellationToken cancellationToken = default) =>
          await CreateTranslationMemoryExportAsync(translationMemory.TranslationMemoryId, cancellationToken)
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
    public async Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          string jobId,
          CancellationToken cancellationToken = default) {
      var job = await GetTranslationMemoryJobAsync(jobId, cancellationToken).ConfigureAwait(false);
      while (!job.Done) {
        if (job.Result == null) {
          throw new DeepLException("Translation memory job status contained no result");
        }

        await Task.Delay(TranslationMemoryJobPollInterval, cancellationToken).ConfigureAwait(false);
        job = await GetTranslationMemoryJobAsync(jobId, cancellationToken).ConfigureAwait(false);
      }

      if (!job.Ok) {
        throw new DeepLException(job.Result?.ErrorMessage ?? $"Translation memory job {job.Status}");
      }

      return job;
    }

    /// <inheritdoc />
    public async Task DownloadTranslationMemoryExportAsync(
          TranslationMemoryJob job,
          Stream outputFile,
          CancellationToken cancellationToken = default) {
      var downloadUrl = job.Result?.DownloadUrl;
      if (string.IsNullOrEmpty(downloadUrl)) {
        throw new ArgumentException(
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
          CancellationToken cancellationToken = default) {
      var inputFileInfo = new FileInfo(inputFilePath);
      if (!inputFileInfo.Exists) {
        throw new ArgumentException($"File does not exist: {inputFilePath}");
      }

      var created = await CreateTranslationMemoryImportAsync(
                  inputFileInfo.Name,
                  inputFileInfo.Length,
                  displayName: displayName,
                  cancellationToken: cancellationToken)
            .ConfigureAwait(false);

      using (var inputFileStream = inputFileInfo.OpenRead()) {
        await UploadTranslationMemoryFileAsync(created, inputFileStream, cancellationToken: cancellationToken)
              .ConfigureAwait(false);
      }

      var job = await WaitUntilTranslationMemoryJobDoneAsync(created.JobId, cancellationToken).ConfigureAwait(false);
      return job;
    }

    /// <inheritdoc />
    public async Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          string translationMemoryId,
          string outputFilePath,
          CancellationToken cancellationToken = default) {
      var created = await CreateTranslationMemoryExportAsync(translationMemoryId, cancellationToken)
            .ConfigureAwait(false);
      var job = await WaitUntilTranslationMemoryJobDoneAsync(created.JobId, cancellationToken).ConfigureAwait(false);
      await DownloadTranslationMemoryExportAsync(job, outputFilePath, cancellationToken).ConfigureAwait(false);
      return job;
    }

    /// <inheritdoc />
    public async Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          TranslationMemoryInfo translationMemory,
          string outputFilePath,
          CancellationToken cancellationToken = default) =>
          await ExportTranslationMemoryToFilepathAsync(
                      translationMemory.TranslationMemoryId,
                      outputFilePath,
                      cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StyleRuleInfo> CreateStyleRuleAsync(
          string name,
          string language,
          ConfiguredRules? configuredRules = null,
          CustomInstruction[]? customInstructions = null,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(name)) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(language)) {
        throw new ArgumentException($"Parameter {nameof(language)} must not be empty");
      }

      var requestData = new Dictionary<string, object> { ["name"] = name, ["language"] = language };
      if (configuredRules != null) requestData["configured_rules"] = configuredRules;
      if (customInstructions != null) requestData["custom_instructions"] = customInstructions;
      using var responseMessage = await _client
            .ApiPostJsonAsync("v3/style_rules", cancellationToken, requestData, SerializationOptions).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
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
    public async Task<StyleRuleInfo> UpdateStyleRuleNameAsync(
          string styleId,
          string name,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(name)) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      var requestData = new Dictionary<string, object> { ["name"] = name };
      using var responseMessage =
            await _client.ApiPatchJsonAsync($"v3/style_rules/{Uri.EscapeDataString(styleId)}", cancellationToken, requestData, SerializationOptions)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<StyleRuleInfo>(responseMessage).ConfigureAwait(false);
    }

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
    public async Task<StyleRuleInfo> UpdateStyleRuleConfiguredRulesAsync(
          string styleId,
          ConfiguredRules configuredRules,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(styleId)) {
        throw new ArgumentException($"Parameter {nameof(styleId)} must not be empty");
      }

      if (configuredRules == null) {
        throw new ArgumentNullException(nameof(configuredRules));
      }

      using var responseMessage = await _client
            .ApiPutJsonAsync($"v3/style_rules/{Uri.EscapeDataString(styleId)}/configured_rules", cancellationToken, configuredRules, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<StyleRuleInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CustomInstruction> CreateStyleRuleCustomInstructionAsync(
          string styleId,
          string label,
          string prompt,
          string? sourceLanguage = null,
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

      var requestData = new Dictionary<string, object> { ["label"] = label, ["prompt"] = prompt };
      if (sourceLanguage != null) requestData["source_language"] = sourceLanguage;
      using var responseMessage = await _client
            .ApiPostJsonAsync($"v3/style_rules/{Uri.EscapeDataString(styleId)}/custom_instructions", cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleRule).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<CustomInstruction>(responseMessage).ConfigureAwait(false);
    }

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
    public async Task<CustomInstruction> UpdateStyleRuleCustomInstructionAsync(
          string styleId,
          string instructionId,
          string label,
          string prompt,
          string? sourceLanguage = null,
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

      var requestData = new Dictionary<string, object> { ["label"] = label, ["prompt"] = prompt };
      if (sourceLanguage != null) requestData["source_language"] = sourceLanguage;
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
                  SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.StyleProfile).ConfigureAwait(false);
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
          await GetStyleProfileAsync(styleProfile.StyleProfileId, cancellationToken).ConfigureAwait(false);

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
          await UpdateStyleProfileAsync(styleProfile.StyleProfileId, options, cancellationToken)
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
          await DeleteStyleProfileAsync(styleProfile.StyleProfileId, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SpokenTerms> CreateSpokenTermsAsync(
          string name,
          IEnumerable<SpokenTermsEntries> termLists,
          CreateSpokenTermsOptions? options = null,
          CancellationToken cancellationToken = default) {
      var requestData = new Dictionary<string, object> {
            ["name"] = name, ["term_lists"] = CreateSpokenTermsListsBody(termLists)
      };
      AddExtraBodyParameters(requestData, options);
      using var responseMessage = await _client
            .ApiPostJsonAsync("v3/spoken-terms", cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.SpokenTerms).ConfigureAwait(false);
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
          await GetSpokenTermsAsync(spokenTerms.SpokenTermsId, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SpokenTerms> UpdateSpokenTermsAsync(
          string spokenTermsId,
          UpdateSpokenTermsOptions? options = null,
          CancellationToken cancellationToken = default) {
      var path = SpokenTermsPath(spokenTermsId);
      // With neither field set the body is {} and the API reports the error.
      var requestData = new Dictionary<string, object>();
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
          await UpdateSpokenTermsAsync(spokenTerms.SpokenTermsId, options, cancellationToken)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SpokenTermsEntries> GetSpokenTermsEntriesAsync(
          string spokenTermsId,
          string lang,
          CancellationToken cancellationToken = default) {
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
          await GetSpokenTermsEntriesAsync(spokenTerms.SpokenTermsId, lang, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SpokenTermsList> ReplaceSpokenTermsListAsync(
          string spokenTermsId,
          SpokenTermsEntries entries,
          ReplaceSpokenTermsListOptions? options = null,
          CancellationToken cancellationToken = default) {
      var requestData = CreateSpokenTermsListBody(entries).ToDictionary(pair => pair.Key, pair => (object)pair.Value);
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
          await ReplaceSpokenTermsListAsync(spokenTerms.SpokenTermsId, entries, options, cancellationToken)
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
          await DeleteSpokenTermsAsync(spokenTerms.SpokenTermsId, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteSpokenTermsListAsync(
          string spokenTermsId,
          string lang,
          CancellationToken cancellationToken = default) {
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
          await DeleteSpokenTermsListAsync(spokenTerms.SpokenTermsId, lang, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryAsync(
          string name,
          MultilingualGlossaryDictionaryEntries[] glossaryDicts,
          CancellationToken cancellationToken = default) {
      if (name.Length == 0) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      if (!glossaryDicts.Any()) throw new ArgumentException("Parameter dictionaries must not be empty");

      var bodyParams = CreateGlossaryHttpParams(name, glossaryDicts);
      using var responseMessage = await _client
            .ApiPostAsync("v3/glossaries", cancellationToken, bodyParams).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
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
          CancellationToken cancellationToken = default) {
      if (name.Length == 0) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(sourceLanguageCode)) {
        throw new ArgumentException($"Parameter {nameof(sourceLanguageCode)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(targetLanguageCode)) {
        throw new ArgumentException($"Parameter {nameof(targetLanguageCode)} must not be empty");
      }

      var csvString = await new StreamReader(csvFile).ReadToEndAsync().ConfigureAwait(false);
      var bodyParams = CreateGlossaryDictionariesHttpParams(sourceLanguageCode, targetLanguageCode, csvString, "csv");
      bodyParams.Add(("name", name));
      using var responseMessage = await _client
            .ApiPostAsync("v3/glossaries", cancellationToken, bodyParams).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var glossary =
            await JsonUtils.DeserializeAsync<MultilingualGlossaryInfo>(responseMessage).ConfigureAwait(false);
      return glossary;
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> GetMultilingualGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(glossaryId))
        throw new ArgumentException($"Parameter {nameof(glossaryId)} must not be empty");
      using var responseMessage =
            await _client.ApiGetAsync($"v3/glossaries/{glossaryId}", cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<MultilingualGlossaryInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(glossaryId)) {
        throw new ArgumentException($"Parameter {nameof(glossaryId)} must not be empty");
      }

      var queryParams = CreateLanguageQueryParams(sourceLanguageCode, targetLanguageCode);

      using var responseMessage =
            await _client.ApiGetAsync(
                  $"v3/glossaries/{glossaryId}/entries",
                  cancellationToken,
                  queryParams).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      var dictionaryEntriesList = await JsonUtils
            .DeserializeAsync<MultilingualGlossaryDictionaryEntriesListResult>(responseMessage)
            .ConfigureAwait(false);

      if (dictionaryEntriesList.Dictionaries.Length == 0) throw new NotFoundException("Glossary dictionary not found");

      // When the source and target language codes are specified, there should be at most one dictionary returned where
      // a NotFoundException would be thrown if no dictionary cannot be found for the given source and target language codes
#pragma warning disable CS0618 // internal use of a type obsolete only for library users
      return new MultilingualGlossaryDictionaryEntries(dictionaryEntriesList.Dictionaries[0]);
#pragma warning restore CS0618
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default) =>
          await GetMultilingualGlossaryDictionaryEntriesAsync(
                glossary.GlossaryId,
                glossaryDict.SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default) =>
          await GetMultilingualGlossaryDictionaryEntriesAsync(
                glossaryId,
                glossaryDict.SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default) =>
          await GetMultilingualGlossaryDictionaryEntriesAsync(
                glossary.GlossaryId,
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
      if (string.IsNullOrWhiteSpace(glossaryId)) {
        throw new ArgumentException($"Parameter {nameof(glossaryId)} must not be empty");
      }

      using var responseMessage =
            await _client.ApiDeleteAsync($"v3/glossaries/{glossaryId}", cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteMultilingualGlossaryAsync(
          MultilingualGlossaryInfo glossary,
          CancellationToken cancellationToken = default) =>
          await DeleteMultilingualGlossaryAsync(glossary.GlossaryId, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(glossaryId)) {
        throw new ArgumentException($"Parameter {nameof(glossaryId)} must not be empty");
      }

      var queryParams = CreateLanguageQueryParams(sourceLanguageCode, targetLanguageCode);

      using var responseMessage =
            await _client.ApiDeleteAsync($"v3/glossaries/{glossaryId}/dictionaries", cancellationToken, queryParams)
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
                glossary.GlossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default) =>
          await DeleteMultilingualGlossaryDictionaryAsync(
                glossary.GlossaryId,
                glossaryDict.SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default) =>
          await DeleteMultilingualGlossaryDictionaryAsync(
                glossaryId,
                glossaryDict.SourceLanguageCode,
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
                entries.ToTsv(),
                "tsv",
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                glossary.GlossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                entries.ToTsv(),
                "tsv",
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                glossary.GlossaryId,
                glossaryDict.SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                glossaryDict.Entries.ToTsv(),
                "tsv",
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                glossaryDict.SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                glossaryDict.Entries.ToTsv(),
                "tsv",
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
                await new StreamReader(csvFile).ReadToEndAsync().ConfigureAwait(false),
                "csv",
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default) =>
          await ReplaceMultilingualGlossaryDictionaryInternalAsync(
                glossary.GlossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                await new StreamReader(csvFile).ReadToEndAsync().ConfigureAwait(false),
                "csv",
                cancellationToken).ConfigureAwait(false);

    private async Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryInternalAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          string entries,
          string entriesFormat,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(glossaryId)) {
        throw new ArgumentException($"Parameter {nameof(glossaryId)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(sourceLanguageCode)) {
        throw new ArgumentException($"Parameter {nameof(sourceLanguageCode)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(targetLanguageCode)) {
        throw new ArgumentException($"Parameter {nameof(targetLanguageCode)} must not be empty");
      }

      var bodyParams = new (string Key, string Value)[] {
            ("source_lang", sourceLanguageCode), ("target_lang", targetLanguageCode), ("entries_format", entriesFormat),
            ("entries", entries)
      };
      using var responseMessage =
            await _client.ApiPutAsync($"v3/glossaries/{glossaryId}/dictionaries", cancellationToken, bodyParams)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<MultilingualGlossaryDictionaryInfo>(responseMessage)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryNameAsync(
          string glossaryId,
          string name,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(name)) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      var bodyParams = new (string Key, string Value)[] { ("name", name) };
      using var responseMessage =
            await _client.ApiPatchAsync($"v3/glossaries/{glossaryId}", cancellationToken, bodyParams)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<MultilingualGlossaryInfo>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                entries.ToTsv(),
                "tsv",
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                glossary.GlossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                entries.ToTsv(),
                "tsv",
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                glossary.GlossaryId,
                glossaryDict.SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                glossaryDict.Entries.ToTsv(),
                "tsv",
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                glossaryDict.SourceLanguageCode,
                glossaryDict.TargetLanguageCode,
                glossaryDict.Entries.ToTsv(),
                "tsv",
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryFromCsvAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                glossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                await new StreamReader(csvFile).ReadToEndAsync().ConfigureAwait(false),
                "csv",
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryFromCsvAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default) =>
          await UpdateMultilingualGlossaryDictionaryInternalAsync(
                glossary.GlossaryId,
                sourceLanguageCode,
                targetLanguageCode,
                await new StreamReader(csvFile).ReadToEndAsync().ConfigureAwait(false),
                "csv",
                cancellationToken).ConfigureAwait(false);

    private async Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryInternalAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          string entries,
          string entriesFormat,
          CancellationToken cancellationToken = default) {
      if (string.IsNullOrWhiteSpace(glossaryId)) {
        throw new ArgumentException($"Parameter {nameof(glossaryId)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(sourceLanguageCode)) {
        throw new ArgumentException($"Parameter {nameof(sourceLanguageCode)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(targetLanguageCode)) {
        throw new ArgumentException($"Parameter {nameof(targetLanguageCode)} must not be empty");
      }

      var bodyParams = CreateGlossaryDictionariesHttpParams(
            sourceLanguageCode,
            targetLanguageCode,
            entries,
            entriesFormat);
      using var responseMessage =
            await _client.ApiPatchAsync($"v3/glossaries/{glossaryId}", cancellationToken, bodyParams)
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
      if (string.IsNullOrEmpty(resource)) {
        throw new ArgumentException($"Parameter {nameof(resource)} must not be empty");
      }

      var queryParams = new List<(string Key, string Value)> { ("resource", resource) };
      if (include != null) {
        queryParams.AddRange(include.Select(value => ("include", value)));
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
      public TextRephraseResult(WriteResult[] improvements) {
        Improvements = improvements;
      }

      /// <summary>Array of <see cref="WriteResult" /> objects holding text rephrase results.</summary>
      public WriteResult[] Improvements { get; }
    }

    /// <summary>Class used for JSON-deserialization of glossary dictionary entries list results.</summary>
    private readonly struct MultilingualGlossaryDictionaryEntriesListResult {
      /// <summary>
      ///   Initializes a new instance of <see cref="MultilingualGlossaryDictionaryEntriesListResult" />, used for JSON
      ///   deserialization.
      /// </summary>
#pragma warning disable CS0618 // internal use of a type obsolete only for library users
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
#pragma warning restore CS0618
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
      if (string.IsNullOrWhiteSpace(sourceLanguageCode)) {
        throw new ArgumentException($"Parameter {nameof(sourceLanguageCode)} must not be empty");
      }

      if (string.IsNullOrWhiteSpace(targetLanguageCode)) {
        throw new ArgumentException($"Parameter {nameof(targetLanguageCode)} must not be empty");
      }

      return new (string Key, string Value)[] {
            ("source_lang", sourceLanguageCode), ("target_lang", targetLanguageCode)
      };
    }

    /// <summary>
    ///   Returns a list of tuples containing the parameters to include in HTTP request.
    /// </summary>
    /// <param name="name"> The name of the glossary </param>
    /// <param name="glossaryDicts">
    ///   A list of glossary dictionaries, each with a source and target language code and
    ///   entries
    /// </param>
    /// <returns>List of tuples containing the parameters to include in HTTP request.</returns>
    private static List<(string Key, string Value)> CreateGlossaryHttpParams(
          string name,
          MultilingualGlossaryDictionaryEntries[] glossaryDicts) {
      var bodyParams = new List<(string Key, string Value)> { ("name", name) };
      for (var i = 0; i < glossaryDicts.Length; i++) {
        bodyParams.Add(($"dictionaries[{i}].source_lang", glossaryDicts[i].SourceLanguageCode));
        bodyParams.Add(($"dictionaries[{i}].target_lang", glossaryDicts[i].TargetLanguageCode));
        bodyParams.Add(($"dictionaries[{i}].entries", glossaryDicts[i].Entries.ToTsv()));
        bodyParams.Add(($"dictionaries[{i}].entries_format", "tsv"));
      }

      return bodyParams;
    }

    /// <summary>
    ///   Returns a list of tuples containing the parameters to include in HTTP request. Used to create a dictionary
    ///   with the glossary dictionaries information including its entries and source and target language pair
    /// </summary>
    /// <param name="sourceLanguageCode">
    ///   Language code of translation source language, or null if auto-detection should be
    ///   used.
    /// </param>
    /// <param name="targetLanguageCode">Language code of translation target language.</param>
    /// <param name="entries">The entries represented as a string in TSV or CSV delimited</param>
    /// <param name="entriesFormat">The format of the entries (either TSV or CSV).</param>
    /// <returns>List of tuples containing the parameters to include in HTTP request.</returns>
    private static List<(string Key, string Value)> CreateGlossaryDictionariesHttpParams(
          string sourceLanguageCode,
          string targetLanguageCode,
          string entries,
          string entriesFormat) {
      var bodyParams = new List<(string Key, string Value)> {
            ("dictionaries[0].source_lang", sourceLanguageCode),
            ("dictionaries[0].target_lang", targetLanguageCode),
            ("dictionaries[0].entries", entries),
            ("dictionaries[0].entries_format", entriesFormat)
      };

      return bodyParams;
    }

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
    private static Dictionary<string, object> CreateStyleProfileBody(
          string? name,
          CreateStyleProfileOptions? options) {
      var body = new Dictionary<string, object>();
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
      // Trimmed like glossary terms; a newline inside a term would silently split it into several terms on the wire.
      var terms = entries.Entries.Select(term => term.Trim()).ToArray();
      foreach (var term in terms) {
        GlossaryEntries.ValidateGlossaryTerm(term);
      }

      return new Dictionary<string, string> { ["lang"] = entries.Lang, ["entries"] = string.Join("\n", terms) };
    }

    /// <summary>Adds the extra body parameters of the options last, replacing fields with the same key.</summary>
    private static void AddExtraBodyParameters(Dictionary<string, object> requestData, BaseRequestOptions? options) {
      if (options?.ExtraBodyParameters == null) return;
      foreach (var pair in options.ExtraBodyParameters) {
        requestData[pair.Key] = pair.Value;
      }
    }

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

    /// <summary>Returns the value of the given key in an optional JSON object, or <c>null</c> if it is absent.</summary>
    /// <param name="parameters">Dictionary holding the JSON object fields, may be null.</param>
    /// <param name="key">Name of the field to read.</param>
    /// <returns>The value of the field, or <c>null</c> if it is absent.</returns>
    private static string? GetParameter(Dictionary<string, string>? parameters, string key) =>
          parameters != null && parameters.TryGetValue(key, out var value) ? value : null;

    /// <summary>Creates a <see cref="TranslationMemoryJob" /> from the JSON-deserialized DeepL API response.</summary>
    /// <param name="response">The deserialized job response.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object holding the flattened job status.</returns>
    private static TranslationMemoryJob CreateTranslationMemoryJob(TranslationMemoryJobResponse response) {
      var results = (response.Results ?? new TranslationMemoryJobResultResponse[0])
            .Select(
                  result => new TranslationMemoryJobResult(
                        result.Status,
                        GetParameter(result.StatusMetadata, "required_action"),
                        result.DownloadUrl,
                        result.ExpiresAt,
                        GetParameter(result.Error, "message"),
                        result.TranslationMemoryId,
                        result.SkippedSegmentCount))
            .ToArray();

      return new TranslationMemoryJob(
            response.JobId,
            response.Operation,
            results,
            response.Product,
            response.CreationTime,
            response.UpdatedTime,
            GetParameter(response.Parameters, "translation_memory_id"),
            GetParameter(response.Parameters, "display_name"),
            response.SourceFile?.ContentType,
            response.SourceFile?.ContentLength);
    }

    /// <summary>Class used for JSON-deserialization of translation memory export creation results.</summary>
    private readonly struct TranslationMemoryExportResult {
      /// <summary>Initializes a new instance of <see cref="TranslationMemoryExportResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public TranslationMemoryExportResult(string jobId, Dictionary<string, string>? parameters) {
        JobId = jobId;
        Parameters = parameters;
      }

      /// <summary>Unique ID assigned to the export job.</summary>
      [JsonPropertyName("job_id")]
      public string JobId { get; }

      /// <summary>Parameters of the export job, holding the translation memory ID.</summary>
      [JsonPropertyName("parameters")]
      public Dictionary<string, string>? Parameters { get; }
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
            TranslationMemoryJobStatus status,
            Dictionary<string, string>? statusMetadata,
            string? downloadUrl,
            DateTime? expiresAt,
            Dictionary<string, string>? error,
            string? translationMemoryId,
            int? skippedSegmentCount) {
        Status = status;
        StatusMetadata = statusMetadata;
        DownloadUrl = downloadUrl;
        ExpiresAt = expiresAt;
        Error = error;
        TranslationMemoryId = translationMemoryId;
        SkippedSegmentCount = skippedSegmentCount;
      }

      /// <summary>Status of the job.</summary>
      [JsonPropertyName("status")]
      public TranslationMemoryJobStatus Status { get; }

      /// <summary>Metadata describing the action the caller must take, if any.</summary>
      [JsonPropertyName("status_metadata")]
      public Dictionary<string, string>? StatusMetadata { get; }

      /// <summary>Download URL of the exported TMX file, set once an export completes.</summary>
      [JsonPropertyName("download_url")]
      public string? DownloadUrl { get; }

      /// <summary>Time after which the download URL is no longer valid.</summary>
      [JsonPropertyName("expires_at")]
      public DateTime? ExpiresAt { get; }

      /// <summary>Error information, set when the job failed.</summary>
      [JsonPropertyName("error")]
      public Dictionary<string, string>? Error { get; }

      /// <summary>ID of the translation memory created by a completed import.</summary>
      [JsonPropertyName("translation_memory_id")]
      public string? TranslationMemoryId { get; }

      /// <summary>Number of segments an import skipped.</summary>
      [JsonPropertyName("skipped_segment_count")]
      public int? SkippedSegmentCount { get; }
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
            Dictionary<string, string>? parameters,
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
      public Dictionary<string, string>? Parameters { get; }

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
