// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace DeepL {
  /// <summary>Class containing containing options controlling <see cref="DeepLClient" /> behaviour.</summary>
#pragma warning disable CS0618 // derives from the deprecated TranslatorOptions until the next major version
  public sealed class DeepLClientOptions : TranslatorOptions { }
#pragma warning restore CS0618
}
