// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;

namespace DeepL {
  /// <summary>
  ///   Base options that apply to all request endpoints.
  /// </summary>
  public class BaseRequestOptions {
    /// <summary>
    ///   Additional key-value pairs to include in the request body sent to the API. They are added last and replace
    ///   built-in parameters with the same key; a <c>null</c> value removes that key from the body. In JSON bodies each
    ///   value is sent with its JSON type (string, boolean, number, array, object). In the multipart body of a document
    ///   upload, strings are sent as given, collections comma-joined, booleans as <c>true</c>/<c>false</c>, numbers in
    ///   the invariant culture and other values as their JSON text. This can be used to access beta features or
    ///   override built-in parameters for testing purposes. Mostly used by DeepL employees to test functionality, or
    ///   for beta programs.
    /// </summary>
    public Dictionary<string, object?>? ExtraBodyParameters { get; set; }
  }
}
