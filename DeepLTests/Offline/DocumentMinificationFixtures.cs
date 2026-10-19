// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DeepLTests.Offline {
  internal static class DocumentMinificationFixtures {
    private static readonly Dictionary<string, string> Templates = new Dictionary<string, string> {
      { ".docx", "example_document_template.docx" },
      { ".pptx", "example_presentation_template.pptx" },
      { ".zip", "example_zip_template.zip" }
    };

    /// <summary>
    ///   Creates a copy of a template document with a multi-megabyte placeholder image added, so minification has
    ///   something to remove. Returns the path of the new file.
    /// </summary>
    internal static string CreateDocumentWithLargeMedia(string extension, string outputDirectory) {
      var extractionDir = Fixtures.TempDir();
      var outputFilePath = Path.Combine(outputDirectory, "test_document" + extension);
      ZipFile.ExtractToDirectory(Fixtures.ResourcePath(Templates[extension]), extractionDir);
      var random = new Random(42);
      var characters = new char[3000000];
      for (var i = 0; i < characters.Length; i++) {
        characters[i] = (char)('!' + random.Next(90));
      }

      File.WriteAllText(Path.Combine(extractionDir, "placeholder_image.png"), new string(characters));
      ZipFile.CreateFromDirectory(extractionDir, outputFilePath);
      Directory.Delete(extractionDir, true);
      return outputFilePath;
    }

    /// <summary>Returns the content of every entry of a zip file by entry name.</summary>
    internal static Dictionary<string, byte[]> ReadEntries(string zipPath) {
      using var zip = ZipFile.OpenRead(zipPath);
      return zip.Entries.Where(entry => entry.Length > 0 || !entry.FullName.EndsWith("/")).ToDictionary(
            entry => entry.FullName,
            entry => {
              using var stream = entry.Open();
              using var memory = new MemoryStream();
              stream.CopyTo(memory);
              return memory.ToArray();
            });
    }
  }
}
