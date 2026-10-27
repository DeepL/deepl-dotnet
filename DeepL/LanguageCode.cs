// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;

namespace DeepL {
  /// <summary>
  ///   Language codes for the languages currently supported by DeepL translation. New languages may be added in
  ///   future; to retrieve the currently supported languages use <see cref="ITranslator.GetSourceLanguagesAsync" /> and
  ///   <see cref="ITranslator.GetTargetLanguagesAsync" />.
  /// </summary>
  public static class LanguageCode {
    private const string DeprecationMessage =
          "Use the language code string, e.g. \"de\", or the languages API. Will be removed in the next major version.";

    /// <summary>Acehnese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Acehnese = "ace";

    /// <summary>Afrikaans language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Afrikaans = "af";

    /// <summary>Aragonese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Aragonese = "an";

    /// <summary>Arabic (MSA) language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Arabic = "ar";

    /// <summary>Assamese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Assamese = "as";

    /// <summary>Aymara language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Aymara = "ay";

    /// <summary>Azerbaijani language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Azerbaijani = "az";

    /// <summary>Bashkir language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Bashkir = "ba";

    /// <summary>Belarusian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Belarusian = "be";

    /// <summary>Bulgarian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Bulgarian = "bg";

    /// <summary>Bhojpuri language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Bhojpuri = "bho";

    /// <summary>Bengali language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Bengali = "bn";

    /// <summary>Breton language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Breton = "br";

    /// <summary>Bosnian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Bosnian = "bs";

    /// <summary>Catalan language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Catalan = "ca";

    /// <summary>Cebuano language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Cebuano = "ceb";

    /// <summary>Kurdish (Sorani) language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string KurdishSorani = "ckb";

    /// <summary>Czech language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Czech = "cs";

    /// <summary>Welsh language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Welsh = "cy";

    /// <summary>Danish language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Danish = "da";

    /// <summary>German language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string German = "de";

    /// <summary>Swiss German language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string GermanSwiss = "de-CH";

    /// <summary>German (Germany) language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string GermanGermany = "de-DE";

    /// <summary>Greek language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Greek = "el";

    /// <summary>
    ///   English language code, may be used as source or target language. As a target language it translates to
    ///   American English; use <see cref="EnglishBritish" /> or <see cref="EnglishAmerican" /> to choose explicitly.
    /// </summary>
    [Obsolete(DeprecationMessage)]
    public const string English = "en";

    /// <summary>British English language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string EnglishBritish = "en-GB";

    /// <summary>American English language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string EnglishAmerican = "en-US";

    /// <summary>Esperanto language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Esperanto = "eo";

    /// <summary>Spanish language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Spanish = "es";

    /// <summary>Latin American Spanish language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string SpanishLatinAmerican = "es-419";

    /// <summary>Spanish (Spain) language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string SpanishEuropean = "es-ES";

    /// <summary>Estonian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Estonian = "et";

    /// <summary>Basque language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Basque = "eu";

    /// <summary>Persian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Persian = "fa";

    /// <summary>Finnish language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Finnish = "fi";

    /// <summary>French language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string French = "fr";

    /// <summary>Canadian French language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string FrenchCanadian = "fr-CA";

    /// <summary>French (France) language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string FrenchEuropean = "fr-FR";

    /// <summary>Irish language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Irish = "ga";

    /// <summary>Galician language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Galician = "gl";

    /// <summary>Guarani language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Guarani = "gn";

    /// <summary>Konkani language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Konkani = "gom";

    /// <summary>Gujarati language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Gujarati = "gu";

    /// <summary>Hausa language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Hausa = "ha";

    /// <summary>Hebrew language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Hebrew = "he";

    /// <summary>Hindi language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Hindi = "hi";

    /// <summary>Croatian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Croatian = "hr";

    /// <summary>Haitian Creole language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string HaitianCreole = "ht";

    /// <summary>Hungarian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Hungarian = "hu";

    /// <summary>Armenian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Armenian = "hy";

    /// <summary>Indonesian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Indonesian = "id";

    /// <summary>Igbo language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Igbo = "ig";

    /// <summary>Icelandic language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Icelandic = "is";

    /// <summary>Italian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Italian = "it";

    /// <summary>Japanese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Japanese = "ja";

    /// <summary>Javanese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Javanese = "jv";

    /// <summary>Georgian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Georgian = "ka";

    /// <summary>Kazakh language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Kazakh = "kk";

    /// <summary>Kurdish (Kurmanji) language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string KurdishKurmanji = "kmr";

