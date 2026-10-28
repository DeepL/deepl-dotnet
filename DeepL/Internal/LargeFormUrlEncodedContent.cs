// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DeepL.Internal {
  /// <summary>Custom replacement for System.Net.Http.FormUrlEncodedContent to avoid size limitations.</summary>
  /// There was a bugfix for .NET 5 (https://github.com/dotnet/corefx/pull/41686) that solved this issue.
  /// This class avoids the problem by using WebUtility.UrlEncoded() instead of Uri.EscapeDataString().
  [Obsolete("Internal type; will be made internal in the next major version.")]
  public class LargeFormUrlEncodedContent : ByteArrayContent {
    private static readonly Encoding Utf8Encoding = Encoding.UTF8;

    public LargeFormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
          : base(GetContentByteArray(nameValueCollection)) {
      Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
    }

    private static byte[] GetContentByteArray(IEnumerable<KeyValuePair<string, string>> nameValueCollection) {
      if (nameValueCollection == null) {
        throw new ArgumentNullException(nameof(nameValueCollection));
      }

      var str = string.Join(
            "&",
            nameValueCollection.Select(pair => $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}"));
      return Utf8Encoding.GetBytes(str);
    }
  }
}
