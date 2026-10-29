// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>Status of an in-progress document translation.</summary>
  [JsonConverter(typeof(DocumentStatusConverter))]
  public sealed class DocumentStatus {
    /// <summary>Status code indicating status of the document translation.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusCode {
      /// <summary>Document translation has not yet started, but will begin soon.</summary>
      [EnumMember(Value = "queued")] Queued,

      /// <summary>Document translation is in progress.</summary>
      [EnumMember(Value = "translating")] Translating,

      /// <summary>Document translation completed successfully, and the translated document may be downloaded.</summary>
      [EnumMember(Value = "done")] Done,

      /// <summary>An error occurred during document translation.</summary>
      [EnumMember(Value = "error")] Error,

      /// <summary>
      ///   A status this library does not know; <see cref="StatusValue" /> holds the value the API returned. Document
      ///   translation is treated as still in progress.
      /// </summary>
      Unknown
    }

    /// <summary>Initializes a new <see cref="DocumentStatus" /> object for an in-progress document translation.</summary>
    /// <param name="documentId">Document ID of the associated document.</param>
    /// <param name="status">Status of the document translation.</param>
    /// <param name="secondsRemaining">
    ///   Number of seconds remaining until translation is complete, only included while
    ///   document is in translating state.
    /// </param>
    /// <param name="billedCharacters">
    ///   Number of characters billed for the translation of this document, only included
    ///   after document translation is finished and the state is done.
    /// </param>
    /// <param name="errorMessage">Short description of the error, if available.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public DocumentStatus(string documentId, StatusCode status, int? secondsRemaining, int? billedCharacters, string? errorMessage) {
      (DocumentId, Status, SecondsRemaining, BilledCharacters, ErrorMessage) =
            (documentId, status, secondsRemaining, billedCharacters, errorMessage);
      StatusValue = ToApiValue(status);
    }

    /// <summary>Initializes a new <see cref="DocumentStatus" /> from the status string returned by the API.</summary>
    internal DocumentStatus(
          string documentId,
          string statusValue,
          int? secondsRemaining,
          int? billedCharacters,
          string? errorMessage) : this(
          documentId,
          FromApiValue(statusValue),
          secondsRemaining,
          billedCharacters,
          errorMessage) {
      StatusValue = statusValue;
    }

    /// <summary>Document ID of the associated document.</summary>
    public string DocumentId { get; }

    /// <summary>Status of the document translation.</summary>
    public StatusCode Status { get; }

    /// <summary>
    ///   Status of the document translation as returned by the API, for example <c>"translating"</c>. Unlike
    ///   <see cref="Status" />, it also holds statuses this library does not know (<see cref="StatusCode.Unknown" />).
    /// </summary>
    public string StatusValue { get; }

    /// <summary>
    ///   Number of seconds remaining until translation is complete, only included while
    ///   document is in translating state.
    /// </summary>
    public int? SecondsRemaining { get; }

    /// <summary>
    ///   Number of characters billed for the translation of this document, only included
    ///   after document translation is finished and the state is done.
    /// </summary>
    public int? BilledCharacters { get; }

    /// <summary>Short description of the error, if available.</summary>
    public string? ErrorMessage { get; }

    /// <summary><c>true</c> if no error has occurred during document translation, otherwise <c>false</c>.</summary>
    public bool Ok => Status != StatusCode.Error;

    /// <summary><c>true</c> if document translation has completed successfully, otherwise <c>false</c>.</summary>
    public bool Done => Status == StatusCode.Done;

    private static StatusCode FromApiValue(string value) =>
          value switch {
            "queued" => StatusCode.Queued,
            "translating" => StatusCode.Translating,
            "done" => StatusCode.Done,
            "error" => StatusCode.Error,
            _ => StatusCode.Unknown
          };

    private static string ToApiValue(StatusCode status) =>
          status switch {
            StatusCode.Queued => "queued",
            StatusCode.Translating => "translating",
            StatusCode.Done => "done",
            StatusCode.Error => "error",
            _ => ""
          };
  }

  /// <summary>
  ///   Reads a <see cref="DocumentStatus" /> tolerantly: an unknown status is kept as a string instead of failing the
  ///   status request or the polling loop, and a non-numeric count is read as <c>null</c>.
  /// </summary>
  internal sealed class DocumentStatusConverter : JsonConverter<DocumentStatus> {
    public override DocumentStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      using var document = JsonDocument.ParseValue(ref reader);
      var root = document.RootElement;
      if (root.ValueKind != JsonValueKind.Object) {
        throw new JsonException("Expected a JSON object for the document status");
      }

      return new DocumentStatus(
            GetString(root, "document_id") ?? "",
            GetString(root, "status") ?? "",
            GetInt(root, "seconds_remaining"),
            GetInt(root, "billed_characters"),
            GetString(root, "error_message"));
    }

    public override void Write(Utf8JsonWriter writer, DocumentStatus value, JsonSerializerOptions options) {
      writer.WriteStartObject();
      writer.WriteString("document_id", value.DocumentId);
      writer.WriteString("status", value.StatusValue);
      if (value.SecondsRemaining != null) {
        writer.WriteNumber("seconds_remaining", value.SecondsRemaining.Value);
      }

      if (value.BilledCharacters != null) {
        writer.WriteNumber("billed_characters", value.BilledCharacters.Value);
      }

      if (value.ErrorMessage != null) {
        writer.WriteString("error_message", value.ErrorMessage);
      }

      writer.WriteEndObject();
    }

    private static string? GetString(JsonElement root, string name) =>
          root.TryGetProperty(name, out var element) && element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : null;

    private static int? GetInt(JsonElement root, string name) =>
          root.TryGetProperty(name, out var element) && element.ValueKind == JsonValueKind.Number &&
          element.TryGetInt32(out var value)
                ? value
                : (int?)null;
  }
}
