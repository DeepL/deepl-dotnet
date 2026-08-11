// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>Status of a translation memory import or export job.</summary>
  [JsonConverter(typeof(TranslationMemoryJobStatusConverter))]
  public enum TranslationMemoryJobStatus {
    /// <summary>The import job is waiting for its TMX file to be uploaded, and will not progress until it is.</summary>
    AwaitingInput,

    /// <summary>The job is being processed.</summary>
    Processing,

    /// <summary>The job finished successfully.</summary>
    Completed,

    /// <summary>The exported file has already been downloaded.</summary>
    Downloaded,

    /// <summary>An error occurred while processing the job.</summary>
    Failed,

    /// <summary>The job expired before it finished.</summary>
    Expired
  }

  /// <summary>
  ///   Converts between <see cref="TranslationMemoryJobStatus" /> values and the lower-snake-case strings used by
  ///   the DeepL API. A dedicated converter is needed because <see cref="JsonStringEnumConverter" /> only matches
  ///   enum names ignoring case, which does not cover the underscore in "awaiting_input".
  /// </summary>
  internal sealed class TranslationMemoryJobStatusConverter : JsonConverter<TranslationMemoryJobStatus> {
    public override TranslationMemoryJobStatus Read(
          ref Utf8JsonReader reader,
          Type typeToConvert,
          JsonSerializerOptions options) =>
          reader.GetString() switch {
            "awaiting_input" => TranslationMemoryJobStatus.AwaitingInput,
            "processing" => TranslationMemoryJobStatus.Processing,
            "completed" => TranslationMemoryJobStatus.Completed,
            "downloaded" => TranslationMemoryJobStatus.Downloaded,
            "failed" => TranslationMemoryJobStatus.Failed,
            "expired" => TranslationMemoryJobStatus.Expired,
            var status => throw new JsonException($"Unrecognized translation memory job status: {status}")
          };

    public override void Write(
          Utf8JsonWriter writer,
          TranslationMemoryJobStatus value,
          JsonSerializerOptions options) =>
          writer.WriteStringValue(
                value switch {
                  TranslationMemoryJobStatus.AwaitingInput => "awaiting_input",
                  TranslationMemoryJobStatus.Processing => "processing",
                  TranslationMemoryJobStatus.Completed => "completed",
                  TranslationMemoryJobStatus.Downloaded => "downloaded",
                  TranslationMemoryJobStatus.Failed => "failed",
                  TranslationMemoryJobStatus.Expired => "expired",
                  _ => throw new JsonException($"Unrecognized translation memory job status: {value}")
                });
  }

  /// <summary>The outcome of a translation memory import or export job.</summary>
  public sealed class TranslationMemoryJobResult {
    /// <summary>Initializes a new instance of <see cref="TranslationMemoryJobResult" />.</summary>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally
    ///   it would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON
    ///   deserialization. In future this function may have backwards-incompatible changes.
    /// </remarks>
    public TranslationMemoryJobResult(
          TranslationMemoryJobStatus status,
          string? requiredAction = null,
          string? downloadUrl = null,
          DateTime? expiresAt = null,
          string? errorMessage = null,
          string? translationMemoryId = null,
          int? skippedSegmentCount = null) {
      Status = status;
      RequiredAction = requiredAction;
      DownloadUrl = downloadUrl;
      ExpiresAt = expiresAt;
      ErrorMessage = errorMessage;
      TranslationMemoryId = translationMemoryId;
      SkippedSegmentCount = skippedSegmentCount;
    }

    /// <summary>Status of the job.</summary>
    public TranslationMemoryJobStatus Status { get; }

    /// <summary>Description of the action the caller must take, set while the job waits on the caller.</summary>
    public string? RequiredAction { get; }

    /// <summary>Download URL of the exported TMX file, set once an export completes.</summary>
    public string? DownloadUrl { get; }

    /// <summary>Time after which the download URL is no longer valid.</summary>
    public DateTime? ExpiresAt { get; }

    /// <summary>Short description of the error, if the job failed.</summary>
    public string? ErrorMessage { get; }

    /// <summary>ID of the translation memory created by a completed import.</summary>
    public string? TranslationMemoryId { get; }

    /// <summary>Number of segments an import skipped.</summary>
    public int? SkippedSegmentCount { get; }

    /// <summary><c>true</c> if the job has finished, successfully or not, otherwise <c>false</c>.</summary>
    public bool Done =>
          Status == TranslationMemoryJobStatus.Completed ||
          Status == TranslationMemoryJobStatus.Downloaded ||
          Status == TranslationMemoryJobStatus.Failed ||
          Status == TranslationMemoryJobStatus.Expired;

    /// <summary><c>true</c> if no error has occurred during the job, otherwise <c>false</c>.</summary>
    public bool Ok =>
          Status != TranslationMemoryJobStatus.Failed && Status != TranslationMemoryJobStatus.Expired;

    /// <summary>Returns a string describing the job result.</summary>
    public override string ToString() => $"TranslationMemoryJobResult ({Status})";
  }

  /// <summary>Status of a translation memory import or export job.</summary>
  public sealed class TranslationMemoryJob {
    /// <summary>Initializes a new instance of <see cref="TranslationMemoryJob" />.</summary>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally
    ///   it would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON
    ///   deserialization. In future this function may have backwards-incompatible changes.
    /// </remarks>
    public TranslationMemoryJob(
          string jobId,
          string operation,
          TranslationMemoryJobResult[] results,
          string? product = null,
          DateTime? creationTime = null,
          DateTime? updatedTime = null,
          string? translationMemoryId = null,
          string? displayName = null,
          string? sourceContentType = null,
          long? sourceContentLength = null) {
      JobId = jobId;
      Operation = operation;
      Results = results;
      Product = product;
      CreationTime = creationTime;
      UpdatedTime = updatedTime;
      TranslationMemoryId = translationMemoryId;
      DisplayName = displayName;
      SourceContentType = sourceContentType;
      SourceContentLength = sourceContentLength;
    }

    /// <summary>Unique ID assigned to the job.</summary>
    public string JobId { get; }

    /// <summary>Operation the job performs, either "import" or "export".</summary>
    public string Operation { get; }

    /// <summary>Results of the job; the DeepL API returns exactly one.</summary>
    public TranslationMemoryJobResult[] Results { get; }

    /// <summary>Product the job belongs to, always "translation_memory".</summary>
    public string? Product { get; }

    /// <summary>Time when the job was created.</summary>
    public DateTime? CreationTime { get; }

    /// <summary>Time when the job was last updated.</summary>
    public DateTime? UpdatedTime { get; }

    /// <summary>ID of the translation memory an export job reads from.</summary>
    public string? TranslationMemoryId { get; }

    /// <summary>Name an import job assigns to the new translation memory.</summary>
    public string? DisplayName { get; }

    /// <summary>MIME type declared for the file of an import job.</summary>
    public string? SourceContentType { get; }

    /// <summary>Size in bytes declared for the file of an import job.</summary>
    public long? SourceContentLength { get; }

    /// <summary>The single result of the job, or <c>null</c> if the DeepL API returned none.</summary>
    public TranslationMemoryJobResult? Result => Results.Length > 0 ? Results[0] : null;

    /// <summary>Status of the job result, or <c>null</c> if the DeepL API returned no result.</summary>
    public TranslationMemoryJobStatus? Status => Result?.Status;

    /// <summary><c>true</c> if the job has finished, successfully or not, otherwise <c>false</c>.</summary>
    public bool Done => Result?.Done ?? false;

    /// <summary><c>true</c> if no error has occurred during the job, otherwise <c>false</c>.</summary>
    public bool Ok => Result?.Ok ?? true;

    /// <summary>Returns a string describing the job.</summary>
    public override string ToString() => $"TranslationMemoryJob {Operation} ({JobId}): {Status}";
  }

  /// <summary>A newly created translation memory import job.</summary>
  /// <remarks>
  ///   The TMX file must be uploaded to <see cref="UploadUrl" /> before <see cref="ExpiresAt" />; processing starts
  ///   automatically once the upload is detected.
  /// </remarks>
  public sealed class TranslationMemoryImport {
    /// <summary>Initializes a new instance of <see cref="TranslationMemoryImport" />.</summary>
    [JsonConstructor]
    public TranslationMemoryImport(string jobId, string uploadUrl, DateTime? expiresAt = null) {
      JobId = jobId;
      UploadUrl = uploadUrl;
      ExpiresAt = expiresAt;
    }

    /// <summary>Unique ID assigned to the import job.</summary>
    [JsonPropertyName("job_id")]
    public string JobId { get; }

    /// <summary>URL to upload the TMX file to.</summary>
    [JsonPropertyName("upload_url")]
    public string UploadUrl { get; }

    /// <summary>Time after which the upload URL is no longer valid.</summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; }

    /// <summary>Returns a string describing the import job.</summary>
    public override string ToString() => $"TranslationMemoryImport ({JobId})";
  }

  /// <summary>A translation memory export job.</summary>
  public sealed class TranslationMemoryExport {
    /// <summary>Initializes a new instance of <see cref="TranslationMemoryExport" />.</summary>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally
    ///   it would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON
    ///   deserialization. In future this function may have backwards-incompatible changes.
    /// </remarks>
    public TranslationMemoryExport(
          string jobId,
          string? translationMemoryId = null,
          bool reusedExisting = false) {
      JobId = jobId;
      TranslationMemoryId = translationMemoryId;
      ReusedExisting = reusedExisting;
    }

    /// <summary>Unique ID assigned to the export job.</summary>
    public string JobId { get; }

    /// <summary>ID of the translation memory being exported.</summary>
    public string? TranslationMemoryId { get; }

    /// <summary>
    ///   <c>true</c> if the DeepL API reused a previously completed export instead of starting a new one,
    ///   otherwise <c>false</c>.
    /// </summary>
    public bool ReusedExisting { get; }

    /// <summary>Returns a string describing the export job.</summary>
    public override string ToString() => $"TranslationMemoryExport ({JobId})";
  }
}
