# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- `DocumentTranslationException` now copies `HttpStatusCode` and `ShouldRetry` from the `DeepLException` that
  caused it; they were always `null` before.
- Added `DeepLClient` overloads of `ListTranslationMemorySegmentsAsync()` (translation memory ID or
  `TranslationMemoryInfo`) taking a `TranslationMemorySegmentsOptions` (`PageSize`, `PageCursor`, `FilterText`,
  `FilterCaseSensitive`); options left `null` are not sent.
- Added a `DeepLClient` overload of `CreateTranslationMemoryImportAsync()` taking a
  `CreateTranslationMemoryImportOptions` (`ContentType`, `DisplayName`); its `ExtraBodyParameters` are added last to
  the JSON request body.
- Added a `DeepLClient` overload of `ImportTranslationMemoryFromFilepathAsync()` taking an
  `ImportTranslationMemoryOptions` (`DisplayName`, `ContentType`, `Timeout`). `ContentType` is declared on the import
  job and sent with the upload (default `application/xml`); `ExtraBodyParameters` are added last to the import job
  request body.
- Added `DeepLClient` overloads of `WaitUntilTranslationMemoryJobDoneAsync()` and
  `ExportTranslationMemoryToFilepathAsync()` taking a `TimeSpan? timeout`; a `DeepLException` is thrown when it
  expires before the job finishes.
- Added `DeepLClient` overloads of `GetTranslationMemoryJobAsync()` and `WaitUntilTranslationMemoryJobDoneAsync()`
  taking a `TranslationMemoryJob` instead of the job ID.
- `TranslationMemoryJob` converts implicitly to its job ID, so a job object can also be passed to the
  `ITranslationMemoryManager` methods taking a job ID.
- Added `TranslationMemoryJobStatus.Unknown` and `StatusValue` on `TranslationMemoryJobResult` and
  `TranslationMemoryJob`, holding the job status as returned by the API.
- The new translation memory overloads are members of `DeepLClient`; the `ITranslationMemoryManager` interface is
  unchanged.
- Note: with the new options overloads, source code that passes a bare `null` as the first optional argument of these
  calls no longer compiles (CS0121, ambiguous call): `ListTranslationMemorySegmentsAsync(id, null)`,
  `ListTranslationMemorySegmentsAsync(translationMemory, null)`, `CreateTranslationMemoryImportAsync(fileName,
  contentLength, null)` and `ImportTranslationMemoryFromFilepathAsync(path, null)`. Drop the `null`, or pass it as a
  named argument (for example `displayName: null`). Already-compiled callers are unaffected.
- Added `DeepLClient` overloads of all eight style rule methods that take a style rule ID (`GetStyleRuleAsync()`,
  `UpdateStyleRuleNameAsync()`, `UpdateStyleRuleConfiguredRulesAsync()`, `DeleteStyleRuleAsync()`,
  `CreateStyleRuleCustomInstructionAsync()`, `GetStyleRuleCustomInstructionAsync()`,
  `UpdateStyleRuleCustomInstructionAsync()`, `DeleteStyleRuleCustomInstructionAsync()`) taking a `StyleRuleInfo`
  instead, and overloads of the get, update and delete custom instruction methods taking a `CustomInstruction`
  instead of the instruction ID. `CustomInstruction` converts implicitly to its ID. These and the following style rule
  overloads are members of `DeepLClient`; the `IStyleRuleManager` interface is unchanged.
- Added `DeepLClient` overloads of `CreateStyleRuleAsync()` and `UpdateStyleRuleConfiguredRulesAsync()` taking the
  configured rules as a `Dictionary<string, Dictionary<string, string>>` (category to rule to value, keyed by the API
  names), sent as given, so categories the `ConfiguredRules` class lacks can be sent. `ConfiguredRules` converts
  implicitly to this map.
- Added `DeepLClient` overloads of `CreateStyleRuleAsync()`, `UpdateStyleRuleNameAsync()`,
  `UpdateStyleRuleConfiguredRulesAsync()`, `CreateStyleRuleCustomInstructionAsync()` and
  `UpdateStyleRuleCustomInstructionAsync()` taking a `CreateStyleRuleOptions`, `UpdateStyleRuleNameOptions`,
  `UpdateStyleRuleConfiguredRulesOptions`, `CreateStyleRuleCustomInstructionOptions` or
  `UpdateStyleRuleCustomInstructionOptions`. The options classes derive from `BaseRequestOptions`: their
  `ExtraBodyParameters` are added last to the JSON request body (for the configured rules, to the rules map as further
  keys).
- Added `DeepLClient.UpdateMultilingualGlossaryAsync()`, taking a glossary ID or `MultilingualGlossaryInfo` and an
  `UpdateMultilingualGlossaryOptions` object (`Name`, `Dictionaries`), to rename a glossary and merge entries into
  its dictionaries in one request. Only the set properties are sent; `ExtraBodyParameters` are added last to the
  JSON request body.
- Added `DeepLClient` overloads of `CreateMultilingualGlossaryAsync()` and `CreateMultilingualGlossaryFromCsvAsync()`
  taking a `CreateMultilingualGlossaryOptions`, and of `ReplaceMultilingualGlossaryDictionaryAsync()` and
  `ReplaceMultilingualGlossaryDictionaryFromCsvAsync()` taking a `ReplaceMultilingualGlossaryDictionaryOptions`. Both
  options classes derive from `BaseRequestOptions`: their `ExtraBodyParameters` are added last to the request body.
