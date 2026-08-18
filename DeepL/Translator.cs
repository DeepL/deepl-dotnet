// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Internal;
using DeepL.Model;

namespace DeepL {
  public interface ITranslator : IDisposable {
    /// <summary>Retrieves the usage in the current billing period for this DeepL account.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="Usage" /> object containing account usage information.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<Usage> GetUsageAsync(CancellationToken cancellationToken = default);

    /// <summary>Translate specified texts from source language into target language.</summary>
    /// <param name="texts">Texts to translate; must not be empty.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">Extra <see cref="TextTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Texts translated into specified target language.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TextResult[]> TranslateTextAsync(
          IEnumerable<string> texts,
          string? sourceLanguageCode,
          string targetLanguageCode,
          TextTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Translate specified text from source language into target language.</summary>
    /// <param name="text">Text to translate; must not be empty.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options"><see cref="TextTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Text translated into specified target language.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TextResult> TranslateTextAsync(
          string text,
          string? sourceLanguageCode,
          string targetLanguageCode,
          TextTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Translate document at specified input <see cref="FileInfo" /> from source language to target language and
    ///   store the translated document at specified output <see cref="FileInfo" />.
    /// </summary>
    /// <param name="inputFileInfo"><see cref="FileInfo" /> object containing path to input document.</param>
    /// <param name="outputFileInfo"><see cref="FileInfo" /> object containing path to store translated document.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options"><see cref="DocumentTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="IOException">If the output path is occupied.</exception>
    /// <exception cref="DocumentTranslationException">
    ///   If cancellation was requested, or an error occurs while translating the document. If the document was uploaded
    ///   successfully, then the <see cref="DocumentTranslationException.DocumentHandle" /> contains the document ID and
    ///   key that may be used to retrieve the document.
    ///   If cancellation was requested, the <see cref="DocumentTranslationException.InnerException" /> will be a
    ///   <see cref="TaskCanceledException" />.
    /// </exception>
    Task TranslateDocumentAsync(
          FileInfo inputFileInfo,
          FileInfo outputFileInfo,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Translate specified document content from source language to target language and store the translated document
    ///   content to specified stream.
    /// </summary>
    /// <param name="inputFile"><see cref="Stream" /> containing input document content.</param>
    /// <param name="inputFileName">Name of the input file. The file extension is used to determine file type.</param>
    /// <param name="outputFile"><see cref="Stream" /> to store translated document content.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options"><see cref="DocumentTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DocumentTranslationException">
    ///   If cancellation was requested, or an error occurs while translating the document. If the document was uploaded
    ///   successfully, then the <see cref="DocumentTranslationException.DocumentHandle" /> contains the document ID and
    ///   key that may be used to retrieve the document.
    ///   If cancellation was requested, the <see cref="DocumentTranslationException.InnerException" /> will be a
    ///   <see cref="TaskCanceledException" />.
    /// </exception>
    Task TranslateDocumentAsync(
          Stream inputFile,
          string inputFileName,
          Stream outputFile,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Upload document at specified input <see cref="FileInfo" /> for translation from source language to target
    ///   language.
    /// </summary>
    /// <param name="inputFileInfo"><see cref="FileInfo" /> object containing path to input document.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options"><see cref="DocumentTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DocumentHandle" /> object associated with the in-progress document translation.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DocumentHandle> TranslateDocumentUploadAsync(
          FileInfo inputFileInfo,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Upload document content with specified filename for translation from source language to target language.</summary>
    /// <param name="inputFile"><see cref="Stream" /> containing input document content.</param>
    /// <param name="inputFileName">Name of the input file. The file extension is used to determine file type.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options"><see cref="DocumentTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DocumentHandle" /> object associated with the in-progress document translation.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DocumentHandle> TranslateDocumentUploadAsync(
          Stream inputFile,
          string inputFileName,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieve the status of in-progress document translation associated with specified
    ///   <see cref="DocumentHandle" />.
    /// </summary>
    /// <param name="handle"><see cref="DocumentHandle" /> associated with an in-progress document translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DocumentStatus" /> object containing the document translation status.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DocumentStatus> TranslateDocumentStatusAsync(
          DocumentHandle handle,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Checks document translation status and asynchronously-waits until document translation is complete or fails
    ///   due to an error.
    /// </summary>
    /// <param name="handle"><see cref="DocumentHandle" /> associated with an in-progress document translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task TranslateDocumentWaitUntilDoneAsync(
          DocumentHandle handle,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Downloads the resulting translated document associated with specified <see cref="DocumentHandle" /> to the
    ///   specified <see cref="FileInfo" />. The <see cref="DocumentStatus" /> for the document must have
    ///   <see cref="DocumentStatus.Done" /> == <c>true</c>.
    /// </summary>
    /// <param name="handle"><see cref="DocumentHandle" /> associated with an in-progress document translation.</param>
    /// <param name="outputFileInfo"><see cref="FileInfo" /> object containing path to store translated document.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="IOException">If the output path is occupied.</exception>
    /// <exception cref="DocumentNotReadyException">
    ///   If the document is not ready to be downloaded, that is, the document status
    ///   is not <see cref="DocumentStatus.Done" />.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If any other error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task TranslateDocumentDownloadAsync(
          DocumentHandle handle,
          FileInfo outputFileInfo,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Downloads the resulting translated document associated with specified <see cref="DocumentHandle" /> to the
    ///   specified <see cref="Stream" />. The <see cref="DocumentStatus" /> for the document must have
    ///   <see cref="DocumentStatus.Done" /> == <c>true</c>.
    /// </summary>
    /// <param name="handle"><see cref="DocumentHandle" /> associated with an in-progress document translation.</param>
    /// <param name="outputFile"><see cref="Stream" /> to store translated document content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DocumentNotReadyException">
    ///   If the document is not ready to be downloaded, that is, the document status
    ///   is not <see cref="DocumentStatus.Done" />.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If any other error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task TranslateDocumentDownloadAsync(
          DocumentHandle handle,
          Stream outputFile,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves the list of supported translation source languages.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="Language" /> objects representing the available translation source languages.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SourceLanguage[]> GetSourceLanguagesAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves the list of supported translation target languages.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="Language" /> objects representing the available translation target languages.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TargetLanguage[]> GetTargetLanguagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves the list of supported glossary language pairs. When creating glossaries, the source and target
    ///   language pair must match one of the available language pairs.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="GlossaryLanguagePair" /> objects representing the available glossary language pairs.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryLanguagePair[]> GetGlossaryLanguagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///   Creates a v2 glossary in your DeepL account with the specified details and returns a
    ///   <see cref="GlossaryInfo" /> object with details about the newly created glossary. The glossary can be used
    ///   in translations to override translations for specific terms (words). The glossary source and target languages
    ///   must match the languages of translations for which it will be used.
    /// </summary>
    /// <param name="name">User-defined name to assign to the glossary; must not be empty.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="entries">Glossary entry list.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="GlossaryInfo" /> object with details about the newly created glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryInfo> CreateGlossaryAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Creates a v2 glossary in your DeepL account with the specified details and returns a
    ///   <see cref="GlossaryInfo" /> object with details about the newly created glossary. The glossary can be used
    ///   in translations to override translations for specific terms (words). The glossary source and target languages
    ///   must match the languages of translations for which it will be used.
    /// </summary>
    /// <param name="name">User-defined name to assign to the glossary; must not be empty.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="GlossaryInfo" /> object with details about the newly created glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryInfo> CreateGlossaryFromCsvAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves information about the glossary with the specified ID and returns a <see cref="GlossaryInfo" />
    ///   object containing details. This does not retrieve the glossary entries; to retrieve entries use
    ///   <see cref="ITranslator.GetGlossaryEntriesAsync(string,System.Threading.CancellationToken)" />
    /// </summary>
    /// <param name="glossaryId">ID of glossary to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="GlossaryInfo" /> object with details about the specified glossary.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryInfo> GetGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default);

    /// <summary>Asynchronously waits until the given glossary is ready to be used for translations.</summary>
    /// <param name="glossaryId">ID of glossary to ensure ready.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryInfo> WaitUntilGlossaryReadyAsync(
          string glossaryId,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves information about all glossaries and returns an array of <see cref="GlossaryInfo" /> objects
    ///   containing details. This does not retrieve the glossary entries; to retrieve entries use
    ///   <see cref="ITranslator.GetGlossaryEntriesAsync(string,System.Threading.CancellationToken)" />
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="GlossaryInfo" /> objects with details about each glossary.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryInfo[]> ListGlossariesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves the entries containing within the glossary with the specified ID and returns them as a
    ///   <see cref="GlossaryEntries" />.
    /// </summary>
    /// <param name="glossaryId">ID of glossary for which to retrieve entries.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="GlossaryEntries" /> containing entry pairs of the glossary.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryEntries> GetGlossaryEntriesAsync(
          string glossaryId,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves the entries containing within the glossary and returns them as a <see cref="GlossaryEntries" />.
    /// </summary>
    /// <param name="glossary"><see cref="GlossaryInfo" /> object corresponding to glossary for which to retrieve entries.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="GlossaryEntries" /> containing entry pairs of the glossary.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryEntries> GetGlossaryEntriesAsync(
          GlossaryInfo glossary,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the glossary with the specified ID.</summary>
    /// <param name="glossaryId">ID of glossary to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the specified glossary.</summary>
    /// <param name="glossary"><see cref="GlossaryInfo" /> object corresponding to glossary to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteGlossaryAsync(
          GlossaryInfo glossary,
          CancellationToken cancellationToken = default);
  }

