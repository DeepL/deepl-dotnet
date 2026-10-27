// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Net.Http;

namespace DeepL {
  /// <summary>
  ///   Class containing containing options controlling <see cref="Translator" /> behaviour.
  ///   Please use <see cref="DeepLClient" /> and <see cref="DeepLClientOptions" /> instead.
  /// </summary>
  public class TranslatorOptions : BaseClientOptions { }

  /// <summary>
  ///   Struct containing an <see cref="HttpClient" /> to be used by a client such as <see cref="DeepLClient" /> or
  ///   <see cref="DeepLAdminClient" />, and a flag whether the client should dispose it when it is disposed.
  /// </summary>
  public struct HttpClientAndDisposeFlag {
    /// <summary>
    ///   <see cref="HttpClient" /> used by the client for all requests to DeepL API.
    /// </summary>
    public HttpClient HttpClient { get; set; }

    /// <summary>
    ///   <c>true</c> if the provided <see cref="HttpClient" /> should be disposed of when the client is disposed;
    ///   <c>false</c> if you intend to reuse it.
    /// </summary>
    public bool DisposeClient { get; set; }
  }

  /// <summary>
  ///   Represents the identifying information of an application that uses this library, its name and version.
  ///   Example: AppName = "myDotNetCustomerChatApp", AppVersion = "1.2.3"
  /// </summary>
  public struct AppInfo {
    /// <summary>
    ///   Name of the application using this client library
    /// </summary>
    public string AppName { get; set; }
    /// <summary>
    ///   Version of the application using this client library
    /// </summary>
    public string AppVersion { get; set; }
  }
}