- Added `DeepLClient` overloads of `GetMultilingualGlossaryAsync()` and `UpdateMultilingualGlossaryNameAsync()` taking
  a `MultilingualGlossaryInfo` instead of the glossary ID.
- The new multilingual glossary methods and overloads are members of `DeepLClient`; the `IGlossaryManager` interface
  is unchanged.
- Added `StyleProfile` to `TextTranslateOptions` and `DocumentTranslateOptions` (the ID or a `StyleProfile` object),
  and constructors taking a `StyleProfile`, to translate text and documents with a style profile (sent as
  `style_profile_id`).
- Added `Glossary`, `StyleRule` and `TranslationMemory` to `DocumentTranslateOptions`, replacing `GlossaryId`,
  `StyleId` and `TranslationMemoryId`, as in `TextTranslateOptions`: each takes the ID or the resource object.
  Glossary objects can also be added to `DocumentTranslateOptions.GlossaryIds`.
- Added `DocumentTranslateOptions.FormalityValue`: the `formality` value as a string, sent unchanged instead of
  `Formality` if set.
- Added `DocumentTranslateOptions.Filename` and `Stream` overloads of `TranslateDocumentAsync()` and
  `TranslateDocumentUploadAsync()` without the `inputFileName` parameter, which send `Filename` as the file name.
  The `FileInfo` overloads send `Filename` instead of the file's name if it is set.
- Added `TranslateDocumentGetStatusAsync()`, replacing `TranslateDocumentStatusAsync()`.
- Added a `TranslateDocumentWaitUntilDoneAsync()` overload with a `TimeSpan? timeout`, and
  `DocumentTranslateOptions.Timeout` for the wait inside `TranslateDocumentAsync()`. When the timeout expires, a
  `DeepLException` is thrown (wrapped in a `DocumentTranslationException` by `TranslateDocumentAsync()`).
- Added `DocumentStatus.StatusValue`, the status string as returned by the API, and
  `DocumentStatus.StatusCode.Unknown` for statuses this library does not know.
- The new document methods and overloads are members of `Translator` and `DeepLClient`; the obsolete `ITranslator`
  interface is unchanged.
- Added `Glossary`, `StyleRule` and `TranslationMemory` to `TextTranslateOptions`, replacing `GlossaryId`, `StyleId`
  and `TranslationMemoryId`. Each takes the ID or the resource object (`GlossaryInfo` or `MultilingualGlossaryInfo`,
  `StyleRuleInfo`, `TranslationMemoryInfo`), so several resource objects can be combined in one options object.
- `GlossaryInfo`, `MultilingualGlossaryInfo`, `StyleRuleInfo`, `StyleProfile` and `TranslationMemoryInfo` convert
  implicitly to their ID, so they can also be added to `TextTranslateOptions.GlossaryIds`, mixed with IDs. A `null`
  object converts to `null`, so assigning one leaves the option unset.
- Added `FormalityValue`, `SplitSentences` and `ModelTypeValue` to `TextTranslateOptions`: the `formality`,
  `split_sentences` and `model_type` values as strings, sent unchanged instead of the `Formality`,
  `SentenceSplittingMode` and `ModelType` enum properties if set, for values the enums do not have.
- Added the `TagHandlingMode` (`Html`, `Xml`) and `TagHandlingVersion` (`V1`, `V2`) constants for
  `TextTranslateOptions.TagHandling` and `TagHandlingVersion`, which still accept any string.
- Added `TextResult.TagHandlingVersion`, the `tag_handling_version` of the response (`null` if absent or not a
  string).
- Added `IStyleProfileManager`, implemented by `DeepLClient`, to manage style profiles (`/v3/style_profiles`):
  `ListStyleProfilesAsync()`, `CreateStyleProfileAsync()`, `GetStyleProfileAsync()`, `UpdateStyleProfileAsync()` and
  `DeleteStyleProfileAsync()`, with the model `StyleProfile` and the options classes `CreateStyleProfileOptions` and
  `UpdateStyleProfileOptions` (which adds `Name`). Options left `null` are not sent; an empty string clears the
  description or industry and an empty list unlinks all components of that kind. Both options classes derive from
  `BaseRequestOptions`: `ExtraBodyParameters` are added to the body last. The glossary, style rule list and
  translation memory lists take IDs or resource objects (`SetGlossaries()`, `SetStyleRules()`,
  `SetTranslationMemories()`). `StyleProfile.CreationTime` and `UpdatedTime` are non-nullable.
- Added `ISpokenTermsManager`, implemented by `DeepLClient`, to manage Spoken Terms collections (`/v3/spoken-terms`):
  `CreateSpokenTermsAsync()`, `ListSpokenTermsAsync()`, `GetSpokenTermsAsync()`, `UpdateSpokenTermsAsync()`,
  `GetSpokenTermsEntriesAsync()`, `ReplaceSpokenTermsListAsync()`, `DeleteSpokenTermsAsync()` and
  `DeleteSpokenTermsListAsync()`, with the models `SpokenTerms`, `SpokenTermsList` and `SpokenTermsEntries`.
  Terms are checked like glossary terms before sending; an invalid term throws `ArgumentException`.
  `UpdateSpokenTermsAsync()` takes the new name and the terms to merge as an `UpdateSpokenTermsOptions` object
  (`Name`, `TermLists`). `CreateSpokenTermsAsync()`, `UpdateSpokenTermsAsync()` and `ReplaceSpokenTermsListAsync()`
  take an optional `CreateSpokenTermsOptions`, `UpdateSpokenTermsOptions` or `ReplaceSpokenTermsListOptions`, whose
  `ExtraBodyParameters` are added last to the JSON request body.
