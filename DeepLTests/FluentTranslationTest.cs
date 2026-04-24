// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using NSubstitute;
using Xunit;

namespace DeepLTests {
  /// <summary>
  ///   Unit tests for the fluent translation / rephrase layer in <c>FluentTranslation.cs</c>.
  ///   Tests mock the underlying <see cref="ITranslator" /> / <see cref="IWriter" /> to verify
  ///   that fluent configuration flows into the correct call arguments.
  /// </summary>
  public sealed class FluentTranslationTest {
    private static TextResult MakeTextResult(string text = "Hallo") =>
          new TextResult(text, "en", text.Length, null);

    private static WriteResult MakeWriteResult(string text = "Better") =>
          new WriteResult(text, "en", "en");

    private static ITranslator MakeTranslator(params TextResult[] results) {
      var translator = Substitute.For<ITranslator>();
      translator.TranslateTextAsync(
                  Arg.Any<IEnumerable<string>>(),
                  Arg.Any<string?>(),
                  Arg.Any<string>(),
                  Arg.Any<TextTranslateOptions?>(),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(results.Length > 0 ? results : new[] { MakeTextResult() }));
      return translator;
    }

    private static IWriter MakeWriter(params WriteResult[] results) {
      var writer = Substitute.For<IWriter>();
      writer.RephraseTextAsync(
                  Arg.Any<IEnumerable<string>>(),
                  Arg.Any<string?>(),
                  Arg.Any<TextRephraseOptions?>(),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(results.Length > 0 ? results : new[] { MakeWriteResult() }));
      return writer;
    }

    // ---------- Text translation: happy paths ----------

    [Fact]
    public async Task SingleText_ToTarget_CallsUnderlyingWithSingletonAndReturnsFirstResult() {
      var expected = MakeTextResult("Hallo");
      var translator = MakeTranslator(expected);

      var result = await translator.Translate("Hello").To("de");

      Assert.Same(expected, result);
      await translator.Received(1).TranslateTextAsync(
            Arg.Is<IEnumerable<string>>(xs => xs.SequenceEqual(new[] { "Hello" })),
            null,
            "de",
            Arg.Any<TextTranslateOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SingleText_WithFrom_PassesSourceLanguage() {
      var translator = MakeTranslator();

      await translator.Translate("Hello").From("en").To("de");

      await translator.Received(1).TranslateTextAsync(
            Arg.Any<IEnumerable<string>>(),
            "en",
            "de",
            Arg.Any<TextTranslateOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BatchText_ReturnsArray() {
      var r1 = MakeTextResult("a");
      var r2 = MakeTextResult("b");
      var translator = MakeTranslator(r1, r2);

      TextResult[] result = await translator.Translate(new[] { "x", "y" }).To("de");

      Assert.Equal(2, result.Length);
      Assert.Same(r1, result[0]);
      Assert.Same(r2, result[1]);
      await translator.Received(1).TranslateTextAsync(
            Arg.Is<IEnumerable<string>>(xs => xs.SequenceEqual(new[] { "x", "y" })),
            null,
            "de",
            Arg.Any<TextTranslateOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ParamsOverload_AcceptsVarargs() {
      var translator = MakeTranslator(MakeTextResult("a"), MakeTextResult("b"), MakeTextResult("c"));

      TextResult[] result = await translator.Translate("x", "y", "z").To("de");

      Assert.Equal(3, result.Length);
      await translator.Received(1).TranslateTextAsync(
            Arg.Is<IEnumerable<string>>(xs => xs.SequenceEqual(new[] { "x", "y", "z" })),
            null,
            "de",
            Arg.Any<TextTranslateOptions?>(),
            Arg.Any<CancellationToken>());
    }

    // ---------- Option configuration ----------

    [Fact]
    public async Task WithFormality_SetsOptionsFormality() {
      var translator = MakeTranslator();
      TextTranslateOptions? captured = null;
      translator.TranslateTextAsync(
                  Arg.Any<IEnumerable<string>>(),
                  Arg.Any<string?>(),
                  Arg.Any<string>(),
                  Arg.Do<TextTranslateOptions?>(o => captured = o),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { MakeTextResult() }));

      await translator.Translate("Hi").To("de").WithFormality(Formality.More);

      Assert.NotNull(captured);
      Assert.Equal(Formality.More, captured!.Formality);
    }

    [Fact]
    public async Task WithGlossaryId_And_WithStyleId_PropagateIds() {
      var translator = MakeTranslator();
      TextTranslateOptions? captured = null;
      translator.TranslateTextAsync(
                  Arg.Any<IEnumerable<string>>(),
                  Arg.Any<string?>(),
                  Arg.Any<string>(),
                  Arg.Do<TextTranslateOptions?>(o => captured = o),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { MakeTextResult() }));

      await translator.Translate("Hi").To("de")
            .WithGlossaryId("glossary-123")
            .WithStyleId("style-456")
            .WithModel(ModelType.QualityOptimized);

      Assert.Equal("glossary-123", captured!.GlossaryId);
      Assert.Equal("style-456", captured.StyleId);
      Assert.Equal(ModelType.QualityOptimized, captured.ModelType);
    }

    [Fact]
    public async Task WithCustomInstructions_AppendsToList() {
      var translator = MakeTranslator();
      TextTranslateOptions? captured = null;
      translator.TranslateTextAsync(
                  Arg.Any<IEnumerable<string>>(),
                  Arg.Any<string?>(),
                  Arg.Any<string>(),
                  Arg.Do<TextTranslateOptions?>(o => captured = o),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { MakeTextResult() }));

      await translator.Translate("Hi").To("de")
            .WithCustomInstruction("keep it short")
            .WithCustomInstructions("no jargon", "playful");

      Assert.Equal(new[] { "keep it short", "no jargon", "playful" }, captured!.CustomInstructions);
    }

    [Fact]
    public async Task UsingDelegate_MutatesOptions() {
      var translator = MakeTranslator();
      TextTranslateOptions? captured = null;
      translator.TranslateTextAsync(
                  Arg.Any<IEnumerable<string>>(),
                  Arg.Any<string?>(),
                  Arg.Any<string>(),
                  Arg.Do<TextTranslateOptions?>(o => captured = o),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { MakeTextResult() }));

      await translator.Translate("Hi").To("de")
            .Using(o => {
              o.Context = "test context";
              o.PreserveFormatting = true;
              o.IgnoreTags.Add("code");
            });

      Assert.Equal("test context", captured!.Context);
      Assert.True(captured.PreserveFormatting);
      Assert.Contains("code", captured.IgnoreTags);
    }

    [Fact]
    public async Task UsingOptionsObject_CopiesFieldsOntoBuilder() {
      var translator = MakeTranslator();
      TextTranslateOptions? captured = null;
      translator.TranslateTextAsync(
                  Arg.Any<IEnumerable<string>>(),
                  Arg.Any<string?>(),
                  Arg.Any<string>(),
                  Arg.Do<TextTranslateOptions?>(o => captured = o),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { MakeTextResult() }));

      var prepared = new TextTranslateOptions {
        Formality = Formality.Less,
        GlossaryId = "g",
        Context = "ctx",
      };
      prepared.IgnoreTags.Add("x");

      await translator.Translate("Hi").To("de").Using(prepared);

      Assert.Equal(Formality.Less, captured!.Formality);
      Assert.Equal("g", captured.GlossaryId);
      Assert.Equal("ctx", captured.Context);
      Assert.Contains("x", captured.IgnoreTags);
    }

    [Fact]
    public async Task WithCancellation_PassesToken() {
      var translator = MakeTranslator();
      using var cts = new CancellationTokenSource();

      await translator.Translate("Hi").To("de").WithCancellation(cts.Token);

      await translator.Received(1).TranslateTextAsync(
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<TextTranslateOptions?>(),
            cts.Token);
    }

    // ---------- Validation ----------

    [Fact]
    public async Task MissingTarget_ThrowsInvalidOperationException() {
      var translator = MakeTranslator();

      await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await translator.Translate("Hi"));
    }

