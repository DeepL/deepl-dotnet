// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using NSubstitute;
using Xunit;

namespace DeepLTests {
  /// <summary>
  ///   Unit tests for the fluent document-translation layer in <c>FluentDocumentTranslation.cs</c>.
  ///   Stream overloads are preferred where possible to avoid disk I/O.
  /// </summary>
  public sealed class FluentDocumentTranslationTest {
    private static readonly DocumentHandle SampleHandle = new DocumentHandle("doc-id", "doc-key");

    // ---------- SaveTo / one-shot translation ----------

    [Fact]
    public async Task StreamInput_SaveToStream_CallsStreamOverload() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream(Encoding.UTF8.GetBytes("content"));
      using var output = new MemoryStream();

      await translator
            .TranslateDocument(input, "input.docx")
            .From("en")
            .To("de")
            .WithFormality(Formality.More)
            .WithGlossaryId("glossary-x")
            .SaveTo(output);

      await translator.Received(1).TranslateDocumentAsync(
            input,
            "input.docx",
            output,
            "en",
            "de",
            Arg.Is<DocumentTranslateOptions?>(o =>
                  o != null && o.Formality == Formality.More && o.GlossaryId == "glossary-x"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FileInput_SaveToFileInfo_CallsFileInfoOverload() {
      var translator = Substitute.For<ITranslator>();
      var input = new FileInfo(Path.GetTempFileName());
      var output = new FileInfo(Path.GetTempFileName() + ".out");
      try {
        File.WriteAllText(input.FullName, "hello");

        await translator
              .TranslateDocument(input)
              .To("de")
              .WithMinification()
              .WithOutputFormat("pdf")
              .SaveTo(output);

        await translator.Received(1).TranslateDocumentAsync(
              Arg.Is<FileInfo>(f => f.FullName == input.FullName),
              Arg.Is<FileInfo>(f => f.FullName == output.FullName),
              null,
              "de",
              Arg.Is<DocumentTranslateOptions?>(o =>
                    o != null && o.EnableDocumentMinification && o.OutputFormat == "pdf"),
              Arg.Any<CancellationToken>());
      } finally {
        input.Refresh();
        if (input.Exists) input.Delete();
      }
    }

    [Fact]
    public async Task UsingDelegate_MutatesOptions() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();
      DocumentTranslateOptions? captured = null;
      translator.TranslateDocumentAsync(
                  Arg.Any<Stream>(),
                  Arg.Any<string>(),
                  Arg.Any<Stream>(),
                  Arg.Any<string?>(),
                  Arg.Any<string>(),
                  Arg.Do<DocumentTranslateOptions?>(o => captured = o),
                  Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

      await translator.TranslateDocument(input, "in.docx").To("de")
            .Using(opts => {
              opts.Formality = Formality.Less;
              opts.OutputFormat = "txt";
            })
            .SaveTo(output);

      Assert.NotNull(captured);
      Assert.Equal(Formality.Less, captured!.Formality);
      Assert.Equal("txt", captured.OutputFormat);
    }

    [Fact]
    public async Task UsingOptionsObject_CopiesFields() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();
      DocumentTranslateOptions? captured = null;
      translator.TranslateDocumentAsync(
                  Arg.Any<Stream>(),
                  Arg.Any<string>(),
                  Arg.Any<Stream>(),
                  Arg.Any<string?>(),
                  Arg.Any<string>(),
                  Arg.Do<DocumentTranslateOptions?>(o => captured = o),
                  Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

      var prepared = new DocumentTranslateOptions {
        Formality = Formality.More,
        GlossaryId = "glossary-id",
        OutputFormat = "docx",
        EnableDocumentMinification = true,
      };

      await translator.TranslateDocument(input, "in.docx").To("de").Using(prepared).SaveTo(output);

      Assert.Equal(Formality.More, captured!.Formality);
      Assert.Equal("glossary-id", captured.GlossaryId);
      Assert.Equal("docx", captured.OutputFormat);
      Assert.True(captured.EnableDocumentMinification);
    }

    [Fact]
    public async Task WithCancellation_PassesToken() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();
      using var cts = new CancellationTokenSource();

      await translator.TranslateDocument(input, "in.docx").To("de").WithCancellation(cts.Token).SaveTo(output);

      await translator.Received(1).TranslateDocumentAsync(
            input,
            "in.docx",
            output,
            Arg.Any<string?>(),
            "de",
            Arg.Any<DocumentTranslateOptions?>(),
            cts.Token);
    }

    // ---------- Upload-only / split flow ----------

    [Fact]
    public async Task UploadAsync_StreamInput_ReturnsHandle() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      translator.TranslateDocumentUploadAsync(
                  input, "in.docx", Arg.Any<string?>(), "de",
                  Arg.Any<DocumentTranslateOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(SampleHandle));

      var handle = await translator.TranslateDocument(input, "in.docx").To("de").UploadAsync();

      Assert.Equal("doc-id", handle.DocumentId);
    }