- Added the API Pro fields of `GET /v2/usage` to `Usage`: `ApiKeyCharacter` (the character usage of the API key,
  as opposed to the account-wide `Character`), `SpeechToTextMinutes`, `SpeechToSpeechMinutes`, `StartTime`,
  `EndTime` and `Products` (per-product usage as `Usage.Product`). They are `null` when the response does not
  contain them, for example for non-Pro accounts. `Usage.JsonFieldsStruct` keeps its six character and document
  fields; the API Pro fields are parsed internally.
- Added the `DeepLAdminClient` class for the Admin API. It must be created with an admin API key and supports managing
  developer keys (`CreateDeveloperKeyAsync()`, `ListDeveloperKeysAsync()`, `DeactivateDeveloperKeyAsync()`,
  `RenameDeveloperKeyAsync()`, `SetDeveloperKeyUsageLimitsAsync()`) and retrieving usage analytics
  (`GetUsageAnalyticsAsync()`, `GetCustomTagUsageAnalyticsAsync()`). `SetDeveloperKeyUsageLimitsAsync()` takes a
  `DeveloperKeyUsageLimitsUpdate` and changes only the limits assigned in it.
  The four key-changing methods take optional `CreateDeveloperKeyOptions` (with the `Label` of the new key),
  `DeactivateDeveloperKeyOptions`, `RenameDeveloperKeyOptions` and `SetDeveloperKeyUsageLimitsOptions`, derived from
  `BaseRequestOptions`: their `ExtraBodyParameters` are added last to the JSON request body.
  `DeepLAdminClient` implements the new `IAdminManager` interface.
  Results use the new types `DeveloperKey`, `DeveloperKeyUsageLimits`, `AdminUsageReport`, `AdminUsageBreakdown`
  (including `SpeechToSpeechMinutes`), `KeyUsage`, `CustomTagUsageReport`, `CustomTagUsage` and
  `CustomTagUsageBreakdown`, and requests the enums `AdminUsageGroupBy` and `CustomTagUsageAggregateBy`; overloads
  of the analytics methods take `groupBy` and `aggregateBy` as strings, sent as given.
  `DeveloperKey.Label`, `DeveloperKey.UsageLimits` and `KeyUsage.ApiKeyLabel` are `null` when the API does not return
  them. Unexpected values in Admin API responses are skipped or replaced by defaults instead of failing the call;
  a response that is not a JSON object or array as expected, or a developer key response without `key_id`, throws a
  `DeepLException`. The SDK passes labels and limit values to the API unchanged.
  The analytics methods accept `DateTime` values, sent as their UTC date (`Local` values are converted to UTC,
  `Unspecified` values are treated as UTC), or `"yyyy-MM-dd"` strings.
  `DeepLAdminClientOptions` and `TranslatorOptions` now share the connection settings of the new `BaseClientOptions`
  base class.
- Added `LanguageCode` constants for target language variants `GermanSwiss` (`de-CH`), `GermanGermany` (`de-DE`),
  `SpanishEuropean` (`es-ES`), `FrenchCanadian` (`fr-CA`) and `FrenchEuropean` (`fr-FR`).
- Added `DeepLClient.GetLanguagesForResourceAsync()` to retrieve the languages supported by a DeepL API resource
  (`GET /v3/languages`), with source/target usability, status and per-language features. Pass `include` values
  `LanguageSupport.IncludeBeta` and `LanguageSupport.IncludeExternal` to also return beta and third-party languages
  and features.
  A `GetLanguagesForResourceAsync()` overload takes a `GetLanguagesForResourceOptions` object whose `Include` list
  holds these values.
- Added `DeepLClient.GetLanguageResourcesAsync()` to retrieve the DeepL API resources and their features, including
  whether each feature requires source and/or target language support (`GET /v3/languages/resources`).
- Added `DocumentTranslateOptions.EnableWatermark` to request a "Translated by DeepL" watermark on translated
  documents (`docx` and `pdf` output only).
- Added `CorrectTextAsync()` method to the `DeepLClient` class for correcting
  grammar and spelling errors via the `v2/write/correct` endpoint. It takes an optional `CorrectTextOptions`, whose
  `ExtraBodyParameters` are added to the request body last. It is a member of `DeepLClient`; the `IWriter` interface
  is unchanged.
- `TextRephraseOptions` now derives from `BaseRequestOptions`: its `ExtraBodyParameters` are added to the
  `RephraseTextAsync()` request body last, replacing built-in parameters with the same key.
- Added `TextRephraseOptions` constructors taking a `WritingStyle` or `WritingTone` enum value, and the
  `ToApiValue()` extension methods for both enums (e.g. `WritingStyle.PreferSimple` is sent as `prefer_simple`).
- The `targetLanguageCode` parameter of `RephraseTextAsync()` and `CorrectTextAsync()` now defaults to `null`, so it
  can be omitted.
- Added `DeepLException.HttpStatusCode`, the HTTP status of the failed response (`null` if there was none), and
  `DeepLException.ShouldRetry`: `true` for HTTP 429, 5xx and transient network failures (timeouts, failed connections),
  `false` for other HTTP statuses. For `CreateDeveloperKeyAsync()`, which is not safe to repeat, it is `true` only for
  HTTP 429, name resolution failures and refused connections. Both are also set for errors of pre-signed storage URLs.
