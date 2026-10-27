// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class TranslateDocumentTest : OfflineTest {
    private const string HandleReply = "{\"document_id\":\"d1\",\"document_key\":\"k1\"}";
    private static readonly DocumentHandle Handle = new DocumentHandle("d1", "k1");

    private static MemoryStream Text(string text) => new MemoryStream(Encoding.UTF8.GetBytes(text));

    private async Task Upload(DocumentTranslateOptions? options, params (string Name, string? FileName, string Value)[] expected) {
      var parts = new[] { ("target_lang", (string?)null, "de") }.Concat(expected)
            .Concat(new[] { ("file", (string?)"a.txt", "hello") })
            .ToArray();
      ExpectMultipart("v2/document", parts).RespondJson(HandleReply);
      await CreateClient().TranslateDocumentUploadAsync(Text("hello"), "a.txt", null, "DE", options);
    }

    [Fact]
    public async Task SendsFileAndTargetLanguageAsMultipart() => await Upload(null);

    [Fact]
    public async Task SendsStandardizedSourceLanguage() {
      ExpectMultipart(
                  "v2/document",
                  ("target_lang", null, "en-US"),
                  ("source_lang", null, "de"),
                  ("file", "a.txt", "hello"))
            .RespondJson(HandleReply);
      await CreateClient().TranslateDocumentUploadAsync(Text("hello"), "a.txt", "DE", "EN-us");
    }

    [Fact]
    public async Task SendsDocumentBytesUnchanged() {
      var bytes = File.ReadAllBytes(Fixtures.ResourcePath("example_document_template.docx"));
      byte[]? sent = null;
      Expect(HttpMethod.Post, "v2/document")
            .With(
                  request => {
                    var file = ((MultipartFormDataContent)request.Content!).Single(
                          part => part.Headers.ContentDisposition!.Name == "file");
                    sent = file.ReadAsByteArrayAsync().Result;
                    return file.Headers.ContentDisposition!.FileName == "a.docx";
                  })
            .RespondJson(HandleReply);
      await CreateClient().TranslateDocumentUploadAsync(new MemoryStream(bytes), "a.docx", null, "DE");
      Assert.True(bytes.SequenceEqual(sent!));
    }

    [Fact]
    public Task SendsFormality() =>
          Upload(new DocumentTranslateOptions { Formality = Formality.PreferMore }, ("formality", null, "prefer_more"));

    [Fact]
    public Task SendsOutputFormat() =>
          Upload(new DocumentTranslateOptions { OutputFormat = "pdf" }, ("output_format", null, "pdf"));

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public Task SendsEnableWatermark(bool value, string expected) =>
          Upload(new DocumentTranslateOptions { EnableWatermark = value }, ("enable_watermark", null, expected));

    [Fact]
    public Task OmitsEnableWatermarkWhenUnset() => Upload(new DocumentTranslateOptions());

    [Fact]
    public Task SendsEnableWatermarkWithOutputFormat() =>
          Upload(
                new DocumentTranslateOptions { OutputFormat = "pdf", EnableWatermark = true },
                ("output_format", null, "pdf"),
                ("enable_watermark", null, "true"));

    [Fact]
    public async Task TranslateDocumentSendsEnableWatermark() {
      ExpectMultipart(
                  "v2/document",
                  ("target_lang", null, "de"),
                  ("enable_watermark", null, "true"),
                  ("file", "a.txt", "hello"))
            .RespondJson(HandleReply);
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"done\"}");
      ExpectForm(HttpMethod.Post, "v2/document/d1/result", ("document_key", "k1")).Respond("text/plain", "hallo");
      using var output = new MemoryStream();
      await CreateClient().TranslateDocumentAsync(
            Text("hello"),
            "a.txt",
            output,
            null,
            "DE",
            new DocumentTranslateOptions { EnableWatermark = true });
    }

    [Fact]
    public Task SendsStyleId() =>
          Upload(new DocumentTranslateOptions { StyleId = "s1" }, ("style_id", null, "s1"));

    // Upload() passes no source language: the glossary source-language check must not fire for a style profile.
    [Fact]
    public Task SendsStyleProfileId() =>
          Upload(new DocumentTranslateOptions { StyleProfileId = "sp1" }, ("style_profile_id", null, "sp1"));

    [Fact]
    public Task SendsStyleProfileIdOfStyleProfile() {
      var styleProfile = new StyleProfile("sp1", "n", null, null, null, null, null, false, null, null);
      return Upload(new DocumentTranslateOptions(styleProfile), ("style_profile_id", null, "sp1"));
    }

    [Fact]
    public Task SendsTranslationMemoryIdAndThreshold() =>
          Upload(
                new DocumentTranslateOptions { TranslationMemoryId = "tm1", TranslationMemoryThreshold = 90 },
                ("translation_memory_id", null, "tm1"),
                ("translation_memory_threshold", null, "90"));

    [Fact]
    public async Task SendsGlossaryIdWithSourceLanguage() {
      ExpectMultipart(
                  "v2/document",
                  ("target_lang", null, "de"),
                  ("source_lang", null, "en"),
                  ("glossary_id", null, "g1"),
                  ("file", "a.txt", "hello"))
            .RespondJson(HandleReply);
      await CreateClient().TranslateDocumentUploadAsync(
            Text("hello"),
            "a.txt",
            "EN",
            "DE",
            new DocumentTranslateOptions { GlossaryId = "g1" });
    }

    [Fact]
    public async Task SendsGlossaryIdsWithSourceLanguage() {
      ExpectMultipart(
                  "v2/document",
                  ("target_lang", null, "de"),
                  ("source_lang", null, "en"),
                  ("glossary_ids", null, "g1,g2"),
                  ("file", "a.txt", "hello"))
            .RespondJson(HandleReply);
      await CreateClient().TranslateDocumentUploadAsync(
            Text("hello"),
            "a.txt",
            "EN",
            "DE",
            new DocumentTranslateOptions { GlossaryIds = { "g1", "g2" } });
    }

    [Fact]
    public Task SendsExtraBodyParametersLast() =>
          Upload(
                new DocumentTranslateOptions {
                  Formality = Formality.Less,
                  ExtraBodyParameters = new System.Collections.Generic.Dictionary<string, string> { { "debug", "1" } }
                },
                ("formality", null, "less"),
                ("debug", null, "1"));

    [Fact]
    public async Task ParsesHandle() {
      ExpectMultipart("v2/document", ("target_lang", null, "de"), ("file", "a.txt", "hello")).RespondJson(HandleReply);
      var handle = await CreateClient().TranslateDocumentUploadAsync(Text("hello"), "a.txt", null, "DE");
      Assert.Equal("d1", handle.DocumentId);
      Assert.Equal("k1", handle.DocumentKey);
    }

    [Fact]
    public async Task SendsDocumentKeyWhenAskingForStatus() {
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson(
                  "{\"document_id\":\"d1\",\"status\":\"translating\",\"seconds_remaining\":3,\"billed_characters\":7}");
      var status = await CreateClient().TranslateDocumentStatusAsync(Handle);
      Assert.Equal("d1", status.DocumentId);
      Assert.Equal(DocumentStatus.StatusCode.Translating, status.Status);
      Assert.Equal(3, status.SecondsRemaining);
      Assert.Equal(7, status.BilledCharacters);
      Assert.True(status.Ok);
      Assert.False(status.Done);
    }

    [Fact]
    public async Task ParsesErrorStatus() {
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"error\",\"error_message\":\"boom\"}");
      var status = await CreateClient().TranslateDocumentStatusAsync(Handle);
      Assert.Equal(DocumentStatus.StatusCode.Error, status.Status);
      Assert.Equal("boom", status.ErrorMessage);
      Assert.False(status.Ok);
    }

    [Fact]
    public async Task RetriesStatusUntilDone() {
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"queued\"}");
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"done\"}");
      await CreateClient().TranslateDocumentWaitUntilDoneAsync(Handle);
    }

    [Fact]
    public async Task ThrowsDeepLExceptionWhenStatusIsError() {
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"error\",\"error_message\":\"boom\"}");
      var exception = await Assert.ThrowsAsync<DeepLException>(
            () => CreateClient().TranslateDocumentWaitUntilDoneAsync(Handle));
      Assert.Equal("boom", exception.Message);
    }

    [Fact]
    public async Task SendsDocumentKeyWhenDownloadingToStream() {
      ExpectForm(HttpMethod.Post, "v2/document/d1/result", ("document_key", "k1"))
            .Respond("application/octet-stream", new MemoryStream(new byte[] { 1, 2, 3 }));
      using var output = new MemoryStream();
      await CreateClient().TranslateDocumentDownloadAsync(Handle, output);
      Assert.Equal(new byte[] { 1, 2, 3 }, output.ToArray());
    }

    [Fact]
    public async Task WritesDownloadToFile() {
      ExpectForm(HttpMethod.Post, "v2/document/d1/result", ("document_key", "k1"))
            .Respond("text/plain", "translated");
      var path = Path.Combine(Fixtures.TempDir(), "out.txt");
      await CreateClient().TranslateDocumentDownloadAsync(Handle, new FileInfo(path));
      Assert.Equal("translated", File.ReadAllText(path));
    }

    [Fact]
    public async Task ThrowsDocumentNotReadyExceptionAndRemovesOutputWhenDownloadIs503() {
      ExpectForm(HttpMethod.Post, "v2/document/d1/result", ("document_key", "k1"))
            .RespondJson("{\"message\":\"not ready\"}", HttpStatusCode.ServiceUnavailable);
      var path = Path.Combine(Fixtures.TempDir(), "out.txt");
      await Assert.ThrowsAsync<DocumentNotReadyException>(
            () => CreateClient().TranslateDocumentDownloadAsync(Handle, new FileInfo(path)));
      Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task ThrowsIOExceptionAndKeepsFileWhenOutputFileExists() {
      var path = Path.Combine(Fixtures.TempDir(), "out.txt");
      File.WriteAllText(path, "keep");
      await Assert.ThrowsAsync<IOException>(
            () => CreateClient().TranslateDocumentDownloadAsync(Handle, new FileInfo(path)));
      Assert.Equal("keep", File.ReadAllText(path));
    }

    [Fact]
    public async Task ThrowsIOExceptionAndKeepsFileWhenTranslatingToExistingFile() {
      var input = Fixtures.CreateTextDocument();
      var output = Path.Combine(Fixtures.TempDir(), "out.txt");
      File.WriteAllText(output, "keep");
      await Assert.ThrowsAsync<IOException>(
            () => CreateClient().TranslateDocumentAsync(new FileInfo(input), new FileInfo(output), null, "DE"));
      Assert.Equal("keep", File.ReadAllText(output));
    }

    [Fact]
    public async Task WrapsCancellationInDocumentTranslationException() {
      using var cancellation = new CancellationTokenSource();
      Expect(HttpMethod.Post, "v2/document")
            .Respond(
                  () => {
                    cancellation.Cancel();
                    throw new OperationCanceledException(cancellation.Token);
                  });
      using var output = new MemoryStream();
      var exception = await Assert.ThrowsAsync<DocumentTranslationException>(
            () => CreateClient()
                  .TranslateDocumentAsync(Text("hello"), "a.txt", output, "EN", "DE", null, cancellation.Token));
      Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
    }

    [Fact]
    public async Task UploadsPollsAndDownloadsStream() {
      ExpectMultipart("v2/document", ("target_lang", null, "de"), ("source_lang", null, "en"), ("file", "a.txt", "hello"))
            .RespondJson(HandleReply);
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"done\",\"billed_characters\":5}");
      ExpectForm(HttpMethod.Post, "v2/document/d1/result", ("document_key", "k1")).Respond("text/plain", "hallo");
      using var output = new MemoryStream();
      await CreateClient().TranslateDocumentAsync(Text("hello"), "a.txt", output, "EN", "DE");
      Assert.Equal("hallo", Encoding.UTF8.GetString(output.ToArray()));
    }

    [Fact]
    public async Task UploadsPollsAndDownloadsFile() {
      ExpectMultipart("v2/document", ("target_lang", null, "de"), ("file", "example_document.txt", Fixtures.EnglishText))
            .RespondJson(HandleReply);
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"done\"}");
      ExpectForm(HttpMethod.Post, "v2/document/d1/result", ("document_key", "k1")).Respond("text/plain", "hallo");
      var output = Path.Combine(Fixtures.TempDir(), "out.txt");
      await CreateClient().TranslateDocumentAsync(
            new FileInfo(Fixtures.CreateTextDocument()),
            new FileInfo(output),
            null,
            "DE");
      Assert.Equal("hallo", File.ReadAllText(output));
    }

    [Fact]
    public async Task ThrowsDocumentTranslationExceptionWithHandleWhenTranslationFails() {
      ExpectMultipart("v2/document", ("target_lang", null, "de"), ("file", "a.txt", "hello")).RespondJson(HandleReply);
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"error\",\"error_message\":\"boom\"}");
      var exception = await Assert.ThrowsAsync<DocumentTranslationException>(
            () => CreateClient().TranslateDocumentAsync(Text("hello"), "a.txt", new MemoryStream(), null, "DE"));
      Assert.Contains("boom", exception.Message);
      Assert.Equal(Handle.DocumentId, exception.DocumentHandle!.Value.DocumentId);
      Assert.Equal(Handle.DocumentKey, exception.DocumentHandle!.Value.DocumentKey);
    }

    [Fact]
    public async Task ThrowsDocumentTranslationExceptionWithoutHandleWhenUploadFails() {
      ExpectMultipart("v2/document", ("target_lang", null, "de"), ("file", "a.xyz", "hello"))
            .RespondJson("{\"message\":\"Invalid file\"}", HttpStatusCode.BadRequest);
      var exception = await Assert.ThrowsAsync<DocumentTranslationException>(
            () => CreateClient().TranslateDocumentAsync(Text("hello"), "a.xyz", new MemoryStream(), null, "DE"));
      Assert.Null(exception.DocumentHandle);
      Assert.Contains("Invalid file", exception.Message);
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenGlossaryHasNoSourceLanguage() {
      var options = new DocumentTranslateOptions { GlossaryId = "g1" };
      await Assert.ThrowsAsync<ArgumentException>(
            () => CreateClient().TranslateDocumentUploadAsync(Text("hello"), "a.txt", null, "DE", options));
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenGlossaryIdsHaveNoSourceLanguage() {
      var options = new DocumentTranslateOptions { GlossaryIds = { "g1" } };
      await Assert.ThrowsAsync<ArgumentException>(
            () => CreateClient().TranslateDocumentUploadAsync(Text("hello"), "a.txt", null, "DE", options));
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenThresholdHasNoTranslationMemory() {
      var options = new DocumentTranslateOptions { TranslationMemoryThreshold = 80 };
      await Assert.ThrowsAsync<ArgumentException>(
            () => CreateClient().TranslateDocumentUploadAsync(Text("hello"), "a.txt", null, "DE", options));
    }

    [Fact]
    public async Task ThrowsArgumentOutOfRangeExceptionWhenThresholdIsOutOfRange() {
      var options = new DocumentTranslateOptions { TranslationMemoryId = "tm1", TranslationMemoryThreshold = 101 };
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => CreateClient().TranslateDocumentUploadAsync(Text("hello"), "a.txt", null, "DE", options));
    }
  }
}
