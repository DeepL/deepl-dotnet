// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Threading.Tasks;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class Usage {
    [Fact]
    public async Task SendsGetV2UsageAndParsesTheUsage() {
      var usage = await RealApiClient.CreateClient().GetUsageAsync();

      Assert.Contains("Usage this billing period", usage.ToString());
    }
  }
}
