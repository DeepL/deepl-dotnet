// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

#if NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis {
  /// <summary>Polyfill of the nullable analysis attribute that netstandard2.0 lacks; used by the compiler only.</summary>
  [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true)]
  internal sealed class NotNullIfNotNullAttribute : Attribute {
    public NotNullIfNotNullAttribute(string parameterName) {
      ParameterName = parameterName;
    }

    public string ParameterName { get; }
  }
}
#endif
