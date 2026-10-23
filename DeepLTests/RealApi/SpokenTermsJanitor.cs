// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeepLTests.RealApi {
  /// <summary>
  ///   Before the spoken terms tests run, deletes sdktest-* Spoken Terms collections left behind by runs that started
  ///   more than 2 hours ago. Best-effort: errors are logged and skipped.
  /// </summary>
  public sealed class SpokenTermsJanitor {
    private static readonly Regex TestName = new Regex("^sdktest-[a-z]+-(\\d{12})-");
    private static readonly TimeSpan MaxAge = TimeSpan.FromHours(2);

    public SpokenTermsJanitor() {
      Task.Run(DeleteStaleAsync).GetAwaiter().GetResult();
    }

    private static async Task DeleteStaleAsync() {
      try {
        var client = RealApiClient.CreateClient();
        foreach (var spokenTerms in await client.ListSpokenTermsAsync()) {
          var match = TestName.Match(spokenTerms.Name ?? string.Empty);
          if (!match.Success ||
              !DateTime.TryParseExact(
                    match.Groups[1].Value,
                    "yyyyMMddHHmm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var started) ||
              DateTime.UtcNow - started <= MaxAge) {
            continue;
          }

          try {
            await client.DeleteSpokenTermsAsync(spokenTerms.SpokenTermsId);
          } catch (Exception exception) {
            Console.Error.WriteLine($"Janitor could not delete {spokenTerms}: {exception.Message}");
          }
        }
      } catch (Exception exception) {
        Console.Error.WriteLine($"Janitor could not list spoken terms: {exception.Message}");
      }
    }
  }
}