    [Fact]
    public void Translate_NullText_Throws() {
      var translator = MakeTranslator();
      Assert.Throws<ArgumentNullException>(() => { _ = translator.Translate((string)null!); });
    }

    [Fact]
    public void Translate_NullEnumerable_Throws() {
      var translator = MakeTranslator();
      Assert.Throws<ArgumentNullException>(() => { _ = translator.Translate((IEnumerable<string>)null!); });
    }

    [Fact]
    public void Translate_NullTranslator_Throws() {
      ITranslator? translator = null;
      Assert.Throws<ArgumentNullException>(() => { _ = translator!.Translate("Hi"); });
    }

    [Fact]
    public void To_NullLanguage_Throws() {
      var translator = MakeTranslator();
      Assert.Throws<ArgumentNullException>(() => { _ = translator.Translate("Hi").To(null!); });
    }

    // ---------- Task conversion ----------

    [Fact]
    public async Task ImplicitTaskConversion_Works() {
      var translator = MakeTranslator(MakeTextResult("Hallo"));

      Task<TextResult> task = translator.Translate("Hello").To("de");
      var result = await task;

      Assert.Equal("Hallo", result.Text);
    }

    [Fact]
    public async Task BatchImplicitTaskConversion_Works() {
      var translator = MakeTranslator(MakeTextResult("a"), MakeTextResult("b"));

      Task<TextResult[]> task = translator.Translate(new[] { "x", "y" }).To("de");
      var result = await task;

      Assert.Equal(2, result.Length);
    }

    // ---------- Rephrase ----------

    [Fact]
    public async Task Rephrase_Single_CallsUnderlyingAndReturnsFirst() {
      var expected = MakeWriteResult("Better");
      var writer = MakeWriter(expected);

      var result = await writer.Rephrase("Bad text").To("en-US").WithStyle("business");

      Assert.Same(expected, result);
      await writer.Received(1).RephraseTextAsync(
            Arg.Is<IEnumerable<string>>(xs => xs.SequenceEqual(new[] { "Bad text" })),
            "en-US",
            Arg.Is<TextRephraseOptions?>(o => o != null && o.WritingStyle == "business"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rephrase_Batch_ReturnsArray() {
      var writer = MakeWriter(MakeWriteResult("a"), MakeWriteResult("b"));

      WriteResult[] result = await writer.Rephrase(new[] { "x", "y" }).To("en").WithTone("friendly");

      Assert.Equal(2, result.Length);
      await writer.Received(1).RephraseTextAsync(
            Arg.Is<IEnumerable<string>>(xs => xs.SequenceEqual(new[] { "x", "y" })),
            "en",
            Arg.Is<TextRephraseOptions?>(o => o != null && o.WritingTone == "friendly"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rephrase_UsingDelegate_Mutates() {
      var writer = MakeWriter();
      TextRephraseOptions? captured = null;
      writer.RephraseTextAsync(
                  Arg.Any<IEnumerable<string>>(),
                  Arg.Any<string?>(),
                  Arg.Do<TextRephraseOptions?>(o => captured = o),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { MakeWriteResult() }));

      await writer.Rephrase("Bad").To(null).Using(o => {
        o.WritingStyle = "academic";
      });

      Assert.Equal("academic", captured!.WritingStyle);
    }
  }
}
