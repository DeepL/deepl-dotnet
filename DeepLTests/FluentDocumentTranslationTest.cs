// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
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
        // EnableDocumentMinification is intentionally omitted: it requires FileInfo→FileInfo and
        // is verified separately in FileInput_SaveToFileInfo_CallsFileInfoOverload.
      };

      await translator.TranslateDocument(input, "in.docx").To("de").Using(prepared).SaveTo(output);

      Assert.Equal(Formality.More, captured!.Formality);
      Assert.Equal("glossary-id", captured.GlossaryId);
      Assert.Equal("docx", captured.OutputFormat);
    }

    [Fact]
    public async Task WithCancellation_ExternalCancelPropagatesThroughLinkedToken() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();
      using var cts = new CancellationTokenSource();

      // Hold the library call open until we cancel, so the linked CTS survives long enough to observe.
      var tcs = new TaskCompletionSource<bool>();
      CancellationToken capturedToken = default;
      translator.TranslateDocumentAsync(
                  Arg.Any<Stream>(),
                  Arg.Any<string>(),
                  Arg.Any<Stream>(),
                  Arg.Any<string?>(),
                  Arg.Any<string>(),
                  Arg.Any<DocumentTranslateOptions?>(),
                  Arg.Do<CancellationToken>(t => {
                    capturedToken = t;
                    t.Register(() => tcs.TrySetCanceled(t));
                  }))
            .Returns(_ => tcs.Task);

      var job = translator.TranslateDocument(input, "in.docx").To("de")
            .WithCancellation(cts.Token).SaveTo(output);

      // On net462 the Task continuation that invokes the library method (and runs our Arg.Do
      // callback to capture the token) may not have landed yet. Wait briefly for the mock to
      // record the token.
      for (var i = 0; i < 50 && capturedToken == default; i++) {
        await Task.Delay(10);
      }
      Assert.NotEqual(default, capturedToken);

      // The token passed to the library is the LINKED token (not the user's raw cts.Token),
      // but cancelling the original cts should propagate cancellation into it.
      Assert.False(capturedToken.IsCancellationRequested);
      cts.Cancel();
      Assert.True(capturedToken.IsCancellationRequested);

      await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await job);
    }

    // ---------- DocumentTranslationJob: Cancel() ----------

    [Fact]
    public async Task SaveTo_ReturnsAwaitableJob() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();

      // The returned value must be awaitable as a Task (implicit conversion) and as a job.
      DocumentTranslationJob job = translator.TranslateDocument(input, "in.docx").To("de").SaveTo(output);
      await job;
      Assert.True(job.IsCompleted);

      // Implicit conversion to Task also works (Task.WhenAll, etc.)
      using var input2 = new MemoryStream();
      using var output2 = new MemoryStream();
      Task t = translator.TranslateDocument(input2, "in.docx").To("de").SaveTo(output2);
      await t;
    }

    [Fact]
    public async Task Job_Cancel_PropagatesThroughLinkedToken() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();

      // Capture the token the library receives, and make its Task never complete until cancelled.
      var tcs = new TaskCompletionSource<bool>();
      CancellationToken capturedToken = default;
      translator.TranslateDocumentAsync(
                  Arg.Any<Stream>(),
                  Arg.Any<string>(),
                  Arg.Any<Stream>(),
                  Arg.Any<string?>(),
                  Arg.Any<string>(),
                  Arg.Any<DocumentTranslateOptions?>(),
                  Arg.Do<CancellationToken>(t => {
                    capturedToken = t;
                    t.Register(() => tcs.TrySetCanceled(t));
                  }))
            .Returns(_ => tcs.Task);

      var job = translator.TranslateDocument(input, "in.docx").To("de").SaveTo(output);

      Assert.False(job.IsCompleted);
      job.Cancel();
      Assert.True(capturedToken.IsCancellationRequested);

      await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await job);
      Assert.True(job.IsCompleted);
    }

    [Fact]
    public async Task Job_Cancel_AfterCompletion_IsNoOp() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();

      var job = translator.TranslateDocument(input, "in.docx").To("de").SaveTo(output);
      await job;

      // Calling Cancel after completion must not throw (and must not affect anything).
      job.Cancel();
      job.Cancel();
      Assert.True(job.IsCompleted);
    }

    // ---------- WithProgress: IProgress<DocumentStatus> callbacks ----------

    [Fact]
    public async Task WithProgress_ReportsStatusDuringPolling() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();
      var handle = new DocumentHandle("doc-id", "doc-key");

      // Return the handle from upload
      translator.TranslateDocumentUploadAsync(
                  Arg.Any<Stream>(), Arg.Any<string>(),
                  Arg.Any<string?>(), Arg.Any<string>(),
                  Arg.Any<DocumentTranslateOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(handle));

      // Sequence of status ticks: translating → translating → done
      var statusQueue = new Queue<DocumentStatus>(new[] {
        new DocumentStatus("doc-id", DocumentStatus.StatusCode.Translating, 1, null, null),
        new DocumentStatus("doc-id", DocumentStatus.StatusCode.Translating, 1, null, null),
        new DocumentStatus("doc-id", DocumentStatus.StatusCode.Done, null, 42, null),
      });
      translator.TranslateDocumentStatusAsync(handle, Arg.Any<CancellationToken>())
            .Returns(_ => statusQueue.Dequeue());

      // Download does nothing; just return a completed Task.
      translator.TranslateDocumentDownloadAsync(handle, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

      var reported = new List<DocumentStatus>();
      var progress = new Progress<DocumentStatus>(reported.Add);

      await translator.TranslateDocument(input, "in.docx").To("de")
            .WithProgress(progress)
            .SaveTo(output);

      // Progress should have been reported 3 times (matching the status sequence).
      // Progress<T> marshals to the captured sync context; give it a tick to flush.
      for (var i = 0; i < 50 && reported.Count < 3; i++) {
        await Task.Delay(10);
      }

      Assert.Equal(3, reported.Count);
      Assert.Equal(DocumentStatus.StatusCode.Translating, reported[0].Status);
      Assert.Equal(DocumentStatus.StatusCode.Done, reported[reported.Count - 1].Status);

      // The upload + download must have been invoked exactly once each.
      await translator.Received(1).TranslateDocumentUploadAsync(
            Arg.Any<Stream>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string>(),
            Arg.Any<DocumentTranslateOptions?>(), Arg.Any<CancellationToken>());
      await translator.Received(1).TranslateDocumentDownloadAsync(
            handle, Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WithProgress_ErrorStatus_ThrowsDeepLException() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();
      var handle = new DocumentHandle("doc-id", "doc-key");

      translator.TranslateDocumentUploadAsync(
                  Arg.Any<Stream>(), Arg.Any<string>(),
                  Arg.Any<string?>(), Arg.Any<string>(),
                  Arg.Any<DocumentTranslateOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(handle));
      translator.TranslateDocumentStatusAsync(handle, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(
                  new DocumentStatus("doc-id", DocumentStatus.StatusCode.Error, null, null, "something went wrong")));

      var progress = new Progress<DocumentStatus>(_ => { });

      var ex = await Assert.ThrowsAsync<DeepLException>(
            async () => await translator.TranslateDocument(input, "in.docx").To("de")
                  .WithProgress(progress).SaveTo(output));
      Assert.Contains("something went wrong", ex.Message);
    }

    [Fact]
    public void WithProgress_NullProgress_Throws() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      var builder = translator.TranslateDocument(input, "in.docx").To("de");
      Assert.Throws<ArgumentNullException>(() => { _ = builder.WithProgress(null!); });
    }

    [Fact]
    public async Task DocumentRef_WaitUntilDoneAsync_WithProgress_ReportsTicks() {
      var translator = Substitute.For<ITranslator>();
      var handle = new DocumentHandle("doc-id", "doc-key");

      var statusQueue = new Queue<DocumentStatus>(new[] {
        new DocumentStatus("doc-id", DocumentStatus.StatusCode.Translating, 1, null, null),
        new DocumentStatus("doc-id", DocumentStatus.StatusCode.Done, null, 42, null),
      });
      translator.TranslateDocumentStatusAsync(handle, Arg.Any<CancellationToken>())
            .Returns(_ => statusQueue.Dequeue());

      var reported = new List<DocumentStatus>();
      var progress = new Progress<DocumentStatus>(reported.Add);

      await translator.Document(handle).WaitUntilDoneAsync(progress);

      for (var i = 0; i < 50 && reported.Count < 2; i++) {
        await Task.Delay(10);
      }

      Assert.Equal(2, reported.Count);
      Assert.Equal(DocumentStatus.StatusCode.Done, reported[reported.Count - 1].Status);
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

    [Fact]
    public void WithMinification_StreamInput_SaveToStream_Throws() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      using var output = new MemoryStream();
      Assert.Throws<InvalidOperationException>(() => {
        translator.TranslateDocument(input, "in.docx").To("de").WithMinification().SaveTo(output).GetAwaiter()
                  .GetResult();
      });
    }

    [Fact]
    public void WithMinification_StreamInput_SaveToFileInfo_Throws() {
      var translator = Substitute.For<ITranslator>();
      using var input = new MemoryStream();
      var output = new FileInfo(Path.GetTempFileName() + ".out");
      Assert.Throws<InvalidOperationException>(() => {
        translator.TranslateDocument(input, "in.docx").To("de").WithMinification().SaveTo(output).GetAwaiter()
                  .GetResult();
      });
    }

    [Fact]
    public void WithMinification_FileInput_SaveToStream_Throws() {
      var translator = Substitute.For<ITranslator>();
      var input = new FileInfo(Path.GetTempFileName());
      try {
        File.WriteAllText(input.FullName, "content");
        using var output = new MemoryStream();
        Assert.Throws<InvalidOperationException>(() => {
          translator.TranslateDocument(input).To("de").WithMinification().SaveTo(output).GetAwaiter().GetResult();
        });
      } finally {
        input.Refresh();
        if (input.Exists) input.Delete();
      }
    }

    [Fact]
    public void WithMinification_WithProgress_Throws() {
      var translator = Substitute.For<ITranslator>();
      var input = new FileInfo(Path.GetTempFileName());
      try {
        File.WriteAllText(input.FullName, "content");
        var output = new FileInfo(Path.GetTempFileName() + ".out");
        var progress = new Progress<DocumentStatus>(_ => { });
        Assert.Throws<InvalidOperationException>(() => {
          translator.TranslateDocument(input).To("de").WithMinification().WithProgress(progress).SaveTo(output)
                    .GetAwaiter().GetResult();
        });
      } finally {
        input.Refresh();
        if (input.Exists) input.Delete();
      }
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
