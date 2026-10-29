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
    public Task SendsFormalityValueUnchanged() =>
          Upload(
                new DocumentTranslateOptions { Formality = Formality.Less, FormalityValue = "formal" },
                ("formality", null, "formal"));

#pragma warning disable CS0618 // The deprecated *Id properties stay covered by the existing tests.
    [Fact]
    public Task SendsStyleId() =>
          Upload(new DocumentTranslateOptions { StyleId = "s1" }, ("style_id", null, "s1"));
#pragma warning restore CS0618

    [Fact]
    public Task SendsStyleRuleOfStyleRuleInfo() {
      var styleRule = new StyleRuleInfo("s1", "n", DateTime.UtcNow, DateTime.UtcNow, "en", 1, null, null);
      return Upload(new DocumentTranslateOptions { StyleRule = styleRule }, ("style_id", null, "s1"));
    }

    // Upload() passes no source language: the glossary source-language check must not fire for a style profile.
    [Fact]
    public Task SendsStyleProfile() =>
          Upload(new DocumentTranslateOptions { StyleProfile = "sp1" }, ("style_profile_id", null, "sp1"));

    [Fact]
    public Task SendsStyleProfileOfStyleProfileObject() {
      var styleProfile = new StyleProfile("sp1", "n", null, null, null, null, null, false, null, null);
      return Upload(new DocumentTranslateOptions { StyleProfile = styleProfile }, ("style_profile_id", null, "sp1"));
    }

    [Fact]
    public Task SendsStyleProfileIdOfStyleProfile() {
      var styleProfile = new StyleProfile("sp1", "n", null, null, null, null, null, false, null, null);
      return Upload(new DocumentTranslateOptions(styleProfile), ("style_profile_id", null, "sp1"));
    }

#pragma warning disable CS0618
    [Fact]
    public Task SendsTranslationMemoryIdAndThreshold() =>
          Upload(
                new DocumentTranslateOptions { TranslationMemoryId = "tm1", TranslationMemoryThreshold = 90 },
                ("translation_memory_id", null, "tm1"),
                ("translation_memory_threshold", null, "90"));
#pragma warning restore CS0618

    [Fact]
    public Task SendsTranslationMemoryOfTranslationMemoryInfoAndThreshold() {
      var translationMemory = new TranslationMemoryInfo("tm1", "n", "en", new[] { "de" }, 1);
      return Upload(
            new DocumentTranslateOptions { TranslationMemory = translationMemory, TranslationMemoryThreshold = 90 },
            ("translation_memory_id", null, "tm1"),
            ("translation_memory_threshold", null, "90"));
    }

    [Fact]
    public async Task SendsGlossaryOfGlossaryInfoWithSourceLanguage() {
      ExpectMultipart(
                  "v2/document",
                  ("target_lang", null, "de"),
                  ("source_lang", null, "en"),
                  ("glossary_id", null, "g1"),
                  ("file", "a.txt", "hello"))
            .RespondJson(HandleReply);
      var glossary = new MultilingualGlossaryInfo("g1", "n", new MultilingualGlossaryDictionaryInfo[0], DateTime.UtcNow);
      await CreateClient().TranslateDocumentUploadAsync(
            Text("hello"),
            "a.txt",
            "EN",
            "DE",
            new DocumentTranslateOptions { Glossary = glossary });
    }

    [Fact]
    public async Task SendsGlossaryIdsMixingIdsAndGlossaryObjects() {
      ExpectMultipart(
                  "v2/document",
                  ("target_lang", null, "de"),
                  ("source_lang", null, "en"),
                  ("glossary_ids", null, "g1,g2,g3"),
                  ("file", "a.txt", "hello"))
            .RespondJson(HandleReply);
      var glossary = new GlossaryInfo("g2", "n", true, "en", "de", DateTime.UtcNow, 1);
      var multilingualGlossary =
            new MultilingualGlossaryInfo("g3", "n", new MultilingualGlossaryDictionaryInfo[0], DateTime.UtcNow);
      // The C# 8 compiler of the .NET Core 3.1 SDK ignores [NotNullIfNotNull] on the conversion, hence the "!".
      var options = new DocumentTranslateOptions { GlossaryIds = { "g1", ((string)glossary)! } };
      options.GlossaryIds.Add(((string)multilingualGlossary)!);
      await CreateClient().TranslateDocumentUploadAsync(Text("hello"), "a.txt", "EN", "DE", options);
    }

    [Fact]
    public async Task UploadsStreamWithFilenameOption() {
      ExpectMultipart("v2/document", ("target_lang", null, "de"), ("file", "b.txt", "hello")).RespondJson(HandleReply);
      await CreateClient().TranslateDocumentUploadAsync(
            Text("hello"),
            null,
            "DE",
            new DocumentTranslateOptions { Filename = "b.txt" });
    }

    [Fact]
    public async Task UploadsFileWithFilenameOptionInsteadOfFileName() {
      ExpectMultipart("v2/document", ("target_lang", null, "de"), ("file", "b.txt", Fixtures.EnglishText))
            .RespondJson(HandleReply);
      await CreateClient().TranslateDocumentUploadAsync(
            new FileInfo(Fixtures.CreateTextDocument()),
            null,
            "DE",
            new DocumentTranslateOptions { Filename = "b.txt" });
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenStreamHasNoFilename() {
      var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => CreateClient().TranslateDocumentUploadAsync(Text("hello"), null, "DE", new DocumentTranslateOptions()));
      Assert.Contains("Filename", exception.Message);
    }

    [Fact]
    public async Task NamesBothFileNameSourcesWhenInputFileNameIsNull() {
      var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => CreateClient().TranslateDocumentUploadAsync(Text("hello"), null!, null, "DE"));
      Assert.Contains("inputFileName", exception.Message);
      Assert.Contains("DocumentTranslateOptions.Filename", exception.Message);
    }

    [Fact]
    public async Task TranslatesStreamWithFilenameOption() {
      ExpectMultipart("v2/document", ("target_lang", null, "de"), ("file", "b.txt", "hello")).RespondJson(HandleReply);
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"done\"}");
      ExpectForm(HttpMethod.Post, "v2/document/d1/result", ("document_key", "k1")).Respond("text/plain", "hallo");
      using var output = new MemoryStream();
      await CreateClient().TranslateDocumentAsync(
            Text("hello"),
            output,
            null,
            "DE",
            new DocumentTranslateOptions { Filename = "b.txt" });
      Assert.Equal("hallo", Encoding.UTF8.GetString(output.ToArray()));
    }

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
#pragma warning disable CS0618
            new DocumentTranslateOptions { GlossaryId = "g1" });
