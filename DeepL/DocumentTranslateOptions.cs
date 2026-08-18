// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;
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
      GlossaryId = glossary.GlossaryId;
    }

    /// <summary>Initializes a new <see cref="DocumentTranslateOptions" /> object including the given v3 glossary.</summary>
    /// <param name="glossary">Glossary to use in translation.</param>
    public DocumentTranslateOptions(MultilingualGlossaryInfo glossary) : this() {
      GlossaryId = glossary.GlossaryId;
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
      StyleId = styleRule.StyleId;
    }

    /// <summary>Initializes a new <see cref="DocumentTranslateOptions" /> object including the given translation memory.</summary>
    /// <param name="translationMemory">Translation memory to use in translation.</param>
    public DocumentTranslateOptions(TranslationMemoryInfo translationMemory) : this() {
      TranslationMemoryId = translationMemory.TranslationMemoryId;
    }

    /// <summary>Controls whether translations should lean toward formal or informal language.</summary>
    /// This option is only applicable for target languages that support the formality option.
    /// <seealso cref="TargetLanguage.SupportsFormality" />
    public Formality Formality { get; set; } = Formality.Default;

    /// <summary>Specifies the ID of a glossary to use with the translation.</summary>
    /// <remarks>Cannot be used together with <see cref="GlossaryIds" />.</remarks>
    public string? GlossaryId { get; set; }

    /// <summary>
    ///   Specifies the IDs of multiple glossaries to use with the translation (maximum of 5). Glossaries are applied in
    ///   order, with the first matching term taking precedence. Using this option requires the source language to be
    ///   specified, and it cannot be combined with <see cref="GlossaryId" />.
    /// </summary>
    public List<string> GlossaryIds { get; set; } = new List<string>();

    /// <summary>Specifies the ID of a style rule to use with the translation.</summary>
    public string? StyleId { get; set; }

    /// <summary>Specifies the ID of a translation memory to use with the translation.</summary>
    public string? TranslationMemoryId { get; set; }

    /// <summary>Specifies the minimum similarity threshold for translation memory matches (0-100).</summary>
    public int? TranslationMemoryThreshold { get; set; }

    /// <summary> Controls whether to use Document Minification for translation, if available.</summary>
    public bool EnableDocumentMinification { get; set; }

    /// <summary>
    ///   File extension of desired format of translated file, for example: docx. If unspecified, by default the
    ///   translated file will be in the same format as the input file.
    /// </summary>
    public string? OutputFormat { get; set; }
  }
}