  /// <summary>
  ///   Deprecated client for the DeepL library. Please use <see cref="DeepLClient" /> instead.
  ///   Do not derive from this class, this class might undergo backward-incompatible changes.
  /// </summary>
  public class Translator : ITranslator {
    /// <summary>Base URL for DeepL API Pro accounts.</summary>
    private const string DeepLServerUrl = "https://api.deepl.com";

    /// <summary>Base URL for DeepL API Free accounts.</summary>
    private const string DeepLServerUrlFree = "https://api-free.deepl.com";

    /// <summary>Internal class implementing HTTP requests.</summary>
    internal readonly DeepLHttpClient _client;

    /// <summary>Initializes a new <see cref="Translator" /> object using your authentication key.</summary>
    /// <param name="authKey">
    ///   Authentication Key as found in your
    ///   <a href="https://www.deepl.com/pro-account/">DeepL API account</a>.
    /// </param>
    /// <param name="options">Additional options controlling Translator behaviour.</param>
    /// <exception cref="ArgumentNullException">If authKey argument is null.</exception>
    /// <exception cref="ArgumentException">If authKey argument is empty.</exception>
    /// <remarks>
    ///   This function does not establish a connection to the DeepL API. To check connectivity, use
    ///   <see cref="GetUsageAsync" />.
    /// </remarks>
    public Translator(string authKey, TranslatorOptions? options = null) {
      options ??= new TranslatorOptions();

      if (authKey == null) {
        throw new ArgumentNullException(nameof(authKey));
      }

      authKey = authKey.Trim();

      if (authKey.Length == 0) {
        throw new ArgumentException($"{nameof(authKey)} is empty");
      }

      var serverUrl = new Uri(
            options.ServerUrl ?? (AuthKeyIsFreeAccount(authKey) ? DeepLServerUrlFree : DeepLServerUrl));

      var headers = new Dictionary<string, string?>(options.Headers, StringComparer.OrdinalIgnoreCase);

      if (!headers.ContainsKey("User-Agent")) {
        headers.Add("User-Agent", ConstructUserAgentString(options.sendPlatformInfo, options.appInfo));
      }

      if (!headers.ContainsKey("Authorization")) {
        headers.Add("Authorization", $"DeepL-Auth-Key {authKey}");
      }

      var clientFactory = options.ClientFactory ?? (() =>
            DeepLHttpClient.CreateDefaultHttpClient(
                  options.PerRetryConnectionTimeout,
                  options.OverallConnectionTimeout,
                  options.MaximumNetworkRetries));

      _client = new DeepLHttpClient(
            serverUrl,
            clientFactory,
            headers);
    }

