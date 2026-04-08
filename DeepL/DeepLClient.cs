// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
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
          string? targetLanguageCode,
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
          string? targetLanguageCode,
          TextRephraseOptions? options = null,
          CancellationToken cancellationToken = default);
  }

  /// <summary>
  ///   Client for the DeepL API. To use the DeepL API, initialize an instance of this class using your DeepL
  ///   Authentication Key. All functions are thread-safe, aside from <see cref="DeepLClient.Dispose" />.
  /// </summary>
  public sealed class DeepLClient : Translator, IWriter, IGlossaryManager, IStyleRuleManager, IVoiceManager {
    /// <summary>Initializes a new instance of the <see cref="AuthorizationException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public DeepLClient(string authKey, DeepLClientOptions? options = null) : base(authKey, options) { }

    /// <inheritdoc />
    public async Task<WriteResult> RephraseTextAsync(
          string text,
          string? targetLanguageCode,
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
          string? targetLanguageCode,
          TextRephraseOptions? options = null,
          CancellationToken cancellationToken = default) {
      var bodyParams = new List<(string Key, string Value)>();
      if (targetLanguageCode != null) {
        CheckValidLanguages(null, targetLanguageCode);
        bodyParams.Add(("target_lang", targetLanguageCode));
      }

      var textParams = texts
            .Where(text => text.Length > 0 ? true : throw new ArgumentException("text must not be empty"))
            .Select(text => ("text", text));
      if (options != null && options.WritingStyle != null) {
        bodyParams.Add(("writing_style", options.WritingStyle));
      }

      if (options != null && options.WritingTone != null) {
        bodyParams.Add(("tone", options.WritingTone));
      }
      // TODO add `show_billed_characters` once write API supports it.

      using var responseMessage = await _client
            .ApiPostAsync("v2/write/rephrase", cancellationToken, bodyParams.Concat(textParams))
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var rephrasedTexts =
            await JsonUtils.DeserializeAsync<TextRephraseResult>(responseMessage).ConfigureAwait(false);
      return rephrasedTexts.Improvements;
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
      return new MultilingualGlossaryDictionaryEntries(dictionaryEntriesList.Dictionaries[0]);
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

    /// <summary>JSON serializer options for JSON-encoded request bodies.</summary>
    private static readonly JsonSerializerOptions SerializationOptions = new JsonSerializerOptions {
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public async Task<IVoiceSession> CreateVoiceSessionAsync(
          VoiceSessionOptions options,
          CancellationToken cancellationToken = default) {
      if (options == null) {
        throw new ArgumentNullException(nameof(options));
      }

      if (options.TargetLanguages == null || options.TargetLanguages.Length == 0) {
        throw new ArgumentException("At least one target language must be specified");
      }

      if (options.TargetLanguages.Length > 5) {
        throw new ArgumentException("Maximum 5 target languages per session");
      }

      var requestData = new Dictionary<string, object> {
        ["source_media_content_type"] = options.SourceMediaContentType,
        ["target_languages"] = options.TargetLanguages
      };

      if (options.MessageFormat != null) {
        requestData["message_format"] = options.MessageFormat.Value.ToApiValue();
      }

      if (options.SourceLanguage != null) {
        requestData["source_language"] = options.SourceLanguage;
      }

      if (options.SourceLanguageMode != null) {
        requestData["source_language_mode"] = options.SourceLanguageMode.Value.ToApiValue();
      }

      if (options.TargetMediaLanguages != null) {
        requestData["target_media_languages"] = options.TargetMediaLanguages;
      }

      if (options.TargetMediaContentType != null) {
        requestData["target_media_content_type"] = options.TargetMediaContentType;
      }

      if (options.TargetMediaVoice != null) {
        requestData["target_media_voice"] = options.TargetMediaVoice.Value.ToApiValue();
      }

      if (options.GlossaryId != null) {
        requestData["glossary_id"] = options.GlossaryId;
      }

      if (options.Formality != null) {
        requestData["formality"] = options.Formality;
      }

      using var responseMessage = await _client
            .ApiPostJsonAsync("v3/voice/realtime", cancellationToken, requestData, SerializationOptions)
            .ConfigureAwait(false);

      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var sessionInfo = await JsonUtils.DeserializeAsync<VoiceSessionInfo>(responseMessage).ConfigureAwait(false);

      // Establish WebSocket connection
      var wsUri = new Uri($"{sessionInfo.StreamingUrl}?token={Uri.EscapeDataString(sessionInfo.Token)}");
      var webSocket = new ClientWebSocket();
      try {
        await webSocket.ConnectAsync(wsUri, cancellationToken).ConfigureAwait(false);
      } catch (Exception ex) {
        webSocket.Dispose();
        throw new DeepLException("Failed to establish Voice API WebSocket connection", ex);
      }

      return new VoiceSession(_client, webSocket, sessionInfo);
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
