// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  public interface IGlossaryManager : IDisposable {
    /// <summary>
    ///   Creates a glossary in your DeepL account with the specified details and returns a
    ///   <see cref="MultilingualGlossaryInfo" />
    ///   object with details about the newly created glossary. The glossary will contain the glossary dictionaries
    ///   specified in <paramref name="glossaryDicts" /> each with their own source language, target language and
    ///   entries.The glossary can be used in translations to override translations for specific terms (words). The
    ///   glossary must contain a glossary dictionary that matches the languages of translations for which it will be
    ///   used.
    /// </summary>
    /// <param name="name">User-defined name to assign to the glossary; must not be empty.</param>
    /// <param name="glossaryDicts"><see cref="MultilingualGlossaryDictionaryInfo" />The dictionaries of the glossary</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the newly created glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryAsync(
          string name,
          MultilingualGlossaryDictionaryEntries[] glossaryDicts,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Creates a glossary in your DeepL account with the specified details and returns a
    ///   <see cref="MultilingualGlossaryInfo" />
    ///   object with details about the newly created glossary. The glossary will contain a glossary dictionary
    ///   with the source and target language codes specified and entries created from the <paramref name="csvFile" />.
    ///   The glossary can be used in translations to override translations for specific terms (words). The glossary
    ///   must contain a glossary dictionary that matches the languages of translations for which it will be used.
    /// </summary>
    /// <param name="name">User-defined name to assign to the glossary; must not be empty.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the newly created glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryFromCsvAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves information about the glossary with the specified ID and returns a <see cref="MultilingualGlossaryInfo" />
    ///   object containing details. This does not retrieve the glossary entries; to retrieve entries use
    ///   <see
    ///     cref="IGlossaryManager.GetMultilingualGlossaryDictionaryEntriesAsync(string,string,string,System.Threading.CancellationToken)" />
    /// </summary>
    /// <param name="glossaryId">ID of glossary to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the specified glossary.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> GetMultilingualGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves information about all glossaries and returns an array of <see cref="MultilingualGlossaryInfo" /> objects
    ///   containing details. This does not retrieve the glossary entries; to retrieve entries use
    ///   <see
    ///     cref="IGlossaryManager.GetMultilingualGlossaryDictionaryEntriesAsync(string,string,string,System.Threading.CancellationToken)" />
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="MultilingualGlossaryInfo" /> objects with details about each glossary.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo[]> ListMultilingualGlossariesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///   For the glossary with the specified ID, retrieves the glossary dictionary with its entries for the given
    ///   source and target language code pair.
    /// </summary>
    /// <param name="glossaryId">ID of glossary for which to retrieve entries.</param>
    /// <param name="sourceLanguageCode">Source language code for the requested glossary dictionary.</param>
    /// <param name="targetLanguageCode">Target language code of the requested glossary dictionary.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryDictionaryEntries" /> object containing a glossary dictionary with entries.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   For the glossary with the specified ID, retrieves the glossary dictionary with its entries for the given
    ///   <see cref="MultilingualGlossaryDictionaryInfo" /> glossary dictionary.
    /// </summary>
    /// <param name="glossaryId">ID of glossary for which to retrieve entries.</param>
    /// <param name="glossaryDict">The requested glossary dictionary.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryDictionaryEntries" /> object containing a glossary dictionary with entries.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   For the specified glossary, retrieves the glossary dictionary with its entries for the given
    ///   source and target language code pair.
    /// </summary>
    /// <param name="glossary">The glossary for which to retrieve entries.</param>
    /// <param name="sourceLanguageCode">Source language code for the requested glossary dictionary.</param>
    /// <param name="targetLanguageCode">Target language code of the requested glossary dictionary.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryDictionaryEntries" /> object containing a glossary dictionary with entries.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   For the specified glossary, retrieves the glossary dictionary with its entries for the given
    ///   <see cref="MultilingualGlossaryDictionaryInfo" /> glossary dictionary.
    /// </summary>
    /// <param name="glossary">The glossary for which to retrieve entries.</param>
    /// <param name="glossaryDict">The requested glossary dictionary.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryDictionaryEntries" /> object containing a glossary dictionary with entries.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryEntries> GetMultilingualGlossaryDictionaryEntriesAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Replaces a glossary dictionary with given entries for the source and target language codes. If no such
    ///   glossary dictionary exists for that language pair, a new glossary dictionary will be created for that
    ///   language pair and entries.
    /// </summary>
    /// <param name="glossaryId">The specified ID of the glossary that contains the dictionary to be replaced/created </param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="entries">The source-target entry pairs in the new glossary dictionary.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the newly replaced glossary dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Replaces a glossary dictionary with given entries for the source and target language codes. If no such
    ///   glossary dictionary exists for that language pair, a new glossary dictionary will be created for that
    ///   language pair and entries.
    /// </summary>
    /// <param name="glossaryId">The specified ID of the glossary that contains the dictionary to be replaced/created </param>
    /// <param name="glossaryDict">
    ///   The glossary dictionary to replace the existing glossary dictionary for that source/target language code pair
    ///   or to be newly created if no such glossary dictionary exists.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the newly replaced glossary dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Replaces a glossary dictionary with given entries for given glossary dictionary. If no such
    ///   glossary dictionary exists for that language pair, a new glossary dictionary will be created for that
    ///   language pair and entries.
    /// </summary>
    /// <param name="glossary">The specified glossary that contains the dictionary to be replaced/created </param>
    /// <param name="glossaryDict">
    ///   The glossary dictionary to replace the existing glossary dictionary for that source/target language code pair
    ///   or to be newly created if no such glossary dictionary exists.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the newly replaced glossary dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Replaces a glossary dictionary with given entries for the source and target language codes. If no such
    ///   glossary dictionary exists for that language pair, a new glossary dictionary will be created for that
    ///   language pair and entries.
    /// </summary>
    /// <param name="glossary">The specified glossary that contains the dictionary to be replaced/created </param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="entries">The source-target entry pairs in the new glossary dictionary.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the newly replaced glossary dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Replaces a glossary dictionary with given entries for the source and target language codes. If no such
    ///   glossary dictionary exists for that language pair, a new glossary dictionary will be created for that
    ///   language pair and entries specified in the <paramref name="csvFile"></paramref>.
    /// </summary>
    /// <param name="glossary">The specified glossary that contains the dictionary to be replaced/created </param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the newly replaced glossary dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Replaces a glossary dictionary with given entries for the source and target language codes. If no such
    ///   glossary dictionary exists for that language pair, a new glossary dictionary will be created for that
    ///   language pair and entries specified in the <paramref name="csvFile"></paramref>.
    /// </summary>
    /// <param name="glossaryId">The specified ID of the glossary that contains the dictionary to be replaced/created </param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the newly replaced glossary dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Updates a glossary with the given name specified in <paramref name="name" />. The given name must not
    ///   be empty.
    /// </summary>
    /// <param name="glossaryId">The specified ID of the glossary that is to be updated </param>
    /// <param name="name">The new name of the glossary</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the newly updated name
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryNameAsync(
          string glossaryId,
          string name,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Updates a glossary dictionary with given entries for the source and target language codes. The glossary
    ///   dictionary must belong to the glossary with the ID specified in <paramref name="glossaryId" />. If a dictionary
    ///   for the provided language pair already exists, the dictionary entries are merged.
    /// </summary>
    /// <param name="glossaryId">The specified ID of the glossary that contains the dictionary to be updated/created </param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="entries">The source-target entry pairs in the new glossary dictionary.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the newly updated glossary
    ///   dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Updates a glossary dictionary with given entries for the source and target language codes. The glossary
    ///   dictionary must belong to the glossary specified in <paramref name="glossary" />. If a dictionary
    ///   for the provided language pair already exists, the dictionary entries are merged.
    /// </summary>
    /// <param name="glossary">The specified ID for the glossary that contains the dictionary to be updated/created </param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="entries">The source-target entry pairs in the new glossary dictionary.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the newly updated glossary
    ///   dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Updates a glossary dictionary with given glossary dictionary specified in <paramref name="glossaryDict" />.
    ///   The glossary dictionary must belong to the glossary with the ID specified in <paramref name="glossaryId" />.
    ///   If a dictionary for the provided language pair already exists, the dictionary entries are merged.
    /// </summary>
    /// <param name="glossaryId">The specified ID of the glossary that contains the dictionary to be updated/created </param>
    /// <param name="glossaryDict">The glossary dictionary to be created/updated</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the newly updated glossary
    ///   dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Updates a glossary dictionary with given entries for the source and target language codes. If a dictionary
    ///   for the provided language pair already exists, the dictionary entries are merged.
    /// </summary>
    /// <param name="glossary">The specified glossary that contains the dictionary to be updated/created </param>
    /// <param name="glossaryDict">The glossary dictionary to be created/updated</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the newly updated glossary
    ///   dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Updates a glossary dictionary correlating to the specified ID with given entries in the
    ///   <paramref name="csvFile"></paramref> for the source and target language codes. If a dictionary for the provided
    ///   language pair already exists, the dictionary entries are merged.
    /// </summary>
    /// <param name="glossaryId">The specified ID of the glossary that contains the dictionary to be updated/created </param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the newly updated glossary
    ///   dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryFromCsvAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Updates a glossary dictionary with given entries in the <paramref name="csvFile"></paramref> for the source and
    ///   target language codes. If a dictionary for the provided language pair already exists, the dictionary entries
    ///   are merged.
    /// </summary>
    /// <param name="glossary">The specified glossary that contains the dictionary to be updated/created </param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the newly updated glossary
    ///   dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryFromCsvAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the glossary with the specified ID.</summary>
    /// <param name="glossaryId">ID of glossary to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteMultilingualGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the specified glossary.</summary>
    /// <param name="glossary"><see cref="MultilingualGlossaryInfo" /> object corresponding to glossary to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteMultilingualGlossaryAsync(
          MultilingualGlossaryInfo glossary,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Deletes the glossary dictionary with the source and target language codes specified in the glossary
    ///   with the specified ID.
    /// </summary>
    /// <param name="glossaryId">ID of glossary that contains the glossary dictionary to delete.</param>
    /// <param name="sourceLanguageCode">Source language code of the glossary dictionary to be deleted.</param>
    /// <param name="targetLanguageCode">Target language code of the glossary dictionary to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the specified glossary dictionary in the glossary with the specified ID.</summary>
    /// <param name="glossaryId">ID of glossary that contains the glossary dictionary to delete.</param>
    /// <param name="glossaryDict">
    ///   <see cref="MultilingualGlossaryDictionaryInfo" /> object corresponding to glossary dictionary to delete.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the specified glossary dictionary in the glossary in the specified glossary.</summary>
    /// <param name="glossary">The glossary that contains the glossary dictionary to delete.</param>
    /// <param name="sourceLanguageCode">Source language code of the glossary dictionary to be deleted.</param>
    /// <param name="targetLanguageCode">Target language code of the glossary dictionary to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the specified glossary dictionary in the glossary in the specified glossary.</summary>
    /// <param name="glossary">The glossary that contains the glossary dictionary to delete.</param>
    /// <param name="glossaryDict">
    ///   <see cref="MultilingualGlossaryDictionaryInfo" /> object corresponding to glossary dictionary to delete.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryInfo glossaryDict,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Creates a v2 glossary in your DeepL account with the specified details and returns a
    ///   <see cref="GlossaryInfo" /> object with details about the newly created glossary. The glossary can be used
    ///   in translations to override translations for specific terms (words). The glossary source and target languages
    ///   must match the languages of translations for which it will be used.
    /// </summary>
    /// <param name="name">User-defined name to assign to the glossary; must not be empty.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="entries">Glossary entry list.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="GlossaryInfo" /> object with details about the newly created glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryInfo> CreateGlossaryAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          GlossaryEntries entries,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Creates a v2 glossary in your DeepL account with the specified details and returns a
    ///   <see cref="GlossaryInfo" /> object with details about the newly created glossary. The glossary can be used
    ///   in translations to override translations for specific terms (words). The glossary source and target languages
    ///   must match the languages of translations for which it will be used.
    /// </summary>
    /// <param name="name">User-defined name to assign to the glossary; must not be empty.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="GlossaryInfo" /> object with details about the newly created glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryInfo> CreateGlossaryFromCsvAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves information about the glossary with the specified ID and returns a <see cref="GlossaryInfo" />
    ///   object containing details. This does not retrieve the glossary entries; to retrieve entries use
    ///   <see cref="GetGlossaryEntriesAsync(string,System.Threading.CancellationToken)" />
    /// </summary>
    /// <param name="glossaryId">ID of glossary to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="GlossaryInfo" /> object with details about the specified glossary.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryInfo> GetGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default);

    /// <summary>Asynchronously waits until the given glossary is ready to be used for translations.</summary>
    /// <param name="glossaryId">ID of glossary to ensure ready.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryInfo> WaitUntilGlossaryReadyAsync(
          string glossaryId,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves information about all glossaries and returns an array of <see cref="GlossaryInfo" /> objects
    ///   containing details. This does not retrieve the glossary entries; to retrieve entries use
    ///   <see cref="GetGlossaryEntriesAsync(string,System.Threading.CancellationToken)" />
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="GlossaryInfo" /> objects with details about each glossary.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryInfo[]> ListGlossariesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves the entries containing within the glossary with the specified ID and returns them as a
    ///   <see cref="GlossaryEntries" />.
    /// </summary>
    /// <param name="glossaryId">ID of glossary for which to retrieve entries.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="GlossaryEntries" /> containing entry pairs of the glossary.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryEntries> GetGlossaryEntriesAsync(
          string glossaryId,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves the entries containing within the glossary and returns them as a <see cref="GlossaryEntries" />.
    /// </summary>
    /// <param name="glossary"><see cref="GlossaryInfo" /> object corresponding to glossary for which to retrieve entries.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="GlossaryEntries" /> containing entry pairs of the glossary.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<GlossaryEntries> GetGlossaryEntriesAsync(
          GlossaryInfo glossary,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the glossary with the specified ID.</summary>
    /// <param name="glossaryId">ID of glossary to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteGlossaryAsync(
          string glossaryId,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the specified glossary.</summary>
    /// <param name="glossary"><see cref="GlossaryInfo" /> object corresponding to glossary to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteGlossaryAsync(
          GlossaryInfo glossary,
          CancellationToken cancellationToken = default);
  }
}
