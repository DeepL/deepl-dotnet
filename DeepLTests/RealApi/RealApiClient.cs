// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using DeepL;

namespace DeepLTests.RealApi {
  internal static class RealApiClient {
    /// <summary>
    ///   Creates a client for the API at <c>DEEPL_SERVER_URL</c> (the default server if unset), authenticating with
    ///   <c>DEEPL_AUTH_KEY</c>.
    /// </summary>
    internal static DeepLClient CreateClient() {
      var authKey = Environment.GetEnvironmentVariable("DEEPL_AUTH_KEY");
      if (string.IsNullOrEmpty(authKey)) {
        throw new InvalidOperationException("The DEEPL_AUTH_KEY environment variable must be set to run the real-API tests.");
      }

      var serverUrl = Environment.GetEnvironmentVariable("DEEPL_SERVER_URL");
      return string.IsNullOrEmpty(serverUrl)
            ? new DeepLClient(authKey)
            : new DeepLClient(authKey, new DeepLClientOptions { ServerUrl = serverUrl });
    }

    /// <summary>
    ///   Creates an admin client for the API at <c>DEEPL_SERVER_URL</c> (the default server if unset), authenticating
    ///   with <c>DEEPL_ADMIN_KEY</c>.
    /// </summary>
    internal static DeepLAdminClient CreateAdminClient() {
      var adminKey = Environment.GetEnvironmentVariable("DEEPL_ADMIN_KEY");
      if (string.IsNullOrEmpty(adminKey)) {
        throw new InvalidOperationException(
              "The DEEPL_ADMIN_KEY environment variable must be set to run the real-API admin tests.");
      }

      var serverUrl = Environment.GetEnvironmentVariable("DEEPL_SERVER_URL");
      return new DeepLAdminClient(
            adminKey,
            new DeepLAdminClientOptions { ServerUrl = string.IsNullOrEmpty(serverUrl) ? null : serverUrl });
    }
  }
}
