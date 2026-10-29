// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
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
      Glossary = glossary.GlossaryId;
    }

    /// <summary>Initializes a new <see cref="TextTranslateOptions" /> object including the given multilingual glossary.</summary>
    /// <param name="glossary">Glossary to use in translation.</param>
    public TextTranslateOptions(MultilingualGlossaryInfo glossary) : this() {
      Glossary = glossary.GlossaryId;
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
      StyleRule = styleRule.StyleId;
    }

    /// <summary>Initializes a new <see cref="TextTranslateOptions" /> object including the given style profile.</summary>
    /// <param name="styleProfile">Style profile to use in translation.</param>
    public TextTranslateOptions(StyleProfile styleProfile) : this() {
      StyleProfile = styleProfile.StyleProfileId;
    }

    /// <summary>Initializes a new <see cref="TextTranslateOptions" /> object including the given translation memory.</summary>
    /// <param name="translationMemory">Translation memory to use in translation.</param>
    public TextTranslateOptions(TranslationMemoryInfo translationMemory) : this() {
      TranslationMemory = translationMemory.TranslationMemoryId;
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
    /// <seealso cref="FormalityValue" />
    public Formality Formality { get; set; } = Formality.Default;

    /// <summary>
    ///   Formality as the string sent to the API (for example <c>"prefer_more"</c>), for values that
    ///   <see cref="DeepL.Formality" /> does not have. If set, it is sent unchanged instead of <see cref="Formality" />.
    /// </summary>
    public string? FormalityValue { get; set; }

    /// <summary>
    ///   Specifies a glossary to use with the translation: its ID, or a <see cref="GlossaryInfo" /> or
    ///   <see cref="MultilingualGlossaryInfo" />, which converts to its ID.
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
    ///   Glossaries are applied in order, with the first matching term taking precedence. Cannot be combined with
    ///   <see cref="Glossary" />.
    /// </summary>
    /// <remarks>
    ///   If the source language is omitted, DeepL detects it and every listed glossary must contain a
    ///   dictionary for the detected language pair.
    /// </remarks>
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
    /// <remarks>The API rejects combining it with glossaries, style rules, translation memories or custom instructions.</remarks>
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
    /// <seealso cref="SplitSentences" />
    public SentenceSplittingMode SentenceSplittingMode { get; set; } = SentenceSplittingMode.All;

    /// <summary>
    ///   Sentence splitting as the string sent to the API as <c>split_sentences</c> (for example <c>"nonewlines"</c>),
    ///   for values that <see cref="DeepL.SentenceSplittingMode" /> does not have. If set, it is sent unchanged instead of
    ///   <see cref="SentenceSplittingMode" />.
    /// </summary>
    public string? SplitSentences { get; set; }

    /// <summary>Specifies a list of XML tags that should be used to split text into sentences.</summary>
    public List<string> SplittingTags { get; } = new List<string>();

    /// <summary>
    ///   Type of tags to parse before translation: <see cref="TagHandlingMode.Html" />, <see cref="TagHandlingMode.Xml" />
    ///   or any other string, which is sent unchanged.
    /// </summary>
    public string? TagHandling { get; set; }

    /// <summary>
    ///   Version of tag handling algorithm to use: <see cref="DeepL.TagHandlingVersion.V1" />,
    ///   <see cref="DeepL.TagHandlingVersion.V2" /> or any other string, which is sent unchanged.
    /// </summary>
    public string? TagHandlingVersion { get; set; }

    /// <summary>
    ///   Sets whether the translation engine should use a newer model type that
    ///   offers higher quality translations at the cost of translation time.
    /// </summary>
    /// <seealso cref="ModelType" />
    /// <seealso cref="ModelTypeValue" />
    public ModelType? ModelType { get; set; }

    /// <summary>
    ///   Model type as the string sent to the API (for example <c>"prefer_quality_optimized"</c>), for values that
    ///   <see cref="DeepL.ModelType" /> does not have. If set, it is sent unchanged instead of <see cref="ModelType" />.
    /// </summary>
    public string? ModelTypeValue { get; set; }

    /// <summary>
    ///   A list of custom instructions to guide the translation.
    ///   Maximum of 10 instructions, each with a maximum length of 300 characters.
    /// </summary>
    public List<string> CustomInstructions { get; } = new List<string>();
  }
}
