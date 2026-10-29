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
    ///   (connect, DNS, timeout). <c>false</c> for other HTTP statuses; <c>null</c> if unknown. For requests that are
    ///   unsafe to repeat (<see cref="DeepLAdminClient.CreateDeveloperKeyAsync" />), it is <c>true</c> only when the
    ///   request was certainly not processed: HTTP 429, a name resolution failure or a refused connection.
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

  /// <summary>Exception thrown when no glossary could be found with the specified ID.</summary>
  public sealed class GlossaryNotFoundException : NotFoundException {
    /// <summary>Initializes a new instance of the <see cref="GlossaryNotFoundException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public GlossaryNotFoundException(string message) : base(message) {
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
    /// <param name="innerException">The exception representing the connection error.</param>
    /// <param name="documentHandle">
    ///   Handle to the in-progress document translation, or null if an error occurred before
    ///   uploading the document.
    /// </param>
    public DocumentTranslationException(string message, Exception innerException, DocumentHandle? documentHandle) :
          base(message, innerException) {
      DocumentHandle = documentHandle;
    }

    /// <summary>Handle to the in-progress document translation, or null if an error occurred before uploading the document.</summary>
    /// The handle can be used to later retrieve the document or to contact DeepL support.
    public DocumentHandle? DocumentHandle { get; }
  }

  /// <summary>
  ///   Exception thrown if an error occurs during the minification phase of document minification.
  ///   See <see cref="DocumentMinifier.MinifyDocument"/>
  /// </summary>
  [Obsolete("Document minification is deprecated and will be removed in the next major version. The DeepL API accepts larger documents; see the usage limits.")]
  public sealed class DocumentMinificationException : DeepLException {
    /// <summary>Initializes a new instance of the <see cref="DocumentMinificationException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception representing the connection error.</param>
    public DocumentMinificationException(string message, Exception innerException) :
          base(message, innerException) {
    }
  }

  /// <summary>
  ///   Exception thrown if an error occurs during the minification phase of document deminification.
  ///   See <see cref="DocumentMinifier.DeminifyDocument"/>
  /// </summary>
  [Obsolete("Document minification is deprecated and will be removed in the next major version. The DeepL API accepts larger documents; see the usage limits.")]
  public sealed class DocumentDeminificationException : DeepLException {
    /// <summary>Initializes a new instance of the <see cref="DocumentDeminificationException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception representing the connection error.</param>
    public DocumentDeminificationException(string message, Exception innerException) :
          base(message, innerException) {
    }
  }
}
