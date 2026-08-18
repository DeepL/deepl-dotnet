# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
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

[Unreleased]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.22.0...HEAD
[1.22.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.21.0...v1.22.0
[1.21.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.20.0...v1.21.0
[1.20.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.19.0...v1.20.0
[1.19.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.18.0...v1.19.0
[1.18.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.17.0...v1.18.0
[1.17.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.16.0...v1.17.0
[1.16.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.15.0...v1.16.0
[1.15.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.14.0...v1.15.0
[1.14.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.13.0...v1.14.0
[1.13.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.12.0...v1.13.0
[1.12.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.11.0...v1.12.0
[1.11.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.10.0...v1.11.0
[1.10.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.9.0...v1.10.0
[1.9.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.8.0...v1.9.0
[1.8.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.7.1...v1.8.0
[1.7.1]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.7.0...v1.7.1
[1.7.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.6.0...v1.7.0
[1.6.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.5.1...v1.6.0
[1.5.1]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.5.0...v1.5.1
[1.5.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.4.0...v1.5.0
[1.4.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.2.1...v1.3.0
[1.2.1]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.0.5...v1.1.0
[1.0.5]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.0.4...v1.0.5
[1.0.4]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/DeepLcom/deepl-dotnet/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/DeepLcom/deepl-dotnet/compare/v0.1.0...v1.0.0
[0.1.0]: https://github.com/DeepLcom/deepl-dotnet/releases/tag/v0.1.0
