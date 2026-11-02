// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

namespace DeepL {
  /// <summary>
  ///   Helpers for language codes. To retrieve the currently supported languages use
  ///   <see cref="DeepLClient.GetSourceLanguagesAsync" /> and <see cref="DeepLClient.GetTargetLanguagesAsync" />.
  /// </summary>
  public static class LanguageCode {
    /// <summary>Removes the regional variant (if any) from the given language code</summary>
    /// <param name="code">Language code possibly containing a regional variant.</param>
    /// <returns>The language code without a regional variant.</returns>
    public static string RemoveRegionalVariant(string code) => code.Split(new[] { '-' }, 2)[0].ToLowerInvariant();

    /// <summary>
    ///   Changes the upper- and lower-casing of the given language code to match ISO 639-1 with an optional regional
    ///   code from ISO 3166-1.
    /// </summary>
    /// <param name="code">String containing language code to standardize.</param>
    /// <returns>String containing the standardized language code.</returns>
    internal static string Standardize(string code) {
      var parts = code.Split(new[] { '-' }, 2);
      return parts.Length == 1
            ? parts[0].ToLowerInvariant()
            : $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}";
    }
  }
}
