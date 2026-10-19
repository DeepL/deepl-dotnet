// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Threading.Tasks;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class RephraseText {
    [Fact]
    public async Task SendsPostV2WriteRephraseAndParsesTheResult() {
      var result = await RealApiClient.CreateClient().RephraseTextAsync(Fixtures.GermanText, "de");

      Assert.NotEmpty(result.Text);
      Assert.NotEmpty(result.DetectedSourceLanguageCode);
      Assert.Equal("de", result.TargetLanguageCode.ToLowerInvariant());
    }
  }
}
