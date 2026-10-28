# DeepL .NET Library

[![NuGet](https://img.shields.io/nuget/v/deepl.net.svg)](https://www.nuget.org/packages/DeepL.net/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blueviolet.svg)](https://github.com/DeepLcom/deepl-dotnet/blob/main/LICENSE)
[![.NET](https://github.com/DeepLcom/deepl-dotnet/actions/workflows/build_default.yml/badge.svg)](https://github.com/DeepLcom/deepl-dotnet/actions/workflows/build_default.yml)

The [DeepL API](https://www.deepl.com/docs-api?utm_source=github&utm_medium=github-dotnet-readme) is a language
AI API that allows other computer programs to send texts and documents to DeepL's servers and receive
high-quality translations and improvements to the text. This opens a whole universe of opportunities
for developers: any translation product you can imagine can now be built on top of DeepL's best-in-class
translation technology.

The DeepL .NET library offers a convenient way for applications written in .NET to interact with the DeepL API. We
intend to support all API functions with the library, though support for new features may be added to the library after
they’re added to the API.

## Getting an authentication key

To use the DeepL .NET Library, you'll need an API authentication key. To get a key,
[please create an account here](https://www.deepl.com/pro?utm_source=github&utm_medium=github-dotnet-readme#developer).
With a DeepL API Free account you can consume up to 500,000 characters/month for free.

## Installation

Using the .NET Core command-line interface (CLI) tools:

```
dotnet add package DeepL.net
```

Using the NuGet Command Line Interface (CLI):

```
nuget install DeepL.net
```

## Usage

All entities in the DeepL .NET library are in the `DeepL` namespace:

```c#
using DeepL;
```

Create a `DeepLClient` object providing your DeepL API authentication key.

Be careful not to expose your key, for example when sharing source code.

```c#
var authKey = "f63c02c5-f056-..."; // Replace with your key
var client = new DeepLClient(authKey);
```

This example is for demonstration purposes only. In production code, the authentication key should not be hard-coded,
but instead fetched from a configuration file or environment variable.

`DeepLClient` accepts options as the second argument, see [Configuration](#configuration) for more information.

### Translating text

To translate text, call `TranslateTextAsync()`. The first argument is a string containing the text to translate, or
`IEnumerable` of strings to translate multiple texts.

The second and third arguments are case-insensitive language codes for the source and target language respectively,
for example `"DE"`, `"FR"`.
Language codes are plain strings; use the languages API (`GetSourceLanguagesAsync()` and
`GetTargetLanguagesAsync()`) to retrieve the currently supported languages. The constants in the `LanguageCode` static
class are deprecated and will be removed in the next major version.
A target language without a regional variant translates to its default variant, for example `"EN"` to `"EN-US"` and
`"PT"` to `"PT-PT"`; specify the variant, for example `"EN-GB"` or `"PT-BR"`, to choose explicitly.
To auto-detect the input text language, specify `null` as the source language.

Additional `TextTranslateOptions` can also be provided, see [Text translation options](#text-translation-options) below.

`TranslateTextAsync()` returns a `TextResult` or `TextResult` array corresponding to the input text(s).
The `TextResult` contains:

- the translated text,
- the detected source language code,
- the number of characters billed for the text, and
- the translation model type used (only non-null if the ModelType option is specified.)

```c#
// Translate text into a target language, in this case, French:
var translatedText = await client.TranslateTextAsync(
      "Hello, world!",
      "EN",
      "FR");
Console.WriteLine(translatedText); // "Bonjour, le monde !"
// Note: printing or converting the result to a string uses the output text.

// Translate multiple texts into British English:
var translations = await client.TranslateTextAsync(
      new[] { "お元気ですか？", "¿Cómo estás?" }, null, "EN-GB");
Console.WriteLine(translations[0].Text); // "How are you?"
Console.WriteLine(translations[0].DetectedSourceLanguageCode); // "JA"
Console.WriteLine(translations[0].BilledCharacters); // 7 - the number of characters in the source text "お元気ですか？"
Console.WriteLine(translations[1].Text); // "How are you?"
Console.WriteLine(translations[1].DetectedSourceLanguageCode); // "ES"
Console.WriteLine(translations[1].BilledCharacters); // 12 - the number of characters in the source text "¿Cómo estás?"

// Translate into German with less and more Formality:
foreach (var formality in new[] { Formality.Less, Formality.More }) {
  Console.WriteLine(
        await client.TranslateTextAsync(
              "How are you?",
              null,
              "DE",
              new TextTranslateOptions { Formality = formality }));
}
// Will print: "Wie geht es dir?" "Wie geht es Ihnen?"
```

#### Text translation options

`TextTranslateOptions` has the following properties that impact text translation:

- `SentenceSplittingMode`: specifies how input text should be split into
  sentences, default: `'SentenceSplittingMode.All'`.
  - `SentenceSplittingMode.All`: input text will be split into sentences using
    both newlines and punctuation.
  - `SentenceSplittingMode.Off`: input text will not be split into sentences.
    Use this for applications where each input text contains only one
    sentence.
  - `SentenceSplittingMode.NoNewlines`: input text will be split into
    sentences using punctuation but not newlines.
- `PreserveFormatting`: controls automatic-formatting-correction. Set to
  `true` to prevent automatic-correction of formatting, default: `false`.
- `Formality`: controls whether translations should lean toward informal or
  formal language. This option is only available for some target languages, see
  [Listing available languages](#listing-available-languages).
  - `Formality.Less`: use informal language.
  - `Formality.More`: use formal, more polite language.
  - `Formality.Default`: standard level of formality.
  - `Formality.PreferLess`: less formality, if available for the specified target language, otherwise default.
  - `Formality.PreferMore`: more formality, if available for the specified target language, otherwise default.
- `GlossaryId`: specifies a glossary to use with translation, as a string
  containing the glossary ID.
- `GlossaryIds`: specifies multiple glossaries to use with translation (up to 5),
  as a list of strings containing the glossary IDs. Glossaries are applied in
  order, with the first matching term taking precedence. Using this option
  requires the source language to be specified, and it cannot be combined with
  `GlossaryId`.
- `StyleId`: specifies a style rule to use with translation, as a string
  containing the ID of the style rule.
- `StyleProfileId`: specifies a style profile to use with translation, as a
  string containing the ID of the style profile (or pass a `StyleProfile` to the
  constructor). The profile applies its glossaries, style rule lists and
  translation memories; a component that does not fit the language pair is left
  out. The source language is not required. It cannot be combined with
  `GlossaryId`, `GlossaryIds`, `StyleId`, `TranslationMemoryId`,
  `TranslationMemoryThreshold` or `CustomInstructions`; the API rejects such a
  request.
- `TranslationMemoryId`: specifies a translation memory to use with translation,
  as a string containing the translation memory ID.
- `TranslationMemoryThreshold`: specifies the minimum matching percentage
  (0 to 100) for translation memory matches. We recommend a minimum
  threshold of 75%.
- `Context`: specifies additional context to influence translations, that is not
  translated itself. Characters in the `context` parameter are not counted toward billing.
  See the [API documentation][api-docs-context-param] for more information and
  example usage.
- `CustomInstructions`: an array of instructions to customize the translation behavior.
  Up to 10 custom instructions can be specified, each with a maximum of 300 characters.
  Important: The target language must be `de`, `en`, `es`, `fr`, `it`, `ja`, `ko`, `zh`
  or any variants of these languages.
  Note: Any request with the `CustomInstructions` parameter enabled will use
  `quality_optimized` models as the default. Requests combining 
  `CustomInstructions` and `ModelType: latency_optimized` will be rejected.
- `ModelType`: specifies the type of translation model to use, options are:
  - `'quality_optimized'` (`ModelType.QualityOptimized`): use a translation
    model that maximizes translation quality, at the cost of response time.
    This option may be unavailable for some language pairs.
  - `'prefer_quality_optimized'` (`ModelType.PreferQualityOptimized`): use
    the highest-quality translation model for the given language pair.
  - `'latency_optimized'` (`ModelType.LatencyOptimized`): use a translation
    model that minimizes response time, at the cost of translation quality.
- `TagHandling`: type of tags to parse before translation, options are
  `"html"` and `"xml"`.
- `TagHandlingVersion`: specifies which version of the tag handling algorithm
  to use. Options are `"v1"` and `"v2"`. This parameter is only used if
  `TagHandling` is set to `"html"` or `"xml"`.

The following options are only used if `TagHandling` is set to `'xml'`:

- `OutlineDetection`: set to `false` to disable automatic tag detection,
  default is `true`.
- `SplittingTags`: `List` of XML tags that should be used to split text into
  sentences. Tags may be specified individually (`['tag1', 'tag2']`),
  or a comma-separated list of strings (`'tag1,tag2'`). The default is an empty
  list.
- `NonSplittingTags`: `List` of XML tags that should not be used to split
  text into sentences. Format and default are the same as for splitting tags.
- `IgnoreTags`: `List` of XML tags that containing content that should not be
  translated. Format and default are the same as for splitting tags.
- `ExtraBodyParameters`: `Dictionary<string, string>` of extra parameters to pass in the body of the HTTP request. Keys
  in this dictionary will be added to the request body and can override built-in parameters. Mostly used by DeepL
  employees to test functionality, or for beta programs.

For a detailed explanation of the XML handling options, see the [API documentation][api-docs-xml-handling].

### Improving text (Write API)

You can use the Write API to improve or rephrase text. This is implemented in
the `RephraseTextAsync()` method. The first argument is a string containing the text
you want to translate, or a list of strings if you want to translate multiple texts.

`targetLanguageCode` optionally specifies the target language, e.g. when you want to change
the variant of a text (for example, you can send an english text to the write API and
use `targetLanguageCode` to turn it into British or American English). Please note that the
Write API itself does NOT translate. If you wish to translate and improve a text, you
will need to make multiple calls in a chain.

Language codes are the same as for translating text.

Example call:

```c#
WriteResult result = await client.RephraseTextAsync("A rainbouw has seven colours.", "EN-US");
Console.WriteLine(result);
```

Additionally, you can optionally specify a style OR a tone (not both at once) that the
improvement should be in. The following styles are supported (`default` will be used if
nothing is selected):

- `academic`
- `business`
- `casual`
- `default`
- `simple`

The following tones are supported (`default` will be used if nothing is selected):

- `confident`
- `default`
- `diplomatic`
- `enthusiastic`
- `friendly`

You can also prefix any non-default style or tone with `prefer_` (`prefer_academic`, etc.),
in which case the style/tone will only be applied if the language supports it. If you do not
use `prefer_`, requests with `targetLanguageCode`s or detected languages that do not support
styles and tones will fail. The current list of supported languages can be found in our
[API documentation][api-docs]. We plan to also expose this information via an API endpoint
in the future.

You can pass a style like so:

```c#
var options = new TextRephraseOptions { WritingStyle = "business" }
var result = await client.RephraseTextAsync("A rainbouw has seven colours.", "EN-US", options);
Console.WriteLine(result);
```

To only fix grammar and spelling errors without otherwise rephrasing the text, use
`CorrectTextAsync()`. It takes the same text argument (a string or a list of strings) and an
optional `targetLanguageCode`, but no style or tone options:

```c#
WriteResult result = await client.CorrectTextAsync("I has a apple.", "EN-US");
Console.WriteLine(result);
```

### Translating documents

To translate documents, call `TranslateDocumentAsync()` with the input and output files as `FileInfo`
objects, and provide the source and target language as above.

Additional `DocumentTranslateOptions` are also available,
see [Document translation options](#document-translation-options) below.
Note that file paths are not accepted as strings, to avoid mixing up the file and language arguments.

```c#
// Translate a formal document from English to German
try {
  await client.TranslateDocumentAsync(
        new FileInfo("Instruction Manual.docx"),
        new FileInfo("Bedienungsanleitung.docx"),
        "EN",
        "DE",
        new DocumentTranslateOptions { Formality = Formality.More });
} catch (DocumentTranslationException exception) {
  // If the error occurs *after* upload, the DocumentHandle will contain the document ID and key
  if (exception.DocumentHandle != null) {
    var handle = exception.DocumentHandle.Value;
    Console.WriteLine($"Document ID: {handle.DocumentId}, Document key: {handle.DocumentKey}");
  } else {
    Console.WriteLine($"Error occurred during document upload: {exception.Message}");
  }
}
```

Alternatively the input and output files may be provided as `Stream` objects; in
that case the input file name (or extension) is required, so the DeepL API can
determine the file type:

```c#
...
  await client.TranslateDocumentAsync(
        new MemoryStream(buffer),
        "Input file.docx", // An extension like ".docx" is also sufficient
        File.OpenWrite(outputDocumentPath),
        "EN",
        "DE",
        new DocumentTranslateOptions { Formality = Formality.More });
```

`TranslateDocumentAsync()` manages the upload, wait until translation is complete, and download steps. If your
application needs to execute these steps individually, you can instead use the following functions directly:

- `TranslateDocumentUploadAsync()`,
- `TranslateDocumentStatusAsync()` (or `TranslateDocumentWaitUntilDoneAsync()`), and
- `TranslateDocumentDownloadAsync()`

#### Document translation options

`DocumentTranslateOptions` has the following properties that impact text translation:

- `Formality`:  same as in [Text translation options](#text-translation-options).
- `GlossaryId`:  same as in [Text translation options](#text-translation-options).
- `GlossaryIds`:  same as in [Text translation options](#text-translation-options).
- `StyleId`:  same as in [Text translation options](#text-translation-options).
- `StyleProfileId`:  same as in [Text translation options](#text-translation-options), except that
  `CustomInstructions` does not exist for documents. An unknown style profile is not rejected at upload: the
  translation fails with a `DocumentTranslationException` whose message contains `Style profile not found.`.
- `TranslationMemoryId`:  same as in [Text translation options](#text-translation-options).
- `TranslationMemoryThreshold`:  same as in [Text translation options](#text-translation-options).
- `EnableDocumentMinification` (obsolete): A `bool` value. If set to `true`, the library will try to minify a document
  before translating it through the API, sending a smaller document if the file contains a lot of media. This is
  currently only supported for `pptx` and `docx` files. See also [Document minification](#document-minification).
  Note that this only works in the high-level `TranslateDocumentDownloadAsync` method, not
  `TranslateDocumentUploadAsync`. However, the behavior can be emulated by creating a new `DocumentMinifier`
  object and calling the minifier's methods in between. This option and `DocumentMinifier` are obsolete and will
  be removed in the next major version.
- `OutputFormat`: using the parameter during document upload, you can select alternative output formats. See
  the [API documentation][api-docs-outputformat-param] for more information and example usage.
- `EnableWatermark`: A `bool?` value. If set to `true`, the API adds a "Translated by DeepL" watermark to the
  translated document. Only supported for `docx` and `pdf` output; for other output formats the API ignores it. If
  unset (the default), the parameter is not sent.
- `ExtraBodyParameters`: `Dictionary<string, string>` of extra parameters to pass in the body of the HTTP request. Keys
  in this dictionary will be added to the request body and can override built-in parameters. Mostly used by DeepL
  employees to test functionality, or for beta programs.

#### Document minification

> **Obsolete:** The `EnableDocumentMinification` option, `DocumentMinifier`, `IDocumentMinifier`,
> `DocumentMinificationException` and `DocumentDeminificationException` are marked `[Obsolete]` and will be removed
> in the next major version. Using them produces compiler warning CS0618; they keep working until then.

In some contexts, one can end up with large document files (e.g. PowerPoint presentations
or Word files with many contributors, especially in a larger organization). The DeepL API
limits the size of uploaded documents; see the [usage limits][api-docs-usage-limits] for the current limits.
In the case that most of this size comes from media included in the documents (e.g. images,
videos, animations), document minification can help.
In this case, the library will create a temporary directory to extract the document into,
replace the large media with tiny placeholders, create a minified document, translate that
via the API, and re-insert the original media into the original file. Please note that this
requires a bit of additional (temporary) disk space, we recommend at least 2x the file size
of the document to be translated.
To use document minification, simply pass the option to the `TranslateDocumentAsync` function:

```c#
await client.TranslateDocumentAsync(
    inFile, outFile, "EN", "DE", new DocumentTranslateOptions { EnableDocumentMinification = true }
);
```

In order to use document minification with the lower-level `TranslateDocumentUploadAsync`,
`TranslateDocumentWaitUntilDoneAsync` and `TranslateDocumentDownloadAsync` methods as well as other details,
see the `DocumentMinifier` class.
Currently supported document types for minification:

1. `pptx`
2. `docx`
   Currently supported media types for minification:
1. `png`
2. `jpg`
3. `jpeg`
4. `emf`
5. `bmp`
6. `tiff`
7. `wdp`
8. `svg`
9. `gif`
10. `mp4`
11. `asf`
12. `avi`
13. `m4v`
14. `mpg`
15. `mpeg`
16. `wmv`
17. `mov`
18. `aiff`
19. `au`
20. `mid`
21. `midi`
22. `mp3`
23. `m4a`
24. `wav`
25. `wma`

### Glossaries

Glossaries allow you to customize your translations using defined terms.
Multiple glossaries can be stored with your account, each with a user-specified name and a uniquely-assigned ID.

### v2 versus v3 glossary APIs

The newest version of the glossary APIs are the `/v3` endpoints, allowing both
editing functionality plus support for multilingual glossaries. New methods and
objects have been created to support interacting with these new glossaries.
Due to this new functionality, users are recommended to utilize these
multilingual glossary methods. However, to continue using the `v2` glossary API
endpoints, please continue to use the existing endpoints in the `Translator.cs`
(e.g. `CreateGlossaryAsync()`, `GetGlossaryAsync()`, etc).

To migrate to use the new multilingual glossary methods from the current
monolingual glossary methods, please refer to
[this migration guide](upgrading_to_multilingual_glossaries.md).

The following sections describe how to interact with multilingual glossaries
using the new functionality:

#### Creating a glossary

You can create a glossary with your desired terms and name using
`CreateMultilingualGlossaryAsync()`. Each glossary applies to a single source-target language
pair. Note: Glossaries are only supported for some language pairs, see
[Listing available glossary languages](#listing-available-glossary-languages)
for more information. The entries should be specified as a `Dictionary`.

If successful, the glossary is created and stored with your DeepL account, and
a `MultilingualGlossaryInfo` object is returned including the ID, name, languages and entry
count.

```c#
// Create a glossary with an English to German dictionary containing two terms:
var entriesDictionary = new Dictionary<string, string>{{"artist", "Maler"}, {"prize", "Gewinn"}};
var glossaryDicts = new[] {new MultilingualGlossaryDictionaryEntries(
  "EN", "DE", new GlossaryEntries(entriesDictionary))};
var glossaryEnToDe = await client.CreateMultilingualGlossaryAsync("My glossary", glossaryDicts);

var glossaryDictEnToDe = glossaryEnToDe.Dictionaries[0]
Console.WriteLine($"Created {glossaryEnToDe.Name}' ({glossaryEnToDe.GlossaryId}) " +
    $"with {glossaryEnToDe.Dictionaries.Length} dictionary where its language pair is "
    $"{glossaryDictEnToDe.SourceLanguageCode}->{glossaryDictEnToDe.TargetLanguageCode} " +
    $"containing {glossaryDictEnToDe.EntryCount} entries"
);
// Example: Created 'My glossary' (559192ed-8e23-...) with 1 dictionary where
// its language pair is EN->DE containing 2 entries

You can also upload a glossary downloaded from the DeepL website using
`CreateMultilingualGlossaryFromCsvAsync()`. Instead of supplying the entries as a dictionary,
specify the CSV data as a `Stream` containing file content:

```c#
var csvStream =  File.OpenRead("myGlossary.csv");
var csvGlossary = await client.CreateMultilingualGlossaryFromCsvAsync("My CSV glossary", "EN", "DE", csvStream);
```

The [API documentation][api-docs-csv-format] explains the expected CSV format in detail.

#### Getting, listing and deleting stored glossaries

Functions to get, list, and delete stored glossaries are also provided:

- `GetMultilingualGlossaryAsync()` takes a glossary ID and returns a
  `MultilingualGlossaryInfo` object for a stored glossary, or raises an
  exception if no such glossary is found.
- `ListMultilingualGlossariesAsync()` returns a `List` of
  `MultilingualGlossaryInfo` objects corresponding to all of your stored
  glossaries.
- `DeleteMultilingualGlossaryAsync()` takes a glossary ID or
  `MultilingualGlossaryInfo` object and deletes the stored glossary from the
  server, or raises an exception if no such glossary is found.
- `DeleteMultilingualGlossaryDictionaryAsync()` takes a glossary ID or
  `MultilingualGlossaryInfo` object to identify the glossary. Additionally
  takes in a source and target language or a
  `MultilingualGlossaryDictionaryInfo` object and deletes the stored dictionary
  from the server, or raises an exception if no such glossary dictionary is
  found.

```c#
// Retrieve a stored glossary using the ID
var myGlossary = await client.GetMultilingualGlossaryAsync("559192ed-8e23-...");

// Delete a glossary dictionary from a stored glossary
await client.DeleteMultilingualGlossaryDictionaryAsync(myGlossary, myGlossary.Dictionaries[0]);

// Find and delete glossaries named 'Old glossary'
var glossaries = await client.ListMultilingualGlossariesAsync();
foreach (var glossaryInfo in glossaries) {
  if (glossaryInfo.Name == "Old glossary")
    await client.DeleteMultilingualGlossaryAsync(glossaryInfo);
}
```

#### Listing entries in a stored glossary

The `MultilingualGlossaryDictionaryInfo` object does not contain the glossary entries, but instead
only the number of entries in the `EntryCount` property.

To list the entries contained within a stored glossary, use
`GetMultilingualGlossaryDictionaryEntriesAsync()` providing either the `MultilingualGlossaryInfo` object or glossary ID
and either a `MultilingualGlossaryDictionaryInfo` or source and target language pair:

```c#
var glossaryDicts = await client.GetMultilingualGlossaryDictionaryEntriesAsync(myGlossary, "en", "de");

foreach (KeyValuePair<string, string> entry in glossaryDicts[0].Entries.ToDictionary()) {
  Console.WriteLine($"{entry.Key}: {entry.Value}");
}
// prints:
//   artist: Maler
//   prize: Gewinn
```

#### Editing a glossary

Functions to edit stored glossaries are also provided:

- `UpdateMultilingualGlossaryDictionaryAsync()` takes a glossary ID or `MultilingualGlossaryInfo`
  object, plus a source language, target language, and a dictionary of entries.
  It will then either update the list of entries for that dictionary (either
  inserting new entires or replacing the target phrase for any existing
  entries) or will insert a new glossary dictionary if that language pair is
  not currently in the stored glossary.
- `ReplaceMultilingualGlossaryDictionaryAsync()` takes a glossary ID or `MultilingualGlossaryInfo`
  object, plus a source language, target language, and a dictionary of entries.
  It will then either set the entries to the parameter value, completely
  replacing any pre-existing entries for that language pair.
- `UpdateMultilingualGlossaryNameAsync()` takes a glossary ID or `MultilingualGlossaryInfo`
  object, plus the new name of the glossary.

```c#
// Update glossary dictionary
var entries = new Dictionary<string, string>{{"artist", "Maler"}, {"hello", "guten tag"}};
var dictionaries = new[] {new MultilingualGlossaryDictionaryEntries("EN", "DE", entries)};
var myGlossary = await client.CreateMultilingualGlossaryAsync(
    "My glossary",
    dictionaries
);
var newEntries = new Dictionary<string, string>{{"hello", "hallo"}, {"prize", "Gewinn"}};
var glossaryDict = new MultilingualGlossaryDictionaryEntries("EN", "DE", newEntries);
var updatedGlossary = await client.UpdateMultilingualGlossaryDictionaryAsync(
    myGlossary,
    glossaryDict
);

var entriesResponse = await client.GetMultilingualGlossaryEntriesAsync(myGlossary, "EN", "DE");

foreach (KeyValuePair<string, string> entry in entriesResponse.Dictionaries[0].Entries.ToDictionary()) {
  Console.WriteLine($"{entry.Key}: {entry.Value}");
}
// prints:
//   artist: Maler
//   hello: hallo
//   prize: Gewinn

// Update a glossary dictionary from CSV
var csvStream =  File.OpenRead("myGlossary.csv");
var csvGlossary = await client.UpdateMultilingualGlossaryDictionaryFromCsvAsync("4c81ffb4-2e...", "EN", "DE", csvStream);

// Replace glossary dictionary
var replacementEntries = new Dictionary<string, string>{{"goodbye", "Auf Wiedersehen"}};
var glossaryDict = new MultilingualGlossaryDictionaryEntries("EN", "DE", new GlossaryEntries(replacementEntries));
var updatedGlossary = await client.ReplaceMultilingualGlossaryDictionaryAsync(
  myGlossary,
  glossaryDict);
var entriesResponse = await client.GetMultilingualGlossaryEntries(myGlossary, "EN", "DE");
foreach (KeyValuePair<string, string> entry in entriesResponse.Dictionaries[0].Entries.ToDictionary()) {
  Console.WriteLine($"{entry.Key}: {entry.Value}");
}
// prints:
//   goodbye: Auf Wiedersehen

// Replace a glossary dictionary from CSV
var csvStream =  File.OpenRead("myGlossary.csv");
var csvGlossary = await client.ReplaceMultilingualGlossaryDictionaryFromCsvAsync("4c81ffb4-2e...", "EN", "DE", csvStream);

// Update the glossary name
var updatedGlossary = await client.UpdateMultilingualGlossaryNameAsync(
  myGlossary,
  "My new glossary name"
);
Console.WriteLine(updatedGlossary.Name); // 'My new glossary name'
```

#### Using a stored glossary

You can use a stored glossary for text (or document) translation by setting the
`TextTranslationOptions` (or `DocumentTranslationOptions`) `GlossaryId` property
to the glossary ID. Set `source_lang` as well, so the glossary's language pair
matches the request:

```c#
var resultWithGlossary = await client.TranslateTextAsync(
    "The artist was awarded a prize.",
    "EN",
    "DE",
    new TextTranslateOptions { GlossaryId = glossaryEnToDe.GlossaryId });
// resultWithGlossary.Text == "Der Maler wurde mit einem Gewinn ausgezeichnet."
// Without using a glossary: "Der Künstler wurde mit einem Preis ausgezeichnet."
```

For text translation the library no longer rejects a glossary with no source
language before sending the request, in preparation for source language
detection with glossaries. Whether such a request succeeds is determined by the
API and by what your account has enabled. Document translation still requires a
source language with a glossary.

### Spoken terms

Spoken Terms collections improve speech recognition in the Voice API: they make
sure that specific words and phrases, such as product names or technical terms,
are transcribed correctly. A collection holds one term list per language
(ISO 639-1 code, e.g. `en`); each term list is a list of strings. Every method
that takes a collection accepts either its ID or a `SpokenTerms` object.

Before sending, the library trims leading and trailing whitespace from each
term and then checks it like a glossary term
(`GlossaryEntries.ValidateGlossaryTerm()`): a term that is blank or contains a
control character or newline throws an `ArgumentException` and no request is
sent. Size limits and the remaining rules are checked by the API: see the
[Spoken Terms requirements](https://developers.deepl.com/docs/customize/spoken-terms-requirements).

#### Creating a Spoken Terms collection

```c#
var spokenTerms = await client.CreateSpokenTermsAsync(
    "Product names",
    new[] { new SpokenTermsEntries("en", new[] { "DeepL", "webhook" }) });
Console.WriteLine($"Created {spokenTerms.Name} ({spokenTerms.SpokenTermsId})");
foreach (var termList in spokenTerms.TermLists) {
  Console.WriteLine($"{termList.Lang}: {termList.EntryCount} terms");
}
```

#### Getting and listing Spoken Terms collections

`GetSpokenTermsAsync()` and `ListSpokenTermsAsync()` return the collections'
details, but not their terms.

```c#
var fetched = await client.GetSpokenTermsAsync("YOUR_SPOKEN_TERMS_ID");
foreach (var collection in await client.ListSpokenTermsAsync()) {
  Console.WriteLine($"{collection.Name} ({collection.SpokenTermsId}), created {collection.CreationTime}");
}
```

#### Reading the terms of a language

```c#
var entries = await client.GetSpokenTermsEntriesAsync(spokenTerms, "en");
Console.WriteLine(string.Join(", ", entries.Entries));
```

#### Updating a Spoken Terms collection

`UpdateSpokenTermsAsync()` renames the collection and/or **merges** the given
terms into the existing term list of each language. Arguments left `null` are
not changed.

```c#
var updated = await client.UpdateSpokenTermsAsync(
    spokenTerms,
    name: "Product names v2",
    termLists: new[] { new SpokenTermsEntries("en", new[] { "authentication" }) });
```

#### Replacing or deleting the term list of a language

`ReplaceSpokenTermsListAsync()` **replaces** the term list of one language, or
adds it if the collection has no list for that language yet.
`DeleteSpokenTermsListAsync()` removes the term list of one language.

```c#
var termList = await client.ReplaceSpokenTermsListAsync(
    spokenTerms, new SpokenTermsEntries("de", new[] { "Schnittstelle" }));
await client.DeleteSpokenTermsListAsync(spokenTerms, "de");
```

#### Deleting a Spoken Terms collection

```c#
await client.DeleteSpokenTermsAsync(spokenTerms);
```

### Style Rules

Style rules allow you to customize your translations using a managed, shared list
of rules for style, formatting, and more. Multiple style rules can be stored with
your account, each with a user-specified name and a uniquely-assigned ID.

Style rules can be applied to both text and document translation by setting the
`StyleId` property (or passing a `StyleRuleInfo` to the constructor) in
`TextTranslateOptions` or `DocumentTranslateOptions`.

#### Creating a style rule

Use `CreateStyleRuleAsync()` to create a new style rule with a name and language
code. You can optionally provide `configuredRules` and `customInstructions`.

```c#
var rule = await client.CreateStyleRuleAsync("My Style Rule", "en");
Console.WriteLine($"Created: {rule.Name} ({rule.StyleId})");
```

#### Retrieving and listing style rules

Use `GetStyleRuleAsync()` to retrieve a single style rule by ID, or
`GetAllStyleRulesAsync()` to list all style rules.

`GetAllStyleRulesAsync()` returns an array of `StyleRuleInfo` objects
corresponding to all of your stored style rules. The method accepts optional
parameters: `page` (page number for pagination, 0-indexed), `pageSize` (number
of items per page), and `detailed`. When `true`, the response includes
`ConfiguredRules` and `CustomInstructions` for each style rule. When `false`
(default), these fields are omitted for faster responses.

```c#
// Get a single style rule by ID
var rule = await client.GetStyleRuleAsync("YOUR_STYLE_ID");
Console.WriteLine($"{rule.Name} ({rule.Language})");

// List all style rules
var styleRules = await client.GetAllStyleRulesAsync();
foreach (var r in styleRules) {
  Console.WriteLine($"{r.Name} ({r.StyleId})");
}

// List with detailed configuration
var styleRulesDetailed = await client.GetAllStyleRulesAsync(detailed: true);
foreach (var r in styleRulesDetailed) {
  if (r.ConfiguredRules?.Numbers != null) {
    Console.WriteLine($"  Number formatting: {string.Join(", ", r.ConfiguredRules.Numbers.Keys)}");
  }
}
```

#### Updating a style rule

Use `UpdateStyleRuleNameAsync()` to rename a style rule, and
`UpdateStyleRuleConfiguredRulesAsync()` to update its configured rules.

```c#
// Update the name
var updated = await client.UpdateStyleRuleNameAsync("YOUR_STYLE_ID", "New Name");

// Update configured rules
var configuredRules = new ConfiguredRules(
    styleAndTone: new Dictionary<string, string> { ["formality"] = "formal" });
var updatedRules = await client.UpdateStyleRuleConfiguredRulesAsync("YOUR_STYLE_ID", configuredRules);
```

#### Managing custom instructions

Custom instructions allow you to add free-text prompts to a style rule. Use
`CreateStyleRuleCustomInstructionAsync()`, `GetStyleRuleCustomInstructionAsync()`,
`UpdateStyleRuleCustomInstructionAsync()`, and
`DeleteStyleRuleCustomInstructionAsync()` to manage them.

```c#
// Create a custom instruction
var instruction = await client.CreateStyleRuleCustomInstructionAsync(
    "YOUR_STYLE_ID", "Formal tone", "Always use formal language");
Console.WriteLine($"Created instruction: {instruction.Id}");

// Get a custom instruction
var retrieved = await client.GetStyleRuleCustomInstructionAsync(
    "YOUR_STYLE_ID", instruction.Id);

// Update a custom instruction
var updated = await client.UpdateStyleRuleCustomInstructionAsync(
    "YOUR_STYLE_ID", instruction.Id, "Updated label", "Use very formal language");

// Delete a custom instruction
await client.DeleteStyleRuleCustomInstructionAsync(
    "YOUR_STYLE_ID", instruction.Id);
```

#### Deleting a style rule

Use `DeleteStyleRuleAsync()` to delete a style rule by ID.

```c#
await client.DeleteStyleRuleAsync("YOUR_STYLE_ID");
```

### Style profiles

A style profile is a named set of glossaries, style rule lists and translation
memories, with an optional description and industry. Every style profile
management method accepts either its ID or a `StyleProfile` object; an empty ID
throws an `ArgumentException` without sending a request.

The optional fields are set with `StyleProfileOptions`. A property left `null`
is not sent. An empty string and an empty list are sent as given: on update, `""`
clears the description or industry, and an empty list unlinks all components of
that kind.

#### Creating a style profile

Only the name is required. If any of the component IDs cannot be used, the API
rejects the request and no style profile is created.

```c#
var profile = await client.CreateStyleProfileAsync("Legal, German");

var linked = await client.CreateStyleProfileAsync(
    "Legal, German (linked)",
    new StyleProfileOptions {
      Description = "Contract wording for the German market.",
      Industry = "Legal",
      GlossaryIds = new[] { "YOUR_GLOSSARY_ID", "YOUR_OTHER_GLOSSARY_ID" },
      StyleRuleIds = new[] { "YOUR_STYLE_ID" },
      TranslationMemoryIds = new[] { "YOUR_TRANSLATION_MEMORY_ID" },
      GlossaryPriorityOrderEnabled = true // the first glossary wins when two define the same term
    });
Console.WriteLine($"Created {linked.Name} ({linked.StyleProfileId})");
```

#### Getting and listing style profiles

`ListStyleProfilesAsync()` accepts the optional parameters `page` (0-indexed)
and `pageSize` (the API reduces values above 100 to 100).

```c#
var fetched = await client.GetStyleProfileAsync("YOUR_STYLE_PROFILE_ID");
Console.WriteLine($"{fetched.Name}: {string.Join(", ", fetched.StyleRuleIds)}");

foreach (var p in await client.ListStyleProfilesAsync(page: 0, pageSize: 100)) {
  Console.WriteLine($"{p.Name} ({p.StyleProfileId}), updated {p.UpdatedTime}");
}
```

#### Updating a style profile

`UpdateStyleProfileAsync()` changes only the fields you set. A component list
replaces the linked list of that kind in full.

```c#
var updated = await client.UpdateStyleProfileAsync(
    profile,
    name: "Legal, German v2",
    options: new StyleProfileOptions {
      Industry = "Legal",
      Description = "",              // clears the description
      StyleRuleIds = new string[0]   // unlinks all style rule lists
    });
```

#### Translating with a style profile

Set `StyleProfileId` in `TextTranslateOptions` or `DocumentTranslateOptions`, or
pass the `StyleProfile` to their constructor, to translate with all components
of the profile through one ID.

```c#
var result = await client.TranslateTextAsync(
    "The artist is here.", null, "DE", new TextTranslateOptions(profile));

await client.TranslateDocumentAsync(
    new FileInfo("input.txt"), new FileInfo("output.txt"), null, "DE",
    new DocumentTranslateOptions { StyleProfileId = "YOUR_STYLE_PROFILE_ID" });
```

#### Deleting a style profile

```c#
await client.DeleteStyleProfileAsync(profile);
```

### Translation Memories

Translation memories store and reuse previously created translations, helping to
ensure consistency across your translations and reduce the number of characters
that need to be translated.

Translation memories can be created, inspected and deleted with the methods
below. Each method that takes a translation memory accepts either its ID as a
string, or a `TranslationMemoryInfo` object as returned by
`ListTranslationMemoriesAsync()`.

#### Listing translation memories

Use `ListTranslationMemoriesAsync()` to retrieve all translation memories
associated with your account:

```c#
var translationMemories = await client.ListTranslationMemoriesAsync();
foreach (var tm in translationMemories) {
  Console.WriteLine($"{tm.Name} ({tm.TranslationMemoryId})");
}
```

#### Retrieving a translation memory

Use `GetTranslationMemoryAsync()` to retrieve a single translation memory:

```c#
var tm = await client.GetTranslationMemoryAsync("YOUR_TM_ID");
Console.WriteLine($"{tm.Name}: {tm.SourceLanguage} -> {string.Join(", ", tm.TargetLanguages)}");
Console.WriteLine($"{tm.SegmentCount} segments, last updated {tm.UpdatedTime}");
```

#### Listing the segments of a translation memory

Use `ListTranslationMemorySegmentsAsync()` to read the segments stored in a
translation memory. Pagination is cursor-based: omit `pageCursor` on the first
call, then pass the `NextPageCursor` of the previous response to fetch the next
page. A `null` next page cursor means the last page has been returned.

You can also narrow the results with `filterText` (a substring of at least two
characters, matched against both source and target text) and
`filterCaseSensitive`. Note that `SegmentCount` is the total number of segments
in the translation memory, and is not reduced by the filter.

```c#
string? pageCursor = null;
do {
  var page = await client.ListTranslationMemorySegmentsAsync(
      "YOUR_TM_ID",
      pageSize: 50,
      pageCursor: pageCursor);
  foreach (var segment in page.Segments) {
    Console.WriteLine(segment.SourceText);
    foreach (var target in segment.Targets) {
      Console.WriteLine($"  {target.TargetLanguage}: {target.TargetText}");
    }
  }

  pageCursor = page.NextPageCursor;
} while (pageCursor != null);
```

#### Importing a translation memory

Use `ImportTranslationMemoryFromFilepathAsync()` to create a new translation
memory from a TMX file. It creates the import job, uploads the file, and waits
for processing to finish. The returned job carries the ID of the new translation
memory:

```c#
var job = await client.ImportTranslationMemoryFromFilepathAsync(
    "legal.tmx",
    displayName: "Legal");
Console.WriteLine($"Imported translation memory: {job.Result!.TranslationMemoryId}");
Console.WriteLine($"Skipped segments: {job.Result!.SkippedSegmentCount}");
```

If you need more control, the three steps are also available separately. Note
that the upload URL is a pre-signed storage URL outside the DeepL API, so your
authentication key is not sent to it:

```c#
var fileInfo = new FileInfo("legal.tmx");
var created = await client.CreateTranslationMemoryImportAsync(
    fileInfo.Name,
    fileInfo.Length,
    displayName: "Legal");

using (var fileStream = fileInfo.OpenRead()) {
  await client.UploadTranslationMemoryFileAsync(created, fileStream);
}

var job = await client.WaitUntilTranslationMemoryJobDoneAsync(created.JobId);
```

#### Exporting a translation memory

Use `ExportTranslationMemoryToFilepathAsync()` to export a translation memory as
a TMX file. It creates the export job, waits for it to finish, and writes the
result to the given path:

```c#
await client.ExportTranslationMemoryToFilepathAsync("YOUR_TM_ID", "exported.tmx");
```

The individual steps are available as `CreateTranslationMemoryExportAsync()`,
`WaitUntilTranslationMemoryJobDoneAsync()` and
`DownloadTranslationMemoryExportAsync()`. If the DeepL API reuses a previously
completed export instead of starting a new one, `ReusedExisting` is `true`:

```c#
var created = await client.CreateTranslationMemoryExportAsync("YOUR_TM_ID");
Console.WriteLine($"Reused a previous export: {created.ReusedExisting}");

var job = await client.WaitUntilTranslationMemoryJobDoneAsync(created.JobId);
await client.DownloadTranslationMemoryExportAsync(job, "exported.tmx");
```

#### Checking import and export jobs

Use `GetTranslationMemoryJobAsync()` to query the status of an import or export
job without waiting for it, and `WaitUntilTranslationMemoryJobDoneAsync()` to
poll it until it finishes. Note that an import job keeps reporting
`AwaitingInput` for a while after its file has been uploaded, because the DeepL
API detects the upload asynchronously; that status is polled through like any
other non-terminal one. A job whose file is never uploaded does not finish on
its own, so pass a `CancellationToken` carrying a timeout when that is a
possibility:

```c#
var job = await client.GetTranslationMemoryJobAsync("YOUR_JOB_ID");
Console.WriteLine($"{job.Operation} job is {job.Status}");
if (job.Status == TranslationMemoryJobStatus.AwaitingInput) {
  Console.WriteLine($"Required action: {job.Result!.RequiredAction}");
}
```

#### Deleting a translation memory

Use `DeleteTranslationMemoryAsync()` to delete a translation memory by ID:

```c#
await client.DeleteTranslationMemoryAsync("YOUR_TM_ID");
```

#### Using a translation memory in translations

You can use a translation memory for text translation by setting the
`TranslationMemoryId` property in `TextTranslateOptions`. Optionally, set
`TranslationMemoryThreshold` to control the minimum similarity score for matches:

```c#
var result = await client.TranslateTextAsync(
    "Hello, world!",
    "EN",
    "DE",
    new TextTranslateOptions {
      TranslationMemoryId = "YOUR_TM_ID",
      TranslationMemoryThreshold = 80
    });
Console.WriteLine(result.Text);
```

You can also construct `TextTranslateOptions` using a `TranslationMemoryInfo`
object:

```c#
var translationMemories = await client.ListTranslationMemoriesAsync();
var tm = translationMemories[0];

var result = await client.TranslateTextAsync(
    "Hello, world!",
    "EN",
    "DE",
    new TextTranslateOptions(tm) { TranslationMemoryThreshold = 80 });
Console.WriteLine(result.Text);
```

Translation memories are also supported for document translation. Set the
`TranslationMemoryId` property (and optionally `TranslationMemoryThreshold`), or
construct `DocumentTranslateOptions` using a `TranslationMemoryInfo` object, in
the same way as for text translation:

```c#
await client.TranslateDocumentAsync(
    inFile,
    outFile,
    "EN",
    "DE",
    new DocumentTranslateOptions {
      TranslationMemoryId = "YOUR_TM_ID",
      TranslationMemoryThreshold = 80
    });
```

### Check account usage

```c#
var usage = await client.GetUsageAsync();
if (usage.AnyLimitReached) {
  Console.WriteLine("Translation limit exceeded.");
} else if (usage.Character != null) {
  Console.WriteLine($"Character usage: {usage.Character}");
} else {
  Console.WriteLine($"{usage}");
}
```

For API Pro accounts, `Usage` also contains the following fields; they are `null` for other account types:

- `ApiKeyCharacter`: the character usage of the API key used for the request. `Character` covers the whole account.
- `SpeechToTextMinutes` and `SpeechToSpeechMinutes`: speech usage in minutes, as `Usage.Detail` like `Character`.
- `StartTime` and `EndTime`: the start and end of the current billing period, as `DateTime`.
- `Products`: the usage per product, as an array of `Usage.Product` with `ProductType` (for example `"translate"`,
  `"write"`, `"speechToText"`, `"speechToSpeech"`), `BillingUnit` (`"characters"` or `"minutes"`),
  `ApiKeyUnitCount` and `AccountUnitCount`.

`AnyLimitReached` checks only `Character`, `Document` and `TeamDocument`. It does not consider these API Pro
fields; check `LimitReached` on them directly, e.g. `usage.ApiKeyCharacter?.LimitReached`.

```c#
var usage = await client.GetUsageAsync();
Console.WriteLine($"Account: {usage.Character?.Count} of {usage.Character?.Limit} characters");
if (usage.ApiKeyCharacter != null) {
  Console.WriteLine($"This API key: {usage.ApiKeyCharacter.Count} of {usage.ApiKeyCharacter.Limit} characters");
  Console.WriteLine($"Billing period: {usage.StartTime:u} to {usage.EndTime:u}");
  foreach (var product in usage.Products ?? Array.Empty<Usage.Product>()) {
    Console.WriteLine($"{product.ProductType}: {product.ApiKeyUnitCount} {product.BillingUnit} by this key, " +
                      $"{product.AccountUnitCount} {product.BillingUnit} by the account");
  }
}
```

### Listing available languages

You can request the list of languages supported by DeepL for text and documents
using the `GetSourceLanguagesAsync()` and `GetTargetLanguagesAsync()` functions.
They both return a list of `Language` objects.

The `Name` property gives the name of the language in English, and the `Code`
property gives the language code. The `SupportsFormality` property only appears
for target languages, and indicates whether the target language supports the
optional `Formality` parameter.

```c#
// Source and target languages
var sourceLanguages = await client.GetSourceLanguagesAsync();
foreach (var lang in sourceLanguages) {
  Console.WriteLine($"{lang.Name} ({lang.Code})"); // Example: "English (EN)"
}
var targetLanguages = await client.GetTargetLanguagesAsync();
foreach (var lang in targetLanguages) {
  if (lang.SupportsFormality ?? false) {
    Console.WriteLine($"{lang.Name} ({lang.Code}) supports formality");
     // Example: "German (DE) supports formality"
  }
}
```

#### Listing available glossary languages

Glossaries are supported for a subset of language pairs. To retrieve those
languages use the `GetGlossaryLanguagesAsync()` function, which returns an array
of `GlossaryLanguagePair` objects. Use the `SourceLanguage` and
`TargetLanguage` properties to check the pair of language codes supported.

```c#

// Glossary languages
var glossaryLanguages = await client.GetGlossaryLanguagesAsync();
foreach (var languagePair in glossaryLanguages) {
  Console.WriteLine($"{languagePair.SourceLanguage} to {languagePair.TargetLanguage}");
  // Example: "EN to DE", "DE to EN", etc.
}
```

You can also find the list of supported glossary language pairs in the
[API documentation][api-docs-glossary-lang-list].

Note that glossaries work for all target regional-variants: a glossary for the
target language English (`"EN"`) supports translations to both American English
(`"EN-US"`) and British English (`"EN-GB"`).

#### Listing languages and features per resource

`DeepLClient.GetLanguagesForResourceAsync()` returns the languages supported by
one DeepL API resource (`LanguageResource.TranslateText`,
`LanguageResource.TranslateDocument`, `LanguageResource.Glossary`,
`LanguageResource.Write`, …) as `LanguageSupport` objects, each with
`UsableAsSource`, `UsableAsTarget`, `Status` and a `Features` map. By default
only stable languages and features are returned; pass
`LanguageSupport.IncludeBeta` and/or `LanguageSupport.IncludeExternal` to
include more.

`DeepLClient.GetLanguageResourcesAsync()` lists the resources and their
features, and whether each feature must be supported by the source language,
the target language, or both.

```c#
var languages = await client.GetLanguagesForResourceAsync(
      LanguageResource.TranslateText,
      new[] { LanguageSupport.IncludeBeta });
foreach (var lang in languages) {
  if (lang.UsableAsTarget && lang.Supports(LanguageFeature.Formality)) {
    Console.WriteLine($"{lang.Name} ({lang.Code}) supports formality"); // Example: "German (de) supports formality"
  }
}

var resources = await client.GetLanguageResourcesAsync();
foreach (var resource in resources) {
  foreach (var feature in resource.Features.Values) {
    Console.WriteLine(
          $"{resource.Name}/{feature.Name}: source={feature.NeedsSourceSupport} target={feature.NeedsTargetSupport}");
  }
}
```

### Admin API

The Admin API lets you manage the developer keys of your organization and
retrieve usage analytics. It requires an admin API key, which you can create in
the "Admin API Keys" tab of your DeepL account. Admin keys cannot be used for
translation; developer keys cannot access the Admin API.

Create a `DeepLAdminClient` with your admin key. It uses the
`https://api.deepl.com` server by default. `DeepLAdminClientOptions` offers the
same connection settings as `DeepLClientOptions`, for example `ServerUrl`,
`Headers`, `ClientFactory` and `appInfo`.

```c#
using DeepL; // this imports the DeepL namespace. Use the code below in your main program.

var adminClient = new DeepLAdminClient("YOUR_ADMIN_KEY");
```

#### Managing developer keys

```c#
var key = await adminClient.CreateDeveloperKeyAsync("Staging Team Key");
Console.WriteLine($"Created {key.Label} ({key.KeyId})");

foreach (var developerKey in await adminClient.GetDeveloperKeysAsync()) {
  Console.WriteLine($"{developerKey.Label}: deactivated={developerKey.IsDeactivated}");
}

key = await adminClient.RenameDeveloperKeyAsync(key, "Production Team Key");

// Limits the key to 500000 characters and removes its speech-to-text limit.
// The speech-to-speech limit keeps its current value.
key = await adminClient.SetDeveloperKeyUsageLimitsAsync(
      key,
      new DeveloperKeyUsageLimitsUpdate { Characters = 500000, SpeechToTextMilliseconds = null });

// Deactivation is permanent.
await adminClient.DeactivateDeveloperKeyAsync(key);
```

`DeactivateDeveloperKeyAsync()`, `RenameDeveloperKeyAsync()` and
`SetDeveloperKeyUsageLimitsAsync()` accept either a key ID or a `DeveloperKey`
object. All functions that change a key return the updated `DeveloperKey`.

`SetDeveloperKeyUsageLimitsAsync()` changes only the limits you assign in the
`DeveloperKeyUsageLimitsUpdate`: `Characters`, `SpeechToTextMilliseconds` and
`SpeechToSpeechMilliseconds`. Limits you do not assign keep their current value,
`null` removes a limit and `0` blocks that usage.

To avoid duplicate keys, `CreateDeveloperKeyAsync()` is retried only when the
API cannot have processed the request: after HTTP 429 or a failure to resolve
the server name. Server errors, timeouts and broken connections are not
retried. If it throws, check `GetDeveloperKeysAsync()` before trying again.

#### Retrieving usage analytics

`GetUsageAnalyticsAsync()` returns an `AdminUsageReport`.

**Dates.** Usage is counted per UTC calendar day, and both the start and the
end day are included. Pass `"yyyy-MM-dd"` strings or `DateTime` values. For a
`DateTime` the SDK sends the UTC date of that moment: `Local` values are
converted to UTC and `Unspecified` values are treated as UTC, so local midnight
on 2026-10-20 in UTC+2 is sent as `2026-10-19`. The end date must be after the
start date, so a report covers at least two days, and at most 366 days after
it. The API rejects other ranges, which throws a `DeepLException`.
The same rules apply to `GetCustomTagUsageAnalyticsAsync()`.

Use
`groupBy` to break the usage down by key (`AdminUsageGroupBy.Key`, see
`KeyUsages`) or by key and day (`AdminUsageGroupBy.KeyAndDay`, see
`KeyAndDayUsages`, whose `KeyUsage` entries also contain the `UsageDate`).
`report.GroupBy` is the raw grouping string returned by the API (`key` or
`keyAndDay` at the time of writing). The usage breakdowns contain the
characters per product, `SpeechToTextMinutes` and `SpeechToSpeechMinutes`.

```c#
var report = await adminClient.GetUsageAnalyticsAsync(
      "2026-06-01",
      "2026-07-01",
      AdminUsageGroupBy.Key);
Console.WriteLine($"Total characters: {report.TotalUsage?.TotalCharacters}");
foreach (var keyUsage in report.KeyUsages) {
  Console.WriteLine($"{keyUsage.ApiKeyLabel}: {keyUsage.Usage.TotalCharacters}");
}
```

`GetCustomTagUsageAnalyticsAsync()` returns a `CustomTagUsageReport` with the
usage per custom tag. Use `aggregateBy` to choose between the whole period
(`CustomTagUsageAggregateBy.Period`, the default) and per-day usage
(`CustomTagUsageAggregateBy.Day`). The results are paginated: pass the
`NextPage` value of a report as `page` to get the next page.

```c#
int? page = null;
do {
  var report = await adminClient.GetCustomTagUsageAnalyticsAsync(
        new DateTime(2026, 6, 1),
        new DateTime(2026, 7, 1),
        page: page);
  foreach (var tagUsage in report.Usage) {
    Console.WriteLine($"{tagUsage.CustomTag}: {tagUsage.Breakdown.TotalCharacters}");
  }
  page = report.NextPage;
} while (page != null);
```

### Exceptions

All library functions may raise `DeepLException` or one of its subclasses. If
invalid arguments are provided, they may raise the standard exceptions
`ArgumentException`.

### Writing a Plugin

If you use this library in an application, please identify the application with
`DeepLClientOptions.appInfo`, which needs the name and version of the app:

```c#
var options = new DeepLClientOptions {
  appInfo =  new AppInfo { AppName = "my-dotnet-test-app", AppVersion = "1.2.3"}
};
var client = new DeepLClient(AuthKey, options);
```

This information is passed along when the library makes calls to the DeepL API.
Both name and version are required. Please note that setting the `User-Agent` header
via `DeepLClientOptions.Headers` will override this setting, if you need to use this,
please manually identify your Application in the `User-Agent` header.

### Configuration

The `DeepLClient` constructor accepts `DeepLClientOptions` as a second argument,
for example:

```c#
var options = new DeepLClientOptions {
      MaximumNetworkRetries = 5,
      PerRetryConnectionTimeout = TimeSpan.FromSeconds(10),
};
var client = new DeepLClient(authKey, options);
```

See the `DeepLClientOptions` class for details about the available options.

#### Proxy configuration

To use the library with a proxy, override the `ClientFactory` with a function returning a custom `HttpClient`:

```c#
var proxyUrl = "http://localhost:3001";
var handler = new System.Net.Http.HttpClientHandler {
      Proxy = new System.Net.WebProxy(proxyUrl), UseProxy = true,
};
var options = new DeepLClientOptions {
      ClientFactory = () => new HttpClientAndDisposeFlag {
            HttpClient = new HttpClient(handler), DisposeClient = true,
      }
};
var client = new DeepLClient(authKey, options);
```

#### Anonymous platform information

By default, we send some basic information about the platform the client library is running
on with each request,
see [here for an explanation](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent).
This data is completely anonymous and only used to improve our product, not track any
individual users. If you do not wish to send this data, you can opt-out when creating
your `DeepLClient` object by setting the `sendPlatformInfo` flag in the
`DeepLClientOptions` to `false` like so:

```c#
var options = new DeepLClientOptions { sendPlatformInfo = false };
var client = new DeepLClient(authKey, options);
```

## Issues

If you experience problems using the library, or would like to request a new feature, please open an
[issue][issues].

## Development

We welcome Pull Requests, please read the [contributing guidelines](CONTRIBUTING.md).

### Tests

The tests are split into two groups in the `DeepLTests` project, selected by namespace:

- `dotnet test --filter "FullyQualifiedName~DeepLTests.Offline."` runs `DeepLTests/Offline`. These tests intercept all
  HTTP traffic with [MockHttp][mockhttp], so they need no network and no authentication key. They check the requests
  that the library sends for every option, and how it handles errors, retries and polling.
- `dotnet test --filter "FullyQualifiedName~DeepLTests.RealApi."` runs `DeepLTests/RealApi`. These tests send one
  request per API endpoint to the DeepL API, using the authentication key and server URL defined by the
  `DEEPL_AUTH_KEY` and `DEEPL_SERVER_URL` environment variables. They create and delete glossaries, style rules and
  translation memories named `sdktest-dotnet-...`. The `DeepLAdminClient` tests use the admin key in
  `DEEPL_ADMIN_KEY` instead; they create a developer key named `sdktest-dotnet-...`, rename it, set its limits and deactivate it.

`dotnet test` runs both groups. Be aware that the real-API tests make DeepL API requests that contribute toward your
API usage.


[api-docs-context-param]: https://www.deepl.com/docs-api/translating-text/?utm_source=github&utm_medium=github-dotnet-readme

[api-docs-csv-format]: https://www.deepl.com/docs-api/managing-glossaries/supported-glossary-formats/?utm_source=github&utm_medium=github-dotnet-readme

[api-docs-glossary-lang-list]: https://www.deepl.com/docs-api/managing-glossaries/?utm_source=github&utm_medium=github-dotnet-readme

[api-docs-outputformat-param]: https://www.deepl.com/docs-api/documents/format-conversions/?utm_source=github&utm_medium=github-dotnet-readme

[api-docs-usage-limits]: https://developers.deepl.com/docs/resources/usage-limits

[api-docs-xml-handling]: https://www.deepl.com/docs-api/handling-xml/?utm_source=github&utm_medium=github-dotnet-readme

[issues]: https://www.github.com/DeepLcom/deepl-dotnet/issues

[mockhttp]: https://github.com/richardszalay/mockhttp