#pragma warning restore CS0618
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
#pragma warning disable CS0618
      var status = await CreateClient().TranslateDocumentStatusAsync(Handle);
#pragma warning restore CS0618
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
      var status = await CreateClient().TranslateDocumentGetStatusAsync(Handle);
      Assert.Equal(DocumentStatus.StatusCode.Error, status.Status);
      Assert.Equal("error", status.StatusValue);
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
    public void DeserializesDocumentStatusStoredByUserCodeWithDefaultOptions() {
      // The shape System.Text.Json wrote for DocumentStatus in earlier versions; the API's converter must not apply.
      var status = System.Text.Json.JsonSerializer.Deserialize<DocumentStatus>(
            "{\"DocumentId\":\"d1\",\"Status\":\"Done\",\"SecondsRemaining\":null,\"BilledCharacters\":7}")!;
      Assert.Equal("d1", status.DocumentId);
      Assert.Equal(DocumentStatus.StatusCode.Done, status.Status);
      Assert.Equal(7, status.BilledCharacters);
    }

    [Fact]
    public async Task ParsesUnknownStatusAsString() {
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"reviewing\"}");
      var status = await CreateClient().TranslateDocumentGetStatusAsync(Handle);
      Assert.Equal(DocumentStatus.StatusCode.Unknown, status.Status);
      Assert.Equal("reviewing", status.StatusValue);
      Assert.True(status.Ok);
      Assert.False(status.Done);
    }

    [Fact]
    public async Task KeepsPollingOnUnknownStatus() {
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"reviewing\"}");
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"done\"}");
      await CreateClient().TranslateDocumentWaitUntilDoneAsync(Handle);
    }

    [Fact]
    public async Task EscapesDocumentIdInStatusAndDownloadPaths() {
      var handle = new DocumentHandle("d/1?x", "k1");
      ExpectForm(HttpMethod.Post, "v2/document/d%2F1%3Fx", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d/1?x\",\"status\":\"done\"}");
      ExpectForm(HttpMethod.Post, "v2/document/d%2F1%3Fx/result", ("document_key", "k1")).Respond("text/plain", "hallo");
      var client = CreateClient();
      await client.TranslateDocumentGetStatusAsync(handle);
      await client.TranslateDocumentDownloadAsync(handle, new MemoryStream());
    }

    [Fact]
    public async Task ThrowsDeepLExceptionWhenWaitTimeoutExpires() {
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"translating\"}");
      var exception = await Assert.ThrowsAsync<DeepLException>(
            () => CreateClient().TranslateDocumentWaitUntilDoneAsync(Handle, TimeSpan.Zero));
      Assert.Contains("timeout", exception.Message);
    }

    [Fact]
    public async Task ThrowsDocumentTranslationExceptionWithHandleWhenTimeoutOptionExpires() {
      ExpectMultipart("v2/document", ("target_lang", null, "de"), ("file", "a.txt", "hello")).RespondJson(HandleReply);
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"queued\"}");
      var exception = await Assert.ThrowsAsync<DocumentTranslationException>(
            () => CreateClient().TranslateDocumentAsync(
                  Text("hello"),
                  "a.txt",
                  new MemoryStream(),
                  null,
                  "DE",
                  new DocumentTranslateOptions { Timeout = TimeSpan.Zero }));
      Assert.Contains("timeout", exception.Message);
      Assert.Equal(Handle.DocumentId, exception.DocumentHandle!.Value.DocumentId);
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
      var options = new DocumentTranslateOptions { Glossary = "g1" };
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
      var options = new DocumentTranslateOptions { TranslationMemory = "tm1", TranslationMemoryThreshold = 101 };
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => CreateClient().TranslateDocumentUploadAsync(Text("hello"), "a.txt", null, "DE", options));
    }
  }
}
