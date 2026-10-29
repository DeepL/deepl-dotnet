// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.
using System;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Manages style profiles: named sets of glossaries, style rule lists and translation memories.
  /// </summary>
  public interface IStyleProfileManager {
    /// <summary>Retrieves the style profiles of the account.</summary>
    /// <param name="page">Optional page number for pagination, 0-indexed; not sent if <c>null</c>.</param>
    /// <param name="pageSize">
    ///   Optional number of items per page; not sent if <c>null</c>. The API reduces values above 100 to 100.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="StyleProfile" /> objects.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<StyleProfile[]> ListStyleProfilesAsync(
          int? page = null,
          int? pageSize = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Creates a style profile. If any of the component IDs cannot be used, the API rejects the request and no style
    ///   profile is created.
    /// </summary>
    /// <param name="name">Name of the style profile.</param>
    /// <param name="options">Optional fields of the style profile; properties left <c>null</c> are not sent.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="StyleProfile" /> object with details about the newly created style profile.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<StyleProfile> CreateStyleProfileAsync(
          string name,
          CreateStyleProfileOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves a style profile.</summary>
    /// <param name="styleProfileId">ID of the style profile.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="StyleProfile" /> object with details about the style profile.</returns>
    /// <exception cref="NotFoundException">If the specified style profile was not found.</exception>
    /// <exception cref="ArgumentException">If the style profile ID is empty.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<StyleProfile> GetStyleProfileAsync(
          string styleProfileId,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves a style profile.</summary>
    /// <param name="styleProfile">The style profile.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="StyleProfile" /> object with details about the style profile.</returns>
    /// <exception cref="NotFoundException">If the specified style profile was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<StyleProfile> GetStyleProfileAsync(
          StyleProfile styleProfile,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Updates a style profile. Fields left <c>null</c> are not changed; a component list replaces the linked list in
    ///   full.
    /// </summary>
    /// <param name="styleProfileId">ID of the style profile.</param>
    /// <param name="options">
    ///   Fields to change, including the name; properties left <c>null</c> are not sent and stay unchanged. An empty
    ///   string clears the description or industry; an empty list unlinks all components of that kind.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="StyleProfile" /> object with details about the updated style profile.</returns>
    /// <exception cref="NotFoundException">If the specified style profile was not found.</exception>
    /// <exception cref="ArgumentException">If the style profile ID is empty.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<StyleProfile> UpdateStyleProfileAsync(
          string styleProfileId,
          UpdateStyleProfileOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Updates a style profile. Fields left <c>null</c> are not changed; a component list replaces the linked list in
    ///   full.
    /// </summary>
    /// <param name="styleProfile">The style profile.</param>
    /// <param name="options">
    ///   Fields to change, including the name; properties left <c>null</c> are not sent and stay unchanged. An empty
    ///   string clears the description or industry; an empty list unlinks all components of that kind.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="StyleProfile" /> object with details about the updated style profile.</returns>
    /// <exception cref="NotFoundException">If the specified style profile was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<StyleProfile> UpdateStyleProfileAsync(
          StyleProfile styleProfile,
          UpdateStyleProfileOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes a style profile.</summary>
    /// <param name="styleProfileId">ID of the style profile.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="NotFoundException">If the specified style profile was not found.</exception>
    /// <exception cref="ArgumentException">If the style profile ID is empty.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteStyleProfileAsync(
          string styleProfileId,
          CancellationToken cancellationToken = default);

    /// <summary>Deletes a style profile.</summary>
    /// <param name="styleProfile">The style profile.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <exception cref="NotFoundException">If the specified style profile was not found.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task DeleteStyleProfileAsync(
          StyleProfile styleProfile,
          CancellationToken cancellationToken = default);
  }
}