    /// <summary>Korean language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Korean = "ko";

    /// <summary>Kyrgyz language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Kyrgyz = "ky";

    /// <summary>Latin language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Latin = "la";

    /// <summary>Luxembourgish language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Luxembourgish = "lb";

    /// <summary>Lombard language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Lombard = "lmo";

    /// <summary>Lingala language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Lingala = "ln";

    /// <summary>Lithuanian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Lithuanian = "lt";

    /// <summary>Latvian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Latvian = "lv";

    /// <summary>Maithili language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Maithili = "mai";

    /// <summary>Malagasy language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Malagasy = "mg";

    /// <summary>Maori language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Maori = "mi";

    /// <summary>Macedonian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Macedonian = "mk";

    /// <summary>Malayalam language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Malayalam = "ml";

    /// <summary>Mongolian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Mongolian = "mn";

    /// <summary>Marathi language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Marathi = "mr";

    /// <summary>Malay language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Malay = "ms";

    /// <summary>Maltese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Maltese = "mt";

    /// <summary>Burmese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Burmese = "my";

    /// <summary>Norwegian (bokmål) language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Norwegian = "nb";

    /// <summary>Nepali language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Nepali = "ne";

    /// <summary>Dutch language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Dutch = "nl";

    /// <summary>Occitan language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Occitan = "oc";

    /// <summary>Oromo language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Oromo = "om";

    /// <summary>Punjabi language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Punjabi = "pa";

    /// <summary>Pangasinan language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Pangasinan = "pag";

    /// <summary>Kapampangan language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Kapampangan = "pam";

    /// <summary>Polish language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Polish = "pl";

    /// <summary>Dari language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Dari = "prs";

    /// <summary>Pashto language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Pashto = "ps";

    /// <summary>
    ///   Portuguese language code, may be used as source or target language. As a target language it translates to
    ///   European Portuguese; use <see cref="PortugueseBrazilian" /> or <see cref="PortugueseEuropean" /> to choose
    ///   explicitly.
    /// </summary>
    [Obsolete(DeprecationMessage)]
    public const string Portuguese = "pt";

    /// <summary>Brazilian Portuguese language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string PortugueseBrazilian = "pt-BR";

    /// <summary>European Portuguese language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string PortugueseEuropean = "pt-PT";

    /// <summary>Quechua language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Quechua = "qu";

    /// <summary>Romanian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Romanian = "ro";

    /// <summary>Russian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Russian = "ru";

    /// <summary>Sanskrit language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Sanskrit = "sa";

    /// <summary>Sicilian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Sicilian = "scn";

    /// <summary>Slovak language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Slovak = "sk";

    /// <summary>Slovenian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Slovenian = "sl";

    /// <summary>Albanian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Albanian = "sq";

    /// <summary>Serbian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Serbian = "sr";

    /// <summary>Sesotho language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Sesotho = "st";

    /// <summary>Sundanese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Sundanese = "su";

    /// <summary>Swedish language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Swedish = "sv";

    /// <summary>Swahili language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Swahili = "sw";

    /// <summary>Tamil language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Tamil = "ta";

    /// <summary>Telugu language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Telugu = "te";

    /// <summary>Tajik language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Tajik = "tg";

    /// <summary>Thai language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Thai = "th";

    /// <summary>Turkmen language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Turkmen = "tk";

    /// <summary>Tagalog language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Tagalog = "tl";

    /// <summary>Tswana language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Tswana = "tn";

    /// <summary>Turkish language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Turkish = "tr";

    /// <summary>Tsonga language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Tsonga = "ts";

    /// <summary>Tatar language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Tatar = "tt";

    /// <summary>Ukrainian language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Ukrainian = "uk";

    /// <summary>Urdu language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Urdu = "ur";

    /// <summary>Uzbek language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Uzbek = "uz";

    /// <summary>Vietnamese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Vietnamese = "vi";

    /// <summary>Wolof language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Wolof = "wo";

    /// <summary>Xhosa language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Xhosa = "xh";

    /// <summary>Yiddish language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Yiddish = "yi";

    /// <summary>Cantonese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Cantonese = "yue";

    /// <summary>Chinese language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Chinese = "zh";

    /// <summary>Chinese (simplified) language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string ChineseSimplified = "zh-HANS";

    /// <summary>Chinese (traditional) language code, may only be used as a target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string ChineseTraditional = "zh-HANT";

    /// <summary>Zulu language code, may be used as source or target language.</summary>
    [Obsolete(DeprecationMessage)]
    public const string Zulu = "zu";

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
