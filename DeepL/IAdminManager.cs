// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Model;

namespace DeepL {
  /// <summary>Functions of the DeepL Admin API, to manage developer keys and retrieve usage analytics.</summary>
  public interface IAdminManager : IDisposable {
    /// <summary>
    ///   Retrieves usage statistics for the organization within the given date range.
    /// </summary>
    /// <param name="startDate">
    ///   Start date of the report. The API counts usage per UTC calendar day and includes both the start and the end
    ///   day. For a <see cref="DateTime" /> the UTC date of that moment is used: <see cref="DateTimeKind.Local" />
    ///   values are converted to UTC, <see cref="DateTimeKind.Unspecified" /> values are treated as UTC. For example,
    ///   local midnight on 2026-10-20 in UTC+2 is sent as 2026-10-19.
    /// </param>
    /// <param name="endDate">
    ///   End date of the report, treated like <paramref name="startDate" />. It must be after the start date, so a
    ///   report covers at least two days, and at most 366 days after it. Otherwise the API rejects the range.
    /// </param>
    /// <param name="groupBy">Optional grouping of the usage by key, or by key and day.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="AdminUsageReport" /> containing the usage statistics.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, including a rejected date range, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<AdminUsageReport> GetUsageAnalyticsAsync(
          DateTime startDate,
          DateTime endDate,
          AdminUsageGroupBy? groupBy = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves usage statistics for the organization within the given date range.
    /// </summary>
    /// <param name="startDate">
    ///   Start date of the report as a <c>yyyy-MM-dd</c> string, a UTC calendar day. The API includes both the start
    ///   and the end day.
    /// </param>
    /// <param name="endDate">
    ///   End date of the report as a <c>yyyy-MM-dd</c> string. It must be after the start date, so a report covers at
    ///   least two days, and at most 366 days after it. Otherwise the API rejects the range.
    /// </param>
    /// <param name="groupBy">Optional grouping of the usage by key, or by key and day.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="AdminUsageReport" /> containing the usage statistics.</returns>
    /// <exception cref="ArgumentException">
    ///   If <paramref name="startDate" /> or <paramref name="endDate" /> is not a <c>yyyy-MM-dd</c> date.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, including a rejected date range, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<AdminUsageReport> GetUsageAnalyticsAsync(
          string startDate,
          string endDate,
          AdminUsageGroupBy? groupBy = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves usage statistics per custom tag within the given date range. Results are
    ///   paginated; pass the <see cref="CustomTagUsageReport.NextPage" /> value of a report as <paramref name="page" />
    ///   to retrieve the following page.
    /// </summary>
    /// <param name="startDate">
    ///   Start date of the report. The API counts usage per UTC calendar day and includes both the start and the end
    ///   day. For a <see cref="DateTime" /> the UTC date of that moment is used: <see cref="DateTimeKind.Local" />
    ///   values are converted to UTC, <see cref="DateTimeKind.Unspecified" /> values are treated as UTC. For example,
    ///   local midnight on 2026-10-20 in UTC+2 is sent as 2026-10-19.
    /// </param>
    /// <param name="endDate">
    ///   End date of the report, treated like <paramref name="startDate" />. It must be after the start date, so a
    ///   report covers at least two days, and at most 366 days after it. Otherwise the API rejects the range.
    /// </param>
    /// <param name="aggregateBy">Optional aggregation of the usage over the whole period or by day.</param>
    /// <param name="page">Optional page number to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="CustomTagUsageReport" /> containing the usage statistics.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, including a rejected date range, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<CustomTagUsageReport> GetCustomTagUsageAnalyticsAsync(
          DateTime startDate,
          DateTime endDate,
          CustomTagUsageAggregateBy? aggregateBy = null,
          int? page = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Retrieves usage statistics per custom tag within the given date range. Results are
    ///   paginated; pass the <see cref="CustomTagUsageReport.NextPage" /> value of a report as <paramref name="page" />
    ///   to retrieve the following page.
    /// </summary>
    /// <param name="startDate">
    ///   Start date of the report as a <c>yyyy-MM-dd</c> string, a UTC calendar day. The API includes both the start
    ///   and the end day.
    /// </param>
    /// <param name="endDate">
    ///   End date of the report as a <c>yyyy-MM-dd</c> string. It must be after the start date, so a report covers at
    ///   least two days, and at most 366 days after it. Otherwise the API rejects the range.
    /// </param>
    /// <param name="aggregateBy">Optional aggregation of the usage over the whole period or by day.</param>
    /// <param name="page">Optional page number to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="CustomTagUsageReport" /> containing the usage statistics.</returns>
    /// <exception cref="ArgumentException">
    ///   If <paramref name="startDate" /> or <paramref name="endDate" /> is not a <c>yyyy-MM-dd</c> date.
    /// </exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, including a rejected date range, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<CustomTagUsageReport> GetCustomTagUsageAnalyticsAsync(
          string startDate,
          string endDate,
          CustomTagUsageAggregateBy? aggregateBy = null,
          int? page = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Creates a developer key in the organization. To avoid creating duplicate keys, the request is retried only
    ///   when the API cannot have processed it: after HTTP 429 (too many requests) or a failure to resolve the server
    ///   name. Server errors, timeouts and broken connections are not retried; after such a
    ///   <see cref="ConnectionException" /> or <see cref="DeepLException" />, call <see cref="ListDeveloperKeysAsync" />
    ///   to check whether the key was created.
    /// </summary>
    /// <param name="options">
    ///   Optional <see cref="CreateDeveloperKeyOptions" />, including the label of the new key. The API uses
    ///   "DeepL API Key" if no label is given.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DeveloperKey" /> describing the created key.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DeveloperKey> CreateDeveloperKeyAsync(
          CreateDeveloperKeyOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Retrieves all developer keys in the organization, including deactivated ones.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Array of <see cref="DeveloperKey" /> objects.</returns>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DeveloperKey[]> ListDeveloperKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>Deactivates a developer key. Deactivation is permanent.</summary>
    /// <param name="keyId">ID of the key to deactivate.</param>
    /// <param name="options">Optional <see cref="DeactivateDeveloperKeyOptions" />.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DeveloperKey" /> describing the deactivated key.</returns>
    /// <exception cref="ArgumentException">If <paramref name="keyId" /> is empty.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DeveloperKey> DeactivateDeveloperKeyAsync(
          string keyId,
          DeactivateDeveloperKeyOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Deactivates a developer key. Deactivation is permanent.</summary>
    /// <param name="key">The key to deactivate.</param>
    /// <param name="options">Optional <see cref="DeactivateDeveloperKeyOptions" />.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DeveloperKey" /> describing the deactivated key.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="key" /> is null.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DeveloperKey> DeactivateDeveloperKeyAsync(
          DeveloperKey key,
          DeactivateDeveloperKeyOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Changes the label of a developer key.</summary>
    /// <param name="keyId">ID of the key to rename.</param>
    /// <param name="label">New label for the key.</param>
    /// <param name="options">Optional <see cref="RenameDeveloperKeyOptions" />.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DeveloperKey" /> describing the renamed key.</returns>
    /// <exception cref="ArgumentException">If <paramref name="keyId" /> is empty.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DeveloperKey> RenameDeveloperKeyAsync(
          string keyId,
          string label,
          RenameDeveloperKeyOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>Changes the label of a developer key.</summary>
    /// <param name="key">The key to rename.</param>
    /// <param name="label">New label for the key.</param>
    /// <param name="options">Optional <see cref="RenameDeveloperKeyOptions" />.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DeveloperKey" /> describing the renamed key.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="key" /> is null.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DeveloperKey> RenameDeveloperKeyAsync(
          DeveloperKey key,
          string label,
          RenameDeveloperKeyOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Changes the usage limits of a developer key. Only the limits assigned in <paramref name="limits" /> are
    ///   changed; the other limits keep their current value. A <c>null</c> limit means unlimited and 0 blocks that
    ///   usage.
    /// </summary>
    /// <param name="keyId">ID of the key to modify.</param>
    /// <param name="limits">The usage limits to change.</param>
    /// <param name="options">Optional <see cref="SetDeveloperKeyUsageLimitsOptions" />.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DeveloperKey" /> describing the modified key.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="limits" /> is null.</exception>
    /// <exception cref="ArgumentException">If <paramref name="keyId" /> is empty.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DeveloperKey> SetDeveloperKeyUsageLimitsAsync(
          string keyId,
          DeveloperKeyUsageLimitsUpdate limits,
          SetDeveloperKeyUsageLimitsOptions? options = null,
          CancellationToken cancellationToken = default);

    /// <summary>
    ///   Changes the usage limits of a developer key. Only the limits assigned in <paramref name="limits" /> are
    ///   changed; the other limits keep their current value. A <c>null</c> limit means unlimited and 0 blocks that
    ///   usage.
    /// </summary>
    /// <param name="key">The key to modify.</param>
    /// <param name="limits">The usage limits to change.</param>
    /// <param name="options">Optional <see cref="SetDeveloperKeyUsageLimitsOptions" />.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see cref="DeveloperKey" /> describing the modified key.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="key" /> or <paramref name="limits" /> is null.</exception>
    /// <exception cref="DeepLException">
    ///   If any error occurs while communicating with the DeepL API, a
    ///   <see cref="DeepLException" /> or a derived class will be thrown.
    /// </exception>
    Task<DeveloperKey> SetDeveloperKeyUsageLimitsAsync(
          DeveloperKey key,
          DeveloperKeyUsageLimitsUpdate limits,
          SetDeveloperKeyUsageLimitsOptions? options = null,
          CancellationToken cancellationToken = default);
  }
}
