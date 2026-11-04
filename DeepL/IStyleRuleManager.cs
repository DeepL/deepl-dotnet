// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  public interface IStyleRuleManager {
    /// <summary>Retrieves the list of all available style rules.</summary>
    /// <param name="page">Optional page number for pagination, 0-indexed.</param>
    /// <param name="pageSize">Optional number of items per page.</param>
    /// <param name="detailed">
    ///   Optional flag indicating whether to include detailed configuration rules in the configuredRules property.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="StyleRuleInfo" /> objects representing the available style rules.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<StyleRuleInfo[]> GetAllStyleRulesAsync(
          int? page = null,
          int? pageSize = null,
          bool? detailed = null,
          CancellationToken cancellationToken = default);

    /// <summary>Creates a new style rule.</summary>
    /// <param name="name">Name for the new style rule.</param>
    /// <param name="language">Language code for the style rule.</param>
    /// <param name="configuredRules">
    ///   Optional configured rules: category to rule to value, keyed by the API names (for example
    ///   <c>dates_and_times</c>), sent as given.
    /// </param>
    /// <param name="customInstructions">Optional custom instructions.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="StyleRuleInfo" /> object for the newly created style rule.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    Task<StyleRuleInfo> CreateStyleRuleAsync(
          string name,
          string language,
          Dictionary<string, Dictionary<string, string>>? configuredRules = null,
          CustomInstruction[]? customInstructions = null,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single style rule by its ID.</summary>
    /// <param name="styleId">The ID of the style rule to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="StyleRuleInfo" /> object for the requested style rule.</returns>
    /// <exception cref="NotFoundException">If the specified style rule was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<StyleRuleInfo> GetStyleRuleAsync(
          string styleId,
          CancellationToken cancellationToken = default);

    /// <summary>Updates the name of a style rule.</summary>
    /// <param name="styleId">The ID of the style rule to update.</param>
    /// <param name="name">The new name for the style rule.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="StyleRuleInfo" /> object for the updated style rule.</returns>
    /// <exception cref="NotFoundException">If the specified style rule was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<StyleRuleInfo> UpdateStyleRuleNameAsync(
          string styleId,
          string name,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes a style rule.</summary>
    /// <param name="styleId">The ID of the style rule to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="NotFoundException">If the specified style rule was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteStyleRuleAsync(
          string styleId,
          CancellationToken cancellationToken = default);

    /// <summary>Updates the configured rules of a style rule.</summary>
    /// <param name="styleId">The ID of the style rule to update.</param>
    /// <param name="configuredRules">
    ///   The new configured rules: category to rule to value, keyed by the API names (for example
    ///   <c>dates_and_times</c>), sent as given.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="StyleRuleInfo" /> object for the updated style rule.</returns>
    /// <exception cref="NotFoundException">If the specified style rule was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<StyleRuleInfo> UpdateStyleRuleConfiguredRulesAsync(
          string styleId,
          Dictionary<string, Dictionary<string, string>> configuredRules,
          CancellationToken cancellationToken = default);

    /// <summary>Creates a custom instruction for a style rule.</summary>
    /// <param name="styleId">The ID of the style rule.</param>
    /// <param name="label">Label for the custom instruction.</param>
    /// <param name="prompt">Prompt text for the custom instruction.</param>
    /// <param name="sourceLanguage">Optional source language code.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="CustomInstruction" /> object for the newly created instruction.</returns>
    /// <exception cref="NotFoundException">If the specified style rule was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    /// <remarks>
    ///   Because the request creates a resource, it is retried only when it was not sent: after HTTP 429, a failure to
    ///   resolve the server name, a refused connection or a connect timeout. Server errors, other timeouts and broken
    ///   connections are not retried. A response body cut off mid-read is not retried either, but the
    ///   <see cref="ConnectionException" /> has <see cref="DeepLException.ShouldRetry" /> <c>true</c>.
    /// </remarks>
    Task<CustomInstruction> CreateStyleRuleCustomInstructionAsync(
          string styleId,
          string label,
          string prompt,
          string? sourceLanguage = null,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves a custom instruction for a style rule.</summary>
    /// <param name="styleId">The ID of the style rule.</param>
    /// <param name="instructionId">The ID of the custom instruction.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="CustomInstruction" /> object for the requested instruction.</returns>
    /// <exception cref="NotFoundException">If the specified style rule or custom instruction was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<CustomInstruction> GetStyleRuleCustomInstructionAsync(
          string styleId,
          string instructionId,
          CancellationToken cancellationToken = default);

    /// <summary>Updates a custom instruction for a style rule.</summary>
    /// <param name="styleId">The ID of the style rule.</param>
    /// <param name="instructionId">The ID of the custom instruction to update.</param>
    /// <param name="label">New label for the custom instruction.</param>
    /// <param name="prompt">New prompt text for the custom instruction.</param>
    /// <param name="sourceLanguage">Optional source language code.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="CustomInstruction" /> object for the updated instruction.</returns>
    /// <exception cref="NotFoundException">If the specified style rule or custom instruction was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<CustomInstruction> UpdateStyleRuleCustomInstructionAsync(
          string styleId,
          string instructionId,
          string label,
          string prompt,
          string? sourceLanguage = null,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes a custom instruction from a style rule.</summary>
    /// <param name="styleId">The ID of the style rule.</param>
    /// <param name="instructionId">The ID of the custom instruction to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="NotFoundException">If the specified style rule or custom instruction was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteStyleRuleCustomInstructionAsync(
          string styleId,
          string instructionId,
          CancellationToken cancellationToken = default);
  }
}
