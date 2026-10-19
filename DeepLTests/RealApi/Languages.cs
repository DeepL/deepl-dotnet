// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Threading.Tasks;
using DeepL.Model;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class Languages {
    [Fact]
    public async Task SendsGetV3LanguagesAndParsesTheSourceLanguages() {
      var languages = await RealApiClient.CreateClient().GetSourceLanguagesAsync();

      Assert.Single(languages, language => language.Code == "en");
      Assert.All(languages, language => Assert.NotEmpty(language.Name));
    }

    [Fact]
    public async Task SendsGetV3LanguagesResourcesAndParsesTheResourcesAndFeatures() {
      var resources = await RealApiClient.CreateClient().GetLanguageResourcesAsync();

      Assert.NotEmpty(resources);
      Assert.All(resources, resource => Assert.All(resource.Features, feature => Assert.Equal(feature.Key, feature.Value.Name)));
      var translateText = Assert.Single(resources, resource => resource.Name == LanguageResource.TranslateText);
      Assert.True(translateText.Features[LanguageFeature.Glossary].NeedsSourceSupport);
      Assert.True(translateText.Features[LanguageFeature.Glossary].NeedsTargetSupport);
    }
  }
}
