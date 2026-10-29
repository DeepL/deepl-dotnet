// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;

namespace DeepL.Model {
  /// <summary>
  ///   Information about a language supported by the DeepL API for a given resource, as returned by
  ///   <see cref="DeepLClient.GetLanguagesForResourceAsync(string, GetLanguagesForResourceOptions?, System.Threading.CancellationToken)" />.
  /// </summary>
  public sealed class LanguageSupport {
    /// <summary>Include value: also return languages and features in beta.</summary>
    public const string IncludeBeta = "beta";

    /// <summary>Include value: also return features that rely on third-party service providers.</summary>
    public const string IncludeExternal = "external";

    /// <summary>Initializes a new instance of <see cref="LanguageSupport" />.</summary>
    /// <param name="code">BCP-47 language code, for example "de" or "en-US".</param>
    /// <param name="name">Name of the language in English.</param>
    /// <param name="status">Availability status of the language, for example "stable" or "beta".</param>
    /// <param name="usableAsSource"><c>true</c> if the language can be used as a source language.</param>
    /// <param name="usableAsTarget"><c>true</c> if the language can be used as a target language.</param>
    /// <param name="features">Map of feature name to <see cref="LanguageFeature" />.</param>
    public LanguageSupport(
          string code,
          string name,
          string status,
          bool usableAsSource,
          bool usableAsTarget,
          IReadOnlyDictionary<string, LanguageFeature> features) {
      Code = code;
      Name = name;
      Status = status;
      UsableAsSource = usableAsSource;
      UsableAsTarget = usableAsTarget;
      Features = features;
    }

    /// <summary>BCP-47 language code, for example "de", "en-US", "pt-BR" or "zh-Hans".</summary>
    public string Code { get; }

    /// <summary>Name of the language in English.</summary>
    public string Name { get; }

    /// <summary>Availability status of the language, for example "stable", "beta" or "early_access".</summary>
    public string Status { get; }

    /// <summary><c>true</c> if the language can be used as a source language for the requested resource.</summary>
    public bool UsableAsSource { get; }

    /// <summary><c>true</c> if the language can be used as a target language for the requested resource.</summary>
    public bool UsableAsTarget { get; }

    /// <summary>
    ///   Map of feature name to <see cref="LanguageFeature" /> describing the features this language supports for the
    ///   requested resource. Empty if no optional features are supported.
    /// </summary>
    public IReadOnlyDictionary<string, LanguageFeature> Features { get; }

    /// <summary>Returns <c>true</c> if this language supports the given feature for the requested resource.</summary>
    /// <param name="feature">Feature name, for example <see cref="LanguageFeature.Formality" />.</param>
    public bool Supports(string feature) => Features.ContainsKey(feature);

    /// <summary>Returns a string containing the name and code of the language.</summary>
    public override string ToString() => $"{Name} ({Code})";
  }

  /// <summary>
  ///   Support information for a single feature of a language, as returned by
  ///   <see cref="DeepLClient.GetLanguagesForResourceAsync(string, GetLanguagesForResourceOptions?, System.Threading.CancellationToken)" />.
  /// </summary>
  public sealed class LanguageFeature {
    /// <summary>Feature: automatic source-language detection.</summary>
    public const string AutoDetection = "auto_detection";

    /// <summary>Feature: formality control.</summary>
    public const string Formality = "formality";

    /// <summary>Feature: glossary support.</summary>
    public const string Glossary = "glossary";

    /// <summary>Feature: style rules.</summary>
    public const string StyleRules = "style_rules";

    /// <summary>Feature: tag handling.</summary>
    public const string TagHandling = "tag_handling";

    /// <summary>Feature: translation memory.</summary>
    public const string TranslationMemory = "translation_memory";

    /// <summary>Feature: tone control (write).</summary>
    public const string Tone = "tone";

    /// <summary>Feature: writing style (write).</summary>
    public const string WritingStyle = "writing_style";

    /// <summary>Feature: transcription (voice).</summary>
    public const string Transcription = "transcription";

    /// <summary>Feature: translated speech (voice).</summary>
    public const string TranslatedSpeech = "translated_speech";

    /// <summary>Feature: spoken terms (voice).</summary>
    public const string SpokenTerms = "spoken_terms";

    /// <summary>Initializes a new instance of <see cref="LanguageFeature" />.</summary>
    /// <param name="status">Availability status of the feature, for example "stable" or "beta".</param>
    public LanguageFeature(string status) {
      Status = status;
    }

    /// <summary>Availability status of the feature for this language, for example "stable", "beta" or "early_access".</summary>
    public string Status { get; }
  }
}