    /// <summary>Releases the unmanaged resources and disposes of the managed resources used by the <see cref="Translator" />.</summary>
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
    ///   <c>true</c> if the Authentication Key belongs to an API Free account, <c>false</c> if it belongs to an API Pro
    ///   account.
    /// </returns>
    public static bool AuthKeyIsFreeAccount(string authKey) => authKey.TrimEnd().EndsWith(":fx");

    /// <inheritdoc />
    public async Task<Usage> GetUsageAsync(CancellationToken cancellationToken = default) {
      using var responseMessage = await _client.ApiGetAsync("v2/usage", cancellationToken).ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var usageFields = await JsonUtils.DeserializeAsync<Usage.JsonFieldsStruct>(responseMessage)
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
      var bodyParams = CreateHttpParams(sourceLanguageCode, targetLanguageCode, options);
      var textParams = texts
            .Where(text => text.Length > 0 ? true : throw new ArgumentException("text must not be empty"))
            .Select(text => ("text", text));

      using var responseMessage = await _client
            .ApiPostAsync("v2/translate", cancellationToken, bodyParams.Concat(textParams)).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var translatedTexts =
            await JsonUtils.DeserializeAsync<TextTranslateResult>(responseMessage).ConfigureAwait(false);
      return translatedTexts.Translations;
    }

