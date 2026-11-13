// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepL.Internal {
  /// <summary>Argument checks shared by the clients and the input models; they run before any request.</summary>
  internal static class Argument {
    /// <summary>Returns the value; throws <see cref="ArgumentNullException" /> naming the parameter if it is <c>null</c>.</summary>
    internal static T NotNull<T>(T? value, string paramName) where T : class =>
          value ?? throw new ArgumentNullException(paramName);

    /// <summary>
    ///   Returns the value; throws <see cref="ArgumentNullException" /> if it is <c>null</c> and
    ///   <see cref="ArgumentException" /> if it is empty or whitespace-only.
    /// </summary>
    internal static string NotEmpty(string? value, string paramName) {
      if (string.IsNullOrWhiteSpace(NotNull(value, paramName))) {
        throw new ArgumentException($"Parameter {paramName} must not be empty", paramName);
      }

      return value!;
    }

    /// <summary>
    ///   Returns the entries as an array; throws <see cref="ArgumentNullException" /> naming the parameter if the list
    ///   or one of its entries is <c>null</c>.
    /// </summary>
    internal static T[] NoNullEntries<T>(IEnumerable<T?>? values, string paramName) where T : class {
      var array = NotNull(values, paramName).ToArray();
      if (array.Any(value => value == null)) {
        throw new ArgumentNullException(paramName, $"{paramName} must not contain null");
      }

      return array!;
    }
  }
}
