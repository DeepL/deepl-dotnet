// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DeepL.Internal {
  /// <summary>
  ///   Request content that records whether the HTTP handler started to write it. The handler writes the request line
  ///   and headers together with the body, after the connection (TCP, proxy tunnel, TLS) is up, so a request whose
  ///   content was never written was not sent.
  /// </summary>
  internal sealed class SendTrackingContent : HttpContent {
    private int _writeStarted;

    /// <summary>Wraps the given content; without one the body is empty, sent as <c>Content-Length: 0</c> like none.</summary>
    internal SendTrackingContent(HttpContent? content) {
      Content = content;
      if (content != null) {
        foreach (var header in content.Headers) {
          Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
      }
    }

    /// <summary>The wrapped content, or <c>null</c> for a request without a body.</summary>
    internal HttpContent? Content { get; }

    /// <summary>Whether the handler started to write the request in any attempt.</summary>
    internal bool WriteStarted => Volatile.Read(ref _writeStarted) != 0;

    /// <inheritdoc />
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) {
      Volatile.Write(ref _writeStarted, 1);
      return Content != null ? Content.CopyToAsync(stream) : Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override bool TryComputeLength(out long length) {
      var contentLength = Content != null ? Content.Headers.ContentLength : 0;
      length = contentLength ?? 0;
      return contentLength.HasValue;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing) {
      if (disposing) {
        Content?.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
