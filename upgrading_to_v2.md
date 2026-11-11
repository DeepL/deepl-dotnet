# Upgrading to 2.0.0

This guide lists what you must change when you upgrade the `DeepL.net` package from 1.23.0 to 2.0.0. Most
applications are affected by the removal of the `Translator` class, the removal of the `LanguageCode` constants and
the new minimum runtime. Everything else is listed by area below. The full list of changes, including additions that
need no action, is in the [CHANGELOG](CHANGELOG.md).

Upgrade to 1.23.0 first and fix the deprecation warnings it reports: many removals in 2.0.0 are of members that
1.23.0 already marks as obsolete.

## Requirements

- The package targets `net8.0` and `netstandard2.0`. The `net5.0` target is removed.
- Supported runtimes are .NET 8 and later, and .NET Framework 4.6.2 and later (through `netstandard2.0`).
- .NET Core 3.1 and .NET 5 to 7 are not supported. They use the `netstandard2.0` build, which has no separate connect
  timeout (see `PerRetryConnectionTimeout` in the README) and does not honour cancellation while it copies a
  translation memory export to a stream. Building for them reports a warning that the package "doesn't support"
  the target framework, an error with `TreatWarningsAsErrors`. The `SuppressTfmSupportBuildWarnings` MSBuild
  property set to `true` silences it, at your own risk.
