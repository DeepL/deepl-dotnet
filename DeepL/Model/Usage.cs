// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>
  ///   Information about DeepL account usage for the current billing period, for example the number of characters
  ///   translated.
  /// </summary>
  /// <remarks>
  ///   Depending on the account type, some usage types will be omitted. See the
  ///   <a href="https://www.deepl.com/docs-api/">API documentation</a> for more information.
  /// </remarks>
  public sealed class Usage {
    /// <summary>
    ///   Initializes a new <see cref="Usage" /> object from the given fields object. Note that this constructor
    ///   should ONLY be used externally in testing code, never in production code.
    /// </summary>
    /// <param name="fieldsStruct"><see cref="JsonFieldsStruct" /> object containing fields read from JSON data.</param>
    public Usage(in JsonFieldsStruct fieldsStruct) {
      Detail? DetailOrNull(long? count, long? limit) {
        return count != null && limit != null ? new Detail((long)count, (long)limit) : null;
      }

      Character = DetailOrNull(fieldsStruct.CharacterCount, fieldsStruct.CharacterLimit);
      Document = DetailOrNull(fieldsStruct.DocumentCount, fieldsStruct.DocumentLimit);
      TeamDocument = DetailOrNull(fieldsStruct.TeamDocumentCount, fieldsStruct.TeamDocumentLimit);
      ApiKeyCharacter = DetailOrNull(fieldsStruct.ApiKeyCharacterCount, fieldsStruct.ApiKeyCharacterLimit);
      SpeechToTextMinutes =
            DetailOrNull(fieldsStruct.SpeechToTextMinutesCount, fieldsStruct.SpeechToTextMinutesLimit);
      SpeechToSpeechMinutes =
            DetailOrNull(fieldsStruct.SpeechToSpeechMinutesCount, fieldsStruct.SpeechToSpeechMinutesLimit);
      Products = fieldsStruct.Products;
      StartTime = fieldsStruct.StartTime;
      EndTime = fieldsStruct.EndTime;
    }

    /// <summary>The character usage if included for the account type, or null.</summary>
    public Detail? Character { get; }

    /// <summary>The document usage if included for the account type, or null.</summary>
    public Detail? Document { get; }

    /// <summary>The team document usage if included for the account type, or null.</summary>
    public Detail? TeamDocument { get; }

    /// <summary>
    ///   The character usage of the API key used for this request, as opposed to <see cref="Character" />, which covers
    ///   the whole account. Only present for API Pro accounts, otherwise null.
    /// </summary>
    public Detail? ApiKeyCharacter { get; }

    /// <summary>The speech-to-text usage in minutes. Only present for API Pro accounts, otherwise null.</summary>
    public Detail? SpeechToTextMinutes { get; }

    /// <summary>The speech-to-speech usage in minutes. Only present for API Pro accounts, otherwise null.</summary>
    public Detail? SpeechToSpeechMinutes { get; }

    /// <summary>The usage broken down per product. Only present for API Pro accounts, otherwise null.</summary>
    public Product[]? Products { get; }

    /// <summary>The start time of the current billing period. Only present for API Pro accounts, otherwise null.</summary>
    public DateTime? StartTime { get; }

    /// <summary>The end time of the current billing period. Only present for API Pro accounts, otherwise null.</summary>
    public DateTime? EndTime { get; }

    /// <summary>
    ///   <c>true</c> if the character, document or team document limit has been reached. The API Pro usage types
    ///   (<see cref="ApiKeyCharacter" />, <see cref="SpeechToTextMinutes" />, <see cref="SpeechToSpeechMinutes" />)
    ///   are not considered; check <see cref="Detail.LimitReached" /> on them directly.
    /// </summary>
    public bool AnyLimitReached => (Character?.LimitReached ?? false) || (Document?.LimitReached ?? false) ||
                                   (TeamDocument?.LimitReached ?? false);

    /// <summary>Returns a string representing the usage.</summary>
    /// <returns>A string containing the usage for this billing period.</returns>
    /// <remarks>
    ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
    ///   compatibility.
    /// </remarks>
    public override string ToString() {
      static string LabelledDetail(string label, Detail? detail) {
        return detail == null ? "" : $"\n{label}: {detail}";
      }

      return "Usage this billing period:" +
             LabelledDetail("Characters", Character) +
             LabelledDetail("Documents", Document) +
             LabelledDetail("Team documents", TeamDocument);
    }

    /// <summary>
    ///   Stores the amount used and maximum amount for one usage type.
    /// </summary>
    public sealed class Detail {
      /// <summary>Initializes a new <see cref="Detail" /> object.</summary>
      /// <param name="count">Amount used of one usage type.</param>
      /// <param name="limit">Maximum amount allowed for one usage type.</param>
      internal Detail(long count, long limit) {
        (Count, Limit) = (count, limit);
      }

      /// <summary>The currently used number of items for this usage type.</summary>
      public long Count { get; }

      /// <summary>The maximum permitted number of items for this usage type.</summary>
      public long Limit { get; }

      /// <summary><c>true</c> if the amount used meets or exceeds the limit, otherwise <c>false</c>.</summary>
      public bool LimitReached => Count >= Limit;

      /// <summary>The usage detail as a string.</summary>
      /// <returns>A string containing the amount used and the limit.</returns>
      /// <remarks>
      ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
      ///   compatibility.
      /// </remarks>
      public override string ToString() => $"{Count} of {Limit}";
    }

    /// <summary>The usage of one product, for example text translation or speech-to-text.</summary>
    public sealed class Product {
      /// <summary>Initializes a new <see cref="Product" /> object.</summary>
      /// <param name="productType">The product type, for example "translate" or "speechToText".</param>
      /// <param name="billingUnit">The unit the product is billed in, for example "characters" or "minutes".</param>
      /// <param name="apiKeyUnitCount">Units used for this product by the API key in the current billing period.</param>
      /// <param name="accountUnitCount">Units used for this product by the account in the current billing period.</param>
      [JsonConstructor]
      public Product(string productType, string billingUnit, long apiKeyUnitCount, long accountUnitCount) {
        (ProductType, BillingUnit, ApiKeyUnitCount, AccountUnitCount) =
              (productType, billingUnit, apiKeyUnitCount, accountUnitCount);
      }

      /// <summary>
      ///   The product type, for example "write", "translate", "speechToText" or "speechToSpeech". The API may add
      ///   further values.
      /// </summary>
      public string ProductType { get; }

      /// <summary>The unit this product is billed in, for example "characters" or "minutes".</summary>
      public string BillingUnit { get; }

      /// <summary>Units used for this product by the API key used for this request in the current billing period.</summary>
      public long ApiKeyUnitCount { get; }

      /// <summary>Units used for this product by the whole account in the current billing period.</summary>
      public long AccountUnitCount { get; }

      /// <summary>The product usage as a string.</summary>
      /// <returns>A string containing the product type, its unit and the API key and account counts.</returns>
      /// <remarks>
      ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
      ///   compatibility.
      /// </remarks>
      public override string ToString() =>
            $"{ProductType}: {ApiKeyUnitCount} {BillingUnit} (API key), {AccountUnitCount} {BillingUnit} (account)";
    }

    /// <summary>
    ///   Internal struct used for JSON deserialization of <see cref="Usage" />. You should only need to
    ///   instantiate this object in testing code.
    /// </summary>
    public readonly struct JsonFieldsStruct {
      public JsonFieldsStruct(
            long? characterCount,
            long? characterLimit,
            long? documentCount,
            long? documentLimit,
            long? teamDocumentCount,
            long? teamDocumentLimit) : this(
            characterCount,
            characterLimit,
            documentCount,
            documentLimit,
            teamDocumentCount,
            teamDocumentLimit,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null) {
      }

      [JsonConstructor]
      public JsonFieldsStruct(
            long? characterCount,
            long? characterLimit,
            long? documentCount,
            long? documentLimit,
            long? teamDocumentCount,
            long? teamDocumentLimit,
            long? apiKeyCharacterCount,
            long? apiKeyCharacterLimit,
            long? speechToTextMinutesCount,
            long? speechToTextMinutesLimit,
            long? speechToSpeechMinutesCount,
            long? speechToSpeechMinutesLimit,
            Product[]? products,
            DateTime? startTime,
            DateTime? endTime) {
        (CharacterCount, CharacterLimit, DocumentCount, DocumentLimit, TeamDocumentCount, TeamDocumentLimit) =
              (characterCount, characterLimit, documentCount, documentLimit, teamDocumentCount, teamDocumentLimit);
        (ApiKeyCharacterCount, ApiKeyCharacterLimit) = (apiKeyCharacterCount, apiKeyCharacterLimit);
        (SpeechToTextMinutesCount, SpeechToTextMinutesLimit) = (speechToTextMinutesCount, speechToTextMinutesLimit);
        (SpeechToSpeechMinutesCount, SpeechToSpeechMinutesLimit) =
              (speechToSpeechMinutesCount, speechToSpeechMinutesLimit);
        (Products, StartTime, EndTime) = (products, startTime, endTime);
      }

      public long? CharacterCount { get; }
      public long? CharacterLimit { get; }
      public long? DocumentCount { get; }
      public long? DocumentLimit { get; }
      public long? TeamDocumentCount { get; }
      public long? TeamDocumentLimit { get; }
      public long? ApiKeyCharacterCount { get; }
      public long? ApiKeyCharacterLimit { get; }
      public long? SpeechToTextMinutesCount { get; }
      public long? SpeechToTextMinutesLimit { get; }
      public long? SpeechToSpeechMinutesCount { get; }
      public long? SpeechToSpeechMinutesLimit { get; }
      public Product[]? Products { get; }

      [JsonConverter(typeof(LenientNullableDateTimeConverter))]
      public DateTime? StartTime { get; }

      [JsonConverter(typeof(LenientNullableDateTimeConverter))]
      public DateTime? EndTime { get; }
    }
  }

  /// <summary>
  ///   Reads an optional ISO 8601 timestamp, returning null instead of throwing when the value is missing or has an
  ///   unexpected format, so that one unexpected field does not fail the whole usage response.
  /// </summary>
  internal sealed class LenientNullableDateTimeConverter : JsonConverter<DateTime?> {
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      if (reader.TokenType == JsonTokenType.String && reader.TryGetDateTime(out var value)) {
        return value;
      }

      reader.Skip();
      return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options) {
      if (value.HasValue) {
        writer.WriteStringValue(value.Value);
      } else {
        writer.WriteNullValue();
      }
    }
  }
}
