// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Linq;
using System.Threading.Tasks;
using DeepL;

namespace DeepLTests.RealApi {
  internal static class RealApiHelpers {
    private static readonly string RunId =
          DateTime.UtcNow.ToString("yyyyMMddHHmm") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);

    /// <summary>Name for a resource created by this test run, e.g. sdktest-dotnet-202610151530-k3f9qz-glossary.</summary>
    internal static string GetUniqueName(string label) => $"sdktest-dotnet-{RunId}-{label}";

    /// <summary>Time limit for waiting on a job that takes a minute or more.</summary>
    internal static readonly TimeSpan JobTimeout = TimeSpan.FromMinutes(3);

    /// <summary>A new resource can be briefly invisible, so retry while the API reports it as not found.</summary>
    internal static async Task<T> RetryWhileNotFound<T>(Func<Task<T>> action) {
      for (var attempt = 0;; attempt++) {
        try {
          return await action();
        } catch (NotFoundException) when (attempt < 9) {
          await Task.Delay(TimeSpan.FromSeconds(1));
        }
      }
    }
  }
}
