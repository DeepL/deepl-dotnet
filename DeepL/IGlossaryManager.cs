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
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryAsync(
          string name,
          MultilingualGlossaryDictionaryEntries[] glossaryDicts,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Creates a glossary with the given dictionaries, like
    ///   <see cref="CreateMultilingualGlossaryAsync(string, MultilingualGlossaryDictionaryEntries[], CancellationToken)" />
    ///   .
    /// </summary>
    /// <param name="name">User-defined name to assign to the glossary; must not be empty.</param>
    /// <param name="glossaryDicts">The dictionaries of the glossary; must not be empty.</param>
    /// <param name="options">
    ///   Optional <see cref="CreateMultilingualGlossaryOptions" />, whose <c>ExtraBodyParameters</c> are added last to
    ///   the request body.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the newly created glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryAsync(
          string name,
          MultilingualGlossaryDictionaryEntries[] glossaryDicts,
          CreateMultilingualGlossaryOptions? options,
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
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryFromCsvAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Creates a glossary with one dictionary read from CSV, like
    ///   <see cref="CreateMultilingualGlossaryFromCsvAsync(string, string, string, Stream, CancellationToken)" />.
    /// </summary>
    /// <param name="name">User-defined name to assign to the glossary; must not be empty.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="options">
    ///   Optional <see cref="CreateMultilingualGlossaryOptions" />, whose <c>ExtraBodyParameters</c> are added last to
    ///   the request body.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the newly created glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    Task<MultilingualGlossaryInfo> CreateMultilingualGlossaryFromCsvAsync(
          string name,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          CreateMultilingualGlossaryOptions? options,
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
    ///   Retrieves information about the given glossary, like
    ///   <see cref="GetMultilingualGlossaryAsync(string, CancellationToken)" />.
    /// </summary>
    /// <param name="glossary">The glossary to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the specified glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> GetMultilingualGlossaryAsync(
          MultilingualGlossaryInfo glossary,
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
    ///   Replaces the glossary dictionary for the given language pair with the given entries, like
    ///   <see cref="ReplaceMultilingualGlossaryDictionaryAsync(string, string, string, GlossaryEntries, CancellationToken)" />
    ///   .
    /// </summary>
    /// <param name="glossaryId">ID of the glossary containing the dictionary.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="entries">The source-target entry pairs of the new dictionary.</param>
    /// <param name="options">
    ///   Optional <see cref="ReplaceMultilingualGlossaryDictionaryOptions" />, whose <c>ExtraBodyParameters</c> are added
    ///   last to the JSON request body.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the replaced dictionary.</returns>
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
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Replaces the glossary dictionary for the given language pair with the given entries, like
    ///   <see cref="ReplaceMultilingualGlossaryDictionaryAsync(MultilingualGlossaryInfo, string, string, GlossaryEntries, CancellationToken)" />
    ///   .
    /// </summary>
    /// <param name="glossary">The glossary containing the dictionary.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="entries">The source-target entry pairs of the new dictionary.</param>
    /// <param name="options">
    ///   Optional <see cref="ReplaceMultilingualGlossaryDictionaryOptions" />, whose <c>ExtraBodyParameters</c> are added
    ///   last to the JSON request body.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the replaced dictionary.</returns>
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
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Replaces the glossary dictionary for the language pair of <paramref name="glossaryDict" /> with its entries,
    ///   like
    ///   <see cref="ReplaceMultilingualGlossaryDictionaryAsync(string, MultilingualGlossaryDictionaryEntries, CancellationToken)" />
    ///   .
    /// </summary>
    /// <param name="glossaryId">ID of the glossary containing the dictionary.</param>
    /// <param name="glossaryDict">The language pair and entries of the new dictionary.</param>
    /// <param name="options">
    ///   Optional <see cref="ReplaceMultilingualGlossaryDictionaryOptions" />, whose <c>ExtraBodyParameters</c> are added
    ///   last to the JSON request body.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the replaced dictionary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Replaces the glossary dictionary for the language pair of <paramref name="glossaryDict" /> with its entries,
    ///   like
    ///   <see cref="ReplaceMultilingualGlossaryDictionaryAsync(MultilingualGlossaryInfo, MultilingualGlossaryDictionaryEntries, CancellationToken)" />
    ///   .
    /// </summary>
    /// <param name="glossary">The glossary containing the dictionary.</param>
    /// <param name="glossaryDict">The language pair and entries of the new dictionary.</param>
    /// <param name="options">
    ///   Optional <see cref="ReplaceMultilingualGlossaryDictionaryOptions" />, whose <c>ExtraBodyParameters</c> are added
    ///   last to the JSON request body.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the replaced dictionary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
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
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, or the CSV content is empty or contains only whitespace.
    /// </exception>
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
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, or the CSV content is empty or contains only whitespace.
    /// </exception>
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
    ///   Replaces the glossary dictionary for the given language pair with entries read from CSV, like
    ///   <see cref="ReplaceMultilingualGlossaryDictionaryFromCsvAsync(string, string, string, Stream, CancellationToken)" />
    ///   .
    /// </summary>
    /// <param name="glossaryId">ID of the glossary containing the dictionary.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="options">
    ///   Optional <see cref="ReplaceMultilingualGlossaryDictionaryOptions" />, whose <c>ExtraBodyParameters</c> are added
    ///   last to the JSON request body.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the replaced dictionary.</returns>
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, or the CSV content is empty or contains only whitespace.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Replaces the glossary dictionary for the given language pair with entries read from CSV, like
    ///   <see cref="ReplaceMultilingualGlossaryDictionaryFromCsvAsync(MultilingualGlossaryInfo, string, string, Stream, CancellationToken)" />
    ///   .
    /// </summary>
    /// <param name="glossary">The glossary containing the dictionary.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="options">
    ///   Optional <see cref="ReplaceMultilingualGlossaryDictionaryOptions" />, whose <c>ExtraBodyParameters</c> are added
    ///   last to the JSON request body.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryDictionaryInfo" /> object with details about the replaced dictionary.</returns>
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, or the CSV content is empty or contains only whitespace.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryDictionaryInfo> ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          ReplaceMultilingualGlossaryDictionaryOptions? options,
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
    ///   Renames the glossary with the given ID, like
    ///   <see cref="UpdateMultilingualGlossaryNameAsync(string, string, CancellationToken)" />.
    /// </summary>
    /// <param name="glossaryId">ID of the glossary to rename.</param>
    /// <param name="name">The new name of the glossary; must not be empty.</param>
    /// <param name="options">
    ///   Optional <see cref="BaseRequestOptions" />, whose <c>ExtraBodyParameters</c> are added last to the request
    ///   body, replacing fields with the same key.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the renamed glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryNameAsync(
          string glossaryId,
          string name,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Renames the given glossary, like
    ///   <see cref="UpdateMultilingualGlossaryNameAsync(string, string, CancellationToken)" />.
    /// </summary>
    /// <param name="glossary">The glossary to rename.</param>
    /// <param name="name">The new name of the glossary; must not be empty.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the renamed glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryNameAsync(
          MultilingualGlossaryInfo glossary,
          string name,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Renames the given glossary, like
    ///   <see cref="UpdateMultilingualGlossaryNameAsync(string, string, BaseRequestOptions?, CancellationToken)" />.
    /// </summary>
    /// <param name="glossary">The glossary to rename.</param>
    /// <param name="name">The new name of the glossary; must not be empty.</param>
    /// <param name="options">
    ///   Optional <see cref="BaseRequestOptions" />, whose <c>ExtraBodyParameters</c> are added last to the request
    ///   body, replacing fields with the same key.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the renamed glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryNameAsync(
          MultilingualGlossaryInfo glossary,
          string name,
          BaseRequestOptions? options,
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
    ///   Merges the given entries into the glossary dictionary for the language pair, creating it if needed, like
    ///   <see cref="UpdateMultilingualGlossaryDictionaryAsync(string, string, string, GlossaryEntries, CancellationToken)" />.
    /// </summary>
    /// <param name="glossaryId">ID of the glossary that contains the dictionary to update or create.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="entries">The source-target entry pairs to merge into the dictionary.</param>
    /// <param name="options">
    ///   Optional <see cref="BaseRequestOptions" />, whose <c>ExtraBodyParameters</c> are added last to the request
    ///   body, replacing fields with the same key.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the updated dictionary.
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
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Merges the given entries into the glossary dictionary for the language pair, creating it if needed, like
    ///   <see cref="UpdateMultilingualGlossaryDictionaryAsync(MultilingualGlossaryInfo, string, string, GlossaryEntries, CancellationToken)" />.
    /// </summary>
    /// <param name="glossary">The glossary that contains the dictionary to update or create.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="entries">The source-target entry pairs to merge into the dictionary.</param>
    /// <param name="options">
    ///   Optional <see cref="BaseRequestOptions" />, whose <c>ExtraBodyParameters</c> are added last to the request
    ///   body, replacing fields with the same key.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the updated dictionary.
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
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Merges the given entries into the glossary dictionary for the language pair, creating it if needed, like
    ///   <see cref="UpdateMultilingualGlossaryDictionaryAsync(MultilingualGlossaryInfo, MultilingualGlossaryDictionaryEntries, CancellationToken)" />.
    /// </summary>
    /// <param name="glossary">The glossary that contains the dictionary to update or create.</param>
    /// <param name="glossaryDict">The dictionary to merge into the glossary.</param>
    /// <param name="options">
    ///   Optional <see cref="BaseRequestOptions" />, whose <c>ExtraBodyParameters</c> are added last to the request
    ///   body, replacing fields with the same key.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the updated dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          MultilingualGlossaryInfo glossary,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Merges the given entries into the glossary dictionary for the language pair, creating it if needed, like
    ///   <see cref="UpdateMultilingualGlossaryDictionaryAsync(string, MultilingualGlossaryDictionaryEntries, CancellationToken)" />.
    /// </summary>
    /// <param name="glossaryId">ID of the glossary that contains the dictionary to update or create.</param>
    /// <param name="glossaryDict">The dictionary to merge into the glossary.</param>
    /// <param name="options">
    ///   Optional <see cref="BaseRequestOptions" />, whose <c>ExtraBodyParameters</c> are added last to the request
    ///   body, replacing fields with the same key.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the updated dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryAsync(
          string glossaryId,
          MultilingualGlossaryDictionaryEntries glossaryDict,
          BaseRequestOptions? options,
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
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, or the CSV content is empty or contains only whitespace.
    /// </exception>
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
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, or the CSV content is empty or contains only whitespace.
    /// </exception>
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

    /// <summary>
    ///   Merges the CSV entries into the glossary dictionary for the language pair, creating it if needed, like
    ///   <see cref="UpdateMultilingualGlossaryDictionaryFromCsvAsync(string, string, string, Stream, CancellationToken)" />.
    /// </summary>
    /// <param name="glossaryId">ID of the glossary that contains the dictionary to update or create.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="options">
    ///   Optional <see cref="BaseRequestOptions" />, whose <c>ExtraBodyParameters</c> are added last to the request
    ///   body, replacing fields with the same key.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the updated dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, or the CSV content is empty or contains only whitespace.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryFromCsvAsync(
          string glossaryId,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          BaseRequestOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Merges the CSV entries into the glossary dictionary for the language pair, creating it if needed, like
    ///   <see cref="UpdateMultilingualGlossaryDictionaryFromCsvAsync(MultilingualGlossaryInfo, string, string, Stream, CancellationToken)" />.
    /// </summary>
    /// <param name="glossary">The glossary that contains the dictionary to update or create.</param>
    /// <param name="sourceLanguageCode">Language code of the source terms language.</param>
    /// <param name="targetLanguageCode">Language code of the target terms language.</param>
    /// <param name="csvFile"><see cref="Stream" /> containing CSV content.</param>
    /// <param name="options">
    ///   Optional <see cref="BaseRequestOptions" />, whose <c>ExtraBodyParameters</c> are added last to the request
    ///   body, replacing fields with the same key.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   <see cref="MultilingualGlossaryInfo" /> object with details about the glossary with the updated dictionary.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, or the CSV content is empty or contains only whitespace.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryDictionaryFromCsvAsync(
          MultilingualGlossaryInfo glossary,
          string sourceLanguageCode,
          string targetLanguageCode,
          Stream csvFile,
          BaseRequestOptions? options,
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
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
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
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
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

    /// <summary>
    ///   Updates the name and/or dictionaries of a glossary in one request. Only the fields set in
    ///   <paramref name="options" /> are sent: the glossary is renamed if <see cref="UpdateMultilingualGlossaryOptions.Name" />
    ///   is set, and the entries of each of <see cref="UpdateMultilingualGlossaryOptions.Dictionaries" /> are merged into
    ///   the glossary's dictionary for that language pair, which is created if it does not exist.
    /// </summary>
    /// <param name="glossaryId">ID of the glossary to update.</param>
    /// <param name="options">
    ///   The <see cref="UpdateMultilingualGlossaryOptions" /> to apply; its <c>ExtraBodyParameters</c> are added last to
    ///   the JSON request body. With no field set, the request body is <c>{}</c> and the API reports the error.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the updated glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryAsync(
          string glossaryId,
          UpdateMultilingualGlossaryOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Updates the name and/or dictionaries of the given glossary in one request, like
    ///   <see cref="UpdateMultilingualGlossaryAsync(string, UpdateMultilingualGlossaryOptions?, CancellationToken)" />.
    /// </summary>
    /// <param name="glossary">The glossary to update.</param>
    /// <param name="options">The <see cref="UpdateMultilingualGlossaryOptions" /> to apply.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="MultilingualGlossaryInfo" /> object with details about the updated glossary.</returns>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<MultilingualGlossaryInfo> UpdateMultilingualGlossaryAsync(
          MultilingualGlossaryInfo glossary,
          UpdateMultilingualGlossaryOptions? options = null,
          CancellationToken cancellationToken = default);
  }
}
