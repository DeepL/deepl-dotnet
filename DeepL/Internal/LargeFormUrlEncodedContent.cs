// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

#if !NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DeepL.Internal {
  /// <summary>
  ///   Custom replacement for <see cref="FormUrlEncodedContent" /> on <c>netstandard2.0</c> (and older .NET Framework)
  ///   to avoid the size limit in the pre-.NET 5 implementation.
  ///   See https://github.com/dotnet/corefx/pull/41686 — the fix shipped in .NET 5, so this type is compiled out for
  ///   modern targets and the built-in <see cref="FormUrlEncodedContent" /> is used directly.
  /// </summary>
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
#endif
