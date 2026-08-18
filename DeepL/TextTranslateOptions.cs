// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Options to control text translation behaviour. These options may be provided to <see cref="Translator" /> text
  ///   translate functions.
  /// </summary>
  public sealed class TextTranslateOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="TextTranslateOptions" /> object.</summary>
    public TextTranslateOptions() { }

    /// <summary>Initializes a new <see cref="TextTranslateOptions" /> object including the given v2 glossary.</summary>
    /// <param name="glossary">Glossary to use in translation.</param>
    public TextTranslateOptions(GlossaryInfo glossary) : this() {
      GlossaryId = glossary.GlossaryId;
    }

    /// <summary>Initializes a new <see cref="TextTranslateOptions" /> object including the given multilingual glossary.</summary>
    /// <param name="glossary">Glossary to use in translation.</param>
    public TextTranslateOptions(MultilingualGlossaryInfo glossary) : this() {
      GlossaryId = glossary.GlossaryId;
    }

    /// <summary>
    ///   Initializes a new <see cref="TextTranslateOptions" /> object including the given v2 glossaries. Multiple
    ///   glossaries are applied in order, with the first matching term taking precedence.
    /// </summary>
    /// <param name="glossaries">Glossaries to use in translation (maximum of 5).</param>
    public TextTranslateOptions(IEnumerable<GlossaryInfo> glossaries) : this() {
      foreach (var glossary in glossaries) {
        GlossaryIds.Add(glossary.GlossaryId);
      }
    }

    /// <summary>
    ///   Initializes a new <see cref="TextTranslateOptions" /> object including the given multilingual glossaries.
    ///   Multiple glossaries are applied in order, with the first matching term taking precedence.
    /// </summary>
    /// <param name="glossaries">Glossaries to use in translation (maximum of 5).</param>
    public TextTranslateOptions(IEnumerable<MultilingualGlossaryInfo> glossaries) : this() {
      foreach (var glossary in glossaries) {
        GlossaryIds.Add(glossary.GlossaryId);
      }
    }

    /// <summary>Initializes a new <see cref="TextTranslateOptions" /> object including the given style rule.</summary>
    /// <param name="styleRule">Style rule to use in translation.</param>
    public TextTranslateOptions(StyleRuleInfo styleRule) : this() {
      StyleId = styleRule.StyleId;
    }

    /// <summary>Initializes a new <see cref="TextTranslateOptions" /> object including the given translation memory.</summary>
    /// <param name="translationMemory">Translation memory to use in translation.</param>
    public TextTranslateOptions(TranslationMemoryInfo translationMemory) : this() {
      TranslationMemoryId = translationMemory.TranslationMemoryId;
    }

    /// <summary>
    ///   Specifies additional context to influence translations, that is not translated itself.
    ///   Characters in the `context` parameter are not counted toward billing.
    ///   See the API documentation for more information and example usage.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>Controls whether translations should lean toward formal or informal language.</summary>
    /// This option is only applicable for target languages that support the formality option.
    /// <seealso cref="TargetLanguage.SupportsFormality" />
    public Formality Formality { get; set; } = Formality.Default;

    /// <summary>Specifies the ID of a glossary to use with the translation.</summary>
    /// <remarks>Cannot be used together with <see cref="GlossaryIds" />.</remarks>
    public string? GlossaryId { get; set; }

    /// <summary>
    ///   Specifies the IDs of multiple glossaries to use with the translation (maximum of 5). Glossaries are applied in
    ///   order, with the first matching term taking precedence. Cannot be combined with
    ///   <see cref="GlossaryId" />.
    /// </summary>
    /// <remarks>
    ///   If the source language is omitted, DeepL detects it and every listed glossary must contain a
    ///   dictionary for the detected language pair.
    /// </remarks>
    public List<string> GlossaryIds { get; set; } = new List<string>();

    /// <summary>Specifies the ID of a style rule to use with the translation.</summary>
    public string? StyleId { get; set; }

    /// <summary>Specifies the ID of a translation memory to use with the translation.</summary>
    public string? TranslationMemoryId { get; set; }

    /// <summary>Specifies the minimum similarity threshold for translation memory matches (0-100).</summary>
    public int? TranslationMemoryThreshold { get; set; }

    /// <summary>Specifies a list of XML tags containing content that should not be translated.</summary>
    public List<string> IgnoreTags { get; } = new List<string>();

    /// <summary>Specifies a list of XML tags that should not be used to split text into sentences.</summary>
    public List<string> NonSplittingTags { get; } = new List<string>();

    /// <summary>Set to <c>false</c> to disable automatic tag detection, default is <c>true</c>.</summary>
    public bool OutlineDetection { get; set; } = true;

    /// <summary>
    ///   Set to <c>true</c> to prevent the translation engine from correcting some formatting aspects, and instead leave the
    ///   formatting unchanged, default is <c>false</c>.
    /// </summary>
    public bool PreserveFormatting { get; set; }

    /// <summary>Specifies how input translation text should be split into sentences.</summary>
    /// <seealso cref="SentenceSplittingMode" />
    public SentenceSplittingMode SentenceSplittingMode { get; set; } = SentenceSplittingMode.All;

    /// <summary>Specifies a list of XML tags that should be used to split text into sentences.</summary>
    public List<string> SplittingTags { get; } = new List<string>();

    /// <summary>Type of tags to parse before translation, only "xml" and "html" are currently available.</summary>
    public string? TagHandling { get; set; }

    /// <summary>Version of tag handling algorithm to use.</summary>
    public string? TagHandlingVersion { get; set; }

    /// <summary>
    ///   Sets whether the translation engine should use a newer model type that
    ///   offers higher quality translations at the cost of translation time.
    /// </summary>
    /// <seealso cref="ModelType" />
    public ModelType? ModelType { get; set; }

    /// <summary>
    ///   A list of custom instructions to guide the translation.
    ///   Maximum of 10 instructions, each with a maximum length of 300 characters.
    /// </summary>
    public List<string> CustomInstructions { get; } = new List<string>();
  }
}