- Added the `Proxy` option to `DeepLClientOptions` and `DeepLAdminClientOptions`: the URL of an HTTP(S) proxy for all
  requests, with optional credentials in the URL. Unlike a custom `ClientFactory`, it keeps the built-in retries and
  timeouts.
- Added the static method `DeepLClient.IsFreeAccountAuthKey()`, replacing `Translator.AuthKeyIsFreeAccount()`.
- Added `DeepLClient` overloads of `UpdateMultilingualGlossaryNameAsync()`,
  `UpdateMultilingualGlossaryDictionaryAsync()` and `UpdateMultilingualGlossaryDictionaryFromCsvAsync()` taking a
  `BaseRequestOptions`; its `ExtraBodyParameters` are added last to the request body. The `IGlossaryManager` interface
  is unchanged.

### Changed
- `TranslateDocumentAsync()` with `FileInfo` arguments throws an `ArgumentException` before any request if the input
  file does not exist (was a `FileNotFoundException`), and wraps the other local failures (an existing output file,
  opening the input, document minification) in a `DocumentTranslationException` with the original exception as
  `InnerException` (was a raw `IOException` or `DocumentMinificationException`). An existing output file is still
  kept unchanged.
- The README documents source language detection with glossaries in text translation: with a `null` source language,
  DeepL detects it and applies the glossary's dictionary for the detected language pair.
- Because `GlossaryInfo`, `MultilingualGlossaryInfo`, `StyleRuleInfo`, `StyleProfile` and `TranslationMemoryInfo` now
  convert implicitly to `string`, code recompiled against this version that passes one of them to an API with a
  `string` overload, such as `Console.WriteLine(glossary)`, `"..." + glossary`, `StringBuilder.Append(glossary)` or
  `string.Join(", ", a, b)`, now gets the bare ID instead of `ToString()` (`Glossary "name" (id)`). Interpolation
  (`$"{glossary}"`) still uses `ToString()`. `ToString()` is for diagnostics only; `Language` already behaves this way.
  `CustomInstruction` now also converts implicitly to `string`. It has no `ToString()` override, so these calls print
  its ID, or an empty string if it has none, instead of the type name `DeepL.Model.CustomInstruction`.
- Target languages `EN` and `PT` are no longer rejected client-side in text translation, document translation and
  rephrasing. The API translates them as `en-US` and `pt-PT` respectively.
- `GetSourceLanguagesAsync()`, `GetTargetLanguagesAsync()`, and `GetGlossaryLanguagesAsync()` now
  call the `GET /v3/languages` endpoint instead of the deprecated `GET /v2/languages` and
  `GET /v2/glossary-language-pairs` endpoints. No public API changes, but the returned language lists may include
  additional languages.
- Retries now cover only HTTP 429, 5xx responses and transient network failures (timeouts, failed connections). Before,
  any exception thrown while sending a request was retried.
- Each attempt of a request may now run until the time the client would wait before retrying it (the backoff delay)
  when that is longer than `PerRetryConnectionTimeout`; `Timeout.InfiniteTimeSpan` still disables it. The default
  client no longer limits a request including all retries to 100 seconds; `OverallConnectionTimeout` still applies
  when it is set.
- `Content-Type`, `Content-Length`, `Transfer-Encoding` and `Host` entries in `Headers` are now ignored; previously the
  first two made every request fail with a `ConnectionException` and the others were sent.

### Deprecated
- `GlossaryNotFoundException` is deprecated; catch `NotFoundException`, its base class. Glossary endpoints
  still throw it on HTTP 404 until the next major version, which throws `NotFoundException`.
- Deprecated the `DeepLClient.CreateStyleRuleAsync()` and `UpdateStyleRuleConfiguredRulesAsync()` overloads taking a
  `ConfiguredRules` object with `[Obsolete]`; use the overloads taking a `Dictionary<string, Dictionary<string, string>>`
  instead. They keep working until the next major version. A `CreateStyleRuleAsync()` call without configured rules
  now uses the new overload and sends the same request; passing `null` for the configured rules still calls the
  deprecated overload.
- Deprecated `TextTranslateOptions.GlossaryId`, `StyleId` and `TranslationMemoryId` with `[Obsolete]`; use `Glossary`,
  `StyleRule` and `TranslationMemory` instead. They keep working until the next major version. Error messages of text
  translation name the new properties, e.g. "Glossary and GlossaryIds cannot be used together".
- Deprecated `DocumentTranslateOptions.GlossaryId`, `StyleId` and `TranslationMemoryId` with `[Obsolete]`; use
  `Glossary`, `StyleRule` and `TranslationMemory` instead. They keep working until the next major version. Error
  messages of document translation name the new properties, as for text translation.
- Deprecated `Translator.TranslateDocumentStatusAsync()` with `[Obsolete]`; use `TranslateDocumentGetStatusAsync()`
  instead. It keeps working until the next major version.
- Deprecated the language code constants of the `LanguageCode` class (for example `LanguageCode.German`) with
  `[Obsolete]`; they will be removed in the next major version. Pass the language code as a string, for example
  `"de"`, and use `GetSourceLanguagesAsync()` and `GetTargetLanguagesAsync()` to retrieve the supported languages.
  `LanguageCode.RemoveRegionalVariant()` is not deprecated.
