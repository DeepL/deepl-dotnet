// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Fluent, LINQ-style entry points for the DeepL API. Builders returned from these
  ///   extensions are directly awaitable; executing them calls the underlying
  ///   <see cref="ITranslator" /> / <see cref="IWriter" /> methods.
  /// </summary>
  /// <example>
  ///   <code>
  ///     TextResult result = await translator.Translate("Hello").From("en").To("de");
  ///
  ///     TextResult styled = await translator
  ///       .Translate("Hello")
  ///       .To("de")
  ///       .WithFormality(Formality.More)
  ///       .WithStyle(styleRule)
  ///       .Using(opts =&gt; opts.CustomInstructions.Add("Keep it playful"));
  ///
  ///     TextResult[] many = await translator.Translate(new[] { "a", "b" }).To("de");
  ///   </code>
  /// </example>
  public static class FluentTranslationExtensions {
    /// <summary>Starts a fluent translation of a single text. Awaiting the returned builder yields a <see cref="TextResult" />.</summary>
    public static TextTranslationBuilder Translate(this ITranslator translator, string text) {
      if (translator == null) throw new ArgumentNullException(nameof(translator));
      if (text == null) throw new ArgumentNullException(nameof(text));
      return new TextTranslationBuilder(translator, new[] { text });
    }

    /// <summary>Starts a fluent translation of multiple texts. Awaiting the returned builder yields a <see cref="TextResult" />[].</summary>
    public static TextTranslationBatchBuilder Translate(this ITranslator translator, IEnumerable<string> texts) {
      if (translator == null) throw new ArgumentNullException(nameof(translator));
      if (texts == null) throw new ArgumentNullException(nameof(texts));
      return new TextTranslationBatchBuilder(translator, texts);
    }

    /// <summary>Starts a fluent translation of multiple texts. Awaiting the returned builder yields a <see cref="TextResult" />[].</summary>
    public static TextTranslationBatchBuilder Translate(this ITranslator translator, params string[] texts) {
      if (translator == null) throw new ArgumentNullException(nameof(translator));
      if (texts == null) throw new ArgumentNullException(nameof(texts));
      return new TextTranslationBatchBuilder(translator, texts);
    }

    /// <summary>Starts a fluent rephrase of a single text. Awaiting yields a <see cref="WriteResult" />.</summary>
    public static TextRephraseBuilder Rephrase(this IWriter writer, string text) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (text == null) throw new ArgumentNullException(nameof(text));
      return new TextRephraseBuilder(writer, new[] { text });
    }

    /// <summary>Starts a fluent rephrase of multiple texts. Awaiting yields a <see cref="WriteResult" />[].</summary>
    public static TextRephraseBatchBuilder Rephrase(this IWriter writer, IEnumerable<string> texts) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (texts == null) throw new ArgumentNullException(nameof(texts));
      return new TextRephraseBatchBuilder(writer, texts);
    }
  }

  /// <summary>
  ///   Common fluent configuration for text-translation builders.
  ///   Derived builders differ only in the shape of the awaited result.
  /// </summary>
  /// <typeparam name="TSelf">The concrete builder type, for fluent chaining.</typeparam>
  public abstract class TextTranslationBuilderBase<TSelf>
        where TSelf : TextTranslationBuilderBase<TSelf> {
    internal readonly ITranslator Translator;
    internal readonly IEnumerable<string> Texts;
    internal readonly TextTranslateOptions Options = new TextTranslateOptions();
    internal string? SourceLanguageCode;
    internal string? TargetLanguageCode;
    internal CancellationToken CancellationToken;

    internal TextTranslationBuilderBase(ITranslator translator, IEnumerable<string> texts) {
      Translator = translator;
      Texts = texts;
    }

    private TSelf Self => (TSelf)this;

    /// <summary>Sets the target language code.</summary>
    public TSelf To(string targetLanguageCode) {
      TargetLanguageCode = targetLanguageCode ?? throw new ArgumentNullException(nameof(targetLanguageCode));
      return Self;
    }

    /// <summary>Sets the source language code. Pass <c>null</c> to rely on auto-detection.</summary>
    public TSelf From(string? sourceLanguageCode) {
      SourceLanguageCode = sourceLanguageCode;
      return Self;
    }

    /// <summary>Copies fields from the supplied options onto this builder.</summary>
    public TSelf Using(TextTranslateOptions options) {
      if (options == null) throw new ArgumentNullException(nameof(options));
      CopyOptions(options, Options);
      return Self;
    }

    /// <summary>Mutates the builder's options via the supplied delegate.</summary>
    public TSelf Using(Action<TextTranslateOptions> configure) {
      if (configure == null) throw new ArgumentNullException(nameof(configure));
      configure(Options);
      return Self;
    }

    /// <summary>Sets translation context (not counted toward billing).</summary>
    public TSelf WithContext(string context) {
      Options.Context = context;
      return Self;
    }

    /// <summary>Sets the desired formality level.</summary>
    public TSelf WithFormality(Formality formality) {
      Options.Formality = formality;
      return Self;
    }

    /// <summary>Uses the specified glossary.</summary>
    public TSelf WithGlossary(GlossaryInfo glossary) {
      if (glossary == null) throw new ArgumentNullException(nameof(glossary));
      Options.GlossaryId = glossary.GlossaryId;
      return Self;
    }

    /// <summary>Uses the specified multilingual glossary.</summary>
    public TSelf WithGlossary(MultilingualGlossaryInfo glossary) {
      if (glossary == null) throw new ArgumentNullException(nameof(glossary));
      Options.GlossaryId = glossary.GlossaryId;
      return Self;
    }

    /// <summary>Uses the glossary identified by <paramref name="glossaryId" />.</summary>
    public TSelf WithGlossaryId(string glossaryId) {
      Options.GlossaryId = glossaryId ?? throw new ArgumentNullException(nameof(glossaryId));
      return Self;
    }

    /// <summary>Uses the specified style rule.</summary>
    public TSelf WithStyle(StyleRuleInfo styleRule) {
      if (styleRule == null) throw new ArgumentNullException(nameof(styleRule));
      Options.StyleId = styleRule.StyleId;
      return Self;
    }

    /// <summary>Uses the style rule identified by <paramref name="styleId" />.</summary>
    public TSelf WithStyleId(string styleId) {
      Options.StyleId = styleId ?? throw new ArgumentNullException(nameof(styleId));
      return Self;
    }

    /// <summary>Selects the translation model to use.</summary>
    public TSelf WithModel(ModelType modelType) {
      Options.ModelType = modelType;
      return Self;
    }

    /// <summary>Enables tag handling. Use "xml" or "html".</summary>
    public TSelf WithTagHandling(string tagHandling, string? tagHandlingVersion = null) {
      Options.TagHandling = tagHandling ?? throw new ArgumentNullException(nameof(tagHandling));
      if (tagHandlingVersion != null) Options.TagHandlingVersion = tagHandlingVersion;
      return Self;
    }

    /// <summary>Adds a single custom instruction to guide the translation.</summary>
    public TSelf WithCustomInstruction(string instruction) {
      if (instruction == null) throw new ArgumentNullException(nameof(instruction));
      Options.CustomInstructions.Add(instruction);
      return Self;
    }

    /// <summary>Adds one or more custom instructions to guide the translation.</summary>
    public TSelf WithCustomInstructions(params string[] instructions) {
      if (instructions == null) throw new ArgumentNullException(nameof(instructions));
      foreach (var i in instructions) Options.CustomInstructions.Add(i);
      return Self;
    }

    /// <summary>Disables automatic tag detection (<see cref="TextTranslateOptions.OutlineDetection" /> = false).</summary>
    public TSelf WithoutOutlineDetection() {
      Options.OutlineDetection = false;
      return Self;
    }

    /// <summary>Preserves original formatting.</summary>
    public TSelf PreserveFormatting() {
      Options.PreserveFormatting = true;
      return Self;
    }

    /// <summary>Sets the sentence splitting mode.</summary>
    public TSelf WithSentenceSplitting(SentenceSplittingMode mode) {
      Options.SentenceSplittingMode = mode;
      return Self;
    }

    /// <summary>Associates a cancellation token with the eventual request.</summary>
    public TSelf WithCancellation(CancellationToken cancellationToken) {
      CancellationToken = cancellationToken;
      return Self;
    }

    internal Task<TextResult[]> ExecuteAllAsync() {
      if (TargetLanguageCode == null) {
        throw new InvalidOperationException(
              "Target language is required. Call .To(targetLanguageCode) before awaiting.");
      }

      return Translator.TranslateTextAsync(
            Texts,
            SourceLanguageCode,
            TargetLanguageCode,
            Options,
            CancellationToken);
    }

    private static void CopyOptions(TextTranslateOptions src, TextTranslateOptions dst) {
      dst.Context = src.Context;
      dst.Formality = src.Formality;
      dst.GlossaryId = src.GlossaryId;
      dst.StyleId = src.StyleId;
      dst.OutlineDetection = src.OutlineDetection;
      dst.PreserveFormatting = src.PreserveFormatting;
      dst.SentenceSplittingMode = src.SentenceSplittingMode;
      dst.TagHandling = src.TagHandling;
      dst.TagHandlingVersion = src.TagHandlingVersion;
      dst.ModelType = src.ModelType;
      ReplaceAll(dst.IgnoreTags, src.IgnoreTags);
      ReplaceAll(dst.NonSplittingTags, src.NonSplittingTags);
      ReplaceAll(dst.SplittingTags, src.SplittingTags);
      ReplaceAll(dst.CustomInstructions, src.CustomInstructions);
    }

    private static void ReplaceAll(List<string> dst, List<string> src) {
      dst.Clear();
      foreach (var item in src) dst.Add(item);
    }
  }

  /// <summary>
  ///   Fluent builder for translating a single text. <c>await</c> produces a <see cref="TextResult" />.
  /// </summary>
  public sealed class TextTranslationBuilder : TextTranslationBuilderBase<TextTranslationBuilder> {
    internal TextTranslationBuilder(ITranslator translator, IEnumerable<string> texts)
          : base(translator, texts) { }

    /// <summary>Executes the translation and returns the single <see cref="TextResult" />.</summary>
    public async Task<TextResult> ExecuteAsync() => (await ExecuteAllAsync().ConfigureAwait(false))[0];

    /// <summary>Enables direct <c>await</c> on the builder.</summary>
    public TaskAwaiter<TextResult> GetAwaiter() => ExecuteAsync().GetAwaiter();

    /// <summary>Implicit conversion so the builder may be passed where a <see cref="Task{TextResult}" /> is expected.</summary>
    public static implicit operator Task<TextResult>(TextTranslationBuilder builder) =>
          builder?.ExecuteAsync() ?? throw new ArgumentNullException(nameof(builder));
  }

  /// <summary>
  ///   Fluent builder for translating multiple texts. <c>await</c> produces a <see cref="TextResult" />[].
  /// </summary>
  public sealed class TextTranslationBatchBuilder : TextTranslationBuilderBase<TextTranslationBatchBuilder> {
    internal TextTranslationBatchBuilder(ITranslator translator, IEnumerable<string> texts)
          : base(translator, texts) { }

    /// <summary>Executes the translation and returns the <see cref="TextResult" /> array.</summary>
    public Task<TextResult[]> ExecuteAsync() => ExecuteAllAsync();

    /// <summary>Enables direct <c>await</c> on the builder.</summary>
    public TaskAwaiter<TextResult[]> GetAwaiter() => ExecuteAsync().GetAwaiter();

    /// <summary>Implicit conversion so the builder may be passed where a <see cref="Task{TextResultArray}" /> is expected.</summary>
    public static implicit operator Task<TextResult[]>(TextTranslationBatchBuilder builder) =>
          builder?.ExecuteAsync() ?? throw new ArgumentNullException(nameof(builder));
  }

  /// <summary>
  ///   Common fluent configuration for text-rephrase builders.
  /// </summary>
  /// <typeparam name="TSelf">The concrete builder type, for fluent chaining.</typeparam>
  public abstract class TextRephraseBuilderBase<TSelf>
        where TSelf : TextRephraseBuilderBase<TSelf> {
    internal readonly IWriter Writer;
    internal readonly IEnumerable<string> Texts;
    internal readonly TextRephraseOptions Options = new TextRephraseOptions();
    internal string? TargetLanguageCode;
    internal CancellationToken CancellationToken;

    internal TextRephraseBuilderBase(IWriter writer, IEnumerable<string> texts) {
      Writer = writer;
      Texts = texts;
    }

    private TSelf Self => (TSelf)this;

    /// <summary>Sets the target language for the rephrasing. Pass <c>null</c> to rephrase in-language.</summary>
    public TSelf To(string? targetLanguageCode) {
      TargetLanguageCode = targetLanguageCode;
      return Self;
    }

    /// <summary>Sets the writing style. Mutually exclusive with <see cref="WithTone" />.</summary>
    public TSelf WithStyle(string writingStyle) {
      Options.WritingStyle = writingStyle ?? throw new ArgumentNullException(nameof(writingStyle));
      return Self;
    }

    /// <summary>Sets the writing tone. Mutually exclusive with <see cref="WithStyle" />.</summary>
    public TSelf WithTone(string writingTone) {
      Options.WritingTone = writingTone ?? throw new ArgumentNullException(nameof(writingTone));
      return Self;
    }

    /// <summary>Copies fields from the supplied options onto this builder.</summary>
    public TSelf Using(TextRephraseOptions options) {
      if (options == null) throw new ArgumentNullException(nameof(options));
      Options.WritingStyle = options.WritingStyle;
      Options.WritingTone = options.WritingTone;
      return Self;
    }

    /// <summary>Mutates the options via the supplied delegate.</summary>
    public TSelf Using(Action<TextRephraseOptions> configure) {
      if (configure == null) throw new ArgumentNullException(nameof(configure));
      configure(Options);
      return Self;
    }

    /// <summary>Associates a cancellation token with the eventual request.</summary>
    public TSelf WithCancellation(CancellationToken cancellationToken) {
      CancellationToken = cancellationToken;
      return Self;
    }

    internal Task<WriteResult[]> ExecuteAllAsync() =>
          Writer.RephraseTextAsync(Texts, TargetLanguageCode, Options, CancellationToken);
  }

  /// <summary>Fluent builder for rephrasing a single text. <c>await</c> produces a <see cref="WriteResult" />.</summary>
  public sealed class TextRephraseBuilder : TextRephraseBuilderBase<TextRephraseBuilder> {
    internal TextRephraseBuilder(IWriter writer, IEnumerable<string> texts) : base(writer, texts) { }

    /// <summary>Executes the rephrase and returns the single <see cref="WriteResult" />.</summary>
    public async Task<WriteResult> ExecuteAsync() => (await ExecuteAllAsync().ConfigureAwait(false))[0];

    /// <summary>Enables direct <c>await</c>.</summary>
    public TaskAwaiter<WriteResult> GetAwaiter() => ExecuteAsync().GetAwaiter();

    public static implicit operator Task<WriteResult>(TextRephraseBuilder builder) =>
          builder?.ExecuteAsync() ?? throw new ArgumentNullException(nameof(builder));
  }

  /// <summary>Fluent builder for rephrasing multiple texts. <c>await</c> produces a <see cref="WriteResult" />[].</summary>
  public sealed class TextRephraseBatchBuilder : TextRephraseBuilderBase<TextRephraseBatchBuilder> {
    internal TextRephraseBatchBuilder(IWriter writer, IEnumerable<string> texts) : base(writer, texts) { }

    /// <summary>Executes the rephrase and returns the <see cref="WriteResult" /> array.</summary>
    public Task<WriteResult[]> ExecuteAsync() => ExecuteAllAsync();

    /// <summary>Enables direct <c>await</c>.</summary>
    public TaskAwaiter<WriteResult[]> GetAwaiter() => ExecuteAsync().GetAwaiter();

    public static implicit operator Task<WriteResult[]>(TextRephraseBatchBuilder builder) =>
          builder?.ExecuteAsync() ?? throw new ArgumentNullException(nameof(builder));
  }
}
