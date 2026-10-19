// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class Languages : OfflineTest {
    private const string TranslateTextLanguages =
          "[{\"lang\":\"de\",\"name\":\"German\",\"usable_as_source\":true,\"usable_as_target\":true," +
          "\"features\":{\"formality\":{\"status\":\"stable\"}}}," +
          "{\"lang\":\"en-GB\",\"name\":\"English (British)\",\"usable_as_source\":false,\"usable_as_target\":true}," +
          "{\"lang\":\"EN\",\"name\":\"English\",\"usable_as_source\":true,\"usable_as_target\":false}]";

    [Fact]
    public async Task SendsTranslateTextResourceAndParsesSourceLanguages() {
      ExpectNoBody(HttpMethod.Get, "v3/languages")
            .WithExactQueryString("resource=translate_text")
            .RespondJson(TranslateTextLanguages);
      var languages = await CreateClient().GetSourceLanguagesAsync();
      Assert.Equal(new[] { "de", "en" }, languages.Select(language => language.Code));
      Assert.Equal("German", languages[0].Name);
    }

    [Fact]
    public async Task ParsesTargetLanguagesWithFormalitySupport() {
      ExpectNoBody(HttpMethod.Get, "v3/languages")
            .WithExactQueryString("resource=translate_text")
            .RespondJson(TranslateTextLanguages);
      var languages = await CreateClient().GetTargetLanguagesAsync();
      Assert.Equal(new[] { "de", "en-GB" }, languages.Select(language => language.Code));
      Assert.True(languages[0].SupportsFormality);
      Assert.False(languages[1].SupportsFormality);
    }

    [Fact]
    public async Task ParsesGlossaryLanguagePairsWithoutIdenticalLanguages() {
      ExpectNoBody(HttpMethod.Get, "v3/languages")
            .WithExactQueryString("resource=glossary")
            .RespondJson(
                  "[{\"lang\":\"de\",\"name\":\"German\",\"usable_as_source\":true,\"usable_as_target\":true}," +
                  "{\"lang\":\"en\",\"name\":\"English\",\"usable_as_source\":true,\"usable_as_target\":true}]");
      var pairs = await CreateClient().GetGlossaryLanguagesAsync();
      Assert.Equal(
            new[] { "de>en", "en>de" },
            pairs.Select(pair => $"{pair.SourceLanguageCode}>{pair.TargetLanguageCode}").OrderBy(pair => pair));
    }

    [Fact]
    public async Task SendsResourceAndIncludesInQuery() {
      ExpectNoBody(HttpMethod.Get, "v3/languages")
            .WithExactQueryString("resource=glossary&include=beta&include=external")
            .RespondJson("[]");
      ExpectNoBody(HttpMethod.Get, "v3/languages").WithExactQueryString("resource=glossary").RespondJson("[]");
      var client = CreateClient();
      await client.GetLanguagesForResourceAsync(
            LanguageResource.Glossary,
            new[] { LanguageSupport.IncludeBeta, LanguageSupport.IncludeExternal });
      await client.GetLanguagesForResourceAsync(LanguageResource.Glossary);
    }

    [Fact]
    public async Task ParsesLanguageSupportAndFeatures() {
      ExpectNoBody(HttpMethod.Get, "v3/languages")
            .WithExactQueryString("resource=translate_text")
            .RespondJson(
                  "[{\"lang\":\"de\",\"name\":\"German\",\"status\":\"stable\",\"usable_as_source\":true," +
                  "\"usable_as_target\":false,\"features\":{\"formality\":{\"status\":\"beta\"}}}," +
                  "{\"lang\":\"fr\",\"name\":\"French\",\"status\":\"beta\",\"usable_as_source\":false," +
                  "\"usable_as_target\":true}]");
      var languages = await CreateClient().GetLanguagesForResourceAsync(LanguageResource.TranslateText);
      var german = languages[0];
      Assert.Equal("de", german.Code);
      Assert.Equal("German", german.Name);
      Assert.Equal("stable", german.Status);
      Assert.True(german.UsableAsSource);
      Assert.False(german.UsableAsTarget);
      Assert.True(german.Supports(LanguageFeature.Formality));
      Assert.Equal("beta", german.Features[LanguageFeature.Formality].Status);
      Assert.Empty(languages[1].Features);
      Assert.False(languages[1].Supports(LanguageFeature.Formality));
    }

    [Fact]
    public async Task ParsesLanguageResources() {
      ExpectNoBody(HttpMethod.Get, "v3/languages/resources")
            .RespondJson(
                  "[{\"name\":\"translate_text\",\"features\":[{\"name\":\"formality\",\"needs_source_support\":false," +
                  "\"needs_target_support\":true},{\"name\":\"glossary\"}]},{\"name\":\"voice\"}]");
      var resources = await CreateClient().GetLanguageResourcesAsync();
      Assert.Equal(new[] { "translate_text", "voice" }, resources.Select(resource => resource.Name));
      var formality = resources[0].Features[LanguageFeature.Formality];
      Assert.Equal("formality", formality.Name);
      Assert.False(formality.NeedsSourceSupport);
      Assert.True(formality.NeedsTargetSupport);
      Assert.False(resources[0].Features[LanguageFeature.Glossary].NeedsSourceSupport);
      Assert.Empty(resources[1].Features);
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenResourceIsEmpty() =>
          await Assert.ThrowsAsync<ArgumentException>(() => CreateClient().GetLanguagesForResourceAsync(""));

    [Fact]
    public async Task ThrowsDeepLExceptionWhenResourceIsInvalid() {
      ExpectNoBody(HttpMethod.Get, "v3/languages")
            .WithExactQueryString("resource=invalid_resource")
            .RespondJson("{\"message\":\"Invalid resource\"}", HttpStatusCode.BadRequest);
      await Assert.ThrowsAsync<DeepLException>(() => CreateClient().GetLanguagesForResourceAsync("invalid_resource"));
    }
  }
}
