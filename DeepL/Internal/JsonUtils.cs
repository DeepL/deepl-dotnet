// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#if !NET8_0_OR_GREATER
using System.Linq;
#else
using System.Net.Http.Json;
#endif

namespace DeepL.Internal {
  /// <summary>Internal class containing utility functions related to JSON-serialization.</summary>
  internal static class JsonUtils {
    /// <summary>Options used to deserialize JSON data.</summary>
    private static JsonSerializerOptions JsonSerializerOptions { get; } = new() {
#if NET8_0_OR_GREATER
      PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
#else
      PropertyNamingPolicy = LowerSnakeCaseNamingPolicy.Instance
#endif
    };

    /// <summary>
    ///   Deserializes JSON data in given HTTP response into a new object of <see cref="TValue" /> type, with fields named in
    ///   lower-snake-case.
    /// </summary>
    internal static async Task<TValue> DeserializeAsync<TValue>(
          HttpResponseMessage responseMessage,
          CancellationToken cancellationToken = default) {
#if NET8_0_OR_GREATER
      var value = await responseMessage.Content
            .ReadFromJsonAsync<TValue>(JsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
      return value ?? throw new DeepLException("Failed to deserialize JSON in received response");
#else
      using var stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
      return await DeserializeAsync<TValue>(stream).ConfigureAwait(false);
#endif
    }

    /// <summary>Deserializes JSON data in given stream into a new object of <see cref="TValue" /> type.</summary>
    internal static async Task<TValue> DeserializeAsync<TValue>(Stream contentStream) {
      return await JsonSerializer.DeserializeAsync<TValue>(contentStream, JsonSerializerOptions)
                   .ConfigureAwait(false) ??
             throw new DeepLException("Failed to deserialize JSON in received response");
    }

#if !NET8_0_OR_GREATER
    /// <summary>JSON-field naming policy for lower-snake-case, e.g. "lower_snake_case". Used on <c>netstandard2.0</c>.</summary>
    private sealed class LowerSnakeCaseNamingPolicy : JsonNamingPolicy {
      public static LowerSnakeCaseNamingPolicy Instance { get; } = new();

      public override string ConvertName(string name) =>
            string
                  .Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
                  .ToLowerInvariant();
    }
#endif
  }
}
