// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace DeepL {
  [Obsolete("Document minification is deprecated and will be removed in the next major version. The DeepL API accepts larger documents; see the usage limits.")]
  public interface IDocumentMinifier {
    /// <summary>
    ///   Minifies a given document using the given <c>tempDir</c>, by extracting it as a ZIP file and
    ///   replacing all supported media files with a small placeholder.
    ///   Created file will be inside the <c>tempDir</c>, the filename can be retrieved by calling
    ///   <see cref="DocumentMinifier.GetMinifiedDocFile(string)" /> with <c>tempDir</c> as a parameter
    ///   Note that this method will minify the file without any checks, you should first call
    ///   <see cref="DocumentMinifier.CanMinifyFile(string)" /> on the input file.
    ///   If <c>cleanup</c> is set to <c>true</c> , the extracted document will be deleted afterwards, and only
    ///   the original media and the minified file will remain in the <c>tempDir</c>.
    /// </summary>
    /// <param name="inputFilePath"> Path to the file to be minified.</param>
    /// <param name="cleanup">
    ///   If <c>true</c>, will delete the extracted document files from the temporary directory.
    ///   Otherwise, the files will remain (useful for debugging).
    /// </param>
    /// <returns>
    ///   The path of the minified document. Can also be retrieved by calling
    ///   <see cref="DocumentMinifier.GetMinifiedDocFile(string)" />
    /// </returns>
    /// <exception cref="DocumentMinificationException">
    ///   If an exception occurred during the minification process
    /// </exception>
    public string MinifyDocument(string inputFilePath, bool cleanup = false);

    /// <summary>
    ///   Deminifies a given file at <c>inputFilePath</c> by reinserting its original media in <c>tempDir</c> and stores
    ///   the resulting document in <c>outputFilePath</c>. If <c>cleanup</c> is set to <c>true</c>, it will delete the
    ///   <c>tempDir</c> afterwards, otherwise nothing will happen after the deminification.
    /// </summary>
    /// <param name="inputFilePath"> Path to document to be deminified with its media.</param>
    /// <param name="outputFilePath"> Where the final (deminified) document will be stored.</param>
    /// <param name="cleanup"> Determines if the <c>tempDir</c> is deleted at the end of this method.</param>
    /// <exception cref="DocumentDeminificationException">
    ///   If an exception occurred during the deminification process
    /// </exception>
    public void DeminifyDocument(string inputFilePath, string outputFilePath, bool cleanup = false);
  }

  /// <summary>
  ///   Class that implements document minification: Stripping supported files like pptx and docx
  ///   of their media (images, videos, etc) before uploading them to the DeepL API to be translated.
  ///   This allows users to translate files that would usually hit the size limit for files.
  ///   Obsolete: will be removed in the next major version, as the DeepL API now accepts larger documents.
  ///   Please note the following:
  ///   <list type="number">
  ///     <item>
  ///       <description>
  ///         To use this class, you first need to check by calling <see cref="DocumentMinifier.CanMinifyFile(string)" />
  ///         if the file type is supported. This class performs no further checks.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         The <c>DocumentMinifier</c> is stateful, so you cannot use it to minify multiple documents at once.
  ///         You need to create a new <c>DocumentMinifier</c> object per document.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         Be very careful when providing a custom <c>tempDir</c> when instantiating the class. For example,
  ///         <see cref="DocumentMinifier.DeminifyDocument" /> will delete the entire <c>tempDir</c> with
  ///         <c>cleanup</c> set to <c>true</c> (disabled by default). In order not to lose any data, ideally always
  ///         call <c>new DocumentMinifier()</c> in order to get a fresh temporary directory.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         If an error occurs during minification, either a <see cref="DocumentMinificationException" /> or a
  ///         <see cref="DocumentDeminificationException" /> will be thrown, depending on which phase the error
  ///         occured in.
  ///       </description>
  ///     </item>
  ///   </list>
  ///   The document minification process works in 2 phases:
  ///   <list type="number">
  ///     <item>
  ///       <description>
  ///         Minification: The document is extracted into a temporary directory, the media files are backed up,
  ///         the media in the document is replaced with placeholders and a minified document is created.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         Deminification: The minified document is extracted into a temporary directory, the media backups are
  ///         reinserted into the extracted document, and the document is deminified into the output path.
  ///       </description>
  ///     </item>
  ///   </list>
  ///   If <c>cleanup</c> is enabled, the minification phase will delete the folder with the extracted document
  ///   and the deminification phase will delete the entire temporary directory.
  ///   Note that by default, the input file will be kept on disk, and as such no further backups of media etc.
  ///   are made (as they are all available from the input file).
  ///   Example usage:
  ///   <code>
  ///     var inputFile = "/home/exampleUser/document.pptx";
  ///     var outputFile = "/home/exampleUser/document_ES.pptx";
  ///     var minifier = new DocumentMinifier();
  ///     if (minifier.CanMinifyFile(inputFile)) {
  ///       try {
  ///         minifier.MinifyDocument(inputFile, true);
  ///         minifiedFile = minifier.GetMinifiedDocFile(inputFile);
  ///         // process file minifiedFile, e.g. translate it with DeepL
  ///         minifier.DeminifyDocument(inputFile, outputFile, true);
  ///         // process file outputFile
  ///         } catch (DocumentMinificationException e) {
  ///         // handle exception during minification, e.g. print list of media, clean up temporary directory, etc
  ///         } catch (DocumentDeminificationException e) {
  ///         // handle exception during deminification, e.g. save minified document, clean up temporary directory, etc
  ///         } catch (DocumentTranslationException e) {
  ///         // handle general DocTrans exception (mostly useful if document is translated between minification
  ///         // and deminification)
  ///         }
  ///       }
  ///   </code>
  /// </summary>
  [Obsolete("Document minification is deprecated and will be removed in the next major version. The DeepL API accepts larger documents; see the usage limits.")]
  public class DocumentMinifier : IDocumentMinifier {
    /// <summary>Which input document types are supported for minification.</summary>
    private static readonly string[] SupportedDocumentTypes = { ".pptx", ".docx" };

    /// <summary>Which media formats in the documents are supported for minification.</summary>
    private static readonly string[] SupportedMediaFormats = {
          // Image formats
          ".png", ".jpg", ".jpeg", ".emf", ".bmp", ".tiff", ".wdp", ".svg", ".gif",
          // Video formats
          // Taken from https://support.microsoft.com/en-gb/office/video-and-audio-file-formats-supported-in-powerpoint-d8b12450-26db-4c7b-a5c1-593d3418fb59
          ".mp4", ".asf", ".avi", ".m4v", ".mpg", ".mpeg", ".wmv", ".mov",
          // Audio formats, taken from the same URL as video
          ".aiff", ".au", ".mid", ".midi", ".mp3", ".m4a", ".wav", ".wma"
    };

    private const string ExtractedDocDirName = "extracted_doc";
    private const string OriginalMediaDirName = "original_media";
    private const string MinifiedDocFileBaseName = "minifiedDoc";
    private const int MinifiedDocSizeLimitWarning = 5000000;

    private readonly string _tempDir;

    /// <summary>
    ///   Initializes a new <see cref="DocumentMinifier" /> object either with a specified or newly created
    ///   temporary directory.
    /// </summary>
    /// <param name="tempDir"> The temporary directory used for media extraction during minification </param>
    public DocumentMinifier(string? tempDir = null) {
      _tempDir = tempDir ?? CreateTemporaryDirectory();
    }

    /// <summary> Checks if a given file can be minified or not </summary>
    /// <param name="inputFilePath"> The path to the file </param>
    /// <returns> <c>true</c> if the file can be minified otherwise <c>false</c> </returns>
    /// <exception cref="ArgumentException">
    ///   if the <c>inputFilePath</c> contains characters not allowed in a path name
    /// </exception>
    public static bool CanMinifyFile(string inputFilePath) {
      return !string.IsNullOrWhiteSpace(inputFilePath) &&
             SupportedDocumentTypes.Contains(Path.GetExtension(inputFilePath).ToLowerInvariant());
    }

    /// <summary> Gets the path for where the minified version of the input file will live </summary>
    /// <param name="inputFilePath"> The path to the file </param>
    /// <returns> The path to the minified version of the file </returns>
    /// <exception cref="ArgumentNullException"> if the <c>inputFilePath</c> is null </exception>
    /// <exception cref="ArgumentException">
    ///   if the <c>inputFilePath</c> contains characters not allowed in a path name
    /// </exception>
    public string GetMinifiedDocFile(string inputFilePath) {
      var minifiedDocFileName = Path.ChangeExtension(MinifiedDocFileBaseName, Path.GetExtension(inputFilePath));
      return Path.Combine(_tempDir, minifiedDocFileName);
    }

    /// <summary> Gets the path to the directory where the input file will be extracted to </summary>
    /// <returns> The path to the directory where the input file will be extracted to </returns>
    public string GetExtractedDocDirectory() {
      return Path.Combine(_tempDir, ExtractedDocDirName);
    }

    /// <summary> Gets the path to the directory where the original media was extracted to </summary>
    /// <returns> The path to the media directory containing the original media </returns>
    public string GetOriginalMediaDirectory() {
      return Path.Combine(_tempDir, OriginalMediaDirName);
    }

    /// <inheritdoc />
    public string MinifyDocument(string inputFilePath, bool cleanup = false) {
      var extractedDocDirectory = GetExtractedDocDirectory();
      var mediaDir = GetOriginalMediaDirectory();
      var minifiedDocFilePath = GetMinifiedDocFile(inputFilePath);

      try {
        ExtractZipTo(inputFilePath, extractedDocDirectory);
      } catch (Exception ex) {
        throw new DocumentMinificationException(
              $"Exception when extracting document: Failed to extract {inputFilePath} to {extractedDocDirectory}",
              ex);
      }

      ExportMediaToMediaDirAndReplace(extractedDocDirectory, mediaDir);

      try {
        ZipFile.CreateFromDirectory(extractedDocDirectory, minifiedDocFilePath);
      } catch (Exception ex) {
        throw new DocumentMinificationException($"Failed creating a zip file at {minifiedDocFilePath}", ex);
      }

      if (cleanup) {
        try {
          Directory.Delete(extractedDocDirectory, true);
        } catch (Exception ex) {
          throw new DocumentMinificationException($"Failed to delete directory {extractedDocDirectory}", ex);
        }
      }

      var fileSizeResponse = new FileInfo(minifiedDocFilePath).Length;
      if (fileSizeResponse > MinifiedDocSizeLimitWarning) {
        Console.Error.WriteLine(
              "The input file could not be minified below 5 MB, likely a media type is missing. "
            + "This might cause the translation to fail.");
      }

      return minifiedDocFilePath;
    }

    /// <inheritdoc />
    public void DeminifyDocument(string inputFilePath, string outputFilePath, bool cleanup = false) {
      var extractedDocDirectory = GetExtractedDocDirectory();
      var mediaDir = GetOriginalMediaDirectory();
      if (!Directory.Exists(extractedDocDirectory)) {
        try {
          Directory.CreateDirectory(extractedDocDirectory);
        } catch (Exception ex) {
          throw new DocumentDeminificationException(
                $"Exception when deminifying, could not create directory at {extractedDocDirectory}.",
                ex);
        }
      }

      try {
        ExtractZipTo(inputFilePath, extractedDocDirectory);
      } catch (Exception ex) {
        throw new DocumentDeminificationException(
              $"Exception when extracting document: Failed to extract {inputFilePath} to {extractedDocDirectory}",
              ex);
      }

      ReplaceMediaInDir(extractedDocDirectory, mediaDir);
      try {
        if (File.Exists(outputFilePath)) {
          File.Delete(outputFilePath);
        }

        ZipFile.CreateFromDirectory(extractedDocDirectory, outputFilePath);
      } catch (Exception ex) {
        throw new DocumentDeminificationException($"Failed creating a zip file at {outputFilePath}", ex);
      }

      if (cleanup) {
        try {
          Directory.Delete(_tempDir, true);
        } catch (Exception ex) {
          throw new DocumentMinificationException($"Failed to delete directory {extractedDocDirectory}", ex);
        }
      }
    }

    /// <summary>
    ///   Creates a temporary directory for use in the <see cref="DocumentMinifier" />
    ///   Uses the system's temporary directory.
    /// </summary>
    /// <returns> The path of the created temporary directory </returns>
    /// <exception cref="DocumentMinificationException"> if the temporary directory could not be created </exception>
    private static string CreateTemporaryDirectory() {
      var tempDir = Path.GetTempPath() + "/document_minification_" + Guid.NewGuid().ToString("N");
      while (Directory.Exists(tempDir)) {
        Thread.Sleep(1);
        tempDir = Path.GetTempPath() + "/document_minification_" + Guid.NewGuid().ToString("N");
      }

      try {
        Directory.CreateDirectory(tempDir);
      } catch (Exception ex) {
        throw new DocumentMinificationException($"Failed creating temporary directory at {tempDir}", ex);
      }

      return tempDir;
    }

    /// <summary> Extracts a zip file to a given directory </summary>
    /// <param name="zippedDocumentPath"> The path to the zip file </param>
    /// <param name="extractionDir">
    ///   The path to the directory where the contents of the zip file will be extracted to
    /// </param>
    private void ExtractZipTo(string zippedDocumentPath, string extractionDir) {
      if (!Directory.Exists(extractionDir)) {
        Directory.CreateDirectory(extractionDir);
      }

      ZipFile.ExtractToDirectory(zippedDocumentPath, extractionDir);
    }

    /// <summary>
    ///   Iterates through the <c>inputDirectory</c> and if it contains a supported media file, will export that media
    ///   to the <c>mediaDirectory</c> and replace the media in the <c>inputDirectory</c> with a placeholder. The
    ///   relative path will be preserved when moving the file to the <c>mediaDirectory</c> (e.g. a file located at
    ///   "/inputDirectory/foo/bar.png" will be exported to "/mediaDirectory/foo/bar.png")
    /// </summary>
    /// <param name="inputDirectory"> The path to the input directory </param>
    /// <param name="mediaDirectory">
    ///   The path to the directory where the supported media from <c>inputDirectory</c> will be exported to
    /// </param>
    /// <exception cref="DocumentMinificationException">
    ///   If a problem occurred when exporting the original media from <c>inputDirectory</c> to <c>mediaDirectory</c>
    /// </exception>
    private void ExportMediaToMediaDirAndReplace(string inputDirectory, string mediaDirectory) {
      foreach (var filePath in Directory.GetFiles(inputDirectory, "*.*", SearchOption.AllDirectories)) {
        if (SupportedMediaFormats.Contains(Path.GetExtension(filePath).ToLowerInvariant())) {
          var relativeFilePath = filePath.Substring(inputDirectory.Length + 1);
          var mediaPath = Path.Combine(mediaDirectory, relativeFilePath);

          // mediaDir should never be null as mediaPath contains the specified mediaDirectory
          var mediaDir = Path.GetDirectoryName(mediaPath);

          try {
            if (!string.IsNullOrWhiteSpace(mediaDir) && !Directory.Exists(mediaDir)) {
              Directory.CreateDirectory(mediaDir);
            }

            File.Move(filePath, mediaPath);
            File.WriteAllText(filePath, "DeepL Media Placeholder");
          } catch (Exception ex) {
            throw new DocumentMinificationException($"Exception when exporting and replacing media files", ex);
          }
        }
      }
    }

    /// <summary>
    ///   Iterates through <c>mediaDirectory</c> and moves all files into the <c>inputDirectory</c> while preserving
    ///   the relative paths. (e.g. /mediaDirectory/foo/bar.png will be moved to the path /inputDirectory/foo/bar.png
    ///   and replace any file if it exists at that path. Any subdirectories in <c>mediaDirectory</c> will also be
    ///   created in <c>inputDirectory</c>).
    /// </summary>
    /// <param name="inputDirectory"> The path to the input directory </param>
    /// <param name="mediaDirectory">
    ///   The path to the directory where the original media lives. This media will be reinserted back and replace any
    ///   placeholder media.
    /// </param>
    /// <exception cref="DocumentMinificationException">
    ///   If a problem occurred when trying to reinsert the media
    /// </exception>
    private void ReplaceMediaInDir(string inputDirectory, string mediaDirectory) {
      foreach (var filePath in Directory.GetFiles(mediaDirectory, "*.*", SearchOption.AllDirectories)) {
        var relativeFilePath = filePath.Substring(mediaDirectory.Length + 1);
        var curMediaPath = Path.Combine(inputDirectory, relativeFilePath);
        var curMediaDir = Path.GetDirectoryName(curMediaPath);
        if (!string.IsNullOrWhiteSpace(curMediaDir) && !Directory.Exists(curMediaDir)) {
          try {
            Directory.CreateDirectory(curMediaDir);
          } catch (Exception ex) {
            throw new DocumentDeminificationException(
                  $"Exception when reinserting media. Failed to create directory at {curMediaDir}.",
                  ex);
          }
        }

        try {
          if (File.Exists(curMediaPath)) {
            File.Delete(curMediaPath);
          }

          File.Move(filePath, curMediaPath);
        } catch (Exception ex) {
          throw new DocumentDeminificationException(
                $"Exception when reinserting media. Failed to move media back to {curMediaPath}.",
                ex);
        }
      }
    }
  }
}