    [Fact]
    public async Task UploadAsync_FileInput_CallsFileOverload() {
      var translator = Substitute.For<ITranslator>();
      var input = new FileInfo(Path.GetTempFileName());
      try {
        File.WriteAllText(input.FullName, "content");
        translator.TranslateDocumentUploadAsync(
                    Arg.Is<FileInfo>(f => f.FullName == input.FullName),
                    Arg.Any<string?>(), "de",
                    Arg.Any<DocumentTranslateOptions?>(), Arg.Any<CancellationToken>())
              .Returns(Task.FromResult(SampleHandle));

        var handle = await translator.TranslateDocument(input).To("de").UploadAsync();

        Assert.Equal("doc-id", handle.DocumentId);
      } finally {
        input.Refresh();
        if (input.Exists) input.Delete();
      }
    }

    // ---------- Validation ----------

    [Fact]
    public async Task MissingTarget_SaveTo_Throws() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();

      await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await translator.TranslateDocument(input, "in.docx").SaveTo(output));
    }

    [Fact]
    public async Task MissingTarget_Upload_Throws() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();

      await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await translator.TranslateDocument(input, "in.docx").UploadAsync());
    }

    [Fact]
    public void TranslateDocument_NullInput_Throws() {
      var translator = Substitute.For<ITranslator>();
      Assert.Throws<ArgumentNullException>(() => { _ = translator.TranslateDocument((FileInfo)null!); });
      Assert.Throws<ArgumentNullException>(() => { _ = translator.TranslateDocument(null!, "in.docx"); });
      Assert.Throws<ArgumentException>(() => { _ = translator.TranslateDocument(new MemoryStream(), ""); });
    }

    [Fact]
    public void TranslateDocument_NullTranslator_Throws() {
      ITranslator? translator = null;
      Assert.Throws<ArgumentNullException>(
            () => { _ = translator!.TranslateDocument(new MemoryStream(), "in.docx"); });
    }

    // ---------- DocumentRef ----------

    [Fact]
    public async Task DocumentRef_GetStatusAsync_Forwards() {
      var translator = Substitute.For<ITranslator>();
      var status = new DocumentStatus("doc-id", DocumentStatus.StatusCode.Done, null, 42, null);
      translator.TranslateDocumentStatusAsync(SampleHandle, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(status));

      var result = await translator.Document(SampleHandle).GetStatusAsync();

      Assert.Same(status, result);
    }

    [Fact]
    public async Task DocumentRef_WaitUntilDoneAsync_Forwards() {
      var translator = Substitute.For<ITranslator>();

      await translator.Document(SampleHandle).WaitUntilDoneAsync();

      await translator.Received(1).TranslateDocumentWaitUntilDoneAsync(
            SampleHandle, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DocumentRef_DownloadToAsync_StreamOverload() {
      var translator = Substitute.For<ITranslator>();
      using var output = new MemoryStream();

      await translator.Document(SampleHandle).DownloadToAsync(output);

      await translator.Received(1).TranslateDocumentDownloadAsync(
            SampleHandle, output, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DocumentRef_DownloadToAsync_FileInfoOverload() {
      var translator = Substitute.For<ITranslator>();
      var output = new FileInfo(Path.GetTempFileName() + ".out");

      await translator.Document(SampleHandle).DownloadToAsync(output);

      await translator.Received(1).TranslateDocumentDownloadAsync(
            SampleHandle,
            Arg.Is<FileInfo>(f => f.FullName == output.FullName),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Document_NullTranslator_Throws() {
      ITranslator? translator = null;
      Assert.Throws<ArgumentNullException>(() => { _ = translator!.Document(SampleHandle); });
    }
  }
}
