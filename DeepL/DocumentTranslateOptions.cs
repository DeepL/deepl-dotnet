// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Options to control document translation behaviour. These options may be provided to <see cref="Translator" />
  ///   document translate functions.
  /// </summary>
  public sealed class DocumentTranslateOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="DocumentTranslateOptions" /> object.</summary>
    public DocumentTranslateOptions() { }

    /// <summary>Initializes a new <see cref="DocumentTranslateOptions" /> object including the given v2 glossary.</summary>
    /// <param name="glossary">Glossary to use in translation.</param>
    public DocumentTranslateOptions(GlossaryInfo glossary) : this() {
      Glossary = glossary.GlossaryId;
    }

    /// <summary>Initializes a new <see cref="DocumentTranslateOptions" /> object including the given v3 glossary.</summary>
    /// <param name="glossary">Glossary to use in translation.</param>
    public DocumentTranslateOptions(MultilingualGlossaryInfo glossary) : this() {
      Glossary = glossary.GlossaryId;
    }

    /// <summary>
    ///   Initializes a new <see cref="DocumentTranslateOptions" /> object including the given v2 glossaries. Multiple
    ///   glossaries are applied in order, with the first matching term taking precedence.
    /// </summary>
    /// <param name="glossaries">Glossaries to use in translation (maximum of 5).</param>
    public DocumentTranslateOptions(IEnumerable<GlossaryInfo> glossaries) : this() {
      foreach (var glossary in glossaries) {
        GlossaryIds.Add(glossary.GlossaryId);
      }
    }

    /// <summary>
    ///   Initializes a new <see cref="DocumentTranslateOptions" /> object including the given multilingual glossaries.
    ///   Multiple glossaries are applied in order, with the first matching term taking precedence.
    /// </summary>
    /// <param name="glossaries">Glossaries to use in translation (maximum of 5).</param>
    public DocumentTranslateOptions(IEnumerable<MultilingualGlossaryInfo> glossaries) : this() {
      foreach (var glossary in glossaries) {
        GlossaryIds.Add(glossary.GlossaryId);
      }
    }

    /// <summary>Initializes a new <see cref="DocumentTranslateOptions" /> object including the given style rule.</summary>
    /// <param name="styleRule">Style rule to use in translation.</param>
    public DocumentTranslateOptions(StyleRuleInfo styleRule) : this() {
      StyleRule = styleRule.StyleId;
    }

    /// <summary>Initializes a new <see cref="DocumentTranslateOptions" /> object including the given style profile.</summary>
    /// <param name="styleProfile">Style profile to use in translation.</param>
    public DocumentTranslateOptions(StyleProfile styleProfile) : this() {
      StyleProfile = styleProfile.StyleProfileId;
    }

    /// <summary>Initializes a new <see cref="DocumentTranslateOptions" /> object including the given translation memory.</summary>
    /// <param name="translationMemory">Translation memory to use in translation.</param>
    public DocumentTranslateOptions(TranslationMemoryInfo translationMemory) : this() {
      TranslationMemory = translationMemory.TranslationMemoryId;
    }

    /// <summary>Controls whether translations should lean toward formal or informal language.</summary>
    /// This option is only applicable for target languages that support the formality option.
    /// <seealso cref="TargetLanguage.SupportsFormality" />
    /// <seealso cref="FormalityValue" />
    public Formality Formality { get; set; } = Formality.Default;

    /// <summary>
    ///   Formality as the string sent to the API (for example <c>"prefer_more"</c>), for values that
    ///   <see cref="DeepL.Formality" /> does not have. If set, it is sent unchanged instead of <see cref="Formality" />.
    /// </summary>
    public string? FormalityValue { get; set; }

    /// <summary>
    ///   Specifies a glossary to use with the translation: its ID, or a <see cref="GlossaryInfo" /> or
    ///   <see cref="MultilingualGlossaryInfo" />, which converts to its ID. Using this option requires the source
    ///   language to be specified.
    /// </summary>
    /// <remarks>Cannot be used together with <see cref="GlossaryIds" />.</remarks>
    public string? Glossary { get; set; }

    /// <summary>Specifies the ID of a glossary to use with the translation.</summary>
    /// <remarks>Cannot be used together with <see cref="GlossaryIds" />.</remarks>
    [Obsolete("Use Glossary instead. Will be removed in the next major version.")]
    public string? GlossaryId { get => Glossary; set => Glossary = value; }

    /// <summary>
    ///   Specifies multiple glossaries to use with the translation (maximum of 5): their IDs, or
    ///   <see cref="GlossaryInfo" /> and <see cref="MultilingualGlossaryInfo" /> objects, which convert to their IDs.
    ///   Glossaries are applied in order, with the first matching term taking precedence. Using this option requires the
    ///   source language to be specified, and it cannot be combined with <see cref="Glossary" />.
    /// </summary>
    public List<string> GlossaryIds { get; set; } = new List<string>();

    /// <summary>
    ///   Specifies a style rule to use with the translation: its ID, or a <see cref="StyleRuleInfo" />, which
    ///   converts to its ID.
    /// </summary>
    public string? StyleRule { get; set; }

    /// <summary>Specifies the ID of a style rule to use with the translation.</summary>
    [Obsolete("Use StyleRule instead. Will be removed in the next major version.")]
    public string? StyleId { get => StyleRule; set => StyleRule = value; }

    /// <summary>
    ///   Specifies a style profile to use with the translation: its ID, or a <see cref="Model.StyleProfile" />, which
    ///   converts to its ID. The style profile applies its glossaries, style rules and translation memories through
    ///   one ID.
    /// </summary>
    /// <remarks>The API rejects combining it with glossaries, style rules or translation memories.</remarks>
    public string? StyleProfile { get; set; }

    /// <summary>
    ///   Specifies a translation memory to use with the translation: its ID, or a <see cref="TranslationMemoryInfo" />,
    ///   which converts to its ID.
    /// </summary>
    public string? TranslationMemory { get; set; }

    /// <summary>Specifies the ID of a translation memory to use with the translation.</summary>
    [Obsolete("Use TranslationMemory instead. Will be removed in the next major version.")]
    public string? TranslationMemoryId { get => TranslationMemory; set => TranslationMemory = value; }

    /// <summary>Specifies the minimum similarity threshold for translation memory matches (0-100).</summary>
    public int? TranslationMemoryThreshold { get; set; }

    /// <summary> Controls whether to use Document Minification for translation, if available.</summary>
    [Obsolete("Document minification is deprecated and will be removed in the next major version. The DeepL API accepts larger documents; see the usage limits.")]
    public bool EnableDocumentMinification { get; set; }

    /// <summary>
    ///   File extension of desired format of translated file, for example: docx. If unspecified, by default the
    ///   translated file will be in the same format as the input file.
    /// </summary>
    public string? OutputFormat { get; set; }

    /// <summary>
    ///   When <c>true</c>, adds a "Translated by DeepL" watermark to the translated document. Only supported for
    ///   <c>docx</c> and <c>pdf</c> output; ignored by the API for other formats. If unset, the parameter is not sent.
    /// </summary>
    public bool? EnableWatermark { get; set; }

    /// <summary>
    ///   Name of the input file sent with the upload; the file extension determines the file type. Required by the
    ///   <see cref="Stream" /> overloads without an <c>inputFileName</c> parameter. The <see cref="FileInfo" /> overloads
    ///   use it instead of the file's name if set; the overloads with an <c>inputFileName</c> parameter ignore it.
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    ///   Client-side limit for the wait between upload and download in
    ///   <see cref="Translator.TranslateDocumentAsync(Stream, Stream, string?, string, DocumentTranslateOptions?, System.Threading.CancellationToken)" />
    ///   and its overloads. If the document is not translated in time, a <see cref="DocumentTranslationException" />
    ///   is thrown, carrying the <see cref="DocumentHandle" /> so the translation can be resumed. If unset, the wait is
    ///   not limited. Not sent to the API.
    /// </summary>
    public TimeSpan? Timeout { get; set; }
  }
}
