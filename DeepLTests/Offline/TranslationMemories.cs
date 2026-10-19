// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class TranslationMemories : OfflineTest {
    private const string Tmx = "<tmx version=\"1.4\"><body/></tmx>";
    private const string UploadUrl = "https://storage.example.com/upload?sig=abc";
    private const string DownloadUrl = "https://storage.example.com/download?sig=def";

    private const string MemoryReply =
          "{\"translation_memory_id\":\"tm1\",\"name\":\"My TM\",\"source_language\":\"en\"," +
          "\"target_languages\":[\"de\",\"fr\"],\"segment_count\":3,\"creation_time\":\"2025-01-02T03:04:05Z\"," +
          "\"updated_time\":\"2025-01-03T03:04:05Z\"}";

    private static string JobReply(string result, string operation = "import") =>
          "{\"job_id\":\"j1\",\"operation\":\"" + operation + "\",\"product\":\"translation_memory\"," +
          "\"creation_time\":\"2025-01-02T03:04:05Z\",\"updated_time\":\"2025-01-02T03:05:05Z\"," +
          "\"parameters\":{\"translation_memory_id\":\"tm1\",\"display_name\":\"My TM\"}," +
          "\"source_file\":{\"content_type\":\"application/xml\",\"content_length\":42},\"results\":[" + result + "]}";

    private const string CompletedResult =
          "{\"status\":\"completed\",\"translation_memory_id\":\"tm1\",\"skipped_segment_count\":2," +
          "\"download_url\":\"" + DownloadUrl + "\",\"expires_at\":\"2025-02-01T00:00:00Z\"}";

    private static string TmxFile() {
      var path = Path.Combine(Fixtures.TempDir(), "example.tmx");
      File.WriteAllText(path, Tmx);
      return path;
    }

    private static TranslationMemoryJob CompletedJob(string? downloadUrl = DownloadUrl) =>
          new TranslationMemoryJob(
                "j1",
                "export",
                new[] { new TranslationMemoryJobResult(TranslationMemoryJobStatus.Completed, downloadUrl: downloadUrl) });

    [Fact]
    public async Task SendsPagingInQueryAndParsesList() {
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories")
            .WithExactQueryString("page=1&page_size=10")
            .RespondJson("{\"translation_memories\":[" + MemoryReply + "]}");
      var memory = Assert.Single(await CreateClient().ListTranslationMemoriesAsync(1, 10));
      Assert.Equal("tm1", memory.TranslationMemoryId);
      Assert.Equal("en", memory.SourceLanguage);
      Assert.Equal(new[] { "de", "fr" }, memory.TargetLanguages);
      Assert.Equal(3, memory.SegmentCount);
      Assert.Equal(new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc), memory.CreationTime!.Value.ToUniversalTime());
      Assert.Equal(new DateTime(2025, 1, 3, 3, 4, 5, DateTimeKind.Utc), memory.UpdatedTime!.Value.ToUniversalTime());
    }

    [Fact]
    public async Task SendsIdInPathWhenGettingByIdOrInfo() {
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/tm1").RespondJson(MemoryReply);
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/tm1").RespondJson(MemoryReply);
      var client = CreateClient();
      var memory = await client.GetTranslationMemoryAsync("tm1");
      await client.GetTranslationMemoryAsync(memory);
    }

    [Fact]
    public async Task SendsSegmentOptionsInQueryAndParsesSegments() {
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/tm1/segments")
            .WithExactQueryString("page_size=5&page_cursor=c1&filter_text=a b&filter_case_sensitive=true")
            .RespondJson(
                  "{\"segment_count\":9,\"next_page_cursor\":\"c2\",\"segments\":[{\"source_segment_id\":\"seg1\"," +
                  "\"source_text\":\"Hello\",\"creation_time\":\"2025-01-02T03:04:05Z\",\"targets\":[" +
                  "{\"target_segment_id\":\"t1\",\"target_language\":\"de\",\"target_text\":\"Hallo\"}]}]}");
      var page = await CreateClient().ListTranslationMemorySegmentsAsync("tm1", 5, "c1", "a b", true);
      Assert.Equal(9, page.SegmentCount);
      Assert.Equal("c2", page.NextPageCursor);
      var segment = Assert.Single(page.Segments);
      Assert.Equal("seg1", segment.SourceSegmentId);
      Assert.Equal("Hello", segment.SourceText);
      var target = Assert.Single(segment.Targets);
      Assert.Equal("de", target.TargetLanguage);
      Assert.Equal("Hallo", target.TargetText);
    }

    [Fact]
    public async Task SendsNoQueryWhenListingSegmentsWithDefaults() {
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/tm1/segments")
            .WithExactQueryString("")
            .RespondJson("{\"segment_count\":0,\"segments\":[]}");
      var page = await CreateClient().ListTranslationMemorySegmentsAsync("tm1");
      Assert.Null(page.NextPageCursor);
    }

    [Fact]
    public async Task SendsDeleteToTranslationMemoryPath() {
      ExpectNoBody(HttpMethod.Delete, "v3/translation_memories/tm1").Respond(HttpStatusCode.NoContent);
      await CreateClient().DeleteTranslationMemoryAsync("tm1");
    }

    [Fact]
    public async Task SendsJsonWithOptionalFieldsWhenCreatingImport() {
      ExpectJson(
                  HttpMethod.Post,
                  "v3/translation_memories/import",
                  "{\"source_file\":{\"file_name\":\"a.tmx\",\"content_length\":5,\"content_type\":\"application/xml\"}," +
                  "\"parameters\":{\"display_name\":\"My TM\"}}")
            .RespondJson("{\"job_id\":\"j1\",\"upload_url\":\"" + UploadUrl + "\",\"expires_at\":\"2025-01-02T03:04:05Z\"}");
      var created = await CreateClient().CreateTranslationMemoryImportAsync("a.tmx", 5, "application/xml", "My TM");
      Assert.Equal("j1", created.JobId);
      Assert.Equal(UploadUrl, created.UploadUrl);
      Assert.Equal(new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc), created.ExpiresAt!.Value.ToUniversalTime());
    }

    [Fact]
    public async Task OmitsOptionalFieldsWhenCreatingImport() {
      ExpectJson(
                  HttpMethod.Post,
                  "v3/translation_memories/import",
                  "{\"source_file\":{\"file_name\":\"a.tmx\",\"content_length\":5}}")
            .RespondJson("{\"job_id\":\"j1\",\"upload_url\":\"" + UploadUrl + "\"}");
      var created = await CreateClient().CreateTranslationMemoryImportAsync("a.tmx", 5);
      Assert.Null(created.ExpiresAt);
    }

    [Fact]
    public async Task UploadsToStorageWithoutDeepLHeaders() {
      Http.Expect(HttpMethod.Put, UploadUrl)
            .With(request => !request.Headers.Contains("Authorization") && !request.Headers.Contains("X-Custom"))
            .With(request => request.Headers.UserAgent.ToString().StartsWith("deepl-dotnet/"))
            .WithHeaders("Content-Type", "application/xml")
            .WithContent(Tmx)
            .Respond(HttpStatusCode.OK);
      var client = CreateClient(options => options.Headers = new System.Collections.Generic.Dictionary<string, string?> {
        { "X-Custom", "1" }
      });
      await client.UploadTranslationMemoryFileAsync(UploadUrl, new MemoryStream(Encoding.UTF8.GetBytes(Tmx)));
    }

    [Fact]
    public async Task UploadsToUrlOfImportWithGivenContentType() {
      Http.Expect(HttpMethod.Put, UploadUrl)
            .WithHeaders("Content-Type", "text/xml")
            .WithContent(Tmx)
            .Respond(HttpStatusCode.OK);
      await CreateClient().UploadTranslationMemoryFileAsync(
            new TranslationMemoryImport("j1", UploadUrl),
            new MemoryStream(Encoding.UTF8.GetBytes(Tmx)),
            "text/xml");
    }

    [Fact]
    public async Task ThrowsDeepLExceptionWhenUploadIsRejected() {
      Http.Expect(HttpMethod.Put, UploadUrl).Respond(HttpStatusCode.Forbidden, "text/plain", "denied");
      var exception = await Assert.ThrowsAsync<DeepLException>(
            () => CreateClient().UploadTranslationMemoryFileAsync(UploadUrl, new MemoryStream(new byte[] { 1 })));
      Assert.Equal("Error uploading translation memory file, HTTP status: 403, detail: denied", exception.Message);
    }

    [Fact]
    public async Task ParsesJob() {
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/jobs/j1").RespondJson(JobReply(CompletedResult));
      var job = await CreateClient().GetTranslationMemoryJobAsync("j1");
      Assert.Equal("j1", job.JobId);
      Assert.Equal("tm1", job.TranslationMemoryId);
      Assert.Equal("My TM", job.DisplayName);
      Assert.Equal("application/xml", job.SourceContentType);
      Assert.Equal(42, job.SourceContentLength);
      Assert.Equal(TranslationMemoryJobStatus.Completed, job.Status);
      Assert.True(job.Done);
      Assert.True(job.Ok);
      Assert.Equal("tm1", job.Result!.TranslationMemoryId);
      Assert.Equal(2, job.Result.SkippedSegmentCount);
      Assert.Equal(DownloadUrl, job.Result.DownloadUrl);
      Assert.Equal(new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc), job.Result.ExpiresAt!.Value.ToUniversalTime());
    }

    [Theory]
    [InlineData("awaiting_input", TranslationMemoryJobStatus.AwaitingInput, false)]
    [InlineData("processing", TranslationMemoryJobStatus.Processing, false)]
    [InlineData("completed", TranslationMemoryJobStatus.Completed, true)]
    [InlineData("downloaded", TranslationMemoryJobStatus.Downloaded, true)]
    [InlineData("failed", TranslationMemoryJobStatus.Failed, true)]
    [InlineData("expired", TranslationMemoryJobStatus.Expired, true)]
    public async Task ParsesJobStatus(string status, TranslationMemoryJobStatus expected, bool done) {
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/jobs/j1")
            .RespondJson(JobReply("{\"status\":\"" + status + "\"}"));
      var job = await CreateClient().GetTranslationMemoryJobAsync("j1");
      Assert.Equal(expected, job.Status);
      Assert.Equal(done, job.Done);
    }

    [Fact]
    public async Task ParsesRequiredActionOfAwaitingJob() {
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/jobs/j1")
            .RespondJson(
                  JobReply("{\"status\":\"awaiting_input\",\"status_metadata\":{\"required_action\":\"upload_file\"}}"));
      var job = await CreateClient().GetTranslationMemoryJobAsync("j1");
      Assert.Equal("upload_file", job.Result!.RequiredAction);
    }

    [Fact]
    public async Task PollsThroughAwaitingInputUntilJobIsDone() {
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/jobs/j1")
            .RespondJson(JobReply("{\"status\":\"awaiting_input\"}"));
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/jobs/j1").RespondJson(JobReply(CompletedResult));
      var job = await CreateClient().WaitUntilTranslationMemoryJobDoneAsync("j1");
      Assert.Equal(TranslationMemoryJobStatus.Completed, job.Status);
    }

    [Theory]
    [InlineData("failed", "{\"message\":\"bad file\"}", "bad file")]
    [InlineData("failed", null, "Translation memory job Failed")]
    [InlineData("expired", null, "Translation memory job Expired")]
    public async Task ThrowsDeepLExceptionWhenJobEndsWithoutSuccess(string status, string? error, string message) {
      var result = "{\"status\":\"" + status + "\"" + (error == null ? "" : ",\"error\":" + error) + "}";
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/jobs/j1").RespondJson(JobReply(result));
      var exception = await Assert.ThrowsAsync<DeepLException>(
            () => CreateClient().WaitUntilTranslationMemoryJobDoneAsync("j1"));
      Assert.Equal(message, exception.Message);
    }

    [Fact]
    public async Task ThrowsDeepLExceptionWhenJobHasNoResult() {
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/jobs/j1").RespondJson(JobReply(""));
      await Assert.ThrowsAsync<DeepLException>(() => CreateClient().WaitUntilTranslationMemoryJobDoneAsync("j1"));
    }

    [Fact]
    public async Task ReportsReusedExistingExportWhenStatusIs200() {
      ExpectNoBody(HttpMethod.Post, "v3/translation_memories/tm1/export")
            .RespondJson("{\"job_id\":\"j1\",\"parameters\":{\"translation_memory_id\":\"tm1\"}}");
      var export = await CreateClient().CreateTranslationMemoryExportAsync("tm1");
      Assert.Equal("j1", export.JobId);
      Assert.Equal("tm1", export.TranslationMemoryId);
      Assert.True(export.ReusedExisting);
    }

    [Fact]
    public async Task ReportsNewExportWhenStatusIs202() {
      ExpectNoBody(HttpMethod.Post, "v3/translation_memories/tm1/export")
            .RespondJson("{\"job_id\":\"j2\"}", HttpStatusCode.Accepted);
      var export = await CreateClient().CreateTranslationMemoryExportAsync("tm1");
      Assert.Equal("j2", export.JobId);
      Assert.Null(export.TranslationMemoryId);
      Assert.False(export.ReusedExisting);
    }

    [Fact]
    public async Task DownloadsExportFromStorageWithoutDeepLHeaders() {
      Http.Expect(HttpMethod.Get, DownloadUrl)
            .With(request => !request.Headers.Contains("Authorization"))
            .Respond("application/xml", Tmx);
      using var output = new MemoryStream();
      await CreateClient().DownloadTranslationMemoryExportAsync(CompletedJob(), output);
      Assert.Equal(Tmx, Encoding.UTF8.GetString(output.ToArray()));
    }

    [Fact]
    public async Task DownloadsExportToFile() {
      Http.Expect(HttpMethod.Get, DownloadUrl).Respond("application/xml", Tmx);
      var path = Path.Combine(Fixtures.TempDir(), "export.tmx");
      await CreateClient().DownloadTranslationMemoryExportAsync(CompletedJob(), path);
      Assert.Equal(Tmx, File.ReadAllText(path));
    }

    [Fact]
    public async Task KeepsExistingFileWhenDownloadingToIt() {
      var path = Path.Combine(Fixtures.TempDir(), "export.tmx");
      File.WriteAllText(path, "keep");
      await Assert.ThrowsAsync<IOException>(
            () => CreateClient().DownloadTranslationMemoryExportAsync(CompletedJob(), path));
      Assert.Equal("keep", File.ReadAllText(path));
    }

    [Fact]
    public async Task RemovesFileWhenDownloadFails() {
      Http.Expect(HttpMethod.Get, DownloadUrl).Respond(HttpStatusCode.Forbidden, "text/plain", "denied");
      var path = Path.Combine(Fixtures.TempDir(), "export.tmx");
      var exception = await Assert.ThrowsAsync<DeepLException>(
            () => CreateClient().DownloadTranslationMemoryExportAsync(CompletedJob(), path));
      Assert.Equal("Error downloading translation memory export, HTTP status: 403, detail: denied", exception.Message);
      Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenJobHasNoDownloadUrl() =>
          await Assert.ThrowsAsync<ArgumentException>(
                () => CreateClient().DownloadTranslationMemoryExportAsync(CompletedJob(null), new MemoryStream()));

    [Fact]
    public async Task ImportsFromFilepathByCreatingUploadingAndWaiting() {
      var path = TmxFile();
      ExpectJson(
                  HttpMethod.Post,
                  "v3/translation_memories/import",
                  "{\"source_file\":{\"file_name\":\"example.tmx\",\"content_length\":" + Tmx.Length + "}," +
                  "\"parameters\":{\"display_name\":\"My TM\"}}")
            .RespondJson("{\"job_id\":\"j1\",\"upload_url\":\"" + UploadUrl + "\"}");
      Http.Expect(HttpMethod.Put, UploadUrl).WithContent(Tmx).Respond(HttpStatusCode.OK);
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/jobs/j1").RespondJson(JobReply(CompletedResult));
      var job = await CreateClient().ImportTranslationMemoryFromFilepathAsync(path, "My TM");
      Assert.Equal("tm1", job.Result!.TranslationMemoryId);
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenImportFileDoesNotExist() =>
          await Assert.ThrowsAsync<ArgumentException>(
                () => CreateClient().ImportTranslationMemoryFromFilepathAsync(
                      Path.Combine(Fixtures.TempDir(), "missing.tmx")));

    [Fact]
    public async Task ExportsToFilepathByCreatingWaitingAndDownloading() {
      ExpectNoBody(HttpMethod.Post, "v3/translation_memories/tm1/export")
            .RespondJson("{\"job_id\":\"j1\"}", HttpStatusCode.Accepted);
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/jobs/j1")
            .RespondJson(JobReply(CompletedResult, "export"));
      Http.Expect(HttpMethod.Get, DownloadUrl).Respond("application/xml", Tmx);
      var path = Path.Combine(Fixtures.TempDir(), "export.tmx");
      await CreateClient().ExportTranslationMemoryToFilepathAsync("tm1", path);
      Assert.Equal(Tmx, File.ReadAllText(path));
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWithResourceNameWhenStatusIs404() {
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/x").RespondJson("{}", HttpStatusCode.NotFound);
      ExpectNoBody(HttpMethod.Get, "v3/translation_memories/jobs/x").RespondJson("{}", HttpStatusCode.NotFound);
      var client = CreateClient();
      var exception = await Assert.ThrowsAsync<NotFoundException>(() => client.GetTranslationMemoryAsync("x"));
      Assert.Equal("Translation memory not found", exception.Message);
      await Assert.ThrowsAsync<NotFoundException>(() => client.GetTranslationMemoryJobAsync("x"));
    }

    [Fact]
    public async Task ThrowsArgumentExceptionWhenArgumentsAreEmpty() {
      var client = CreateClient();
      await Assert.ThrowsAsync<ArgumentException>(() => client.GetTranslationMemoryAsync(""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.ListTranslationMemorySegmentsAsync(" "));
      await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteTranslationMemoryAsync(""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTranslationMemoryExportAsync(""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.GetTranslationMemoryJobAsync(""));
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTranslationMemoryImportAsync("", 100));
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTranslationMemoryImportAsync("a.tmx", 0));
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.UploadTranslationMemoryFileAsync("", new MemoryStream()));
    }
  }
}
