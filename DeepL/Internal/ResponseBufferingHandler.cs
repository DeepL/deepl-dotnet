// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DeepL.Internal {
  /// <summary>Marks a failure while reading the response body, after the request was sent.</summary>
  internal sealed class ResponseBodyCutOffException : IOException {
    /// <summary>Initializes a new <see cref="ResponseBodyCutOffException" />.</summary>
    /// <param name="innerException">The failure while reading the body.</param>
    internal ResponseBodyCutOffException(Exception innerException) :
          base($"Response body cut off: {innerException.Message}", innerException) {
    }
  }

  /// <summary>
  ///   Reads the whole response body inside the handler chain, so that the retry policy around this handler sees a body
  ///   cut off mid-read as a failed attempt. Such a failure is thrown as an <see cref="HttpRequestException" /> whose
  ///   inner exception is a <see cref="ResponseBodyCutOffException" />.
  /// </summary>
  internal sealed class ResponseBufferingHandler : DelegatingHandler {
    /// <summary>Initializes a new <see cref="ResponseBufferingHandler" /> sending requests with the given handler.</summary>
    internal ResponseBufferingHandler(HttpMessageHandler innerHandler) : base(innerHandler) {
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage request,
          CancellationToken cancellationToken) {
      var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
      try {
        await LoadIntoBufferAsync(response, cancellationToken).ConfigureAwait(false);
        return response;
      } catch (Exception ex) when (ex is IOException || ex is HttpRequestException) {
        response.Dispose();
        throw new HttpRequestException("Response body cut off", new ResponseBodyCutOffException(ex));
      } catch {
        response.Dispose();
        throw;
      }
    }

    /// <summary>
    ///   Reads the whole body of the given response into memory. Cancelling the token disposes the response, which
    ///   aborts the read, and throws an <see cref="OperationCanceledException" />.
    /// </summary>
    internal static async Task LoadIntoBufferAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
      if (response.Content == null) {
        return;
      }

      using (cancellationToken.Register(response.Dispose)) {
        try {
          await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
        } catch (Exception) when (cancellationToken.IsCancellationRequested) {
          cancellationToken.ThrowIfCancellationRequested();
          throw;
        }
      }

      cancellationToken.ThrowIfCancellationRequested();
    }
  }
}
