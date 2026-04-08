// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

namespace DeepL {
  /// <summary>
  ///   Options to control Voice API session creation. These options are provided to
  ///   <see cref="DeepLClient.CreateVoiceSessionAsync" />.
  /// </summary>
  public sealed class VoiceSessionOptions {
    /// <summary>Initializes a new <see cref="VoiceSessionOptions" /> object.</summary>
    public VoiceSessionOptions() { }

    /// <summary>
    ///   The audio format for streaming, which specifies container, codec, and encoding parameters.
    ///   Use constants from <see cref="SourceMediaContentType" /> for supported values. Required.
    /// </summary>
    public string SourceMediaContentType { get; set; } = DeepL.SourceMediaContentType.Auto;

    /// <summary>
    ///   Message encoding format for WebSocket communication. If <c>null</c>, the API default is used
    ///   (currently <see cref="VoiceMessageFormat.Json" />).
    /// </summary>
    public VoiceMessageFormat? MessageFormat { get; set; }

    /// <summary>
    ///   The source language of the audio stream, or null for auto-detection.
    ///   Must be a supported Voice API source language complying with IETF BCP 47 language tags.
    /// </summary>
    public string? SourceLanguage { get; set; }

    /// <summary>
    ///   Controls how the <see cref="SourceLanguage" /> value is used.
    ///   Defaults to <see cref="DeepL.SourceLanguageMode.Auto" /> if not specified.
    /// </summary>
    public SourceLanguageMode? SourceLanguageMode { get; set; }

    /// <summary>
    ///   List of target languages for translation. The stream will emit translations for each language.
    ///   Maximum 5 target languages per session. Language identifiers must comply with IETF BCP 47.
    /// </summary>
    public string[] TargetLanguages { get; set; } = System.Array.Empty<string>();

    /// <summary>
    ///   List of target languages for which to generate synthesized audio. This feature is in closed beta.
    ///   Languages specified here will automatically be added to <see cref="TargetLanguages" /> if not already present.
    ///   Maximum 5 target media languages per session.
    /// </summary>
    public string[]? TargetMediaLanguages { get; set; }

    /// <summary>
    ///   The audio format for synthesized target media streaming. This feature is in closed beta.
    ///   Defaults to <c>"audio/webm;codecs=opus"</c> if not specified.
    /// </summary>
    public string? TargetMediaContentType { get; set; }

    /// <summary>
    ///   Target audio voice selection for synthesized speech. This feature is in closed beta.
    /// </summary>
    public TargetMediaVoice? TargetMediaVoice { get; set; }

    /// <summary>A glossary ID to use for translation.</summary>
    public string? GlossaryId { get; set; }

    /// <summary>
    ///   Sets whether the translated text should lean towards formal or informal language.
    ///   Possible values: <c>"default"</c>, <c>"formal"</c>, <c>"more"</c>, <c>"informal"</c>, <c>"less"</c>.
    /// </summary>
    public string? Formality { get; set; }
  }
}
