// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.IO;

namespace DeepLTests {
  /// <summary>Fixtures shared by the offline and the real-API suite.</summary>
  internal static class Fixtures {
    internal const string EnglishText = "proton beam";
    internal const string GermanText = "Protonenstrahl";

    /// <summary>Creates an empty temporary directory.</summary>
    internal static string TempDir() {
      var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(path);
      return path;
    }

    /// <summary>Full path of a file in the <c>resources</c> directory next to the test assembly.</summary>
    internal static string ResourcePath(string fileName) =>
          Path.Combine(Directory.GetCurrentDirectory(), "resources", fileName);

    /// <summary>Writes an example .txt document into a new temporary directory and returns its path.</summary>
    internal static string CreateTextDocument(string? content = null) {
      var path = Path.Combine(TempDir(), "example_document.txt");
      File.WriteAllText(path, content ?? EnglishText);
      return path;
    }
  }
}
