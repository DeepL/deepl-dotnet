// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Threading.Tasks;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class TranslateText {
    [Fact]
    public async Task SendsPostV2TranslateAndParsesTheResult() {
      var result = await RealApiClient.CreateClient().TranslateTextAsync(Fixtures.EnglishText, null, "de");

      Assert.NotEmpty(result.Text);
      Assert.Equal("en", result.DetectedSourceLanguageCode);
      Assert.Equal(Fixtures.EnglishText.Length, result.BilledCharacters);
    }
  }
}
