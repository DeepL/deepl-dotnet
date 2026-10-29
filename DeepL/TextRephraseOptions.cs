// Copyright 2022 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Options to control text rephrasing behaviour. These options may be provided to the
  ///   <see cref="IWriter.RephraseTextAsync(string, string?, TextRephraseOptions?, System.Threading.CancellationToken)" />
  ///   functions.
  /// </summary>
  public sealed class TextRephraseOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="TextRephraseOptions" /> object.</summary>
    public TextRephraseOptions() { }

    /// <summary>Initializes a new <see cref="TextRephraseOptions" /> object with the given writing style.</summary>
    /// <param name="writingStyle">The style the rephrasing should be in, sent as its API value.</param>
    public TextRephraseOptions(WritingStyle writingStyle) {
      WritingStyle = writingStyle.ToApiValue();
    }

    /// <summary>Initializes a new <see cref="TextRephraseOptions" /> object with the given writing tone.</summary>
    /// <param name="writingTone">The tone the rephrasing should be in, sent as its API value.</param>
    public TextRephraseOptions(WritingTone writingTone) {
      WritingTone = writingTone.ToApiValue();
    }

    /// <summary>Controls the style the rephrasing should be in.</summary>
    /// This option is only applicable for target languages that support styles, and only a tone
    /// OR a style can be chosen in the same request.
    public string? WritingStyle { get; set; } = null;

    /// <summary>Controls the tone the rephrasing should be in.</summary>
    /// This option is only applicable for target languages that support tones, and only a tone
    /// OR a style can be chosen in the same request.
    public string? WritingTone { get; set; } = null;
  }

}
