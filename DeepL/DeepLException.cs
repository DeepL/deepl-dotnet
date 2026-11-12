// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using DeepL.Model;

namespace DeepL {
  /// <summary>Base class for all exceptions thrown by this library.</summary>
  public class DeepLException : Exception {
    /// <summary>Initializes a new instance of the <see cref="AuthorizationException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public DeepLException(string message) : base(message) {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="AuthorizationException" /> class including the exception that
    ///   caused this one.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">Exception that is the cause of this exception.</param>
    public DeepLException(string message, Exception innerException) : base(message, innerException) {
    }

    /// <summary>HTTP status code of the failed response, or <c>null</c> if the error has no HTTP response.</summary>
    public int? HttpStatusCode { get; internal set; }

    /// <summary>
    ///   <c>true</c> if repeating the request could succeed: HTTP 429, any 5xx status, and transient network failures
    ///   (connect, DNS, timeout, connection reset). <c>false</c> for other HTTP statuses and for TLS, certificate and
    ///   proxy failures; <c>null</c> if unknown. For operations that create a resource (document upload, developer
    ///   key, glossary, style rule, custom instruction, style profile, spoken terms and translation memory import and
    ///   export creates), it is <c>true</c> only when the request was certainly not processed: HTTP 429, a name
    ///   resolution failure, a refused connection, an unreachable host or network or a connect timeout. A response body
    ///   cut off mid-read is always <c>true</c>, also for these creates, which do not retry it themselves.
    /// </summary>
    public bool? ShouldRetry { get; internal set; }
  }

  /// <summary>Exception thrown when the specified authentication key was invalid.</summary>
  public sealed class AuthorizationException : DeepLException {
    /// <summary>Initializes a new instance of the <see cref="AuthorizationException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public AuthorizationException(string message) : base(message) {
    }
  }

  /// <summary>Exception thrown when the specified resource could not be found.</summary>
  public class NotFoundException : DeepLException {
    /// <summary>Initializes a new instance of the <see cref="NotFoundException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public NotFoundException(string message) : base(message) {
    }
  }

  /// <summary>Exception thrown when the DeepL translation quota has been reached.</summary>
  public sealed class QuotaExceededException : DeepLException {
    /// <summary>Initializes a new instance of the <see cref="QuotaExceededException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public QuotaExceededException(string message) : base(message) {
    }
  }

  /// <summary>Exception thrown when too many requests are made to the DeepL API too quickly.</summary>
  public sealed class TooManyRequestsException : DeepLException {
    /// <summary>Initializes a new instance of the <see cref="TooManyRequestsException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public TooManyRequestsException(string message) : base(message) {
    }
  }

  /// <summary>Exception thrown when a connection error occurs while accessing the DeepL API.</summary>
  public sealed class ConnectionException : DeepLException {
    /// <summary>Initializes a new instance of the <see cref="ConnectionException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception representing the connection error.</param>
    public ConnectionException(string message, Exception innerException) : base(
          message,
          innerException) {
    }
  }

  /// <summary>Exception thrown when attempting to download a translated document before it is ready.</summary>
  public sealed class DocumentNotReadyException : DeepLException {
    /// <summary>Initializes a new instance of the <see cref="DocumentNotReadyException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public DocumentNotReadyException(string message) : base(message) { }
  }

  /// <summary>
  ///   Exception thrown when an error occurs related to a document translation. If the error occurs after the
  ///   document was successfully uploaded, the <see cref="DocumentHandle" /> for the associated document is included,
  ///   to allow later retrieval of the document.
  /// </summary>
  public sealed class DocumentTranslationException : DeepLException {
    /// <summary>Initializes a new instance of the <see cref="DocumentTranslationException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    ///   The exception that caused this one. If it is a <see cref="DeepLException" />, its
    ///   <see cref="DeepLException.HttpStatusCode" /> and <see cref="DeepLException.ShouldRetry" /> are copied.
    /// </param>
    /// <param name="documentHandle">
    ///   Handle to the in-progress document translation, or null if an error occurred before
    ///   uploading the document.
    /// </param>
    public DocumentTranslationException(string message, Exception innerException, DocumentHandle? documentHandle) :
          base(message, innerException) {
      DocumentHandle = documentHandle;
      if (innerException is DeepLException deepLException) {
        HttpStatusCode = deepLException.HttpStatusCode;
        ShouldRetry = deepLException.ShouldRetry;
      }
    }

    /// <summary>Handle to the in-progress document translation, or null if an error occurred before uploading the document.</summary>
    /// The handle can be used to later retrieve the document or to contact DeepL support.
    public DocumentHandle? DocumentHandle { get; }
  }
  /// <summary>
  ///   Exception thrown by the translation memory file helpers
  ///   (<see cref="DeepLClient.ImportTranslationMemoryFromFilepathAsync(string, ImportTranslationMemoryOptions, System.Threading.CancellationToken)" />,
  ///   <see cref="DeepLClient.ExportTranslationMemoryToFilepathAsync(string, string, System.TimeSpan?, System.Threading.CancellationToken)" />
  ///   and their overloads) when any step fails. The cause is the <see cref="Exception.InnerException" />. Once the job
  ///   was created, <see cref="JobId" /> is set, and <see cref="Job" /> holds its last known state, so the job can be
  ///   inspected or awaited again.
  /// </summary>
  public sealed class TranslationMemoryJobException : DeepLException {
    /// <summary>Initializes a new instance of the <see cref="TranslationMemoryJobException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    ///   The exception that caused this one. If it is a <see cref="DeepLException" />, its
    ///   <see cref="DeepLException.HttpStatusCode" /> and <see cref="DeepLException.ShouldRetry" /> are copied.
    /// </param>
    /// <param name="jobId">ID of the translation memory job, or null if the error occurred before it was created.</param>
    /// <param name="job">Last known state of the job, or null if it was not retrieved before the error.</param>
    public TranslationMemoryJobException(
          string message,
          Exception innerException,
          string? jobId,
          TranslationMemoryJob? job) : base(message, innerException) {
      JobId = jobId ?? job?.JobId;
      Job = job;
      if (innerException is DeepLException deepLException) {
        HttpStatusCode = deepLException.HttpStatusCode;
        ShouldRetry = deepLException.ShouldRetry;
      }
    }

    /// <summary>ID of the translation memory job, or null if the error occurred before the job was created.</summary>
    public string? JobId { get; }

    /// <summary>
    ///   Last known state of the translation memory job, for example the failed job, or null if the error occurred
    ///   before the job state was retrieved (before the job was created, or while uploading the file to import).
    /// </summary>
    public TranslationMemoryJob? Job { get; }
  }
}
