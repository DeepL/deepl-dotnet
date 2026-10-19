// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System.Collections.Generic;
using System.Threading.Tasks;
using DeepL;
using NSubstitute;
using Xunit;

namespace DeepLTests.Offline {
  internal class SystemUnderTest {
    private readonly ITranslator _translator;

    internal SystemUnderTest(ITranslator translator) => _translator = translator;

    internal Task<GlossaryEntries> DoSomething() => _translator.GetGlossaryEntriesAsync("test123");
  }

  public class SubstitutionExamples {
    [Fact]
    public void Test() {
      // Arrange
      var translator = Substitute.For<ITranslator>();
      var returnedResult = new GlossaryEntries(new List<(string Key, string Value)> { ("foo", "bar") });
      var task = Task.FromResult(returnedResult);
      translator.GetGlossaryEntriesAsync(Arg.Any<string>()).Returns(task);
      var sut = new SystemUnderTest(translator);

      // Act
      var result = sut.DoSomething();

      // Assert
      translator.Received().GetGlossaryEntriesAsync(Arg.Any<string>());
      Assert.Equal(task, result);
    }
  }
}
