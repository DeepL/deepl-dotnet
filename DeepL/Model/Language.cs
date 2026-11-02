// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace DeepL.Model {
  /// <summary>
  ///   A language supported by DeepL translation. The <see cref="DeepLClient" /> class provides functions to retrieve
  ///   the available source and target languages. <see cref="Language" /> objects are considered equal if their
  ///   language codes match.
  /// </summary>
  /// <seealso cref="DeepLClient.GetSourceLanguagesAsync" />
  /// <seealso cref="DeepLClient.GetTargetLanguagesAsync" />
  public class Language : IEquatable<Language> {
    /// <summary>Initializes a new Language object.</summary>
    /// <param name="code">The language code.</param>
    /// <param name="name">The name of the language in English.</param>
    protected Language(string code, string name) {
      Code = LanguageCode.Standardize(code);
      Name = name;
    }

    /// <summary>The name of the language in English, for example "Italian" or "Romanian".</summary>
    public string Name { get; }

    /// <summary>
    ///   The language code, for example "it", "ro" or "en-US". Language codes follow ISO 639-1 with an optional
    ///   regional code from ISO 3166-1.
    /// </summary>
    [JsonPropertyName("language")]
    public string Code { get; }

    /// <summary>Creates a <see cref="CultureInfo" /> object corresponding to this language.</summary>
    public CultureInfo CultureInfo => new CultureInfo(Code);

    /// <summary>
    ///   Returns <c>true</c> if the other <see cref="Language" /> object has the same language code, otherwise
    ///   <c>false</c>.
    /// </summary>
    /// <param name="other"><see cref="Language" /> to compare with.</param>
    /// <returns><c>true</c> if languages have the same language code, otherwise <c>false</c>.</returns>
    public bool Equals(Language? other) {
      if (ReferenceEquals(null, other)) {
        return false;
      }

      if (ReferenceEquals(this, other)) {
        return true;
      }

      return Code == other.Code;
    }

    /// <summary>Converts the language to a string containing the name and language code.</summary>
    /// <returns>A string containing the name and language code of the language.</returns>
    /// <remarks>
    ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
    ///   compatibility.
    /// </remarks>
    public override string ToString() => $"{Name} ({Code})";

    /// <summary>Implicitly cast <see cref="Language" /> to string using the language code.</summary>
    /// <param name="language"><see cref="Language" /> object to cast into string.</param>
    /// <returns>String containing the language code.</returns>
    public static implicit operator string(Language language) => language.Code;

    /// <summary>
    ///   Determines whether this instance and a specified object, which must also be a <see cref="Language" /> object,
    ///   have the same value. <see cref="Language" /> objects are considered equal if their language codes match.
    /// </summary>
    /// <param name="obj">The Language to compare to this instance.</param>
    /// <returns>
    ///   <c>true</c> if <paramref name="obj" /> is a <see cref="Language" /> and its value is the same as this
    ///   instance; otherwise, <c>false</c>.  If <paramref name="obj" /> is <c>null</c>, the method
    ///   returns <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is Language other && Equals(other));

    /// <summary>
    ///   Returns the hash code for this <see cref="Language" /> object, that is simply the hash code of the language
    ///   code.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => Code.GetHashCode();

    /// <summary>Determines whether two specified Languages have the same value.</summary>
    /// <param name="a">The first Language to compare, or <c>null</c>.</param>
    /// <param name="b">The second Language to compare, or <c>null</c>.</param>
    /// <returns>
    ///   <c>true</c> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" />;
    ///   otherwise, <c>false</c>.
    /// </returns>
    public static bool operator ==(Language? a, Language? b) {
      if ((object?)a == null || (object?)b == null) {
        return Equals(a, b);
      }

      return a.Equals(b);
    }

    /// <summary>Determines whether two specified Languages have different values.</summary>
    /// <param name="a">The first Language to compare, or <c>null</c>.</param>
    /// <param name="b">The second Language to compare, or <c>null</c>.</param>
    /// <returns>
    ///   <c>true</c> if the value of <paramref name="a" /> is different from the value of <paramref name="b" />,
    ///   otherwise <c>false</c>.
    /// </returns>
    public static bool operator !=(Language? a, Language? b) => !(a == b);
  }

  /// <summary>
  ///   A source language supported by DeepL translation, returned by <see cref="DeepLClient.GetSourceLanguagesAsync" />.
  /// </summary>
  public sealed class SourceLanguage : Language {
    /// <summary>Initializes a new Language object.</summary>
    /// <param name="code">The language code.</param>
    /// <param name="name">The name of the language in English.</param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public SourceLanguage(string code, string name) : base(code, name) {
    }
  }

  /// <summary>
  ///   A target language supported by DeepL translation, returned by <see cref="DeepLClient.GetTargetLanguagesAsync" />.
  /// </summary>
  public sealed class TargetLanguage : Language {
    /// <summary>Initializes a new Language object.</summary>
    /// <param name="code">The language code.</param>
    /// <param name="name">The name of the language in English.</param>
    /// <param name="supportsFormality">
    ///   <c>true</c> if this language supports the <see cref="Formality" /> parameter for
    ///   translations, otherwise <c>false</c>.
    /// </param>
    /// <remarks>
    ///   The constructor for this class (and all other Model classes) should not be used by library users. Ideally it
    ///   would be marked <see langword="internal" />, but needs to be <see langword="public" /> for JSON deserialization.
    ///   In future this function may have backwards-incompatible changes.
    /// </remarks>
    [JsonConstructor]
    public TargetLanguage(string code, string name, bool supportsFormality) : base(code, name) {
      SupportsFormality = supportsFormality;
    }

    /// <summary>
    ///   <c>true</c> if the language supports the <see cref="Formality" /> parameter for translations, otherwise <c>false</c>.
    /// </summary>
    public bool SupportsFormality { get; }
  }
}
