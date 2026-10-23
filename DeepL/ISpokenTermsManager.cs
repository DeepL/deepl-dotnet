// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>Manages Spoken Terms collections, which improve speech recognition in the Voice API.</summary>
  public interface ISpokenTermsManager {
    /// <summary>Creates a Spoken Terms collection with one term list per language.</summary>
    /// <param name="name">Name of the Spoken Terms collection.</param>
    /// <param name="termLists">The terms of each language, as <see cref="SpokenTermsEntries" /> objects.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="SpokenTerms" /> object with details about the newly created collection.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SpokenTerms> CreateSpokenTermsAsync(
          string name,
          IEnumerable<SpokenTermsEntries> termLists,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves all Spoken Terms collections of the account, without their terms.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="SpokenTerms" /> objects.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SpokenTerms[]> ListSpokenTermsAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves the details of a Spoken Terms collection, without its terms.</summary>
    /// <param name="spokenTermsId">ID of the Spoken Terms collection.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="SpokenTerms" /> object with details about the collection.</returns>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SpokenTerms> GetSpokenTermsAsync(
          string spokenTermsId,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves the details of a Spoken Terms collection, without its terms.</summary>
    /// <param name="spokenTerms">The Spoken Terms collection.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="SpokenTerms" /> object with details about the collection.</returns>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SpokenTerms> GetSpokenTermsAsync(
          SpokenTerms spokenTerms,
          CancellationToken cancellationToken = default);

    /// <summary>Renames a Spoken Terms collection and/or merges terms into its term lists.</summary>
    /// <remarks>
    ///   The given terms are merged into the existing list of that language. To replace a list instead, use
    ///   <see cref="ReplaceSpokenTermsListAsync(string, SpokenTermsEntries, CancellationToken)" />.
    /// </remarks>
    /// <param name="spokenTermsId">ID of the Spoken Terms collection.</param>
    /// <param name="name">Optional new name for the collection; not sent if <c>null</c>.</param>
    /// <param name="termLists">
    ///   Optional terms to merge into the existing term list of each language; not sent if <c>null</c>.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="SpokenTerms" /> object with details about the updated collection.</returns>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SpokenTerms> UpdateSpokenTermsAsync(
          string spokenTermsId,
          string? name = null,
          IEnumerable<SpokenTermsEntries>? termLists = null,
          CancellationToken cancellationToken = default);

    /// <summary>Renames a Spoken Terms collection and/or merges terms into its term lists.</summary>
    /// <remarks>
    ///   The given terms are merged into the existing list of that language. To replace a list instead, use
    ///   <see cref="ReplaceSpokenTermsListAsync(string, SpokenTermsEntries, CancellationToken)" />.
    /// </remarks>
    /// <param name="spokenTerms">The Spoken Terms collection.</param>
    /// <param name="name">Optional new name for the collection; not sent if <c>null</c>.</param>
    /// <param name="termLists">
    ///   Optional terms to merge into the existing term list of each language; not sent if <c>null</c>.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="SpokenTerms" /> object with details about the updated collection.</returns>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SpokenTerms> UpdateSpokenTermsAsync(
          SpokenTerms spokenTerms,
          string? name = null,
          IEnumerable<SpokenTermsEntries>? termLists = null,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves the terms of one language of a Spoken Terms collection.</summary>
    /// <param name="spokenTermsId">ID of the Spoken Terms collection.</param>
    /// <param name="lang">Language code (ISO 639-1) of the term list, for example "en".</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="SpokenTermsEntries" /> object containing the terms.</returns>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SpokenTermsEntries> GetSpokenTermsEntriesAsync(
          string spokenTermsId,
          string lang,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves the terms of one language of a Spoken Terms collection.</summary>
    /// <param name="spokenTerms">The Spoken Terms collection.</param>
    /// <param name="lang">Language code (ISO 639-1) of the term list, for example "en".</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="SpokenTermsEntries" /> object containing the terms.</returns>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SpokenTermsEntries> GetSpokenTermsEntriesAsync(
          SpokenTerms spokenTerms,
          string lang,
          CancellationToken cancellationToken = default);

    /// <summary>Replaces the term list of one language in a Spoken Terms collection, or adds it if the language is new.</summary>
    /// <param name="spokenTermsId">ID of the Spoken Terms collection.</param>
    /// <param name="entries">The language and terms of the new term list.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="SpokenTermsList" /> object with details about the new term list.</returns>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SpokenTermsList> ReplaceSpokenTermsListAsync(
          string spokenTermsId,
          SpokenTermsEntries entries,
          CancellationToken cancellationToken = default);

    /// <summary>Replaces the term list of one language in a Spoken Terms collection, or adds it if the language is new.</summary>
    /// <param name="spokenTerms">The Spoken Terms collection.</param>
    /// <param name="entries">The language and terms of the new term list.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="SpokenTermsList" /> object with details about the new term list.</returns>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<SpokenTermsList> ReplaceSpokenTermsListAsync(
          SpokenTerms spokenTerms,
          SpokenTermsEntries entries,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes a Spoken Terms collection.</summary>
    /// <param name="spokenTermsId">ID of the Spoken Terms collection.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteSpokenTermsAsync(
          string spokenTermsId,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes a Spoken Terms collection.</summary>
    /// <param name="spokenTerms">The Spoken Terms collection.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteSpokenTermsAsync(
          SpokenTerms spokenTerms,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the term list of one language from a Spoken Terms collection.</summary>
    /// <param name="spokenTermsId">ID of the Spoken Terms collection.</param>
    /// <param name="lang">Language code (ISO 639-1) of the term list to delete, for example "de".</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteSpokenTermsListAsync(
          string spokenTermsId,
          string lang,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the term list of one language from a Spoken Terms collection.</summary>
    /// <param name="spokenTerms">The Spoken Terms collection.</param>
    /// <param name="lang">Language code (ISO 639-1) of the term list to delete, for example "de".</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="NotFoundException">If the specified Spoken Terms collection was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteSpokenTermsListAsync(
          SpokenTerms spokenTerms,
          string lang,
          CancellationToken cancellationToken = default);
  }
}
