// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using Xunit;

namespace DeepLTests {
  public sealed class TranslationMemoryTest : BaseDeepLTest {
    private const string DefaultTranslationMemoryId = "a74d88fb-ed2a-4943-a664-a4512398b994";
    private const string UnknownId = "00000000-0000-0000-0000-000000000000";

    private const string ExampleTmx = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                      "<tmx version=\"1.4\"><body>" +
                                      "<tu><tuv xml:lang=\"de\"><seg>Hallo</seg></tuv>" +
                                      "<tuv xml:lang=\"en\"><seg>Hello</seg></tuv></tu>" +
                                      "</body></tmx>\n";

    /// <summary>Writes an example TMX file into a fresh temporary directory and returns its path.</summary>
    private static string CreateTmxFile() {
      var tmxFilePath = Path.Combine(TempDir(), "example.tmx");
      File.WriteAllText(tmxFilePath, ExampleTmx);
      return tmxFilePath;
    }

    /// <summary>Imports the example TMX file and returns the ID of the resulting translation memory.</summary>
    private static async Task<string> ImportExampleTranslationMemory(
          DeepLClient client,
          string? displayName = null) {
      var job = await client.ImportTranslationMemoryFromFilepathAsync(CreateTmxFile(), displayName);
      var translationMemoryId = job.Result!.TranslationMemoryId;
      Assert.NotNull(translationMemoryId);
      return translationMemoryId!;
    }

