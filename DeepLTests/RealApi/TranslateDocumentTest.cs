// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class TranslateDocumentTest {
    [Fact]
    public async Task UploadsPollsAndDownloadsATxtDocument() {
      var input = Fixtures.CreateTextDocument();
      var output = Path.Combine(Fixtures.TempDir(), "output_document.txt");

      await RealApiClient.CreateClient().TranslateDocumentAsync(new FileInfo(input), new FileInfo(output), null, "de");

      Assert.True(new FileInfo(output).Length > 0);
    }
  }
}