    /// <inheritdoc />
    public async Task<TextResult> TranslateTextAsync(
          string text,
          string? sourceLanguageCode,
          string targetLanguageCode,
          TextTranslateOptions? options = null,
          CancellationToken cancellationToken = default)
      => (await TranslateTextAsync(
                  new[] { text },
                  sourceLanguageCode,
                  targetLanguageCode,
                  options,
                  cancellationToken)
            .ConfigureAwait(false))[0];

    /// <inheritdoc />
    public async Task TranslateDocumentAsync(
          FileInfo inputFileInfo,
          FileInfo outputFileInfo,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default) {
      var willMinify = (options?.EnableDocumentMinification ?? false) &&
                       DocumentMinifier.CanMinifyFile(inputFileInfo.Name);
      var fileToUpload = inputFileInfo;
      var minifier = new DocumentMinifier();
      if (willMinify) {
        minifier.MinifyDocument(inputFileInfo.FullName, true);
        fileToUpload = new FileInfo(minifier.GetMinifiedDocFile(inputFileInfo.FullName));
      }

      using var inputFile = fileToUpload.OpenRead();
      using var outputFile = outputFileInfo.Open(FileMode.CreateNew, FileAccess.Write);
      try {
        await TranslateDocumentAsync(
              inputFile,
              fileToUpload.Name,
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

      if (willMinify) {
        outputFile.Dispose();
        // Translated minified file is at `outputFileName`. Reinsert media (deminify) before returning
        minifier.DeminifyDocument(outputFileInfo.FullName, outputFileInfo.FullName, true);
      }
    }

    /// <inheritdoc />
    public async Task TranslateDocumentAsync(
          Stream inputFile,
          string inputFileName,
          Stream outputFile,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default) {
      DocumentHandle? handle = null;
      try {
        handle = await TranslateDocumentUploadAsync(
                    inputFile,
                    inputFileName,
                    sourceLanguageCode,
                    targetLanguageCode,
                    options,
                    cancellationToken)
              .ConfigureAwait(false);
        await TranslateDocumentWaitUntilDoneAsync(handle.Value, cancellationToken).ConfigureAwait(false);
        await TranslateDocumentDownloadAsync(handle.Value, outputFile, cancellationToken).ConfigureAwait(false);
      } catch (Exception exception) {
        throw new DocumentTranslationException(
              $"Error occurred during document translation: {exception.Message}",
              exception,
              handle);
      }
    }

    /// <inheritdoc />
    public async Task<DocumentHandle> TranslateDocumentUploadAsync(
          FileInfo inputFileInfo,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default) {
      using var inputFileStream = inputFileInfo.OpenRead();
      return await TranslateDocumentUploadAsync(
            inputFileStream,
            inputFileInfo.Name,
            sourceLanguageCode,
            targetLanguageCode,
            options,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DocumentHandle> TranslateDocumentUploadAsync(
          Stream inputFile,
          string inputFileName,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default) {
      // Document translation cannot detect the source language when a glossary is used.
      if (sourceLanguageCode == null && (options?.GlossaryId != null || options?.GlossaryIds?.Count > 0)) {
        throw new ArgumentException($"{nameof(sourceLanguageCode)} is required if using a glossary");
      }

      var bodyParams = CreateCommonHttpParams(
            sourceLanguageCode,
            targetLanguageCode,
            options?.Formality,
            options?.GlossaryId,
            options?.StyleId,
            options?.TranslationMemoryId,
            options?.GlossaryIds);

      if (options?.TranslationMemoryThreshold != null) {
        if (options.TranslationMemoryId == null) {
          throw new ArgumentException("TranslationMemoryThreshold requires TranslationMemoryId");
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

      AddExtraBodyParameters(bodyParams, options?.ExtraBodyParameters);

      using var responseMessage = await _client.ApiUploadAsync(
                  "v2/document",
                  cancellationToken,
                  bodyParams,
                  inputFile,
                  inputFileName)
            .ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<DocumentHandle>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DocumentStatus> TranslateDocumentStatusAsync(
          DocumentHandle handle,
          CancellationToken cancellationToken = default) {
      var bodyParams = new (string Key, string Value)[] { ("document_key", handle.DocumentKey) };
      using var responseMessage =
            await _client.ApiPostAsync(
                        $"v2/document/{handle.DocumentId}",
                        cancellationToken,
                        bodyParams)
                  .ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<DocumentStatus>(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task TranslateDocumentWaitUntilDoneAsync(
          DocumentHandle handle,
          CancellationToken cancellationToken = default) {
      var status = await TranslateDocumentStatusAsync(handle, cancellationToken).ConfigureAwait(false);
      while (status.Ok && !status.Done) {
        await Task.Delay(CalculateDocumentWaitTime(status.SecondsRemaining), cancellationToken).ConfigureAwait(false);
        status = await TranslateDocumentStatusAsync(handle, cancellationToken).ConfigureAwait(false);
      }

      if (!status.Ok) {
        throw new DeepLException(status.ErrorMessage ?? "Unknown error");
      }
    }

    /// <inheritdoc />
    public async Task TranslateDocumentDownloadAsync(
          DocumentHandle handle,
          FileInfo outputFileInfo,
          CancellationToken cancellationToken = default) {
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
      var bodyParams = new (string Key, string Value)[] { ("document_key", handle.DocumentKey) };
      using var responseMessage = await _client.ApiPostAsync(
                  $"v2/document/{handle.DocumentId}/result",
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
      using var responseMessage = await _client
            .ApiGetAsync("v2/glossary-language-pairs", cancellationToken).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var languages = await JsonUtils.DeserializeAsync<GlossaryLanguageListResult>(responseMessage)
            .ConfigureAwait(false);
      return languages.GlossaryLanguagePairs;
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
                entries.ToTsv(),
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
                await new StreamReader(csvFile).ReadToEndAsync().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<GlossaryInfo> GetGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default) {
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
          await GetGlossaryEntriesAsync(glossary.GlossaryId, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DeleteGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default) {
      using var responseMessage =
            await _client.ApiDeleteAsync($"v2/glossaries/{glossaryId}", cancellationToken)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteGlossaryAsync(
          GlossaryInfo glossary,
          CancellationToken cancellationToken = default) =>
          await DeleteGlossaryAsync(glossary.GlossaryId, cancellationToken).ConfigureAwait(false);

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
      var queryParams = new (string Key, string Value)[] { ("type", target ? "target" : "source") };
      using var responseMessage =
            await _client.ApiGetAsync("v2/languages", cancellationToken, queryParams)
                  .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      return await JsonUtils.DeserializeAsync<TValue[]>(responseMessage).ConfigureAwait(false);
    }

    /// <summary>
    ///   Builds the user-agent string we want to send to the API with every request. This string contains
    ///   basic information about the client environment, such as the deepl client library version, operating
    ///   system and language runtime version.
    /// </summary>
    /// <param name="sendPlatformInfo">
    ///   <c>true</c> to send platform information with every API request (default),
    ///   <c>false</c> to only send the library version.
    /// </param>
    /// <param name="AppInfo">
    ///   Name and version of the application using this library. Ignored if null.
    /// </param>
    /// <returns>Enumerable of tuples containing the parameters to include in HTTP request.</returns>
    private String ConstructUserAgentString(bool sendPlatformInfo = true, AppInfo? appInfo = null) {
      var platformInfoString = $"deepl-dotnet/{Version()}";
      if (sendPlatformInfo) {
        var osDescription = RuntimeInformation.OSDescription;
        var clrVersion = Environment.Version.ToString();
        platformInfoString += $" ({osDescription}) dotnet-clr/{clrVersion}";
      }

      if (appInfo != null) {
        platformInfoString += $" {appInfo?.AppName}/{appInfo?.AppVersion}";
      }

      return platformInfoString;
    }

    /// <summary>
    ///   Checks the specified languages and options are valid, and returns an enumerable of tuples containing the parameters
    ///   to include in HTTP request.
    /// </summary>
    /// <param name="sourceLanguageCode">
    ///   Language code of translation source language, or null if auto-detection should be
    ///   used.
    /// </param>
    /// <param name="targetLanguageCode">Language code of translation target language.</param>
    /// <param name="options">Extra <see cref="TextTranslateOptions" /> influencing translation.</param>
    /// <returns>Enumerable of tuples containing the parameters to include in HTTP request.</returns>
    /// <exception cref="ArgumentException">If the specified languages or options are invalid.</exception>
    private static IEnumerable<(string Key, string Value)> CreateHttpParams(
          string? sourceLanguageCode,
          string targetLanguageCode,
          TextTranslateOptions? options) {
      var bodyParams = CreateCommonHttpParams(
            sourceLanguageCode,
            targetLanguageCode,
            options?.Formality,
            options?.GlossaryId,
            options?.StyleId,
            options?.TranslationMemoryId,
            options?.GlossaryIds);

      // Always send show_billed_characters=1, remove when the API default is changed to true
      bodyParams.Add(("show_billed_characters", "1"));

      if (options == null) {
        return bodyParams;
      }

      if (options.TranslationMemoryThreshold != null) {
        if (options.TranslationMemoryId == null) {
          throw new ArgumentException("TranslationMemoryThreshold requires TranslationMemoryId");
        }

        if (options.TranslationMemoryThreshold.Value < 0 || options.TranslationMemoryThreshold.Value > 100) {
          throw new ArgumentOutOfRangeException(
                nameof(options.TranslationMemoryThreshold),
                options.TranslationMemoryThreshold.Value,
                $"{nameof(options.TranslationMemoryThreshold)} must be between 0 and 100");
        }

        bodyParams.Add(("translation_memory_threshold", options.TranslationMemoryThreshold.Value.ToString()));
      }

      if (options.Context != null) {
        bodyParams.Add(("context", options.Context));
      }

      if (options.SentenceSplittingMode != SentenceSplittingMode.All) {
        bodyParams.Add(
              ("split_sentences", options.SentenceSplittingMode == SentenceSplittingMode.Off ? "0" : "nonewlines"));
      }

      if (options.PreserveFormatting) {
        bodyParams.Add(("preserve_formatting", "1"));
      }

      if (options.TagHandling != null) {
        bodyParams.Add(("tag_handling", options.TagHandling));
      }

      if (options.TagHandlingVersion != null) {
        bodyParams.Add(("tag_handling_version", options.TagHandlingVersion));
      }

      if (!options.OutlineDetection) {
        bodyParams.Add(("outline_detection", "0"));
      }

      if (options.NonSplittingTags.Count > 0) {
        bodyParams.Add(("non_splitting_tags", string.Join(",", options.NonSplittingTags)));
      }

      if (options.SplittingTags.Count > 0) {
        bodyParams.Add(("splitting_tags", string.Join(",", options.SplittingTags)));
      }

      if (options.IgnoreTags.Count > 0) {
        bodyParams.Add(("ignore_tags", string.Join(",", options.IgnoreTags)));
      }

      if (options.ModelType != null) {
        bodyParams.Add(("model_type", options.ModelType.Value.ToApiValue()));
      }

      if (options.CustomInstructions.Count > 0) {
        foreach (var instruction in options.CustomInstructions) {
          bodyParams.Add(("custom_instructions", instruction));
        }
      }

      AddExtraBodyParameters(bodyParams, options.ExtraBodyParameters);

      return bodyParams;
    }

    /// <summary>
    ///   Checks the specified languages and options are valid, and returns a list of tuples containing the parameters
    ///   to include in HTTP request.
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
    /// <returns>List of tuples containing the parameters to include in HTTP request.</returns>
    /// <exception cref="ArgumentException">If the specified languages or options are invalid.</exception>
    private static List<(string Key, string Value)> CreateCommonHttpParams(
          string? sourceLanguageCode,
          string targetLanguageCode,
          Formality? formality,
          string? glossaryId,
          string? styleId = null,
          string? translationMemoryId = null,
          ICollection<string>? glossaryIds = null) {
      targetLanguageCode = LanguageCode.Standardize(targetLanguageCode);
      sourceLanguageCode = sourceLanguageCode == null ? null : LanguageCode.Standardize(sourceLanguageCode);

      CheckValidLanguages(sourceLanguageCode, targetLanguageCode);

      var bodyParams = new List<(string Key, string Value)> { ("target_lang", targetLanguageCode) };
      if (sourceLanguageCode != null) {
        bodyParams.Add(("source_lang", sourceLanguageCode));
      }

      var hasGlossaryIds = glossaryIds != null && glossaryIds.Count > 0;

      if (glossaryId != null && hasGlossaryIds) {
        throw new ArgumentException("GlossaryId and GlossaryIds cannot be used together");
      }

      if (glossaryId != null) {
        bodyParams.Add(("glossary_id", glossaryId));
      }

      if (hasGlossaryIds) {
        if (glossaryIds!.Count > 5) {
          throw new ArgumentException("GlossaryIds must not contain more than 5 glossary IDs");
        }

        bodyParams.Add(("glossary_ids", string.Join(",", glossaryIds)));
      }

      if (styleId != null) {
        bodyParams.Add(("style_id", styleId));
      }

      if (translationMemoryId != null) {
        bodyParams.Add(("translation_memory_id", translationMemoryId));
      }

      switch (formality) {
        case null:
        case Formality.Default:
          break;
        case Formality.Less:
          bodyParams.Add(("formality", "less"));
          break;
        case Formality.More:
          bodyParams.Add(("formality", "more"));
          break;
        case Formality.PreferLess:
          bodyParams.Add(("formality", "prefer_less"));
          break;
        case Formality.PreferMore:
          bodyParams.Add(("formality", "prefer_more"));
          break;
        default:
          throw new ArgumentException($"{nameof(formality)} value is out of range");
      }

      return bodyParams;
    }

    /// <summary>Checks the specified source and target language are valid, and throws an exception if not.</summary>
    /// <param name="sourceLanguageCode">Language code of translation source language, or null if auto-detection is used.</param>
    /// <param name="targetLanguageCode">Language code of translation target language.</param>
    /// <exception cref="ArgumentException">If source or target language code are not valid.</exception>
    protected static void CheckValidLanguages(string? sourceLanguageCode, string targetLanguageCode) {
      if (sourceLanguageCode is { Length: 0 }) {
        throw new ArgumentException($"{nameof(sourceLanguageCode)} must not be empty");
      }

      if (targetLanguageCode.Length == 0) {
        throw new ArgumentException($"{nameof(targetLanguageCode)} must not be empty");
      }

      switch (targetLanguageCode) {
        case "en":
          throw new ArgumentException(
                $"{nameof(targetLanguageCode)}=\"en\" is deprecated, please use \"en-GB\" or \"en-US\" instead");
        case "pt":
          throw new ArgumentException(
                $"{nameof(targetLanguageCode)}=\"pt\" is deprecated, please use \"pt-PT\" or \"pt-BR\" instead");
      }
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

    /// <summary>
    ///   Adds extra body parameters from options to the request body parameters list. Extra parameters can override
    ///   existing parameters.
    /// </summary>
    /// <param name="bodyParams">List of body parameters to add to.</param>
    /// <param name="extraBodyParameters">Dictionary of extra parameters to add.</param>
    private static void AddExtraBodyParameters(
          List<(string Key, string Value)> bodyParams,
          Dictionary<string, string>? extraBodyParameters) {
      if (extraBodyParameters == null) {
        return;
      }

      foreach (var kvp in extraBodyParameters) {
        bodyParams.RemoveAll(p => p.Key == kvp.Key);
        bodyParams.Add((kvp.Key, kvp.Value));
      }
    }

    /// <summary>Creates a glossary with given details.</summary>
    private async Task<GlossaryInfo> CreateGlossaryInternalAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          string entriesFormat,
          string entries,
          CancellationToken cancellationToken) {
      if (name.Length == 0) {
        throw new ArgumentException($"Parameter {nameof(name)} must not be empty");
      }

      sourceLanguageCode = LanguageCode.RemoveRegionalVariant(sourceLanguageCode);
      targetLanguageCode = LanguageCode.RemoveRegionalVariant(targetLanguageCode);

      var bodyParams = new (string Key, string Value)[] {
            ("name", name), ("source_lang", sourceLanguageCode), ("target_lang", targetLanguageCode),
            ("entries_format", entriesFormat), ("entries", entries)
      };
      using var responseMessage =
            await _client.ApiPostAsync("v2/glossaries", cancellationToken, bodyParams).ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage, ResourceType.Glossary).ConfigureAwait(false);
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

    /// <summary>Class used for JSON-deserialization of results of supported languages for glossaries.</summary>
    private readonly struct GlossaryLanguageListResult {
      /// <summary>Initializes a new instance of <see cref="GlossaryLanguageListResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public GlossaryLanguageListResult(GlossaryLanguagePair[] glossaryLanguagePairs) {
        GlossaryLanguagePairs = glossaryLanguagePairs;
      }

      /// <summary>Array of <see cref="GlossaryLanguagePair" /> objects holding supported glossary language pairs.</summary>
      [JsonPropertyName("supported_languages")]
      public GlossaryLanguagePair[] GlossaryLanguagePairs { get; }
    }
  }
}
