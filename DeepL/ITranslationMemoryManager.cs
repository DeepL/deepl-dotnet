// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
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

    /// <summary>Deletes a translation memory.</summary>
    /// <param name="translationMemoryId">The ID of the translation memory to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
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
    /// </remarks>
    /// <param name="fileName">Name of the TMX file to import, for example "legal.tmx".</param>
    /// <param name="contentLength">Size of the TMX file in bytes.</param>
    /// <param name="contentType">Optional MIME type of the file, defaults to "application/xml".</param>
    /// <param name="displayName">
    ///   Optional name for the resulting translation memory, defaults to the file name.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryImport" /> object with the job ID and upload URL.</returns>
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

    /// <summary>Uploads a TMX file to the upload URL of an import job, which starts processing.</summary>
    /// <param name="translationMemoryImport">
    ///   The <see cref="TranslationMemoryImport" /> returned by <see cref="CreateTranslationMemoryImportAsync" />.
    /// </param>
    /// <param name="fileContent">Stream containing the TMX file content.</param>
    /// <param name="contentType">
    ///   MIME type of the file, which must match the content type declared when the import job was created.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">If any error occurs while uploading the file.</exception>
    Task UploadTranslationMemoryFileAsync(
          TranslationMemoryImport translationMemoryImport,
          Stream fileContent,
          string contentType = "application/xml",
          CancellationToken cancellationToken = default);

    /// <summary>Uploads a TMX file to the upload URL of an import job, which starts processing.</summary>
    /// <param name="uploadUrl">
    ///   The <see cref="TranslationMemoryImport.UploadUrl" /> of the import job. This is a pre-signed storage URL
    ///   outside the DeepL API, so the DeepL authentication key is not sent to it.
    /// </param>
    /// <param name="fileContent">Stream containing the TMX file content.</param>
    /// <param name="contentType">
    ///   MIME type of the file, which must match the content type declared when the import job was created.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">If any error occurs while uploading the file.</exception>
    Task UploadTranslationMemoryFileAsync(
          string uploadUrl,
          Stream fileContent,
          string contentType = "application/xml",
          CancellationToken cancellationToken = default);

    /// <summary>Creates an export job for a translation memory.</summary>
    /// <remarks>
    ///   Poll <see cref="GetTranslationMemoryJobAsync" /> for the download URL of the exported TMX file. Use
    ///   <see cref="ExportTranslationMemoryToFilepathAsync(string, string, CancellationToken)" /> to do both steps
    ///   and write the file at once.
    /// </remarks>
    /// <param name="translationMemoryId">The ID of the translation memory to export.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryExport" /> object with the job ID.</returns>
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
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemoryExport> CreateTranslationMemoryExportAsync(
          TranslationMemoryInfo translationMemory,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves the status of a translation memory import or export job.</summary>
    /// <param name="jobId">The ID of the job to query.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object with the current status.</returns>
    /// <exception cref="NotFoundException">If the specified job was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<TranslationMemoryJob> GetTranslationMemoryJobAsync(
          string jobId,
          CancellationToken cancellationToken = default);

    /// <summary>Polls a translation memory job until it finishes, and returns its final status.</summary>
    /// <remarks>
    ///   Note that an import job keeps reporting <see cref="TranslationMemoryJobStatus.AwaitingInput" /> for a while
    ///   after its file has been uploaded, because the DeepL API detects the upload asynchronously. That status is
    ///   therefore polled through like any other non-terminal one. A job whose file is never uploaded does not
    ///   finish on its own, so pass a <paramref name="cancellationToken" /> carrying a timeout when that is a
    ///   possibility.
    /// </remarks>
    /// <param name="jobId">The ID of the job to wait for.</param>
    /// <param name="cancellationToken">
    ///   The cancellation token to cancel operation. Note that cancellation is not accurate to the second, but only
    ///   observed every 5 seconds between polls.
    /// </param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object holding the status once the job has finished.</returns>
    /// <exception cref="DeepLException">
    ///   If the job failed or expired, or if any error occurs while communicating with the DeepL API.
    /// </exception>
    Task<TranslationMemoryJob> WaitUntilTranslationMemoryJobDoneAsync(
          string jobId,
          CancellationToken cancellationToken = default);

    /// <summary>Downloads the TMX file of a completed export job.</summary>
    /// <param name="job">Completed export job carrying the download URL.</param>
    /// <param name="outputFile">Stream to write the downloaded TMX file to.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="ArgumentException">If the job carries no download URL.</exception>
    /// <exception cref="DeepLException">If any error occurs while downloading the file.</exception>
    Task DownloadTranslationMemoryExportAsync(
          TranslationMemoryJob job,
          Stream outputFile,
          CancellationToken cancellationToken = default);

    /// <summary>Downloads the TMX file of a completed export job.</summary>
    /// <param name="job">Completed export job carrying the download URL.</param>
    /// <param name="outputFilePath">Path of the file to write the downloaded TMX file to.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="ArgumentException">If the job carries no download URL.</exception>
    /// <exception cref="DeepLException">If any error occurs while downloading the file.</exception>
    Task DownloadTranslationMemoryExportAsync(
          TranslationMemoryJob job,
          string outputFilePath,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Imports a TMX file as a new translation memory: creates the import job, uploads the file, and waits for
    ///   processing to finish.
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
    /// <exception cref="ArgumentException">If any argument is invalid.</exception>
    /// <exception cref="DeepLException">
    ///   If the import failed, or if any error occurs while communicating with the DeepL API.
    /// </exception>
    Task<TranslationMemoryJob> ImportTranslationMemoryFromFilepathAsync(
          string inputFilePath,
          string? displayName = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Exports a translation memory to a TMX file: creates the export job, waits for it to finish, and writes the
    ///   result to the given path.
    /// </summary>
    /// <param name="translationMemoryId">The ID of the translation memory to export.</param>
    /// <param name="outputFilePath">Path of the file to write the exported TMX file to.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object for the completed export.</returns>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If the export failed, or if any error occurs while communicating with the DeepL API.
    /// </exception>
    Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          string translationMemoryId,
          string outputFilePath,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Exports the given translation memory to a TMX file: creates the export job, waits for it to finish, and
    ///   writes the result to the given path.
    /// </summary>
    /// <param name="translationMemory">The translation memory to export.</param>
    /// <param name="outputFilePath">Path of the file to write the exported TMX file to.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="TranslationMemoryJob" /> object for the completed export.</returns>
    /// <exception cref="NotFoundException">If the specified translation memory was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If the export failed, or if any error occurs while communicating with the DeepL API.
    /// </exception>
    Task<TranslationMemoryJob> ExportTranslationMemoryToFilepathAsync(
          TranslationMemoryInfo translationMemory,
          string outputFilePath,
          CancellationToken cancellationToken = default);
  }
}
