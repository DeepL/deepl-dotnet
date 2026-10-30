// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepL {
  /// <summary>Stores the entries of a glossary.</summary>
  public sealed class GlossaryEntries {
    private const char TermSeparator = '\t';
    private static readonly string[] LineSeparators = { "\r\n", "\n", "\r" };
    private readonly string _contentTsv;

    /// <summary>
    ///   Initializes a new <see cref="GlossaryEntries" /> object using the given source-target entry pairs.
    /// </summary>
    /// <param name="entryPairs">Source-target entry pairs to convert.</param>
    /// <param name="skipChecks">If <c>true</c>, validity checks on the entries are skipped, defaults to <c>false</c>.</param>
    /// <returns>String containing the entries in TSV format.</returns>
    /// <exception cref="ArgumentException">If the entries fail any validity check.</exception>
    public GlossaryEntries(
          IEnumerable<KeyValuePair<string, string>> entryPairs,
          bool skipChecks = false) {
      _contentTsv = ConvertToTsv(entryPairs.Select(pair => (pair.Key, pair.Value)), skipChecks);
    }

    /// <summary>
    ///   Initializes a new <see cref="GlossaryEntries" /> object using the given source-target entry pairs.
    /// </summary>
    /// <param name="entryPairs">Source-target entry pairs to convert.</param>
    /// <param name="skipChecks">If <c>true</c>, validity checks on the entries are skipped, defaults to <c>false</c>.</param>
    /// <returns>String containing the entries in TSV format.</returns>
    /// <exception cref="ArgumentException">If the entries fail any validity check.</exception>
    public GlossaryEntries(
          IEnumerable<(string Key, string Value)> entryPairs,
          bool skipChecks = false) {
      _contentTsv = ConvertToTsv(entryPairs, skipChecks);
    }

    /// <summary>
    ///   Converts the given tab-separated-value (TSV) string of glossary entries into a new <see cref="GlossaryEntries" />.
    ///   Whitespace is trimmed from the start and end of each term.
    /// </summary>
    /// <param name="contentTsv">String containing the entries in TSV format.</param>
    /// <param name="skipChecks">If <c>true</c>, validity checks on the entries are skipped, defaults to <c>false</c>.</param>
    /// <returns><see cref="GlossaryEntries" /> containing the source-target entry pairs.</returns>
    /// <exception cref="ArgumentException">If the entries fail any validity check.</exception>
    private GlossaryEntries(string contentTsv, bool skipChecks = false) {
      // Without skipChecks, the validated, trimmed entries are stored (and sent), one per line separated by "\n".
      _contentTsv = skipChecks
            ? contentTsv
            : string.Join("\n", ParseTsv(contentTsv).Select(entry => $"{entry.Source}{TermSeparator}{entry.Target}"));
    }

    /// <summary>
    ///   Converts the given tab-separated-value (TSV) string of glossary entries into a new <see cref="GlossaryEntries" />.
    ///   Whitespace is trimmed from the start and end of each term, then each term is validated. The trimmed entries
    ///   are stored and sent, one per line separated by <c>\n</c>, without blank lines. With
    ///   <paramref name="skipChecks" />, the string is stored and sent unchanged.
    /// </summary>
    /// <param name="contentTsv">String containing the entries in TSV format.</param>
    /// <param name="skipChecks">If <c>true</c>, validity checks on the entries are skipped, defaults to <c>false</c>.</param>
    /// <returns><see cref="GlossaryEntries" /> containing the source-target entry pairs.</returns>
    /// <exception cref="ArgumentException">If the entries fail any validity check.</exception>
    public static GlossaryEntries FromTsv(string contentTsv, bool skipChecks = false) =>
          new GlossaryEntries(contentTsv, skipChecks);

    /// <summary>
    ///   Converts the glossary entry list into a dictionary of source-target entry pairs. Whitespace is trimmed from the start
    ///   and end of each term.
    /// </summary>
    /// <param name="skipChecks">If <c>true</c>, validity checks on the entries are skipped, defaults to <c>false</c>.</param>
    /// <returns>Dictionary containing the source-target entry pairs.</returns>
    public Dictionary<string, string> ToDictionary(bool skipChecks = false) =>
          ToDictionary(_contentTsv, skipChecks);

    /// <summary>Converts the <see cref="GlossaryEntries" /> to a tab-separated-value (TSV) string.</summary>
    /// <returns>TSV format string containing glossary entries.</returns>
    public string ToTsv() => _contentTsv;

    /// <summary>Converts the <see cref="GlossaryEntries" /> to a string containing the source-target entries.</summary>
    /// <returns>A string containing the source-target entries.</returns>
    /// <remarks>
    ///   This function is for diagnostic purposes only; the content of the returned string is exempt from backwards
    ///   compatibility.
    /// </remarks>
    public override string ToString() =>
          nameof(GlossaryEntries) + "[" +
          string.Join(", ", ToDictionary().Select(pair => $"[\"{pair.Key}\", \"{pair.Value}\"]")) + "]";

    /// <summary>
    ///   Checks the validity of the given glossary term, for example that it contains no invalid characters. Whitespace
    ///   at the start and end of the term is ignored.
    /// </summary>
    /// <param name="term">String containing term to check.</param>
    /// <exception cref="ArgumentException">If the term is invalid.</exception>
    /// <remarks>
    ///   Terms are considered valid if they comprise at least one non-whitespace character, and contain no invalid
    ///   characters: C0 and C1 control characters, and Unicode newlines.
    /// </remarks>
    public static void ValidateGlossaryTerm(string term) {
      var termTrimmed = term.Trim();

      if (termTrimmed.Length == 0) {
        throw new ArgumentException($"Argument {nameof(term)} \"{term}\" contains no non-whitespace characters");
      }

      foreach (var ch in termTrimmed) {
        if ((0 <= ch && ch <= 31) || // C0 control characters
            (128 <= ch && ch <= 159) || // C1 control characters
            ch == '\u2028' || ch == '\u2029' // Unicode newlines
        ) {
          throw new ArgumentException(
                $"Argument {nameof(term)} \"{term}\" contains invalid character: '{ch}' (U+{Convert.ToInt32(ch):X4})");
        }
      }
    }

    /// <summary>
    ///   Converts the given source-target entry pairs to a string containing the entries in tab-separated-value (TSV)
    ///   format.
    /// </summary>
    /// <param name="entries">IEnumerable of source-target entry pairs to convert.</param>
    /// <param name="skipChecks">If <c>true</c>, validity checks on the entries are skipped, defaults to <c>false</c>.</param>
    /// <returns>String containing the entries in TSV format.</returns>
    private static string ConvertToTsv(
          IEnumerable<(string Key, string Value)> entries,
          bool skipChecks = false) {
      var builder = new StringBuilder();
      var dictionary = skipChecks ? null : new Dictionary<string, string>();
      foreach (var pair in entries) {
        var source = pair.Key.Trim();
        var target = pair.Value.Trim();
        if (!skipChecks) {
          ValidateGlossaryTerm(source);
          ValidateGlossaryTerm(target);
          if (dictionary!.ContainsKey(source)) {
            throw new ArgumentException($"{nameof(entries)} contains duplicate source term: \"{source}\"");
          }

          dictionary.Add(source, target);
        }

        if (builder.Length > 0) {
          builder.Append("\n");
        }

        builder.Append($"{source}\t{target}");
      }

      if (builder.Length == 0) {
        throw new ArgumentException($"{nameof(entries)} contains no entries");
      }

      return builder.ToString();
    }

    /// <summary>
    ///   Converts the given TSV string into a dictionary of source-target entry pairs. Whitespace is trimmed from the start
    ///   and end of each term.
    /// </summary>
    /// <param name="contentTsv">String containing the entries in TSV format.</param>
    /// <param name="skipChecks">If <c>true</c>, validity checks on the entries are skipped, defaults to <c>false</c>.</param>
    /// <returns>Dictionary containing the source-target entry pairs.</returns>
    private static Dictionary<string, string> ToDictionary(
          string contentTsv,
          bool skipChecks = false) =>
          ParseTsv(contentTsv, skipChecks).ToDictionary(entry => entry.Source, entry => entry.Target);

    /// <summary>
    ///   Parses the given TSV string into source-target entry pairs in input order, skipping blank lines. Whitespace is
    ///   trimmed from the start and end of each term.
    /// </summary>
    /// <param name="contentTsv">String containing the entries in TSV format.</param>
    /// <param name="skipChecks">If <c>true</c>, validity checks on the entries are skipped, defaults to <c>false</c>.</param>
    /// <returns>List of the source-target entry pairs.</returns>
    private static List<(string Source, string Target)> ParseTsv(
          string contentTsv,
          bool skipChecks = false) {
      var entries = new List<(string Source, string Target)>();
      var sourceTerms = new HashSet<string>();
      var lineNumber = 0;
      foreach (var line in contentTsv.Split(LineSeparators, StringSplitOptions.None)) {
        lineNumber += 1;
        var lineTrimmed = line.Trim();
        if (lineTrimmed.Length == 0) {
          continue;
        }

        var termSeparatorPos = lineTrimmed.IndexOf(TermSeparator);
        if (termSeparatorPos == -1) {
          throw new ArgumentException($"Entry on line {lineNumber} does not contain term separator: {line}");
        }

        var source = lineTrimmed.Substring(0, termSeparatorPos).Trim();
        var target = lineTrimmed.Substring(termSeparatorPos + 1).Trim();
        if (!skipChecks) {
          if (target.Contains(TermSeparator)) {
            throw new ArgumentException($"Entry on line {lineNumber} contains more than one term separator: {line}");
          }

          ValidateGlossaryTerm(source);
          ValidateGlossaryTerm(target);
        }

        if (!sourceTerms.Add(source)) {
          throw new ArgumentException($"Entry on line {lineNumber} duplicates source term \"{source}\"");
        }

        entries.Add((source, target));
      }

      if (!skipChecks && entries.Count == 0) {
        throw new ArgumentException($"Argument {nameof(contentTsv)} contains no TSV entries");
      }

      return entries;
    }
  }
}
