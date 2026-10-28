// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using RichardSzalay.MockHttp;
using Xunit;

// Tests the obsolete document minification, which stays supported until the next major.
#pragma warning disable CS0618
namespace DeepLTests.Offline {
  public sealed class DocumentMinificationTest : OfflineTest {
    private readonly string _tempDir = Fixtures.TempDir();

    [Theory]
    [InlineData(".pptx")]
    [InlineData(".docx")]
    public void MinifiesDocumentBelowFiftyKilobytes(string extension) {
      var document = DocumentMinificationFixtures.CreateDocumentWithLargeMedia(extension, _tempDir);
      var minifiedPath = new DocumentMinifier(_tempDir).MinifyDocument(document, false);

      Assert.True(new FileInfo(minifiedPath).Length < new FileInfo(document).Length);
      Assert.InRange(new FileInfo(minifiedPath).Length, 100, 50000);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CleansUpExtractedDirectoryOnlyWhenAskedAfterMinifying(bool cleanup) {
      var document = DocumentMinificationFixtures.CreateDocumentWithLargeMedia(".pptx", _tempDir);
      var minifier = new DocumentMinifier(_tempDir);
      minifier.MinifyDocument(document, cleanup);

      Assert.Equal(cleanup, !Directory.Exists(minifier.GetExtractedDocDirectory()));
    }

    [Fact]
    public void RestoresMediaWhenDeminifying() {
      var input = DocumentMinificationFixtures.CreateDocumentWithLargeMedia(".zip", _tempDir);
      var output = Path.Combine(_tempDir, "example_zip_transformed.zip");
      var minifier = new DocumentMinifier(_tempDir);
      var minified = minifier.MinifyDocument(input, true);
      minifier.DeminifyDocument(minified, output, false);

      var inputEntries = DocumentMinificationFixtures.ReadEntries(input);
      var outputEntries = DocumentMinificationFixtures.ReadEntries(output);
      Assert.Equal(inputEntries.Keys.OrderBy(name => name), outputEntries.Keys.OrderBy(name => name));
      foreach (var entry in inputEntries) {
        Assert.True(entry.Value.SequenceEqual(outputEntries[entry.Key]), entry.Key);
      }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CleansUpTemporaryDirectoryOnlyWhenAskedAfterDeminifying(bool cleanup) {
      var input = DocumentMinificationFixtures.CreateDocumentWithLargeMedia(".zip", _tempDir);
      var output = Path.Combine(_tempDir, "example_zip_transformed.zip");
      var minifier = new DocumentMinifier();
      var minified = minifier.MinifyDocument(input, true);
      minifier.DeminifyDocument(minified, output, cleanup);

      Assert.Equal(cleanup, !Directory.Exists(minifier.GetExtractedDocDirectory()));
    }

    [Fact]
    public async Task UploadsMinifiedFileAndRestoresMediaInOutputWhenTranslatingFiles() {
      var input = DocumentMinificationFixtures.CreateDocumentWithLargeMedia(".docx", _tempDir);
      var output = Path.Combine(_tempDir, "output.docx");
      byte[]? uploaded = null;
      Expect(HttpMethod.Post, "v2/document")
            .With(
                  request => {
                    var file = ((MultipartFormDataContent)request.Content!).Single(
                          part => part.Headers.ContentDisposition!.Name == "file");
                    uploaded = file.ReadAsByteArrayAsync().Result;
                    return true;
                  })
            .RespondJson("{\"document_id\":\"d1\",\"document_key\":\"k1\"}");
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"done\"}");
      ExpectForm(HttpMethod.Post, "v2/document/d1/result", ("document_key", "k1"))
            .Respond(() => System.Threading.Tasks.Task.FromResult(
                  new HttpResponseMessage { Content = new ByteArrayContent(uploaded!) }));

      await CreateClient().TranslateDocumentAsync(
            new FileInfo(input),
            new FileInfo(output),
            "EN",
            "DE",
            new DocumentTranslateOptions { EnableDocumentMinification = true });

      Assert.InRange(uploaded!.Length, 100, 50000);
      var inputEntries = DocumentMinificationFixtures.ReadEntries(input);
      var outputEntries = DocumentMinificationFixtures.ReadEntries(output);
      Assert.True(inputEntries["placeholder_image.png"].SequenceEqual(outputEntries["placeholder_image.png"]));
    }

    [Fact]
    public async Task UploadsOriginalBytesWhenTranslatingStreamsWithMinificationEnabled() {
      var input = DocumentMinificationFixtures.CreateDocumentWithLargeMedia(".docx", _tempDir);
      var bytes = File.ReadAllBytes(input);
      byte[]? uploaded = null;
      Expect(HttpMethod.Post, "v2/document")
            .With(
                  request => {
                    var file = ((MultipartFormDataContent)request.Content!).Single(
                          part => part.Headers.ContentDisposition!.Name == "file");
                    uploaded = file.ReadAsByteArrayAsync().Result;
                    return true;
                  })
            .RespondJson("{\"document_id\":\"d1\",\"document_key\":\"k1\"}");
      ExpectForm(HttpMethod.Post, "v2/document/d1", ("document_key", "k1"))
            .RespondJson("{\"document_id\":\"d1\",\"status\":\"done\"}");
      ExpectForm(HttpMethod.Post, "v2/document/d1/result", ("document_key", "k1")).Respond("text/plain", "x");

      using var output = new MemoryStream();
      await CreateClient().TranslateDocumentAsync(
            new MemoryStream(bytes),
            "a.docx",
            output,
            "EN",
            "DE",
            new DocumentTranslateOptions { EnableDocumentMinification = true });

      Assert.True(bytes.SequenceEqual(uploaded!));
    }
  }
}
#pragma warning restore CS0618
