// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  public interface ITranslationMemoryManager {
    /// <summary>Retrieves the list of all available translation memories.</summary>
    /// <param name="page">Optional page number for pagination, 0-indexed.</param>
    /// <param name="pageSize">Optional number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="TranslationMemoryInfo" /> objects.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemoryInfo[]> ListTranslationMemoriesAsync(
          int? page = null,
          int? pageSize = null,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single translation memory by its ID.</summary>
    /// <param name="translationMemoryId">The ID of the translation memory to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryInfo" /> object for the requested translation memory.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="translationMemoryId" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemoryInfo> GetTranslationMemoryAsync(
          string translationMemoryId,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves the current information of the given translation memory.</summary>
    /// <param name="translationMemory">The translation memory to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryInfo" /> object for the requested translation memory.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="translationMemory" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemoryInfo> GetTranslationMemoryAsync(
          TranslationMemoryInfo translationMemory,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves one page of the segments stored in a translation memory.</summary>
    /// <remarks>
    ///   Pagination is cursor-based: omit <paramref name="pageCursor" /> on the first call, then pass the
    ///   <see cref="TranslationMemorySegments.NextPageCursor" /> of the previous response to fetch the next page.
    ///   An absent next page cursor means the last page has been returned.
    /// </remarks>
    /// <param name="translationMemoryId">The ID of the translation memory to read segments from.</param>
    /// <param name="pageSize">Optional maximum number of segments per page (1-100, defaults to 50).</param>
    /// <param name="pageCursor">Optional cursor from a previous response; omit on the first call.</param>
    /// <param name="filterText">
    ///   Optional substring filter applied to source and target text, at least 2 characters.
    /// </param>
    /// <param name="filterCaseSensitive">
    ///   Optional flag indicating whether the text filter is case-sensitive, defaults to <c>false</c>.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemorySegments" /> object holding the requested page.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="translationMemoryId" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemorySegments> ListTranslationMemorySegmentsAsync(
          string translationMemoryId,
          int? pageSize = null,
          string? pageCursor = null,
          string? filterText = null,
          bool? filterCaseSensitive = null,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves one page of the segments stored in the given translation memory.</summary>
    /// <param name="translationMemory">The translation memory to read segments from.</param>
    /// <param name="pageSize">Optional maximum number of segments per page (1-100, defaults to 50).</param>
    /// <param name="pageCursor">Optional cursor from a previous response; omit on the first call.</param>
    /// <param name="filterText">
    ///   Optional substring filter applied to source and target text, at least 2 characters.
    /// </param>
    /// <param name="filterCaseSensitive">
    ///   Optional flag indicating whether the text filter is case-sensitive, defaults to <c>false</c>.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemorySegments" /> object holding the requested page.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="translationMemory" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemorySegments> ListTranslationMemorySegmentsAsync(
          TranslationMemoryInfo translationMemory,
          int? pageSize = null,
          string? pageCursor = null,
          string? filterText = null,
          bool? filterCaseSensitive = null,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves one page of the segments stored in a translation memory.</summary>
    /// <remarks>
    ///   Pagination is cursor-based: leave <see cref="TranslationMemorySegmentsOptions.PageCursor" /> unset on the first
    ///   call, then set it to the <see cref="TranslationMemorySegments.NextPageCursor" /> of the previous response to
    ///   fetch the next page. An absent next page cursor means the last page has been returned.
    /// </remarks>
    /// <param name="translationMemoryId">The ID of the translation memory to read segments from.</param>
    /// <param name="options">Optional <see cref="TranslationMemorySegmentsOptions" /> for paging and filtering.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemorySegments" /> object holding the requested page.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="translationMemoryId" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemorySegments> ListTranslationMemorySegmentsAsync(
          string translationMemoryId,
          TranslationMemorySegmentsOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves one page of the segments stored in the given translation memory.</summary>
    /// <param name="translationMemory">The translation memory to read segments from.</param>
    /// <param name="options">Optional <see cref="TranslationMemorySegmentsOptions" /> for paging and filtering.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemorySegments" /> object holding the requested page.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="translationMemory" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemorySegments> ListTranslationMemorySegmentsAsync(
          TranslationMemoryInfo translationMemory,
          TranslationMemorySegmentsOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes a translation memory.</summary>
    /// <param name="translationMemoryId">The ID of the translation memory to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="translationMemoryId" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteTranslationMemoryAsync(
          string translationMemoryId,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes the given translation memory.</summary>
    /// <param name="translationMemory">The translation memory to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="translationMemory" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteTranslationMemoryAsync(
          TranslationMemoryInfo translationMemory,
          CancellationToken cancellationToken = default);

    /// <summary>Creates an import job for a new translation memory.</summary>
    /// <remarks>
    ///   The job only declares the file; upload the TMX file itself to the returned upload URL with
    ///   <see cref="UploadTranslationMemoryFileAsync(TranslationMemoryImport, Stream, string, CancellationToken)" />,
    ///   then poll <see cref="GetTranslationMemoryJobAsync" /> for the outcome. Use
    ///   <see cref="ImportTranslationMemoryFromFilepathAsync(string, string, CancellationToken)" /> to do all three
    ///   steps at once.
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, and after a
    ///   network failure before any request byte is written, such as a failure to resolve the server name, a refused,
    ///   reset or timed-out connection, an unreachable host or network, or a reset or timeout in the TLS handshake or
    ///   the proxy tunnel. Server errors, and timeouts and broken connections after the request was written, are not
    ///   retried. A response body cut off mid-read is not retried either, but the <see cref="ConnectionException" />
    ///   has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    /// <param name="fileName">Name of the TMX file to import, for example "legal.tmx".</param>
    /// <param name="contentLength">Size of the TMX file in bytes.</param>
    /// <param name="contentType">Optional MIME type of the file, defaults to "application/xml".</param>
    /// <param name="displayName">
    ///   Optional name for the resulting translation memory, defaults to the file name.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryImport" /> object with the job ID and upload URL.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="fileName" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemoryImport> CreateTranslationMemoryImportAsync(
          string fileName,
          long contentLength,
          string? contentType = null,
          string? displayName = null,
          CancellationToken cancellationToken = default);

    /// <summary>Creates an import job for a new translation memory.</summary>
    /// <remarks>
    ///   The job only declares the file; upload the TMX file itself to the returned upload URL with
    ///   <see cref="UploadTranslationMemoryFileAsync(TranslationMemoryImport, Stream, string, CancellationToken)" />,
    ///   then poll <see cref="GetTranslationMemoryJobAsync(string, CancellationToken)" /> for the outcome. Use
    ///   <see cref="ImportTranslationMemoryFromFilepathAsync(string, ImportTranslationMemoryOptions, CancellationToken)" />
    ///   to do all three steps at once.
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, and after a
    ///   network failure before any request byte is written, such as a failure to resolve the server name, a refused,
    ///   reset or timed-out connection, an unreachable host or network, or a reset or timeout in the TLS handshake or
    ///   the proxy tunnel. Server errors, and timeouts and broken connections after the request was written, are not
    ///   retried. A response body cut off mid-read is not retried either, but the <see cref="ConnectionException" />
    ///   has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    /// <param name="fileName">Name of the TMX file to import, for example "legal.tmx".</param>
    /// <param name="contentLength">Size of the TMX file in bytes.</param>
    /// <param name="options">
    ///   Optional <see cref="CreateTranslationMemoryImportOptions" />, whose <c>ExtraBodyParameters</c> are added last to
    ///   the JSON request body.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryImport" /> object with the job ID and upload URL.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="fileName" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemoryImport> CreateTranslationMemoryImportAsync(
          string fileName,
          long contentLength,
          CreateTranslationMemoryImportOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>Uploads a TMX file to the upload URL of an import job, which starts processing.</summary>
    /// <param name="translationMemoryImport">
    ///   The <see cref="TranslationMemoryImport" /> returned by <see cref="CreateTranslationMemoryImportAsync" />.
    /// </param>
    /// <param name="fileContent">Stream containing the TMX file content.</param>
    /// <param name="contentType">
    ///   MIME type of the file, which must match the content type declared when the import job was created.
    ///   <c>null</c> uses the default.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="ArgumentNullException">
    ///   If <paramref name="translationMemoryImport" /> or <paramref name="fileContent" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, for example the upload URL is empty or not an absolute http or https URL.
    /// </exception>
    /// <exception cref="DeepLException">If any error occurs while uploading the file.</exception>
    Task UploadTranslationMemoryFileAsync(
          TranslationMemoryImport translationMemoryImport,
          Stream fileContent,
          string? contentType = "application/xml",
          CancellationToken cancellationToken = default);

    /// <summary>Uploads a TMX file to the upload URL of an import job, which starts processing.</summary>
    /// <param name="uploadUrl">
    ///   The <see cref="TranslationMemoryImport.UploadUrl" /> of the import job. This is a pre-signed storage URL
    ///   outside the DeepL API, so the DeepL authentication key is not sent to it.
    /// </param>
    /// <param name="fileContent">Stream containing the TMX file content.</param>
    /// <param name="contentType">
    ///   MIME type of the file, which must match the content type declared when the import job was created.
    ///   <c>null</c> uses the default.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="ArgumentNullException">
    ///   If <paramref name="uploadUrl" /> or <paramref name="fileContent" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   If any argument is invalid, for example the upload URL is empty or not an absolute http or https URL.
    /// </exception>
    /// <exception cref="DeepLException">If any error occurs while uploading the file.</exception>
    Task UploadTranslationMemoryFileAsync(
          string uploadUrl,
          Stream fileContent,
          string? contentType = "application/xml",
          CancellationToken cancellationToken = default);

    /// <summary>Creates an export job for a translation memory.</summary>
    /// <remarks>
    ///   Poll <see cref="GetTranslationMemoryJobAsync" /> for the download URL of the exported TMX file. Use
    ///   <see cref="ExportTranslationMemoryToFilepathAsync(string, string, CancellationToken)" /> to do both steps
    ///   and write the file at once.
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, and after a
    ///   network failure before any request byte is written, such as a failure to resolve the server name, a refused,
    ///   reset or timed-out connection, an unreachable host or network, or a reset or timeout in the TLS handshake or
    ///   the proxy tunnel. Server errors, and timeouts and broken connections after the request was written, are not
    ///   retried. A response body cut off mid-read is not retried either, but the <see cref="ConnectionException" />
    ///   has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    /// <param name="translationMemoryId">The ID of the translation memory to export.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryExport" /> object with the job ID.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="translationMemoryId" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemoryExport> CreateTranslationMemoryExportAsync(
          string translationMemoryId,
          CancellationToken cancellationToken = default);

    /// <summary>Creates an export job for the given translation memory.</summary>
    /// <param name="translationMemory">The translation memory to export.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryExport" /> object with the job ID.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="translationMemory" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, and after a
    ///   network failure before any request byte is written, such as a failure to resolve the server name, a refused,
    ///   reset or timed-out connection, an unreachable host or network, or a reset or timeout in the TLS handshake or
    ///   the proxy tunnel. Server errors, and timeouts and broken connections after the request was written, are not
    ///   retried. A response body cut off mid-read is not retried either, but the <see cref="ConnectionException" />
    ///   has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    Task<TranslationMemoryExport> CreateTranslationMemoryExportAsync(
          TranslationMemoryInfo translationMemory,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves the status of a translation memory import or export job.</summary>
    /// <param name="jobId">
    ///   The ID of the job to query; a <see cref="TranslationMemoryJob" /> converts implicitly to its ID.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object with the current status.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="jobId" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified job was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemoryJob> GetTranslationMemoryJobAsync(
          string jobId,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves the current status of the given translation memory import or export job.</summary>
    /// <param name="job">The job to query.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object with the current status.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="job" /> is <c>null</c>.</exception>
    /// <exception cref="NotFoundException">If the specified job was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemoryJob> GetTranslationMemoryJobAsync(
          TranslationMemoryJob job,
          CancellationToken cancellationToken = default);

    /// <summary>Polls a translation memory job until it finishes, and returns its final status.</summary>
    /// <remarks>
    ///   Note that an import job keeps reporting <see cref="TranslationMemoryJobStatus.AwaitingInput" /> for a while
    ///   after its file has been uploaded, because the DeepL API detects the upload asynchronously. That status is
    ///   therefore polled through like any other non-terminal one. A job whose file is never uploaded does not
    ///   finish on its own, so pass a <paramref name="cancellationToken" /> carrying a timeout when that is a
    ///   possibility.
    /// </remarks>
    /// <param name="jobId">
    ///   The ID of the job to wait for; a <see cref="TranslationMemoryJob" /> converts implicitly to its ID.
    /// </param>
    /// <param name="cancellationToken">
    ///   The cancellation token to cancel operation. Note that cancellation is not accurate to the second, but only
    ///   observed every 5 seconds between polls.
    /// </param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object holding the status once the job has finished.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="jobId" /> is <c>null</c>.</exception>
    /// <exception cref="DeepLException">
    ///   If the job failed or expired, or if any error occurs while communicating with the DeepL API.
    /// </exception>
    Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          string jobId,
          CancellationToken cancellationToken = default);

    /// <summary>Polls a translation memory job until it finishes, for at most the given time.</summary>
    /// <remarks>
    ///   An import job keeps reporting <see cref="TranslationMemoryJobStatus.AwaitingInput" /> for a while after its
    ///   file has been uploaded, and a status this library does not know is treated as in progress; both are polled
    ///   through. A job whose file is never uploaded does not finish on its own, so pass a timeout when that is a
    ///   possibility.
    /// </remarks>
    /// <param name="jobId">The ID of the job to wait for.</param>
    /// <param name="timeout">Maximum time to wait, or <c>null</c> to wait without a limit.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object holding the status once the job has finished.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="jobId" /> is <c>null</c>.</exception>
    /// <exception cref="DeepLException">
    ///   If the job failed or expired, the <paramref name="timeout" /> expires before it finishes, or any error occurs
    ///   while communicating with the DeepL API.
    /// </exception>
    Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          string jobId,
          TimeSpan? timeout,
          CancellationToken cancellationToken = default);

    /// <summary>Polls the given translation memory job until it finishes, and returns its final status.</summary>
    /// <param name="job">The job to wait for.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object holding the status once the job has finished.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="job" /> is <c>null</c>.</exception>
    /// <exception cref="DeepLException">
    ///   If the job failed or expired, or if any error occurs while communicating with the DeepL API.
    /// </exception>
    Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          TranslationMemoryJob job,
          CancellationToken cancellationToken = default);

    /// <summary>Polls the given translation memory job until it finishes, for at most the given time.</summary>
    /// <param name="job">The job to wait for.</param>
    /// <param name="timeout">Maximum time to wait, or <c>null</c> to wait without a limit.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object holding the status once the job has finished.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="job" /> is <c>null</c>.</exception>
    /// <exception cref="DeepLException">
    ///   If the job failed or expired, the <paramref name="timeout" /> expires before it finishes, or any error occurs
    ///   while communicating with the DeepL API.
    /// </exception>
    Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          TranslationMemoryJob job,
          TimeSpan? timeout,
          CancellationToken cancellationToken = default);

    /// <summary>Downloads the TMX file of a completed export job.</summary>
    /// <param name="job">Completed export job carrying the download URL.</param>
    /// <param name="outputFile">Stream to write the downloaded TMX file to.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="ArgumentNullException">
    ///   If <paramref name="job" /> or <paramref name="outputFile" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If the job carries no download URL, or any error occurs while downloading the file.
    /// </exception>
    Task DownloadTranslationMemoryExportAsync(
          TranslationMemoryJob job,
          Stream outputFile,
          CancellationToken cancellationToken = default);

    /// <summary>Downloads the TMX file of a completed export job.</summary>
    /// <param name="job">Completed export job carrying the download URL.</param>
    /// <param name="outputFilePath">Path of the file to write the downloaded TMX file to.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="job" /> is <c>null</c>.</exception>
    /// <exception cref="DeepLException">
    ///   If the job carries no download URL, or any error occurs while downloading the file.
    /// </exception>
    Task DownloadTranslationMemoryExportAsync(
          TranslationMemoryJob job,
          string outputFilePath,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Imports a TMX file as a new translation memory: reads the file, creates the import job, uploads the file, and
    ///   waits for processing to finish.
    /// </summary>
    /// <param name="inputFilePath">Path of the TMX file to import.</param>
    /// <param name="displayName">
    ///   Optional name for the resulting translation memory, defaults to the file name.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   A <see cref="TranslationMemoryJob" /> object for the completed import; its result carries the ID of the
    ///   new translation memory.
    /// </returns>
    /// <exception cref="ArgumentException">If the input file does not exist; raised before any request.</exception>
    /// <exception cref="TranslationMemoryJobException">
    ///   If any other step fails; the cause is the <see cref="System.Exception.InnerException" />: the input file is
    ///   empty or cannot be read (checked before any job is created; an <see cref="IOException" /> or
    ///   <see cref="System.UnauthorizedAccessException" /> cause), an argument is invalid, the import job failed, or
    ///   any error occurs while communicating with the DeepL API. Once the job was created,
    ///   <see cref="TranslationMemoryJobException.JobId" /> is set, and <see cref="TranslationMemoryJobException.Job" />
    ///   holds its last retrieved state. Cancellation through <paramref name="cancellationToken" /> is not wrapped.
    /// </exception>
    Task<TranslationMemoryJob> ImportTranslationMemoryFromFilepathAsync(
          string inputFilePath,
          string? displayName = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Imports a TMX file as a new translation memory: reads the file, creates the import job, uploads the file, and
    ///   waits for processing to finish.
    /// </summary>
    /// <param name="inputFilePath">Path of the TMX file to import.</param>
    /// <param name="options">
    ///   Optional <see cref="ImportTranslationMemoryOptions" />: display name, content type (declared on the import job
    ///   and sent with the upload), wait timeout, and <c>ExtraBodyParameters</c> added last to the import job request.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///   A <see cref="TranslationMemoryJob" /> object for the completed import; its result carries the ID of the
    ///   new translation memory.
    /// </returns>
    /// <exception cref="ArgumentException">If the input file does not exist; raised before any request.</exception>
    /// <exception cref="TranslationMemoryJobException">
    ///   If any other step fails; the cause is the <see cref="System.Exception.InnerException" />: the input file is
    ///   empty or cannot be read (checked before any job is created; an <see cref="IOException" /> or
    ///   <see cref="System.UnauthorizedAccessException" /> cause), an argument is invalid, the import job failed or the timeout expired, or
    ///   any error occurs while communicating with the DeepL API. Once the job was created,
    ///   <see cref="TranslationMemoryJobException.JobId" /> is set, and <see cref="TranslationMemoryJobException.Job" />
    ///   holds its last retrieved state. Cancellation through <paramref name="cancellationToken" /> is not wrapped.
    /// </exception>
    Task<TranslationMemoryJob> ImportTranslationMemoryFromFilepathAsync(
          string inputFilePath,
          ImportTranslationMemoryOptions? options,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Exports a translation memory to a TMX file: creates the output file, creates the export job, waits for it to
    ///   finish, and writes the result to the file.
    /// </summary>
    /// <param name="translationMemoryId">The ID of the translation memory to export.</param>
    /// <param name="outputFilePath">
    ///   Path of the file to write the exported TMX file to. The file must not exist yet; it is deleted again if the
    ///   export fails.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object for the completed export.</returns>
    /// <exception cref="TranslationMemoryJobException">
    ///   If any step fails; the cause is the <see cref="System.Exception.InnerException" />: the output file already
    ///   exists, cannot be created or cannot be written (it is created before any job is created; an
    ///   <see cref="IOException" /> or <see cref="System.UnauthorizedAccessException" /> cause), an argument is invalid
    ///   (an <see cref="ArgumentNullException" /> cause for a <c>null</c> translation memory or ID),
    ///   the translation memory was not found (a <see cref="NotFoundException" /> cause), the export job failed, or any
    ///   error occurs while communicating with the DeepL API. Once the job was created,
    ///   <see cref="TranslationMemoryJobException.JobId" /> is set, and <see cref="TranslationMemoryJobException.Job" />
    ///   holds its last retrieved state. Cancellation through <paramref name="cancellationToken" /> is not wrapped.
    /// </exception>
    Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          string translationMemoryId,
          string outputFilePath,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Exports the given translation memory to a TMX file: creates the output file, creates the export job, waits for
    ///   it to finish, and writes the result to the file.
    /// </summary>
    /// <param name="translationMemory">The translation memory to export.</param>
    /// <param name="outputFilePath">
    ///   Path of the file to write the exported TMX file to. The file must not exist yet; it is deleted again if the
    ///   export fails.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object for the completed export.</returns>
    /// <exception cref="TranslationMemoryJobException">
    ///   If any step fails; the cause is the <see cref="System.Exception.InnerException" />: the output file already
    ///   exists, cannot be created or cannot be written (it is created before any job is created; an
    ///   <see cref="IOException" /> or <see cref="System.UnauthorizedAccessException" /> cause), an argument is invalid
    ///   (an <see cref="ArgumentNullException" /> cause for a <c>null</c> translation memory or ID),
    ///   the translation memory was not found (a <see cref="NotFoundException" /> cause), the export job failed, or any
    ///   error occurs while communicating with the DeepL API. Once the job was created,
    ///   <see cref="TranslationMemoryJobException.JobId" /> is set, and <see cref="TranslationMemoryJobException.Job" />
    ///   holds its last retrieved state. Cancellation through <paramref name="cancellationToken" /> is not wrapped.
    /// </exception>
    Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          TranslationMemoryInfo translationMemory,
          string outputFilePath,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Exports a translation memory to a TMX file: creates the output file, creates the export job, waits for it to
    ///   finish for at most the given time, and writes the result to the file.
    /// </summary>
    /// <param name="translationMemoryId">The ID of the translation memory to export.</param>
    /// <param name="outputFilePath">
    ///   Path of the file to write the exported TMX file to. The file must not exist yet; it is deleted again if the
    ///   export fails.
    /// </param>
    /// <param name="timeout">Maximum time to wait for the export job, or <c>null</c> to wait without a limit.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object for the completed export.</returns>
    /// <exception cref="TranslationMemoryJobException">
    ///   If any step fails; the cause is the <see cref="System.Exception.InnerException" />: the output file already
    ///   exists, cannot be created or cannot be written (it is created before any job is created; an
    ///   <see cref="IOException" /> or <see cref="System.UnauthorizedAccessException" /> cause), an argument is invalid
    ///   (an <see cref="ArgumentNullException" /> cause for a <c>null</c> translation memory or ID),
    ///   the translation memory was not found (a <see cref="NotFoundException" /> cause), the export job failed, the <paramref name="timeout" /> expired, or any
    ///   error occurs while communicating with the DeepL API. Once the job was created,
    ///   <see cref="TranslationMemoryJobException.JobId" /> is set, and <see cref="TranslationMemoryJobException.Job" />
    ///   holds its last retrieved state. Cancellation through <paramref name="cancellationToken" /> is not wrapped.
    /// </exception>
    Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          string translationMemoryId,
          string outputFilePath,
          TimeSpan? timeout,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Exports the given translation memory to a TMX file: creates the output file, creates the export job, waits for
    ///   it to finish for at most the given time, and writes the result to the file.
    /// </summary>
    /// <param name="translationMemory">The translation memory to export.</param>
    /// <param name="outputFilePath">
    ///   Path of the file to write the exported TMX file to. The file must not exist yet; it is deleted again if the
    ///   export fails.
    /// </param>
    /// <param name="timeout">Maximum time to wait for the export job, or <c>null</c> to wait without a limit.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object for the completed export.</returns>
    /// <exception cref="TranslationMemoryJobException">
    ///   If any step fails; the cause is the <see cref="System.Exception.InnerException" />: the output file already
    ///   exists, cannot be created or cannot be written (it is created before any job is created; an
    ///   <see cref="IOException" /> or <see cref="System.UnauthorizedAccessException" /> cause), an argument is invalid
    ///   (an <see cref="ArgumentNullException" /> cause for a <c>null</c> translation memory or ID),
    ///   the translation memory was not found (a <see cref="NotFoundException" /> cause), the export job failed, the <paramref name="timeout" /> expired, or any
    ///   error occurs while communicating with the DeepL API. Once the job was created,
    ///   <see cref="TranslationMemoryJobException.JobId" /> is set, and <see cref="TranslationMemoryJobException.Job" />
    ///   holds its last retrieved state. Cancellation through <paramref name="cancellationToken" /> is not wrapped.
    /// </exception>
    Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          TranslationMemoryInfo translationMemory,
          string outputFilePath,
          TimeSpan? timeout,
          CancellationToken cancellationToken = default);
  }
}
