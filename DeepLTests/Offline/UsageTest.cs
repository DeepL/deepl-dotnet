// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class UsageTest : OfflineTest {
    [Fact]
    public async Task ParsesCharacterAndDocumentCounts() {
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .RespondJson(
                  "{\"character_count\":20,\"character_limit\":20,\"document_count\":0,\"document_limit\":1}");
      var usage = await CreateClient().GetUsageAsync();
      Assert.Equal(20, usage.Character!.Count);
      Assert.Equal(20, usage.Character.Limit);
      Assert.True(usage.Character.LimitReached);
      Assert.False(usage.Document!.LimitReached);
      Assert.Null(usage.TeamDocument);
      Assert.True(usage.AnyLimitReached);
      Assert.Contains("Characters: 20 of 20", usage.ToString());
      Assert.Contains("Documents: 0 of 1", usage.ToString());
      Assert.DoesNotContain("Team documents", usage.ToString());
    }

    [Fact]
    public async Task ParsesTeamDocumentLimitOnly() {
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .RespondJson("{\"team_document_count\":1,\"team_document_limit\":1}");
      var usage = await CreateClient().GetUsageAsync();
      Assert.Null(usage.Character);
      Assert.Null(usage.Document);
      Assert.True(usage.TeamDocument!.LimitReached);
      Assert.True(usage.AnyLimitReached);
      Assert.Contains("Team documents: 1 of 1", usage.ToString());
      Assert.DoesNotContain("Characters", usage.ToString());
    }

    [Fact]
    public async Task ReportsNoLimitReachedBelowLimits() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson("{\"character_count\":1,\"character_limit\":2}");
      Assert.False((await CreateClient().GetUsageAsync()).AnyLimitReached);
    }
    private const string ProUsageReply = @"{
      ""character_count"": 5947223,
      ""character_limit"": 1000000000000,
      ""products"": [
        {""product_type"": ""write"", ""billing_unit"": ""characters"", ""api_key_unit_count"": 0,
         ""account_unit_count"": 5643, ""api_key_character_count"": 0, ""character_count"": 5643},
        {""product_type"": ""translate"", ""billing_unit"": ""characters"", ""api_key_unit_count"": 636,
         ""account_unit_count"": 5941580, ""api_key_character_count"": 636, ""character_count"": 5941580},
        {""product_type"": ""speechToText"", ""billing_unit"": ""minutes"", ""api_key_unit_count"": 30,
         ""account_unit_count"": 30, ""api_key_character_count"": 0, ""character_count"": 0},
        {""product_type"": ""speechToSpeech"", ""billing_unit"": ""minutes"", ""api_key_unit_count"": 12,
         ""account_unit_count"": 12, ""api_key_character_count"": 0, ""character_count"": 0}
      ],
      ""api_key_character_count"": 636,
      ""api_key_character_limit"": 1000000000000,
      ""speech_to_text_milliseconds_count"": 0,
      ""speech_to_text_milliseconds_limit"": 0,
      ""speech_to_text_minutes_count"": 30,
      ""speech_to_text_minutes_limit"": 600,
      ""speech_to_speech_minutes_count"": 12,
      ""speech_to_speech_minutes_limit"": 600,
      ""start_time"": ""2025-05-13T09:18:42Z"",
      ""end_time"": ""2025-06-13T09:18:42Z""
    }";

    [Fact]
    public async Task ParsesApiProFields() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson(ProUsageReply);
      var usage = await CreateClient().GetUsageAsync();

      Assert.Equal(5947223, usage.Character!.Count);
      Assert.Equal(1000000000000, usage.Character.Limit);
      Assert.Equal(636, usage.ApiKeyCharacter!.Count);
      Assert.Equal(1000000000000, usage.ApiKeyCharacter.Limit);
      Assert.Equal(30, usage.SpeechToTextMinutes!.Count);
      Assert.Equal(600, usage.SpeechToTextMinutes.Limit);
      Assert.Equal(12, usage.SpeechToSpeechMinutes!.Count);
      Assert.Equal(600, usage.SpeechToSpeechMinutes.Limit);
      Assert.Equal(new DateTime(2025, 5, 13, 9, 18, 42, DateTimeKind.Utc), usage.StartTime);
      Assert.Equal(new DateTime(2025, 6, 13, 9, 18, 42, DateTimeKind.Utc), usage.EndTime);

      var products = usage.Products!;
      Assert.Equal(new[] { "write", "translate", "speechToText", "speechToSpeech" },
            Array.ConvertAll(products, p => p.ProductType));
      Assert.Equal(new[] { "characters", "characters", "minutes", "minutes" },
            Array.ConvertAll(products, p => p.BillingUnit));
      Assert.Equal(new long?[] { 0, 636, 30, 12 }, Array.ConvertAll(products, p => p.ApiKeyUnitCount));
      Assert.Equal(new long?[] { 5643, 5941580, 30, 12 }, Array.ConvertAll(products, p => p.AccountUnitCount));

      Assert.False(usage.AnyLimitReached);
      Assert.Equal("Usage this billing period:\nCharacters: 5947223 of 1000000000000", usage.ToString());
    }

    [Fact]
    public async Task ParsesProductEntriesWithMissingOrNullFields() {
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .RespondJson(
                  "{\"character_count\":5,\"character_limit\":10,\"products\":[{}," +
                  "{\"product_type\":null,\"billing_unit\":null,\"api_key_unit_count\":null," +
                  "\"account_unit_count\":null}]}");
      var usage = await CreateClient().GetUsageAsync();

      Assert.Equal(2, usage.Products!.Length);
      foreach (var product in usage.Products) {
        Assert.Null(product.ProductType);
        Assert.Null(product.BillingUnit);
        Assert.Null(product.ApiKeyUnitCount);
        Assert.Null(product.AccountUnitCount);
      }

      Assert.Equal(5, usage.Character!.Count);
      Assert.Equal(10, usage.Character.Limit);
    }

    [Fact]
    public async Task LeavesApiProFieldsNullForFreeResponse() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson("{\"character_count\":180118,\"character_limit\":1250000}");
      var usage = await CreateClient().GetUsageAsync();

      Assert.Equal(180118, usage.Character!.Count);
      Assert.Equal(1250000, usage.Character.Limit);
      Assert.Null(usage.Document);
      Assert.Null(usage.TeamDocument);
      Assert.Null(usage.ApiKeyCharacter);
      Assert.Null(usage.SpeechToTextMinutes);
      Assert.Null(usage.SpeechToSpeechMinutes);
      Assert.Null(usage.Products);
      Assert.Null(usage.StartTime);
      Assert.Null(usage.EndTime);
      Assert.Equal("Usage this billing period:\nCharacters: 180118 of 1250000", usage.ToString());
    }

    [Fact]
    public async Task IgnoresAnUnparseableBillingPeriod() {
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .RespondJson(
                  "{\"character_count\":20,\"character_limit\":500000," +
                  "\"start_time\":\"not a timestamp\",\"end_time\":\"\"}");
      var usage = await CreateClient().GetUsageAsync();

      Assert.Equal(20, usage.Character!.Count);
      Assert.Null(usage.StartTime);
      Assert.Null(usage.EndTime);
    }

    [Fact]
    public void KeepsTheSixArgumentJsonFieldsStructConstructor() {
      var usage = new DeepL.Model.Usage(new DeepL.Model.Usage.JsonFieldsStruct(1, 2, null, null, null, null));
      Assert.Equal(1, usage.Character!.Count);
      Assert.Null(usage.ApiKeyCharacter);
      Assert.Null(usage.Products);
    }

    [Fact]
    public void JsonFieldsStructExposesOnlyTheReleasedFields() {
      var type = typeof(DeepL.Model.Usage.JsonFieldsStruct);
      var constructor = Assert.Single(type.GetConstructors());
      Assert.Equal(6, constructor.GetParameters().Length);
      Assert.Equal(
            new[] {
                  "CharacterCount", "CharacterLimit", "DocumentCount", "DocumentLimit", "TeamDocumentCount",
                  "TeamDocumentLimit"
            },
            Array.ConvertAll(type.GetProperties(), p => p.Name));
    }
  }
}
