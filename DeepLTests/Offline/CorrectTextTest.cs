// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DeepL;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class CorrectTextTest : OfflineTest {
    private const string Reply =
          "{\"improvements\":[{\"text\":\"fixed\",\"target_language\":\"DE\",\"detected_source_language\":\"DE\"}]}";

    [Fact]
    public async Task SendsTargetLanguageAndText() {
      ExpectForm(HttpMethod.Post, "v2/write/correct", ("target_lang", "de"), ("text", "typo"))
            .RespondJson(Reply);
      await CreateClient().CorrectTextAsync("typo", "de");
    }

    [Fact]
    public async Task SendsOnlyTextWhenTargetLanguageIsNull() {
      ExpectForm(HttpMethod.Post, "v2/write/correct", ("text", "typo")).RespondJson(Reply);
      await CreateClient().CorrectTextAsync("typo", null);
    }

    [Fact]
    public async Task SendsTextArrayInOrder() {
      Expect(HttpMethod.Post, "v2/write/correct")
            .WithContent("text=a&text=b")
            .RespondJson(
                  "{\"improvements\":[{\"text\":\"x\",\"target_language\":\"DE\",\"detected_source_language\":\"DE\"}," +
                  "{\"text\":\"y\",\"target_language\":\"DE\",\"detected_source_language\":\"DE\"}]}");
      await CreateClient().CorrectTextAsync(new[] { "a", "b" }, null);
    }

    [Fact]
    public async Task ParsesCorrection() {
      ExpectForm(HttpMethod.Post, "v2/write/correct", ("text", "typo")).RespondJson(Reply);
      var result = await CreateClient().CorrectTextAsync("typo", null);
      Assert.Equal("de", result.DetectedSourceLanguageCode);
    }

    [Fact]
    public async Task SendsOnlyTextWhenTargetLanguageIsOmitted() {
      ExpectForm(HttpMethod.Post, "v2/write/correct", ("text", "typo")).RespondJson(Reply);
      await CreateClient().CorrectTextAsync("typo");
    }

    [Fact]
    public async Task SendsExtraBodyParametersLastReplacingFields() {
      Expect(HttpMethod.Post, "v2/write/correct")
            .WithContent("text=typo&target_lang=en-GB&extra=1")
            .RespondJson(Reply);
      await CreateClient().CorrectTextAsync(
            "typo",
            "de",
            new CorrectTextOptions {
                  ExtraBodyParameters = new Dictionary<string, string> { ["target_lang"] = "en-GB", ["extra"] = "1" }
            });
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenTextIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().CorrectTextAsync("", "de"));
  }
}
