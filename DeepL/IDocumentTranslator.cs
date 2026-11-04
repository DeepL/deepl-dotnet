// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>Translates documents with the DeepL API. Implemented by <see cref="DeepLClient" />.</summary>
  public interface IDocumentTranslator : IDisposable {
    /// <summary>
    ///   Translate document at specified input <see cref="FileInfo" /> from source language to target language and
    ///   store the translated document at specified output <see cref="FileInfo" />.
    /// </summary>
    /// <param name="inputFileInfo"><see cref="FileInfo" /> object containing path to input document.</param>
    /// <param name="outputFileInfo"><see cref="FileInfo" /> object containing path to store translated document.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options"><see cref="DocumentTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The final <see cref="DocumentStatus" /> of the translation, with status done.</returns>
    /// <exception cref="ArgumentException">
    ///   If the input file does not exist or is not a file; raised before any request.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///   If cancellation was requested through <paramref name="cancellationToken" />; it is not wrapped.
    /// </exception>
    /// <exception cref="DocumentTranslationException">
    ///   If an error occurs while translating the document, including local file failures: the input cannot be read
    ///   or the output path is occupied (both checked before any request; an existing output file is left
    ///   unchanged), with the <see cref="IOException" /> or <see cref="UnauthorizedAccessException" /> as
    ///   <see cref="Exception.InnerException" />. If the document was uploaded successfully, then the
    ///   <see cref="DocumentTranslationException.DocumentHandle" /> contains the document ID and key that may be used
    ///   to retrieve the document.
    /// </exception>
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    Task<DocumentStatus> TranslateDocumentAsync(
          FileInfo inputFileInfo,
          FileInfo outputFileInfo,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Translate specified document content from source language to target language and store the translated document
    ///   content to specified stream.
    /// </summary>
    /// <param name="inputFile"><see cref="Stream" /> containing input document content.</param>
    /// <param name="inputFileName">Name of the input file. The file extension is used to determine file type.</param>
    /// <param name="outputFile"><see cref="Stream" /> to store translated document content.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options"><see cref="DocumentTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The final <see cref="DocumentStatus" /> of the translation, with status done.</returns>
    /// <exception cref="OperationCanceledException">
    ///   If cancellation was requested through <paramref name="cancellationToken" />; it is not wrapped.
    /// </exception>
    /// <exception cref="DocumentTranslationException">
    ///   If an error occurs while translating the document. If the document was uploaded successfully, then the
    ///   <see cref="DocumentTranslationException.DocumentHandle" /> contains the document ID and key that may be used
    ///   to retrieve the document.
    /// </exception>
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    Task<DocumentStatus> TranslateDocumentAsync(
          Stream inputFile,
          string inputFileName,
          Stream outputFile,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Translate specified document content from source language to target language and store the translated document
    ///   content to specified stream. The file name is taken from <see cref="DocumentTranslateOptions.Filename" />.
    /// </summary>
    /// <param name="inputFile"><see cref="Stream" /> containing input document content.</param>
    /// <param name="outputFile"><see cref="Stream" /> to store translated document content.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">
    ///   <see cref="DocumentTranslateOptions" /> influencing translation; <see cref="DocumentTranslateOptions.Filename" />
    ///   is required.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The final <see cref="DocumentStatus" /> of the translation, with status done.</returns>
    /// <exception cref="OperationCanceledException">
    ///   If cancellation was requested through <paramref name="cancellationToken" />; it is not wrapped.
    /// </exception>
    /// <exception cref="DocumentTranslationException">
    ///   If the <see cref="DocumentTranslateOptions.Timeout" /> expired, or an error occurs
    ///   while translating the document, including a missing <see cref="DocumentTranslateOptions.Filename" />. If the
    ///   document was uploaded successfully, then the <see cref="DocumentTranslationException.DocumentHandle" />
    ///   contains the document ID and key that may be used to retrieve the document.
    /// </exception>
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    Task<DocumentStatus> TranslateDocumentAsync(
          Stream inputFile,
          Stream outputFile,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Upload document at specified input <see cref="FileInfo" /> for translation from source language to target
    ///   language.
    /// </summary>
    /// <param name="inputFileInfo"><see cref="FileInfo" /> object containing path to input document.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options"><see cref="DocumentTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DocumentHandle" /> object associated with the in-progress document translation.</returns>
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, or the input file does not exist or is not a file; raised before any request.
    /// </exception>
    /// <exception cref="IOException">
    ///   If the input cannot be opened; it is opened before any request.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///   If the input cannot be read due to permissions; raised before any request.
    /// </exception>
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
    Task<DocumentHandle> TranslateDocumentUploadAsync(
          FileInfo inputFileInfo,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Upload document content for translation from source language to target language. The file name is taken from
    ///   <see cref="DocumentTranslateOptions.Filename" />.
    /// </summary>
    /// <param name="inputFile"><see cref="Stream" /> containing input document content.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options">
    ///   <see cref="DocumentTranslateOptions" /> influencing translation; <see cref="DocumentTranslateOptions.Filename" />
    ///   is required.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DocumentHandle" /> object associated with the in-progress document translation.</returns>
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, or <see cref="DocumentTranslateOptions.Filename" /> is not set.
    /// </exception>
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
    Task<DocumentHandle> TranslateDocumentUploadAsync(
          Stream inputFile,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Upload document content with specified filename for translation from source language to target language.</summary>
    /// <param name="inputFile"><see cref="Stream" /> containing input document content.</param>
    /// <param name="inputFileName">Name of the input file. The file extension is used to determine file type.</param>
    /// <param name="sourceLanguageCode">Language code of the input language, or null to use auto-detection.</param>
    /// <param name="targetLanguageCode">Language code of the desired output language.</param>
    /// <param name="options"><see cref="DocumentTranslateOptions" /> influencing translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DocumentHandle" /> object associated with the in-progress document translation.</returns>
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
    Task<DocumentHandle> TranslateDocumentUploadAsync(
          Stream inputFile,
          string inputFileName,
          string? sourceLanguageCode,
          string targetLanguageCode,
          DocumentTranslateOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieve the status of in-progress document translation associated with specified
    ///   <see cref="DocumentHandle" />.
    /// </summary>
    /// <param name="handle"><see cref="DocumentHandle" /> associated with an in-progress document translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DocumentStatus" /> object containing the document translation status.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DocumentStatus> TranslateDocumentGetStatusAsync(
          DocumentHandle handle,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Checks document translation status and asynchronously-waits until document translation is complete or fails
    ///   due to an error.
    /// </summary>
    /// <param name="handle"><see cref="DocumentHandle" /> associated with an in-progress document translation.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The final <see cref="DocumentStatus" />, with status done.</returns>
    /// <exception cref="DeepLException">
    ///   If the document translation fails (status error, with the API's error message), or any error occurs while
    ///   communicating with the DeepL API, a <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DocumentStatus> TranslateDocumentWaitUntilDoneAsync(
          DocumentHandle handle,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Checks document translation status and asynchronously-waits until document translation is complete or fails
    ///   due to an error, for at most the given time.
    /// </summary>
    /// <param name="handle"><see cref="DocumentHandle" /> associated with an in-progress document translation.</param>
    /// <param name="timeout">Maximum time to wait, or <c>null</c> to wait without a limit.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The final <see cref="DocumentStatus" />, with status done.</returns>
    /// <exception cref="DeepLException">
    ///   If the document translation fails, the <paramref name="timeout" /> expires before it is complete, or any error
    ///   occurs while communicating with the DeepL API, a <see cref="DeepLException" /> or a derived class will be
    ///   thrown.
    /// </exception>
    Task<DocumentStatus> TranslateDocumentWaitUntilDoneAsync(
          DocumentHandle handle,
          TimeSpan? timeout,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Downloads the resulting translated document associated with specified <see cref="DocumentHandle" /> to the
    ///   specified <see cref="FileInfo" />. The <see cref="DocumentStatus" /> for the document must have
    ///   <see cref="DocumentStatus.Done" /> == <c>true</c>.
    /// </summary>
    /// <param name="handle"><see cref="DocumentHandle" /> associated with an in-progress document translation.</param>
    /// <param name="outputFileInfo"><see cref="FileInfo" /> object containing path to store translated document.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="IOException">If the output path is occupied.</exception>
    /// <exception cref="DocumentNotReadyException">
    ///   If the document is not ready to be downloaded, that is, the document status
    ///   is not <see cref="DocumentStatus.Done" />.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If any other error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task TranslateDocumentDownloadAsync(
          DocumentHandle handle,
          FileInfo outputFileInfo,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Downloads the resulting translated document associated with specified <see cref="DocumentHandle" /> to the
    ///   specified <see cref="Stream" />. The <see cref="DocumentStatus" /> for the document must have
    ///   <see cref="DocumentStatus.Done" /> == <c>true</c>.
    /// </summary>
    /// <param name="handle"><see cref="DocumentHandle" /> associated with an in-progress document translation.</param>
    /// <param name="outputFile"><see cref="Stream" /> to store translated document content.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="DocumentNotReadyException">
    ///   If the document is not ready to be downloaded, that is, the document status
    ///   is not <see cref="DocumentStatus.Done" />.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If any other error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task TranslateDocumentDownloadAsync(
          DocumentHandle handle,
          Stream outputFile,
          CancellationToken cancellationToken = default);
  }
}
