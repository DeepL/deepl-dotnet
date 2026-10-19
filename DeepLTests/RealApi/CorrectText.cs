// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Threading.Tasks;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class CorrectText {
    [Fact]
    public async Task SendsPostV2WriteCorrectAndParsesTheResult() {
      var result = await RealApiClient.CreateClient().CorrectTextAsync(Fixtures.GermanText, "de");

      Assert.NotEmpty(result.Text);
      Assert.Equal("de", result.DetectedSourceLanguageCode);
      Assert.Equal("de", result.TargetLanguageCode.ToLowerInvariant());
    }
  }
}
