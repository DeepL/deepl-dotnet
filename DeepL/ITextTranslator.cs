// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>Translates text with the DeepL API. Implemented by <see cref="DeepLClient" />.</summary>
  public interface ITextTranslator : IDisposable {
    /// <summary>Translate specified texts from source language into target language.</summary>
    /// <param name="texts">Texts to translate; must not be empty.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">Extra <see cref="TextTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Texts translated into specified target language.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TextResult[]> TranslateTextAsync(
          IEnumerable<string> texts,
          string? sourceLanguageCode,
          string targetLanguageCode,
          TextTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Translate specified text from source language into target language.</summary>
    /// <param name="text">Text to translate; must not be empty.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options"><see cref="TextTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Text translated into specified target language.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TextResult> TranslateTextAsync(
          string text,
          string? sourceLanguageCode,
          string targetLanguageCode,
          TextTranslateOptions? options = null,
          CancellationToken cancellationToken = default);
  }
}
