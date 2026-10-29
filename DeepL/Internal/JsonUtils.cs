// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeepL.Internal {
  /// <summary>Internal class containing utility functions related to JSON-serialization.</summary>
  internal static class JsonUtils {
    /// <summary>Options used to deserialize JSON data.</summary>
    private static JsonSerializerOptions JsonSerializerOptions { get; } =
      new JsonSerializerOptions { PropertyNamingPolicy = LowerSnakeCaseNamingPolicy.Instance };

    /// <summary>
    ///   Deserializes JSON data in given HTTP response into a new object of <see cref="TValue" /> type, with fields named in
    ///   lower-snake-case.
    /// </summary>
    /// <param name="responseMessage"><see cref="HttpResponseMessage" /> containing HTTP response received from DeepL API.</param>
    /// <typeparam name="TValue">Type of deserialized object.</typeparam>
    /// <returns>Object of <see cref="TValue" /> type initialized with values from JSON data.</returns>
    /// <exception cref="DeepLException">If the JSON data could not be deserialized correctly.</exception>
    internal static async Task<TValue> DeserializeAsync<TValue>(HttpResponseMessage responseMessage) =>
          await DeserializeAsync<TValue>(await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                .ConfigureAwait(false);

    /// <summary>
    ///   Deserializes JSON data in given stream into a new object of <see cref="TValue" /> type, with fields named in
    ///   lower-snake-case.
    /// </summary>
    /// <param name="contentStream">Stream containing JSON data.</param>
    /// <typeparam name="TValue">Type of deserialized object.</typeparam>
    /// <returns>Object of <see cref="TValue" /> type initialized with values from JSON data.</returns>
    /// <exception cref="DeepLException">If the JSON data could not be deserialized correctly.</exception>
    internal static async Task<TValue> DeserializeAsync<TValue>(Stream contentStream) {
      using var reader = new StreamReader(contentStream);

      try {
        return await JsonSerializer.DeserializeAsync<TValue>(contentStream, JsonSerializerOptions)
                     .ConfigureAwait(false) ??
               throw new DeepLException("Failed to deserialize JSON in received response");
      } catch (JsonException exception) {
        throw new DeepLException("Failed to deserialize JSON in received response", exception);
      }
    }

    /// <summary>Deserializes a JSON element into <typeparamref name="TValue" />, wrapping malformed content.</summary>
    /// <exception cref="DeepLException">If the JSON data could not be deserialized correctly.</exception>
    internal static TValue Deserialize<TValue>(JsonElement element) {
      try {
        return JsonSerializer.Deserialize<TValue>(element.GetRawText(), JsonSerializerOptions) ??
               throw new DeepLException("Failed to deserialize JSON in received response");
      } catch (JsonException exception) {
        throw new DeepLException("Failed to deserialize JSON in received response", exception);
      }
    }

    /// <summary>JSON-field naming policy for lower-snake-case for example: "lower_snake_case".</summary>
    private sealed class LowerSnakeCaseNamingPolicy : JsonNamingPolicy {
      static LowerSnakeCaseNamingPolicy() {
        Instance = new LowerSnakeCaseNamingPolicy();
      }

      public static LowerSnakeCaseNamingPolicy Instance { get; }

      public override string ConvertName(string name) =>
            string
                  .Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
                  .ToLowerInvariant();
    }
  }
}
