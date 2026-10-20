// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Threading.Tasks;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class TranslateTextTest {
    [Fact]
    public async Task SendsPostV2TranslateAndParsesTheResult() {
      var result = await RealApiClient.CreateClient().TranslateTextAsync(Fixtures.EnglishText, null, "de");

      Assert.NotEmpty(result.Text);
      Assert.NotEmpty(result.DetectedSourceLanguageCode);
      Assert.True(result.BilledCharacters > 0);
    }
  }
}