- Deprecated document minification with `[Obsolete]`: `DocumentTranslateOptions.EnableDocumentMinification`,
  `DocumentMinifier`, `IDocumentMinifier`, `DocumentMinificationException` and `DocumentDeminificationException`.
  They keep working and will be removed in the next major version. The DeepL API accepts larger documents than when
  minification was added; see the [usage limits](https://developers.deepl.com/docs/resources/usage-limits).
- Deprecated public types that are implementation details with `[Obsolete]`: `DeepL.Internal.LargeFormUrlEncodedContent`,
  `MultilingualGlossaryDictionaryEntriesResult`, and the `MultilingualGlossaryDictionaryEntries` constructor that takes a
  `MultilingualGlossaryDictionaryEntriesResult`. They keep working and will be made internal in the next major version.
- Deprecated `Translator`, `ITranslator` and `TranslatorOptions` with `[Obsolete]`; use `DeepLClient` and
  `DeepLClientOptions` instead. They keep working and will be removed in the next major version.
- Deprecated `Translator.AuthKeyIsFreeAccount()` with `[Obsolete]`; use `DeepLClient.IsFreeAccountAuthKey()` instead.
- Deprecated `OverallConnectionTimeout` with `[Obsolete]`; each attempt is limited by `PerRetryConnectionTimeout`.
  Its default is now no overall timeout.

### Fixed
- The first retry now waits exactly 1 second, and the first attempt's backoff deadline is exactly 1 second; the ±23 %
  jitter applies only from the second backoff on.
- `TranslateDocumentUploadAsync(FileInfo, ...)` throws an `ArgumentException` before any request when the input file
  does not exist, like `TranslateDocumentAsync(FileInfo, ...)`. Before, it threw a `FileNotFoundException`.
- A 3xx response the HTTP client does not follow (for example 304, or a redirect without a `Location` header) now
  throws `DeepLException` with `HttpStatusCode` set instead of being treated as a success.
- TLS and certificate failures and proxy tunnel rejections are no longer retried, and their `ConnectionException`
  has `ShouldRetry` set to `false`. Connect, name resolution, timeout and connection reset failures are still
  retried.
- `GlossaryEntries.FromTsv()` now stores and sends the trimmed, validated entries joined with `\n`, without blank
  lines; it sent the original string, with untrimmed terms and `\r\n` line endings. With `skipChecks: true` the
  string is still sent unchanged.
- Document uploads from a non-seekable `Stream` (e.g. a network stream) are now buffered before sending, so a retry
  after HTTP 429, 5xx or a network failure resends the full document instead of failing.
- `CreateDeveloperKeyAsync` retries refused connections and connect timeouts, as its documentation says.
- On .NET 5 and later, connecting is limited to half of `PerRetryConnectionTimeout`, so that a hanging connect fails
  as a connect timeout instead of a per-attempt timeout, and `CreateDeveloperKeyAsync` retries it. Before, a hanging
  connect was cut by the per-attempt timeout, which `CreateDeveloperKeyAsync` does not retry.
- `ImportTranslationMemoryFromFilepathAsync` reads the input file before creating the import job, and throws a
  `DeepLException` (with the original exception as `InnerException`) if it cannot be read, instead of an
  `IOException` or `UnauthorizedAccessException` after the job was created.
- `ExportTranslationMemoryToFilepathAsync` creates the output file before creating the export job, and throws a
  `DeepLException` (with the original exception as `InnerException`) if it already exists or cannot be created or
  written, instead of an `IOException` after the export ran. A completed export job without a download URL also
  throws a `DeepLException` instead of an `ArgumentException`. The output file is deleted if the export fails.
- Fixed `GetTranslationMemoryJobAsync()` failing on a job status this library does not know: it is now parsed as
  `TranslationMemoryJobStatus.Unknown`, and `WaitUntilTranslationMemoryJobDoneAsync()` keeps polling on it.
- Fixed `GetTranslationMemoryJobAsync()` and `CreateTranslationMemoryExportAsync()` failing when the job's
  `parameters`, `error` or `status_metadata` object contains a non-string field; unknown fields are now ignored.
- Multilingual glossary IDs are now URL-encoded in request paths.
- `CreateMultilingualGlossaryAsync()` and `CreateMultilingualGlossaryFromCsvAsync()` now throw
  `GlossaryNotFoundException` on HTTP 404, like the other multilingual glossary methods.
- `GetMultilingualGlossaryDictionaryEntriesAsync()` no longer throws `ArgumentException` for entries returned by the
  API that the client-side entry checks would reject.
- `ReplaceMultilingualGlossaryDictionaryAsync()` and `ReplaceMultilingualGlossaryDictionaryFromCsvAsync()` now send
  the request body as JSON, the only content type the API accepts for this endpoint.
- `RephraseTextAsync()` and `CorrectTextAsync()` throw a `DeepLException` if the response holds no `improvements` or
  fewer than texts were sent, instead of returning `null`, a short array, or throwing a `NullReferenceException` or
  `IndexOutOfRangeException`.
- A Write API response without `detected_source_language` throws a `DeepLException` instead of a
  `NullReferenceException`.
- An unknown document status in the response no longer fails `TranslateDocumentStatusAsync()`,
  `TranslateDocumentWaitUntilDoneAsync()` and `TranslateDocumentAsync()`. Before, it threw a `DeepLException`
  ("Failed to deserialize JSON in received response"); now `DocumentStatus.Status` is `StatusCode.Unknown` and the
  waiting methods keep polling.
- The document ID is now URL-encoded in the paths of the document status and download requests. Before, an ID
  containing characters such as `/` or `?` was inserted as is and addressed a different URL.
- `ExtraBodyParameters` with a `text` key now replace the texts of `TranslateTextAsync()`, like any other parameter.
  Before, the extra `text` was sent in addition to the texts.
- `TranslateTextAsync()` now throws a `DeepLException` when the response holds no `translations`, an empty list, or
  fewer translations than texts sent, or a translation without `detected_source_language`. Before, it returned `null`
  or threw a `NullReferenceException` or `IndexOutOfRangeException`.
- A request that exceeds `PerRetryConnectionTimeout` now throws a `ConnectionException` with a "Request timed out"
  message, like the other timeouts, instead of "Unexpected request failure".
- Cancelling a request through the `CancellationToken` argument now always rethrows the cancellation exception. Before,
  a plain `OperationCanceledException` raised by the HTTP handler was wrapped in a `ConnectionException`.
  Document translation still wraps it in a `DocumentTranslationException`, with the cancellation exception as
  `InnerException`.
- Fixed the doc comments of `LanguageCode.English` and `LanguageCode.Portuguese`, which claimed they may only be used as
  source languages.
- HTTP 401 now throws an `AuthorizationException`, like HTTP 403. Before, it threw a `DeepLException` with an
  "Unexpected status code" message.
- A malformed or mistyped JSON response now throws a `DeepLException` with the `JsonException` as `InnerException`.
  Before, the raw `JsonException` was thrown.

## [1.22.1] - 2026-08-18
### Changed
- In preparation for source language detection with glossaries, text
  translation no longer rejects a glossary with no `sourceLanguageCode` before
  sending the request. Whether the request succeeds is determined by the API.
  Document translation still requires a source language with a glossary.
- `TextTranslateOptions.GlossaryIds` and `DocumentTranslateOptions.GlossaryIds`
  are now settable, so they can be assigned in an object initializer instead of
  only appended to.

## [1.22.0] - 2026-08-11
### Added
- Added support for using multiple glossaries in text and document translation
  via the `GlossaryIds` property (up to 5 glossary IDs) in `TextTranslateOptions`
  and `DocumentTranslateOptions`.
- Added support for style rules in document translation via the `StyleId`
  property in `DocumentTranslateOptions`.
- Added support for translation memories in document translation via the
  `TranslationMemoryId` and `TranslationMemoryThreshold` properties in
  `DocumentTranslateOptions`.
- Added support for the translation memory management endpoints in the
  `DeepLClient` class: `GetTranslationMemoryAsync()`,
  `ListTranslationMemorySegmentsAsync()`, `DeleteTranslationMemoryAsync()`,
  `CreateTranslationMemoryImportAsync()`, `UploadTranslationMemoryFileAsync()`,
  `CreateTranslationMemoryExportAsync()`, `GetTranslationMemoryJobAsync()`,
  `WaitUntilTranslationMemoryJobDoneAsync()`,
  `DownloadTranslationMemoryExportAsync()`,
  `ImportTranslationMemoryFromFilepathAsync()` and
  `ExportTranslationMemoryToFilepathAsync()`, along with the
  `TranslationMemorySegments`, `TranslationMemorySegment`,
  `TranslationMemoryTargetSegment`, `TranslationMemoryImport`,
  `TranslationMemoryExport`, `TranslationMemoryJob` and
  `TranslationMemoryJobResult` model classes.
  Please refer to the README for usage instructions.
- Added the `CreationTime` and `UpdatedTime` properties to
  `TranslationMemoryInfo`.

### Changed
- Requests to the pre-signed storage URLs used for translation memory import and export now
  carry a separate, minimal header set rather than the DeepL API headers with `Authorization`
  filtered out. Headers supplied via `DeepLClientOptions.Headers` are no longer sent to those
  URLs either — only the `User-Agent` and the per-request `Content-Type`. Proxy settings are
  unaffected.

### Fixed
- Fixed incorrect example texts in test suite for several languages (Danish, Indonesian,
  Japanese, Portuguese, and Russian).

## [1.21.0] - 2026-04-09
### Added
- Added support for translation memories in text translation via
  `TranslationMemoryId` and `TranslationMemoryThreshold` properties in
  `TextTranslateOptions`.
- Added `ListTranslationMemoriesAsync()` method to the `DeepLClient` class
  for listing available translation memories.

### Fixed
- Fixed `gitlab release` CI job by explicitly setting the `release-cli` image to avoid
  issues with a missing `$HOME` environment variable from the global image.

## [1.20.0] - 2026-03-26
### Added
- Added new language constants from January 2026 API release of 81 new languages.
- Added `ApiPostJsonAsync()`, `ApiPutJsonAsync()`, and `ApiPatchJsonAsync()` methods to
  `DeepLHttpClient`, enabling JSON-encoded API calls.
- Added support for style rules CRUD endpoints in the `DeepLClient` class:
  `CreateStyleRuleAsync()`, `GetStyleRuleAsync()`, `UpdateStyleRuleNameAsync()`,
  `UpdateStyleRuleConfiguredRulesAsync()`, and `DeleteStyleRuleAsync()`.
- Added support for style rule custom instruction CRUD endpoints in the
  `DeepLClient` class: `CreateStyleRuleCustomInstructionAsync()`,
  `GetStyleRuleCustomInstructionAsync()`, `UpdateStyleRuleCustomInstructionAsync()`,
  and `DeleteStyleRuleCustomInstructionAsync()`.
  Please refer to the README for usage instructions.

### Changed
- Updated formality tests to accept either formal or informal output when using default formality,
  since the default formality is automatic.
- Updated `TestTranslateWithModelType()` to expect the `model_type_used` to be non-null if the `model_type` is specified in the request
- Updated `TestMixedDirectionText()` to add a missing `</p>` tag
- Improved `NotFoundException` error message by removing the misleading "check ServerUrl" suggestion.

### Fixed
- Fixed relative URI resolution so that a `ServerUrl` with a path prefix (e.g. a
  reverse-proxy base path like `https://proxy.example.com/deepl/`) is correctly
  preserved when constructing API request URLs. Previously, API paths starting with
  `/` were treated as absolute paths by RFC 3986 URI resolution, causing the base
  path to be silently discarded.
  - Thanks to [vernou](https://github.com/vernou) for the report in [#73](https://github.com/DeepLcom/deepl-dotnet/issues/73)

## [1.19.0] - 2025-12-10
### Added
- Added `TagHandlingVersion` parameter to `TextTranslateOptions` to specify which version of the tag handling algorithm to use. Options are `v1` and `v2`.

## [1.18.0] - 2025-12-03
### Added
- Added `CustomInstructions` property to `TextTranslateOptions` to customize translation
  behavior with up to 10 instructions (max 300 characters each). Only supported for
  target languages: `de`, `en`, `es`, `fr`, `it`, `ja`, `ko`, `zh` and their variants.
  Note: using the `CustomInstructions` parameter will use `quality_optimized` model type
  as the default. Requests combining `CustomInstructions` and the `LatencyOptimized`
  model type will be rejected.

## [1.17.0] - 2025-11-12
### Added
- Added support for style rules in text translation via the `StyleId` property in `TextTranslateOptions`.
- Added `GetAllStyleRulesAsync()` method to `DeepLClient` to retrieve all available style rules with optional pagination and detailed configuration.
- Added new model classes: `StyleRuleInfo`, `ConfiguredRules`, and `CustomInstruction` to support style rules functionality.

## [1.16.0] - 2025-11-04
### Added
- Added `ExtraBodyParameters` option to text and document translation methods to pass arbitrary parameters in the request body. This can be used to access beta features or override built-in parameters (such as `target_lang`, `source_lang`, etc.).

## [1.15.0] - 2025-04-25
### Added
- Added support for the /v3 Multilingual Glossary APIs in the client library
  while providing backwards compatability for the previous /v2 Glossary
  endpoints. Please refer to the README or
  [upgrading_to_multilingual_glossaries.md](upgrading_to_multilingual_glossaries.md)
  for usage instructions.

## [1.14.0] - 2025-02-25
### Added
- Added support for the `output_format` parameter when translating documents.
  * [#50](https://github.com/DeepLcom/deepl-dotnet/pull/50) thanks for [JuergenRB](https://github.com/JuergenRB)

## [1.13.0] - 2025-01-28
### Added
- Added support for the Write API in the client library, the implementation
  can be found in the `DeepLClient` class. Please refer to the README for usage
  instructions.

### Changed
- The main functionality of the library is now also exposed via the `DeepLClient`
  class. Please change your code to use this over the `Translator` class whenever
  convenient.

### Fixed
- Fixed code example for getting glossary entries in the README
  * [#58](https://github.com/DeepLcom/deepl-dotnet/issues/58) thanks to [KurtBildeSDU](https://github.com/KurtBildeSDU)

## [1.12.0] - 2025-01-09
### Added
- Added document minification as a feature before document translation, to
  allow translation of large docx or pptx files. For more info check the README.

## [1.11.0] - 2024-11-15
### Added
- Added `ModelType` option for text translation to use models with higher
  translation quality (available for some language pairs), or better latency.
  Options are:
  - `ModelType.QualityOptimized ('quality_optimized')`,
  - `ModelType.QualityOptimized ('latency_optimized')`, and
  - `ModelType.QualityOptimized ('prefer_quality_optimized')`
- Added the `ModelTypeUsed` field to text translation response, that
  indicates the translation model used when the `ModelType` option is
  specified.

## [1.10.0] - 2024-09-17
### Added
- Added `BilledCharacters` to the translate text response.

## [1.9.0] - 2024-03-15
### Added
- New language available: Arabic (MSA) (`'ar'`). Add language code constants and tests.

  Note: older library versions also support the new language, this update only
  adds new code constants.
  * [#44](https://github.com/DeepLcom/deepl-dotnet/pull/44) thanks to [Ilmalte](https://github.com/ilmalte)

### Fixed
- Change document upload to use the path `/v2/document` instead of `/v2/document/` (no trailing `/`).
  Both paths will continue to work in the v2 version of the API, but `/v2/document` is the intended one.
- Made `DeepLException` and subclasses, `Usage` and `JsonFieldsStruct` constructors public, to allow for easier mocking of the `ITranslator` interface.
  * [#40](https://github.com/DeepLcom/deepl-dotnet/issues/40) thanks to [PascalVorwerkSaixon](https://github.com/PascalVorwerkSaxion)

## [1.8.0] - 2023-11-03
### Added
- Add optional `Context` parameter for text translation, that specifies
  additional context to influence translations, that is not translated itself.

### Changed
- Improvements to readme examples.

## [1.7.1] - 2023-04-17
### Fixed
- Changed document translation to poll the server every 5 seconds. This should greatly reduce observed document translation processing time.

## [1.7.0] - 2023-03-22
### Added
- Script to check our source code for license headers and a step for them in the CI.
- Added platform and node version information to the user-agent string that is sent with API calls, along with an opt-out.
- Added method for applications that use this library to identify themselves in API requests they make.

## [1.6.0] - 2023-01-26
### Added
- New languages available: Korean (`'ko'`) and Norwegian (bokmål) (`'nb'`). Add
  language code constants and tests.

  Note: older library versions also support the new languages, this update only
  adds new code constants.

## [1.5.1] - 2023-01-25
### Fixed
- Fix CI build for netcoreapp3.1.
- Fix `Formality` options `PreferLess` and `PreferMore` for document
  translation.

## [1.5.0] - 2022-09-30
### Added
- Add new `Formality` options: `PreferLess` and `PreferMore`.

### Changed
- Requests resulting in `503 Service Unavailable` errors are now retried.
  Attempting to download a document before translation is completed will now
  wait and retry (up to 5 times by default), rather than throwing an exception.

## [1.4.0] - 2022-09-09
### Added
- New language available: Ukrainian (`'uk'`). Add language code constant and
  tests.

  Note: older library versions also support new languages, this update only
  adds new code constant.

## [1.3.0] - 2022-08-02
### Added
- Add `Translator.createGlossaryFromCsvAsync()` allowing glossaries downloaded
  from website to be easily uploaded to API.

### Changed
- Strong name DeepL.net assembly.

## [1.2.1] - 2022-06-29
### Changed
- Update contributing guidelines, we can now accept Pull Requests.

### Fixed
- Fix bug in OutlineDetection option for text translation.

## [1.2.0] - 2022-05-18
### Added
- New languages available: Indonesian (`'id'`) and Turkish (`'tr'`). Add
  language code constants and tests.

  Note: older library versions also support the new languages, this update only
  adds new code constants.
- Added `ITranslator` interface implemented by `Translator` class, to achieve a
  better mock-ability in conjunction with common mocking frameworks.

### Changed
- Improve package icon, tags and project URLs.

### Fixed
- Test fix: tests should still succeed after new languages are added.

## [1.1.0] - 2022-04-13
### Added
- Add `ErrorMessage` property to `DocumentStatus` that contains a short
  description of document translation error, if available.

## [1.0.5] - 2022-04-12
### Added
- Add support for `TextTranslateOptions.TagHandling = "html"`. No code changes
  were needed, only comments and tests were changed.
- Add test for proxy support.

### Fixed
- Fix some tests that intermittently failed.

## [1.0.4] - 2022-01-27
### Fixed
- Fix issue in .NET versions earlier than 5.0, when creating large glossaries or
   translating texts larger than 64 KiB.

## [1.0.3] - 2022-01-19
### Added
- Add contribution guidelines -- currently we are unable to accept Pull Requests.

### Changed
- Improve README tests section.

## [1.0.2] - 2022-01-03
### Changed
- Include additional information with error status codes.

## [1.0.1] - 2021-12-07
### Fixed
- Use default HTTP version in requests rather than always using HTTP/2.

## [1.0.0] - 2021-11-18
### Changed
- Add missing properties in package, e.g. icon.
- Introduce GlossaryEntries class to encapsulate glossary entries.
- SplitSentences enum renamed to SentenceSplittingMode.
- Exceptions thrown by some functions were changed to use standard exceptions.
- Improvements to tests, README and documentation.

## [0.1.0] - 2021-11-05
Initial release.

[Unreleased]: https://github.com/DeepL/deepl-dotnet/compare/v1.22.1...HEAD
[1.22.1]: https://github.com/DeepL/deepl-dotnet/compare/v1.22.0...v1.22.1
[1.22.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.21.0...v1.22.0
[1.21.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.20.0...v1.21.0
[1.20.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.19.0...v1.20.0
[1.19.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.18.0...v1.19.0
[1.18.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.17.0...v1.18.0
[1.17.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.16.0...v1.17.0
[1.16.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.15.0...v1.16.0
[1.15.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.14.0...v1.15.0
[1.14.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.13.0...v1.14.0
[1.13.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.12.0...v1.13.0
[1.12.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.11.0...v1.12.0
[1.11.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.10.0...v1.11.0
[1.10.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.9.0...v1.10.0
[1.9.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.8.0...v1.9.0
[1.8.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.7.1...v1.8.0
[1.7.1]: https://github.com/DeepL/deepl-dotnet/compare/v1.7.0...v1.7.1
[1.7.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.6.0...v1.7.0
[1.6.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.5.1...v1.6.0
[1.5.1]: https://github.com/DeepL/deepl-dotnet/compare/v1.5.0...v1.5.1
[1.5.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.4.0...v1.5.0
[1.4.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.2.1...v1.3.0
[1.2.1]: https://github.com/DeepL/deepl-dotnet/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/DeepL/deepl-dotnet/compare/v1.0.5...v1.1.0
[1.0.5]: https://github.com/DeepL/deepl-dotnet/compare/v1.0.4...v1.0.5
[1.0.4]: https://github.com/DeepL/deepl-dotnet/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/DeepL/deepl-dotnet/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/DeepL/deepl-dotnet/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/DeepL/deepl-dotnet/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/DeepL/deepl-dotnet/compare/v0.1.0...v1.0.0
[0.1.0]: https://github.com/DeepL/deepl-dotnet/releases/tag/v0.1.0
