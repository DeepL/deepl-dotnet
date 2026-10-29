// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

namespace DeepL {
  /// <summary>
  ///   Options for the <c>DeepLClient.CreateStyleRuleAsync()</c> overload taking options. Its
  ///   <see cref="BaseRequestOptions.ExtraBodyParameters" /> are added last to the JSON request body.
  /// </summary>
  public sealed class CreateStyleRuleOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="CreateStyleRuleOptions" /> object.</summary>
    public CreateStyleRuleOptions() { }
  }

  /// <summary>
  ///   Options for the <c>DeepLClient.UpdateStyleRuleNameAsync()</c> overloads taking options. Its
  ///   <see cref="BaseRequestOptions.ExtraBodyParameters" /> are added last to the JSON request body.
  /// </summary>
  public sealed class UpdateStyleRuleNameOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="UpdateStyleRuleNameOptions" /> object.</summary>
    public UpdateStyleRuleNameOptions() { }
  }

  /// <summary>
  ///   Options for the <c>DeepLClient.UpdateStyleRuleConfiguredRulesAsync()</c> overloads taking options. The request
  ///   body is the configured rules map itself; its <see cref="BaseRequestOptions.ExtraBodyParameters" /> are added last
  ///   to it as further keys.
  /// </summary>
  public sealed class UpdateStyleRuleConfiguredRulesOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="UpdateStyleRuleConfiguredRulesOptions" /> object.</summary>
    public UpdateStyleRuleConfiguredRulesOptions() { }
  }

  /// <summary>
  ///   Options for the <c>DeepLClient.CreateStyleRuleCustomInstructionAsync()</c> overloads taking options. Its
  ///   <see cref="BaseRequestOptions.ExtraBodyParameters" /> are added last to the JSON request body.
  /// </summary>
  public sealed class CreateStyleRuleCustomInstructionOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="CreateStyleRuleCustomInstructionOptions" /> object.</summary>
    public CreateStyleRuleCustomInstructionOptions() { }
  }

  /// <summary>
  ///   Options for the <c>DeepLClient.UpdateStyleRuleCustomInstructionAsync()</c> overloads taking options. Its
  ///   <see cref="BaseRequestOptions.ExtraBodyParameters" /> are added last to the JSON request body.
  /// </summary>
  public sealed class UpdateStyleRuleCustomInstructionOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="UpdateStyleRuleCustomInstructionOptions" /> object.</summary>
    public UpdateStyleRuleCustomInstructionOptions() { }
  }
}