    [MockServerOnlyFact]
    public async Task TestListTranslationMemories() {
      var client = CreateTestClient();
      var translationMemories = await client.ListTranslationMemoriesAsync(0, 10);
      Assert.NotNull(translationMemories);
      Assert.True(translationMemories.Length > 0);
      // Look the default translation memory up by ID rather than by position: imports made by
      // other tests share this auth key, and the API does not guarantee an ordering.
      var defaultTranslationMemory = Assert.Single(
            translationMemories.Where(tm => tm.TranslationMemoryId == DefaultTranslationMemoryId));
      Assert.Equal("Default Translation Memory", defaultTranslationMemory.Name);
      Assert.NotNull(defaultTranslationMemory.SourceLanguage);
      Assert.NotNull(defaultTranslationMemory.TargetLanguages);
      Assert.True(defaultTranslationMemory.SegmentCount >= 0);
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTextWithTranslationMemoryId() {
      // Note: this test may use the mock server that will not translate the text,
      // therefore we do not check the translated result.
      var client = CreateTestClient();
      const string exampleText = "Hallo, Welt!";

      var result = await client.TranslateTextAsync(
            exampleText,
            "de",
            "en-US",
            new TextTranslateOptions { TranslationMemoryId = DefaultTranslationMemoryId });

      Assert.NotNull(result);
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTextWithTranslationMemoryIdAndThreshold() {
      // Note: this test may use the mock server that will not translate the text,
      // therefore we do not check the translated result.
      var client = CreateTestClient();
      const string exampleText = "Hallo, Welt!";

      var result = await client.TranslateTextAsync(
            exampleText,
            "de",
            "en-US",
            new TextTranslateOptions {
              TranslationMemoryId = DefaultTranslationMemoryId,
              TranslationMemoryThreshold = 80
            });

      Assert.NotNull(result);
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTextWithThresholdWithoutIdThrows() {
      var client = CreateTestClient();
      await Assert.ThrowsAsync<ArgumentException>(() =>
            client.TranslateTextAsync("Hello", "en", "de",
                  new TextTranslateOptions { TranslationMemoryThreshold = 80 }));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateTextWithInvalidThresholdThrows() {
      var client = CreateTestClient();
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.TranslateTextAsync("Hello", "en", "de",
                  new TextTranslateOptions {
                    TranslationMemoryId = DefaultTranslationMemoryId,
                    TranslationMemoryThreshold = 101
                  }));
    }

    private static (FileInfo Input, FileInfo Output) CreateDocumentPaths() {
      var tempDir = TempDir();
      var inputFilePath = Path.Combine(tempDir, "example_document.txt");
      File.Delete(inputFilePath);
      File.WriteAllText(inputFilePath, "Hallo, Welt!");
      var outputFilePath = Path.Combine(tempDir, "output_document.txt");
      File.Delete(outputFilePath);
      return (new FileInfo(inputFilePath), new FileInfo(outputFilePath));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithTranslationMemoryId() {
      // Note: this test may use the mock server that will not translate the text,
      // therefore we do not check the translated result.
      var translator = CreateTestTranslator();
      var (input, output) = CreateDocumentPaths();

      await translator.TranslateDocumentAsync(
            input,
            output,
            "de",
            "en-US",
            new DocumentTranslateOptions { TranslationMemoryId = DefaultTranslationMemoryId });

      Assert.True(File.Exists(output.FullName));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithTranslationMemoryIdAndThreshold() {
      // Note: this test may use the mock server that will not translate the text,
      // therefore we do not check the translated result.
      var translator = CreateTestTranslator();
      var (input, output) = CreateDocumentPaths();

      await translator.TranslateDocumentAsync(
            input,
            output,
            "de",
            "en-US",
            new DocumentTranslateOptions {
              TranslationMemoryId = DefaultTranslationMemoryId,
              TranslationMemoryThreshold = 80
            });

      Assert.True(File.Exists(output.FullName));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithTranslationMemoryInfo() {
      var client = CreateTestClient();
      var translationMemories = await client.ListTranslationMemoriesAsync(0, 10);
      var translationMemory = translationMemories[0];
      var (input, output) = CreateDocumentPaths();

      await client.TranslateDocumentAsync(
            input,
            output,
            "de",
            "en-US",
            new DocumentTranslateOptions(translationMemory));

      Assert.True(File.Exists(output.FullName));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithThresholdWithoutIdThrows() {
      var translator = CreateTestTranslator();
      var (input, _) = CreateDocumentPaths();
      // Option validation is performed at upload time and throws the raw
      // ArgumentException; the all-in-one TranslateDocumentAsync would wrap it
      // in a DocumentTranslationException.
      await Assert.ThrowsAsync<ArgumentException>(() =>
            translator.TranslateDocumentUploadAsync(input, "de", "en-US",
                  new DocumentTranslateOptions { TranslationMemoryThreshold = 80 }));
    }

    [MockServerOnlyFact]
    public async Task TestTranslateDocumentWithInvalidThresholdThrows() {
      var translator = CreateTestTranslator();
      var (input, _) = CreateDocumentPaths();
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            translator.TranslateDocumentUploadAsync(input, "de", "en-US",
                  new DocumentTranslateOptions {
                    TranslationMemoryId = DefaultTranslationMemoryId,
                    TranslationMemoryThreshold = 101
                  }));
    }

    [MockServerOnlyFact]
    public async Task TestGetTranslationMemory() {
      var client = CreateTestClient();
      var translationMemory = await client.GetTranslationMemoryAsync(DefaultTranslationMemoryId);

      Assert.Equal(DefaultTranslationMemoryId, translationMemory.TranslationMemoryId);
      Assert.NotEmpty(translationMemory.Name);
      Assert.NotEmpty(translationMemory.SourceLanguage);
      Assert.NotEmpty(translationMemory.TargetLanguages);
      Assert.NotNull(translationMemory.CreationTime);
      Assert.NotNull(translationMemory.UpdatedTime);
    }

    [MockServerOnlyFact]
    public async Task TestGetTranslationMemoryWithTranslationMemoryInfo() {
      var client = CreateTestClient();
      var listed = (await client.ListTranslationMemoriesAsync(0, 10))[0];

      var translationMemory = await client.GetTranslationMemoryAsync(listed);

      Assert.Equal(listed.TranslationMemoryId, translationMemory.TranslationMemoryId);
    }

    [MockServerOnlyFact]
    public async Task TestGetTranslationMemoryWithUnknownIdThrows() {
      var client = CreateTestClient();
      await Assert.ThrowsAsync<NotFoundException>(() => client.GetTranslationMemoryAsync(UnknownId));
    }

    [MockServerOnlyFact]
    public async Task TestListTranslationMemorySegments() {
      var client = CreateTestClient();
      var page = await client.ListTranslationMemorySegmentsAsync(DefaultTranslationMemoryId);

      Assert.NotEmpty(page.Segments);
      Assert.True(page.SegmentCount > 0);
      var segment = page.Segments[0];
      Assert.NotEmpty(segment.SourceSegmentId);
      Assert.NotEmpty(segment.SourceText);
      Assert.NotEmpty(segment.Targets);
      Assert.NotEmpty(segment.Targets[0].TargetLanguage);
      Assert.NotEmpty(segment.Targets[0].TargetText);
    }

    [MockServerOnlyFact]
    public async Task TestListTranslationMemorySegmentsPagination() {
      var client = CreateTestClient();
      var first = await client.ListTranslationMemorySegmentsAsync(DefaultTranslationMemoryId, 5);

      Assert.Equal(5, first.Segments.Length);
      Assert.NotNull(first.NextPageCursor);

      var second = await client.ListTranslationMemorySegmentsAsync(
            DefaultTranslationMemoryId,
            5,
            first.NextPageCursor);

      Assert.NotEmpty(second.Segments);
      var firstIds = first.Segments.Select(segment => segment.SourceSegmentId).ToArray();
      var secondIds = second.Segments.Select(segment => segment.SourceSegmentId).ToArray();
      Assert.Empty(firstIds.Intersect(secondIds));
    }

    [MockServerOnlyFact]
    public async Task TestListTranslationMemorySegmentsFilter() {
      var client = CreateTestClient();
      var unfiltered = await client.ListTranslationMemorySegmentsAsync(DefaultTranslationMemoryId);
      var filtered = await client.ListTranslationMemorySegmentsAsync(
            DefaultTranslationMemoryId,
            filterText: "Nummer 7");

      Assert.True(filtered.Segments.Length < unfiltered.Segments.Length);
      // SegmentCount is translation-memory-level metadata and unaffected by the filter.
      Assert.Equal(unfiltered.SegmentCount, filtered.SegmentCount);
    }

    [MockServerOnlyFact]
    public async Task TestImportTranslationMemoryFromFilepath() {
      var client = CreateTestClient();
      var job = await client.ImportTranslationMemoryFromFilepathAsync(CreateTmxFile(), "Imported TM");

      Assert.Equal("import", job.Operation);
      Assert.Equal("translation_memory", job.Product);
      Assert.Equal(TranslationMemoryJobStatus.Completed, job.Status);
      var translationMemoryId = job.Result!.TranslationMemoryId;
      Assert.NotNull(translationMemoryId);

      var imported = await client.GetTranslationMemoryAsync(translationMemoryId!);
      Assert.Equal("Imported TM", imported.Name);

      await client.DeleteTranslationMemoryAsync(imported);
    }

    [MockServerOnlyFact]
    public async Task TestCreateTranslationMemoryImportAwaitsUpload() {
      var client = CreateTestClient();
      var created = await client.CreateTranslationMemoryImportAsync(
            "example.tmx",
            1024,
            displayName: "Awaiting Upload TM");

      Assert.NotEmpty(created.JobId);
      Assert.NotEmpty(created.UploadUrl);

      var job = await client.GetTranslationMemoryJobAsync(created.JobId);
      Assert.Equal(TranslationMemoryJobStatus.AwaitingInput, job.Status);
      Assert.NotNull(job.Result!.RequiredAction);
      Assert.False(job.Done);
    }

    /// <summary>
    ///   An uploaded import keeps reporting "awaiting_input" for a while, because the DeepL API detects the upload
    ///   asynchronously. The wait loop must poll through that status instead of throwing.
    /// </summary>
    [MockServerOnlyFact]
    public async Task TestWaitUntilTranslationMemoryJobDonePollsThroughAwaitingInput() {
      // One poll of "awaiting_input" after the upload: the first poll of the wait loop reports it, the second
      // reports the completed job. Keep this at 1 so the test only waits a single poll interval.
      var client = CreateTestClientWithMockSession(
            nameof(TestWaitUntilTranslationMemoryJobDonePollsThroughAwaitingInput),
            new SessionOptions { TranslationMemoryJobProcessingPolls = 1 });
      var tmxFileInfo = new FileInfo(CreateTmxFile());
      var created = await client.CreateTranslationMemoryImportAsync(
            tmxFileInfo.Name,
            tmxFileInfo.Length,
            displayName: "Awaiting Input Poll TM");

      // Polling before the upload does not consume one of the configured polls.
      var awaitingUpload = await client.GetTranslationMemoryJobAsync(created.JobId);
      Assert.Equal(TranslationMemoryJobStatus.AwaitingInput, awaitingUpload.Status);
      Assert.False(awaitingUpload.Done);

      using (var fileStream = tmxFileInfo.OpenRead()) {
        await client.UploadTranslationMemoryFileAsync(created, fileStream);
      }

      var job = await client.WaitUntilTranslationMemoryJobDoneAsync(created.JobId);

      Assert.Equal(TranslationMemoryJobStatus.Completed, job.Status);
      Assert.True(job.Done);
      Assert.True(job.Ok);
      var translationMemoryId = job.Result!.TranslationMemoryId;
      Assert.NotNull(translationMemoryId);

      await client.DeleteTranslationMemoryAsync(translationMemoryId!);
    }

    [MockServerOnlyFact]
    public async Task TestCreateTranslationMemoryImportWithInvalidArgumentsThrows() {
      var client = CreateTestClient();
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTranslationMemoryImportAsync("", 100));
      await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTranslationMemoryImportAsync("example.tmx", 0));
    }

    [MockServerOnlyFact]
    public async Task TestExportTranslationMemoryToFilepath() {
      var client = CreateTestClient();
      var translationMemoryId = await ImportExampleTranslationMemory(client);
      var outputFilePath = Path.Combine(TempDir(), "exported.tmx");

      var job = await client.ExportTranslationMemoryToFilepathAsync(translationMemoryId, outputFilePath);

      Assert.Equal("export", job.Operation);
      Assert.Equal(TranslationMemoryJobStatus.Completed, job.Status);
      Assert.Contains("<tmx", File.ReadAllText(outputFilePath));

      await client.DeleteTranslationMemoryAsync(translationMemoryId);
    }

    [MockServerOnlyFact]
    public async Task TestCreateTranslationMemoryExportReusesCompletedJob() {
      var client = CreateTestClient();
      var translationMemoryId = await ImportExampleTranslationMemory(client);

      var created = await client.CreateTranslationMemoryExportAsync(translationMemoryId);
      Assert.False(created.ReusedExisting);
      Assert.Equal(translationMemoryId, created.TranslationMemoryId);
      await client.WaitUntilTranslationMemoryJobDoneAsync(created.JobId);

      var reused = await client.CreateTranslationMemoryExportAsync(translationMemoryId);
      Assert.True(reused.ReusedExisting);
      Assert.Equal(created.JobId, reused.JobId);

      await client.DeleteTranslationMemoryAsync(translationMemoryId);
    }

    [MockServerOnlyFact]
    public async Task TestGetTranslationMemoryJobWithUnknownIdThrows() {
      var client = CreateTestClient();
      await Assert.ThrowsAsync<NotFoundException>(() => client.GetTranslationMemoryJobAsync(UnknownId));
    }

    /// <summary>
    ///   The upload URL is a pre-signed storage URL outside the DeepL API, so the DeepL authentication key must not
    ///   be sent to it.
    /// </summary>
    [Fact]
    public async Task TestUploadTranslationMemoryFileOmitsAuthorizationHeader() {
      var mockHandler = getMockHandler("");
      var client = new DeepLClient(
            AuthKey,
            new DeepLClientOptions {
              ClientFactory = () =>
                    new HttpClientAndDisposeFlag { HttpClient = new HttpClient(mockHandler), DisposeClient = true }
            });
      using var fileContent = new MemoryStream(Encoding.UTF8.GetBytes(ExampleTmx));

      await client.UploadTranslationMemoryFileAsync("https://storage.example.com/upload", fileContent);

      Assert.Single(mockHandler.requests);
      var request = mockHandler.requests[0];
      Assert.False(request.Headers.Contains("Authorization"));
      Assert.Contains("deepl-dotnet/", request.Headers.UserAgent.ToString());
      Assert.Equal("application/xml", request.Content!.Headers.ContentType!.MediaType);
    }

    [MockServerOnlyFact]
    public async Task TestDeleteTranslationMemory() {
      var client = CreateTestClient();
      var translationMemoryId = await ImportExampleTranslationMemory(client);

      await client.DeleteTranslationMemoryAsync(translationMemoryId);

      await Assert.ThrowsAsync<NotFoundException>(() => client.GetTranslationMemoryAsync(translationMemoryId));
    }
  }
}
