// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;

namespace DeepL {
  /// <summary>
  ///   Options for <see cref="DeepLClient.ListTranslationMemorySegmentsAsync(string, TranslationMemorySegmentsOptions?, System.Threading.CancellationToken)" />.
  ///   A property left <c>null</c> is not sent.
  /// </summary>
  public sealed class TranslationMemorySegmentsOptions {
    /// <summary>Initializes a new <see cref="TranslationMemorySegmentsOptions" /> object.</summary>
    public TranslationMemorySegmentsOptions() { }

    /// <summary>Maximum number of segments per page (1-100, the API defaults to 50).</summary>
    public int? PageSize { get; set; }

    /// <summary>
    ///   Cursor from the <see cref="Model.TranslationMemorySegments.NextPageCursor" /> of a previous response; omit on
    ///   the first call.
    /// </summary>
    public string? PageCursor { get; set; }

    /// <summary>Substring filter applied to source and target text, at least 2 characters.</summary>
    public string? FilterText { get; set; }

    /// <summary>Whether the text filter is case-sensitive, the API defaults to <c>false</c>.</summary>
    public bool? FilterCaseSensitive { get; set; }
  }

  /// <summary>
  ///   Options for <see cref="DeepLClient.CreateTranslationMemoryImportAsync(string, long, CreateTranslationMemoryImportOptions?, System.Threading.CancellationToken)" />.
  ///   A property left <c>null</c> is not sent; <see cref="BaseRequestOptions.ExtraBodyParameters" /> are added last
  ///   to the top level of the JSON request body.
  /// </summary>
  public sealed class CreateTranslationMemoryImportOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="CreateTranslationMemoryImportOptions" /> object.</summary>
    public CreateTranslationMemoryImportOptions() { }

    /// <summary>MIME type of the file; when not set, the API uses <c>application/xml</c>.</summary>
    public string? ContentType { get; set; }

    /// <summary>Name for the resulting translation memory; when not set, the API uses the file name.</summary>
    public string? DisplayName { get; set; }
  }

  /// <summary>
  ///   Options for <see cref="DeepLClient.ImportTranslationMemoryFromFilepathAsync(string, ImportTranslationMemoryOptions?, System.Threading.CancellationToken)" />.
  ///   <see cref="BaseRequestOptions.ExtraBodyParameters" /> are added last to the JSON body of the import job
  ///   creation request.
  /// </summary>
  public sealed class ImportTranslationMemoryOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="ImportTranslationMemoryOptions" /> object.</summary>
    public ImportTranslationMemoryOptions() { }

    /// <summary>Name for the resulting translation memory; when not set, the API uses the file name.</summary>
    public string? DisplayName { get; set; }

    /// <summary>
    ///   MIME type of the file, declared when creating the import job and sent with the upload. When not set, it is not
    ///   declared and the file is uploaded as <c>application/xml</c>, the API's default.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    ///   Maximum time to wait for the import job to finish once the file is uploaded, or <c>null</c> to wait without a
    ///   limit. When it expires, a <see cref="DeepLException" /> is thrown.
    /// </summary>
    public TimeSpan? Timeout { get; set; }
  }
}
