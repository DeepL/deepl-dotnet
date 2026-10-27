// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepL.Internal {
  /// <summary>
  ///   Reads an optional ISO 8601 timestamp, returning null instead of throwing when the value is missing or has an
  ///   unexpected format, so that one unexpected field does not fail the whole response. Values without a time zone
  ///   are taken as UTC.
  /// </summary>
  internal sealed class LenientNullableDateTimeConverter : JsonConverter<DateTime?> {
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      if (reader.TokenType == JsonTokenType.String && reader.TryGetDateTime(out var value)) {
        return value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value;
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

  /// <summary>Reads a number as <c>double</c>, accepting numeric strings and returning null for anything else.</summary>
  internal static class LenientNumber {
    internal static double? Read(ref Utf8JsonReader reader) {
      double? result = null;
      if (reader.TokenType == JsonTokenType.Number) {
        result = reader.GetDouble();
      } else if (reader.TokenType == JsonTokenType.String &&
                 double.TryParse(
                       reader.GetString(),
                       NumberStyles.Float,
                       CultureInfo.InvariantCulture,
                       out var parsed)) {
        result = parsed;
      }

      reader.Skip();
      return result;
    }

    internal static long? ToLong(double? value) =>
          value.HasValue && !double.IsNaN(value.Value) && !double.IsInfinity(value.Value)
                ? (long)Math.Max(long.MinValue, Math.Min(long.MaxValue, Math.Round(value.Value)))
                : (long?)null;
  }

  /// <summary>Reads an optional integer, returning null if the value is missing or not a number.</summary>
  internal sealed class LenientNullableLongConverter : JsonConverter<long?> {
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
          LenientNumber.ToLong(LenientNumber.Read(ref reader));

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options) {
      if (value.HasValue) {
        writer.WriteNumberValue(value.Value);
      } else {
        writer.WriteNullValue();
      }
    }
  }

  /// <summary>Reads an integer, returning 0 if the value is missing or not a number.</summary>
  internal sealed class LenientLongConverter : JsonConverter<long> {
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
          LenientNumber.ToLong(LenientNumber.Read(ref reader)) ?? 0;

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options) =>
          writer.WriteNumberValue(value);
  }

  /// <summary>Reads a number, returning 0 if the value is missing or not a number.</summary>
  internal sealed class LenientDoubleConverter : JsonConverter<double> {
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
          LenientNumber.Read(ref reader) ?? 0;

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options) =>
          writer.WriteNumberValue(value);
  }

  /// <summary>Reads an optional page number, returning null if the value is missing or not a number.</summary>
  internal sealed class LenientNullableIntConverter : JsonConverter<int?> {
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      var value = LenientNumber.ToLong(LenientNumber.Read(ref reader));
      return value.HasValue && value.Value >= int.MinValue && value.Value <= int.MaxValue ? (int)value.Value : (int?)null;
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options) {
      if (value.HasValue) {
        writer.WriteNumberValue(value.Value);
      } else {
        writer.WriteNullValue();
      }
    }
  }

  /// <summary>Reads a boolean that is true only for the JSON value <c>true</c>; anything else is false.</summary>
  internal sealed class LenientBoolConverter : JsonConverter<bool> {
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      var result = reader.TokenType == JsonTokenType.True;
      reader.Skip();
      return result;
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
          writer.WriteBooleanValue(value);
  }

  /// <summary>Reads a string, returning null if the value is missing or not a string.</summary>
  internal sealed class LenientStringConverter : JsonConverter<string?> {
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      var result = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
      reader.Skip();
      return result;
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) =>
          writer.WriteStringValue(value);
  }

  /// <summary>Reads an object, returning null if the value is missing or not an object.</summary>
  internal sealed class LenientObjectConverter<T> : JsonConverter<T?> where T : class {
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      if (reader.TokenType == JsonTokenType.StartObject) {
        return JsonSerializer.Deserialize<T>(ref reader, options);
      }

      reader.Skip();
      return null;
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options) =>
          JsonSerializer.Serialize(writer, value, options);
  }

  /// <summary>
  ///   Reads an array of objects, skipping entries that are not objects, and returning an empty array if the value is
  ///   missing or not an array.
  /// </summary>
  internal sealed class LenientArrayConverter<T> : JsonConverter<T[]> where T : class {
    public override T[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      if (reader.TokenType != JsonTokenType.StartArray) {
        reader.Skip();
        return Array.Empty<T>();
      }

      var result = new List<T>();
      while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
        if (reader.TokenType == JsonTokenType.StartObject) {
          var item = JsonSerializer.Deserialize<T>(ref reader, options);
          if (item != null) {
            result.Add(item);
          }
        } else {
          reader.Skip();
        }
      }

      return result.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, T[] value, JsonSerializerOptions options) =>
          JsonSerializer.Serialize(writer, value, options);
  }
}
