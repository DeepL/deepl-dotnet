// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

namespace DeepL {
  /// <summary>Options for <see cref="IAdminManager.CreateDeveloperKeyAsync" />.</summary>
  public sealed class CreateDeveloperKeyOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="CreateDeveloperKeyOptions" /> object.</summary>
    public CreateDeveloperKeyOptions() { }

    /// <summary>
    ///   Label for the new key. If <c>null</c>, no label is sent and the API uses "DeepL API Key"; any other value,
    ///   including an empty string, is sent as given.
    /// </summary>
    public string? Label { get; set; }
  }

  /// <summary>Options for <see cref="IAdminManager.DeactivateDeveloperKeyAsync(string, DeactivateDeveloperKeyOptions?, System.Threading.CancellationToken)" />.</summary>
  public sealed class DeactivateDeveloperKeyOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="DeactivateDeveloperKeyOptions" /> object.</summary>
    public DeactivateDeveloperKeyOptions() { }
  }

  /// <summary>Options for <see cref="IAdminManager.RenameDeveloperKeyAsync(string, string, RenameDeveloperKeyOptions?, System.Threading.CancellationToken)" />.</summary>
  public sealed class RenameDeveloperKeyOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="RenameDeveloperKeyOptions" /> object.</summary>
    public RenameDeveloperKeyOptions() { }
  }

  /// <summary>
  ///   Options for
  ///   <see cref="IAdminManager.SetDeveloperKeyUsageLimitsAsync(string, DeveloperKeyUsageLimitsUpdate, SetDeveloperKeyUsageLimitsOptions?, System.Threading.CancellationToken)" />.
  /// </summary>
  public sealed class SetDeveloperKeyUsageLimitsOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="SetDeveloperKeyUsageLimitsOptions" /> object.</summary>
    public SetDeveloperKeyUsageLimitsOptions() { }
  }
}
