// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Optional fields of a style profile, for <see cref="IStyleProfileManager.CreateStyleProfileAsync" />. A property
  ///   left <c>null</c> is not sent; an empty string or an empty list is sent as given.
  /// </summary>
  /// <remarks>
  ///   The lists are nullable on purpose: <c>null</c> leaves the linked components unchanged, an empty list unlinks
  ///   all of them. Each list takes IDs; the resource objects convert to their ID implicitly, so a list can mix IDs and
  ///   objects (<c>new string[] { glossaryInfo, "id" }</c>). <see cref="SetGlossaries(IEnumerable{GlossaryInfo})" />,
  ///   <see cref="SetStyleRules" /> and <see cref="SetTranslationMemories" /> take a list of resource objects.
  /// </remarks>
  public class CreateStyleProfileOptions : BaseRequestOptions {
    /// <summary>Initializes a new <see cref="CreateStyleProfileOptions" /> object.</summary>
    public CreateStyleProfileOptions() { }

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

    /// <summary>Sets <see cref="GlossaryIds" /> to the IDs of the given glossaries, highest priority first.</summary>
    /// <param name="glossaries">Glossaries to link.</param>
    public void SetGlossaries(IEnumerable<GlossaryInfo> glossaries) =>
          GlossaryIds = glossaries.Select(glossary => glossary.GlossaryId).ToArray();

    /// <summary>
    ///   Sets <see cref="GlossaryIds" /> to the IDs of the given multilingual glossaries, highest priority first.
    /// </summary>
    /// <param name="glossaries">Glossaries to link.</param>
    public void SetGlossaries(IEnumerable<MultilingualGlossaryInfo> glossaries) =>
          GlossaryIds = glossaries.Select(glossary => glossary.GlossaryId).ToArray();

    /// <summary>Sets <see cref="StyleRuleIds" /> to the IDs of the given style rule lists.</summary>
    /// <param name="styleRules">Style rule lists to link.</param>
    public void SetStyleRules(IEnumerable<StyleRuleInfo> styleRules) =>
          StyleRuleIds = styleRules.Select(styleRule => styleRule.StyleId).ToArray();

    /// <summary>Sets <see cref="TranslationMemoryIds" /> to the IDs of the given translation memories.</summary>
    /// <param name="translationMemories">Translation memories to link.</param>
    public void SetTranslationMemories(IEnumerable<TranslationMemoryInfo> translationMemories) =>
          TranslationMemoryIds = translationMemories.Select(memory => memory.TranslationMemoryId).ToArray();
  }

  /// <summary>
  ///   Fields to change in a style profile, for
  ///   <see cref="IStyleProfileManager.UpdateStyleProfileAsync(string, UpdateStyleProfileOptions?, System.Threading.CancellationToken)" />.
  ///   A property left <c>null</c> is not sent and stays unchanged; an empty string clears the description or
  ///   industry, an empty list unlinks all components of that kind.
  /// </summary>
  public sealed class UpdateStyleProfileOptions : CreateStyleProfileOptions {
    /// <summary>Initializes a new <see cref="UpdateStyleProfileOptions" /> object.</summary>
    public UpdateStyleProfileOptions() { }

    /// <summary>New name of the style profile.</summary>
    public string? Name { get; set; }
  }
}
