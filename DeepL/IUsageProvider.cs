// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>Retrieves the account usage of the DeepL API. Implemented by <see cref="DeepLClient" />.</summary>
  public interface IUsageProvider : IDisposable {
    /// <summary>Retrieves the usage in the current billing period for this DeepL account.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="Usage" /> object containing account usage information.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<Usage> GetUsageAsync(CancellationToken cancellationToken = default);
  }
}