- Dependencies: `Microsoft.Extensions.Http.Polly` is now `8.0.33` (still Polly 7) and `System.Text.Json` is `10.0.14`
  (`netstandard2.0` build only; `net8.0` uses the framework's own). If your application pins older versions of
  `Microsoft.Extensions.Http`, `Microsoft.Extensions.Http.Polly` or `System.Text.Json`, raise them.

## Quick checklist

1. [Replace `Translator` and `TranslatorOptions` with `DeepLClient` and `DeepLClientOptions`](#translator-is-removed)
2. [Replace `ITranslator` in mocks and parameters with the new interfaces](#translator-is-removed)
3. [Replace `LanguageCode.German` and the other constants with strings](#language-code-constants-are-removed)
4. [Rename `GlossaryId`, `StyleId` and `TranslationMemoryId` options](#option-properties-are-renamed)
5. [Rename `TranslateDocumentStatusAsync()` to `TranslateDocumentGetStatusAsync()`](#documents)
6. [Stop catching `GlossaryNotFoundException`](#glossary-not-found-errors)
7. [Replace the `ConfiguredRules` class with a dictionary](#configured-rules-are-a-dictionary)
8. [Replace `Usage.AnyLimitReached`, `Usage.Document` and `Usage.TeamDocument`](#usage)
9. [Declare `ExtraBodyParameters` as `Dictionary<string, object?>` and pass typed values](#extra-body-parameters)
10. [Review exception handling and retries](#errors-validation-and-retries)

## Client and options

### `Translator` is removed

`Translator`, `ITranslator` and `TranslatorOptions` no longer exist. `DeepLClient` declares all their members itself,
and `DeepLClientOptions` derives from `BaseClientOptions`.

```csharp
// before
var translator = new Translator(authKey, new TranslatorOptions { MaximumNetworkRetries = 3 });
bool isFree = Translator.AuthKeyIsFreeAccount(authKey);

// after
var client = new DeepLClient(authKey, new DeepLClientOptions { MaximumNetworkRetries = 3 });
bool isFree = DeepLClient.IsFreeAccountAuthKey(authKey);
```

Code that mocked or implemented `ITranslator` must use the smaller interfaces, which `DeepLClient` implements:

| Interface | Methods |
|---|---|
| `ITextTranslator` | `TranslateTextAsync()` |
| `IDocumentTranslator` | `TranslateDocumentAsync()`, `TranslateDocumentUploadAsync()`, `TranslateDocumentGetStatusAsync()`, `TranslateDocumentWaitUntilDoneAsync()`, `TranslateDocumentDownloadAsync()` |
| `ILanguageProvider` | `GetSourceLanguagesAsync()`, `GetTargetLanguagesAsync()`, `GetGlossaryLanguagesAsync()` |
| `IUsageProvider` | `GetUsageAsync()` |
| `IGlossaryManager` | the v2 glossary methods (`CreateGlossaryAsync()`, `GetGlossaryAsync()`, ...) in addition to the multilingual ones |

```csharp
// before
public class Service {
  public Service(ITranslator translator) { }
}

// after
public class Service {
  public Service(ITextTranslator translator) { }
}
```

### Server URL and proxy validation

`DeepLClient` and `DeepLAdminClient` now throw an `ArgumentException` at construction when `ServerUrl` or `Proxy` is
not an absolute `http` or `https` URL. An unparsable `ServerUrl` threw a `UriFormatException`, a relative `Proxy` an
`InvalidOperationException`, and other schemes such as `ftp://` were accepted and failed at the first request.

```csharp
// before
try { new DeepLClient(authKey, new DeepLClientOptions { ServerUrl = serverUrl }); }
catch (UriFormatException) { }

// after
try { new DeepLClient(authKey, new DeepLClientOptions { ServerUrl = serverUrl }); }
catch (ArgumentException) { }
```

### Extra body parameters

`BaseRequestOptions.ExtraBodyParameters` is a `Dictionary<string, object?>` instead of `Dictionary<string, string>`.
Collection initializers compile unchanged. Variables and helpers declared with the old type must change. The API does
not convert types, so a string for a field it types as boolean, number or array is now rejected: pass the typed
value, or use the matching option.

```csharp
// before
Dictionary<string, string> extra = new() { ["outline_detection"] = "0" };

// after
Dictionary<string, object?> extra = new() { ["outline_detection"] = false };
var options = new TextTranslateOptions { ExtraBodyParameters = extra };
```

A `null` value is now sent as JSON `null` in JSON request bodies instead of removing the key. Leave the key out to not
send it. In the multipart body of a document upload, a `null` value still removes the key.

### Request bodies are JSON

Text translation, rephrasing and correction, v2 glossary creation, and v3 glossary creation, renaming and dictionary
updates now send `application/json` bodies instead of form-encoded ones. Document upload stays multipart. This only
matters to proxies and test doubles that inspect request bodies.

### Content headers in `Headers`

Entries of the `Headers` option that .NET treats as content headers (for example `Content-Language`) are now sent
on requests with a body. Every request failed with a `ConnectionException` before.

## Text translation

### Language code constants are removed

The roughly 120 constants of `LanguageCode` (for example `LanguageCode.German`) are removed. Use string literals;
`GetSourceLanguagesAsync()` and `GetTargetLanguagesAsync()` return the supported codes. `LanguageCode.RemoveRegionalVariant()`
remains.

```csharp
// before
await translator.TranslateTextAsync("Hello", LanguageCode.English, LanguageCode.German);

// after
await client.TranslateTextAsync("Hello", "en", "de");
```

### Option properties are renamed

`GlossaryId`, `StyleId` and `TranslationMemoryId` are removed from `TextTranslateOptions` and
`DocumentTranslateOptions`. Use `Glossary`, `StyleRule` and `TranslationMemory`. They take the same ID, and also
accept the resource object.

```csharp
// before
var options = new TextTranslateOptions { GlossaryId = glossaryId, StyleId = styleId };

// after
var options = new TextTranslateOptions { Glossary = glossaryId, StyleRule = styleId };
```

### Billed characters are `long`

`TextResult.BilledCharacters` is now a `long`. Code that assigns it to an `int` must cast or use `long`.

```csharp
// before
int billed = result.BilledCharacters;

// after
long billed = result.BilledCharacters;
```

## Documents

### `TranslateDocumentAsync()` returns the final status

All overloads of `TranslateDocumentAsync()` and `TranslateDocumentWaitUntilDoneAsync()` now return
`Task<DocumentStatus>` instead of `Task`. Callers that ignore the result compile unchanged. Classes implementing
`IDocumentTranslator` must change their return type.

```csharp
// before
await translator.TranslateDocumentAsync(inputFile, outputFile, "en", "de");

// after
DocumentStatus status = await client.TranslateDocumentAsync(inputFile, outputFile, "en", "de");
long? billed = status.BilledCharacters;
```

`DocumentStatus.BilledCharacters` is now a `long?` (it was an `int?`), and the `DocumentStatus` and `TextResult`
constructors take `long` and `long?`.

### `TranslateDocumentStatusAsync()` is renamed

```csharp
// before
var status = await translator.TranslateDocumentStatusAsync(handle);

// after
var status = await client.TranslateDocumentGetStatusAsync(handle);
```

### Document minification is removed

`DocumentTranslateOptions.EnableDocumentMinification`, `DocumentMinifier`, `IDocumentMinifier`,
`DocumentMinificationException` and `DocumentDeminificationException` are removed. Documents are sent as they are.
Delete the option and any catch blocks for the removed exceptions.

### Cancellation and file errors

- `TranslateDocumentAsync()` no longer wraps a cancellation through its `CancellationToken` in a
  `DocumentTranslationException`. The `OperationCanceledException` propagates. Timeouts and other failures are still
  wrapped.
- The `FileInfo` overloads of `TranslateDocumentAsync()` and `TranslateDocumentUploadAsync()` throw an
  `ArgumentException` before any request when the input file does not exist or is a directory. They threw
  `FileNotFoundException`, `DirectoryNotFoundException` or `UnauthorizedAccessException` before.
- Other local file failures of `TranslateDocumentAsync(FileInfo, ...)` (unreadable input, existing or uncreatable
  output file) throw a `DocumentTranslationException` with the I/O exception as `InnerException`.

```csharp
// before
try { await translator.TranslateDocumentAsync(input, output, "en", "de", null, token); }
catch (DocumentTranslationException) when (token.IsCancellationRequested) { }

// after
try { await client.TranslateDocumentAsync(input, output, "en", "de", null, token); }
catch (OperationCanceledException) { }
```

## Glossaries

### Glossary not found errors

`GlossaryNotFoundException` is removed. Glossary endpoints (v2 and v3) throw `NotFoundException` on HTTP 404, like all
other endpoints. Code that already catches `NotFoundException` is unaffected.

```csharp
// before
catch (GlossaryNotFoundException) { }

// after
catch (NotFoundException) { }
```

### Validation changes

- `UpdateMultilingualGlossaryNameAsync()` throws an `ArgumentException` for an empty or whitespace glossary ID. It
  sent a request to the glossary collection path before.
- `UpdateMultilingualGlossaryDictionaryFromCsvAsync()` and `ReplaceMultilingualGlossaryDictionaryFromCsvAsync()` throw
  an `ArgumentException` for empty or whitespace CSV content, without sending a request.

### `MultilingualGlossaryDictionaryEntries` constructor

The constructor taking a `MultilingualGlossaryDictionaryEntriesResult` is now internal, as is that class. Construct
the entries from the language codes and a `GlossaryEntries` object.

```csharp
// after
var entries = new MultilingualGlossaryDictionaryEntries(
  "en", "de", new GlossaryEntries(new Dictionary<string, string> { ["hello"] = "hallo" }));
```

## Style rules

### Configured rules are a dictionary

The `ConfiguredRules` class is removed, together with the `CreateStyleRuleAsync()` and
`UpdateStyleRuleConfiguredRulesAsync()` overloads taking it. Pass a `Dictionary<string, Dictionary<string, string>>`
keyed by the API names: category, then rule, then value. `StyleRuleInfo.ConfiguredRules` is the same type, so
categories the class did not model are no longer dropped.

```csharp
// before
await translator.CreateStyleRuleAsync("Rules", "en", new ConfiguredRules(datesAndTimes: new() { ["calendar_era"] = "use_bc_and_ad" }));
var era = rule.ConfiguredRules.DatesAndTimes;

// after
var configured = new Dictionary<string, Dictionary<string, string>> {
  ["dates_and_times"] = new() { ["calendar_era"] = "use_bc_and_ad" },
};
var created = await client.CreateStyleRuleAsync("Rules", "en", configured);
var era = created.ConfiguredRules?["dates_and_times"];
```

## Translation memories

### Job status values

`TranslationMemoryJobStatus.Downloaded` is removed, because the API does not define that status. A `"downloaded"`
status now parses to `Unknown`, with the raw value in `StatusValue`, and `WaitUntilTranslationMemoryJobDoneAsync()`
keeps polling on it. Removing `Downloaded` also renumbers the enum: `Failed` is now 3 (was 4), `Expired` 4 (was 5)
and `Unknown` 5 (was 6). Code that stores or compares the numeric values must change; store the names instead.

```csharp
// before
if (job.Status == TranslationMemoryJobStatus.Downloaded) { }

// after
if (job.StatusValue == "downloaded") { }
```

### `Done` means completed

`TranslationMemoryJob.Done` and `TranslationMemoryJobResult.Done` are `true` only for `Completed`. A failed or expired
job is no longer done.

```csharp
// before
if (job.Done) { }

// after
if (job.Done || !job.Ok) { }
```

### Types and constructors

- `TranslationMemoryJobResult.SkippedSegmentCount` is a `long?` (was `int?`).
- `TranslationMemoryJob.Product` is a non-nullable `string`, and `CreationTime` and `UpdatedTime` are non-nullable
  `DateTime`. A job response without one of them throws a `DeepLException`. The constructor takes them as required
  parameters after `results`. Remove null checks and `.Value` on them.

### File methods wrap failures

`ImportTranslationMemoryFromFilepathAsync()` and `ExportTranslationMemoryToFilepathAsync()` wrap every failure in the
new `TranslationMemoryJobException`, a `DeepLException` with the cause as `InnerException`. `JobId` and `Job` are set
once the job exists, and `HttpStatusCode` and `ShouldRetry` are copied from a `DeepLException` cause. A missing or
empty input file still throws an `ArgumentException` before any request, and cancellation is not wrapped. Code
catching a subclass such as `NotFoundException` must catch `TranslationMemoryJobException` and inspect
`InnerException`.

```csharp
// before
catch (NotFoundException) { }

// after
catch (TranslationMemoryJobException e) when (e.InnerException is NotFoundException) { }
```

### Other errors

- `UploadTranslationMemoryFileAsync()` throws an `ArgumentException` (was `UriFormatException`) when the upload URL is
  not an absolute `http` or `https` URL.
- `DownloadTranslationMemoryExportAsync()` throws a `DeepLException` (was `ArgumentException`) when the job has no
  download URL.

## Usage

- `Usage.AnyLimitReached` is replaced by `Usage.AnyCharacterLimitReached`: `true` when the account's character limit
  (`Character`) or this API key's character limit (`ApiKeyCharacter`) is reached. Speech minutes are not included;
  they have their own details (`SpeechToTextMinutes`, `SpeechToSpeechMinutes`). To check only the account-wide
  limit, check `Character` directly.
- `Usage.Document` and `Usage.TeamDocument` are removed, because the API no longer returns document usage.
  `Usage.ToString()` has no "Documents" and "Team documents" lines.
- `Usage.JsonFieldsStruct` and its constructor are removed; `Usage` has no public constructor. Obtain it from
  `GetUsageAsync()`.

```csharp
// before
if (usage.AnyLimitReached) { }

// after
if (usage.AnyCharacterLimitReached) { }
```

## Errors, validation and retries

### Retries of resource-creating calls

Calls that create a resource are retried only when the request was not sent: after HTTP 429, a name resolution
failure, a refused connection or a connect timeout. They are `TranslateDocumentAsync()`,
`TranslateDocumentUploadAsync()`, the glossary, style rule, style profile, spoken terms and translation memory
`Create*` methods, and `ImportTranslationMemoryFromFilepathAsync()` and `ExportTranslationMemoryToFilepathAsync()`.
`DeepLAdminClient.CreateDeveloperKeyAsync()` already behaved this way. After a 5xx, a timeout or a broken connection
they no longer retry and `DeepLException.ShouldRetry` is `false`, because the resource may already exist. Check
whether it exists before creating it again.

### Cut-off response bodies

A response body cut off mid-read is now retried like a connection reset by every method except the creates above.
For the creates it throws a `ConnectionException` with `ShouldRetry` set to `true` (it was `false`).

### Null arguments

The overloads taking a `MultilingualGlossaryInfo`, `TranslationMemoryInfo`, `TranslationMemoryJob`, `StyleProfile` or
`SpokenTerms` throw an `ArgumentNullException` for a `null` object, without sending a request. They threw a
`NullReferenceException` before.

### Error messages

Error messages include the API's nested `error.message` when the response has no top-level `message`.

## Types and signatures

### Interfaces gained members

These interfaces declare more methods. Classes that implement them themselves must add the members; `DeepLClient`
already has them.

| Interface | Added |
|---|---|
| `IWriter` | both `CorrectTextAsync()` overloads |
| `IGlossaryManager` | the v2 glossary methods and the v3 overloads taking options or `MultilingualGlossaryInfo` |
| `IStyleRuleManager` | overloads taking `StyleRuleInfo`, `CustomInstruction` or the new options classes |
| `ITranslationMemoryManager` | overloads taking segment, import or timeout options, or a `TranslationMemoryJob` |

### Binary-incompatible changes

Recompile every project that references `DeepL.net`. The changes below also break binary compatibility where source
code compiles unchanged: `TranslateDocumentAsync()` and `TranslateDocumentWaitUntilDoneAsync()` return types, the
`long` and `long?` properties, `ExtraBodyParameters`, and the `TranslationMemoryJobStatus` numbering.

## Internal classes

These classes were public by accident and are no longer part of the public API:

- `DeepL.Internal.LargeFormUrlEncodedContent` is now internal.
- `MultilingualGlossaryDictionaryEntriesResult` is now internal, as is the `MultilingualGlossaryDictionaryEntries`
  constructor taking it.
- `Usage.JsonFieldsStruct` is removed.
