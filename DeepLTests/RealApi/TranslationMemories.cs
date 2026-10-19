// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;
using Xunit;

namespace DeepLTests.RealApi {
  public sealed class TranslationMemories {
    private const string ExampleTmx =
          "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
          "<tmx version=\"1.4\"><header creationtool=\"deepl-dotnet-tests\" creationtoolversion=\"1\"" +
          " segtype=\"sentence\" o-tmf=\"plain\" adminlang=\"en\" srclang=\"en\" datatype=\"plaintext\"/>" +
          "<body><tu><tuv xml:lang=\"en\"><seg>Segment number 1</seg></tuv>" +
          "<tuv xml:lang=\"de\"><seg>Segment Nummer 1</seg></tuv></tu></body></tmx>\n";

    [Fact]
    public async Task SendsTheTranslationMemoryRequestsImportUploadGetListSegmentsExportAndDelete() {
      var client = RealApiClient.CreateClient();
      var name = RealApiHelpers.GetUniqueName("translation-memory");
      string? translationMemoryId = null;
      try {
        var created = await client.CreateTranslationMemoryImportAsync(
              "example.tmx",
              Encoding.UTF8.GetByteCount(ExampleTmx),
              displayName: name);
        Assert.NotEmpty(created.JobId);
        Assert.NotEmpty(created.UploadUrl);

        await client.UploadTranslationMemoryFileAsync(created, new MemoryStream(Encoding.UTF8.GetBytes(ExampleTmx)));

        // The memory only exists once the import job completes, which takes about a minute
        using var importTimeout = new CancellationTokenSource(RealApiHelpers.JobTimeout);
        var importJob = await client.WaitUntilTranslationMemoryJobDoneAsync(created.JobId, importTimeout.Token);
        translationMemoryId = importJob.Result!.TranslationMemoryId;
        Assert.Equal("import", importJob.Operation);
        Assert.Equal(TranslationMemoryJobStatus.Completed, importJob.Status);
        Assert.NotEmpty(translationMemoryId!);

        var fetched = await client.GetTranslationMemoryAsync(translationMemoryId!);
        Assert.Equal(translationMemoryId, fetched.TranslationMemoryId);
        Assert.Equal(name, fetched.Name);
        Assert.NotEmpty(fetched.SourceLanguage);

        var listed = false;
        for (var page = 0; !listed && page < 20; page++) {
          var memories = await client.ListTranslationMemoriesAsync(page, 100);
          if (memories.Length == 0) {
            break;
          }

          listed = memories.Any(memory => memory.TranslationMemoryId == translationMemoryId);
        }

        Assert.True(listed);

        var segments = await client.ListTranslationMemorySegmentsAsync(translationMemoryId!);
        Assert.NotEmpty(segments.Segments);
        Assert.NotEmpty(segments.Segments[0].SourceSegmentId);

        var exported = await client.CreateTranslationMemoryExportAsync(translationMemoryId!);
        Assert.NotEmpty(exported.JobId);
        using var exportTimeout = new CancellationTokenSource(RealApiHelpers.JobTimeout);
        var exportJob = await client.WaitUntilTranslationMemoryJobDoneAsync(exported.JobId, exportTimeout.Token);
        using var output = new MemoryStream();
        await client.DownloadTranslationMemoryExportAsync(exportJob, output);
        Assert.Contains("<tmx", Encoding.UTF8.GetString(output.ToArray()));
      } finally {
        if (translationMemoryId != null) {
          await client.DeleteTranslationMemoryAsync(translationMemoryId);
        }
      }
    }
  }
}
