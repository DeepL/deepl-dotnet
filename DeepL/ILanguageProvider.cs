// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>Retrieves the languages supported by the DeepL API for translation and glossaries. Implemented by <see cref="DeepLClient" />.</summary>
  public interface ILanguageProvider : IDisposable {
    /// <summary>Retrieves the list of supported translation source languages.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="Language" /> objects representing the available translation source languages.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SourceLanguage[]> GetSourceLanguagesAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves the list of supported translation target languages.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="Language" /> objects representing the available translation target languages.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TargetLanguage[]> GetTargetLanguagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves the list of supported glossary language pairs. When creating glossaries, the source and target
    ///   language pair must match one of the available language pairs.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="GlossaryLanguagePair" /> objects representing the available glossary language pairs.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryLanguagePair[]> GetGlossaryLanguagesAsync(CancellationToken cancellationToken = default);
  }
}
