// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;

namespace DeepL {
  /// <summary>
  ///   Optional fields of a style profile, for <see cref="IStyleProfileManager.CreateStyleProfileAsync" /> and
  ///   <see cref="IStyleProfileManager.UpdateStyleProfileAsync(string, string?, StyleProfileOptions?, System.Threading.CancellationToken)" />.
  ///   A property left <c>null</c> is not sent; an empty string or an empty list is sent as given.
  /// </summary>
  /// <remarks>
  ///   The lists are nullable on purpose: <c>null</c> leaves the linked components unchanged, an empty list unlinks
  ///   all of them.
  /// </remarks>
  public sealed class StyleProfileOptions {
    /// <summary>Initializes a new <see cref="StyleProfileOptions" /> object.</summary>
    public StyleProfileOptions() { }

    /// <summary>Free-text description of the style profile. On update, an empty string clears it.</summary>
    public string? Description { get; set; }

    /// <summary>The industry the style profile is written for. On update, an empty string clears it.</summary>
    public string? Industry { get; set; }

    /// <summary>
    ///   IDs of the glossaries to link, highest priority first. On update, the list replaces the linked glossaries in
    ///   full; an empty list unlinks all glossaries.
    /// </summary>
    public IEnumerable<string>? GlossaryIds { get; set; }

    /// <summary>
    ///   IDs of the style rule lists to link, at most one per target language. On update, the list replaces the linked
    ///   style rule lists in full; an empty list unlinks all style rule lists.
    /// </summary>
    public IEnumerable<string>? StyleRuleIds { get; set; }

    /// <summary>
    ///   IDs of the translation memories to link, at most one per source language. On update, the list replaces the
    ///   linked translation memories in full; an empty list unlinks all translation memories.
    /// </summary>
    public IEnumerable<string>? TranslationMemoryIds { get; set; }

    /// <summary>
    ///   Whether the order of <see cref="GlossaryIds" /> sets priority: when two linked glossaries define the same source
    ///   term, the one listed first wins.
    /// </summary>
    public bool? GlossaryPriorityOrderEnabled { get; set; }
  }
}
