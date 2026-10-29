// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DeepL.Internal;
using DeepL.Model;

namespace DeepL {
  /// <summary>
  ///   Client for the DeepL Admin API, used to manage the developer keys of your organization and retrieve usage
  ///   analytics. It must be initialized with an admin API key; admin keys cannot be used for translation, so use
  ///   <see cref="DeepLClient" /> for that.
  /// </summary>
  public sealed class DeepLAdminClient : IAdminManager {
    /// <summary>Base URL for the DeepL Admin API.</summary>
    private const string DeepLServerUrl = "https://api.deepl.com";

    /// <summary>Internal class implementing HTTP requests.</summary>
    private readonly DeepLHttpClient _client;

    /// <summary>Initializes a new <see cref="DeepLAdminClient" /> object using your admin API key.</summary>
    /// <param name="adminKey">
    ///   Admin API key as found in your <a href="https://www.deepl.com/your-account/">DeepL account</a>. Developer keys
    ///   cannot access the Admin API; the API rejects them with an <see cref="AuthorizationException" />.
    /// </param>
    /// <param name="options">Additional options controlling <see cref="DeepLAdminClient" /> behaviour.</param>
    /// <exception cref="ArgumentNullException">If adminKey argument is null.</exception>
    /// <exception cref="ArgumentException">If adminKey argument is empty.</exception>
    /// <remarks>This function does not establish a connection to the DeepL API.</remarks>
    public DeepLAdminClient(string adminKey, DeepLAdminClientOptions? options = null) {
      options ??= new DeepLAdminClientOptions();

      if (adminKey == null) {
        throw new ArgumentNullException(nameof(adminKey));
      }

      adminKey = adminKey.Trim();

      if (adminKey.Length == 0) {
        throw new ArgumentException($"{nameof(adminKey)} is empty");
      }

      _client = DeepLHttpClient.Create(adminKey, new Uri(options.ServerUrl ?? DeepLServerUrl), options);
    }

    /// <summary>
    ///   Releases the unmanaged resources and disposes of the managed resources used by the
    ///   <see cref="DeepLAdminClient" />.
    /// </summary>
    public void Dispose() => _client.Dispose();

    /// <inheritdoc />
    public Task<AdminUsageReport> GetUsageAnalyticsAsync(
          DateTime startDate,
          DateTime endDate,
          AdminUsageGroupBy? groupBy = null,
          CancellationToken cancellationToken = default) =>
          GetUsageAnalyticsAsync(CreateDateRangeParams(startDate, endDate), groupBy, cancellationToken);

    /// <inheritdoc />
    public Task<AdminUsageReport> GetUsageAnalyticsAsync(
          string startDate,
          string endDate,
          AdminUsageGroupBy? groupBy = null,
          CancellationToken cancellationToken = default) =>
          GetUsageAnalyticsAsync(CreateDateRangeParams(startDate, endDate), groupBy, cancellationToken);

    /// <inheritdoc />
    public Task<CustomTagUsageReport> GetCustomTagUsageAnalyticsAsync(
          DateTime startDate,
          DateTime endDate,
          CustomTagUsageAggregateBy? aggregateBy = null,
          int? page = null,
          CancellationToken cancellationToken = default) =>
          GetCustomTagUsageAnalyticsAsync(
                CreateDateRangeParams(startDate, endDate), aggregateBy, page, cancellationToken);

    /// <inheritdoc />
    public Task<CustomTagUsageReport> GetCustomTagUsageAnalyticsAsync(
          string startDate,
          string endDate,
          CustomTagUsageAggregateBy? aggregateBy = null,
          int? page = null,
          CancellationToken cancellationToken = default) =>
          GetCustomTagUsageAnalyticsAsync(
                CreateDateRangeParams(startDate, endDate), aggregateBy, page, cancellationToken);

    private async Task<AdminUsageReport> GetUsageAnalyticsAsync(
          List<(string Key, string Value)> queryParams,
          AdminUsageGroupBy? groupBy,
          CancellationToken cancellationToken) {
      if (groupBy != null) {
        queryParams.Add(("group_by", groupBy.Value.ToApiValue()));
      }

      using var responseMessage = await _client
            .ApiGetAsync("v2/admin/analytics", cancellationToken, queryParams).ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var element = await ReadElementAsync(responseMessage, JsonValueKind.Object, "Usage analytics")
            .ConfigureAwait(false);
      return JsonUtils.Deserialize<UsageReportResult>(element).UsageReport;
    }

    private async Task<CustomTagUsageReport> GetCustomTagUsageAnalyticsAsync(
          List<(string Key, string Value)> queryParams,
          CustomTagUsageAggregateBy? aggregateBy,
          int? page,
          CancellationToken cancellationToken) {
      if (aggregateBy != null) {
        queryParams.Add(("aggregate_by", aggregateBy.Value.ToApiValue()));
      }

      if (page != null) {
        queryParams.Add(("page", page.Value.ToString(CultureInfo.InvariantCulture)));
      }

      using var responseMessage = await _client
            .ApiGetAsync("v2/admin/analytics/custom-tags", cancellationToken, queryParams).ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var element = await ReadElementAsync(
                  responseMessage, JsonValueKind.Object, "Custom tag usage analytics")
            .ConfigureAwait(false);
      return JsonUtils.Deserialize<CustomTagUsageReportResult>(element).CustomTagUsageReport;
    }

    /// <inheritdoc />
    public async Task<DeveloperKey> CreateDeveloperKeyAsync(
          string? label = null,
          CancellationToken cancellationToken = default) {
      var requestData = new Dictionary<string, object>();
      if (label != null) {
        requestData["label"] = label;
      }

      using var responseMessage = await _client
            .ApiPostJsonAsync("v2/admin/developer-keys", cancellationToken, requestData, retryOnlyUnprocessed: true)
            .ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      return await ReadKeyAsync(responseMessage).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DeveloperKey[]> GetDeveloperKeysAsync(CancellationToken cancellationToken = default) {
      using var responseMessage =
            await _client.ApiGetAsync("v2/admin/developer-keys", cancellationToken).ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      var list = await ReadElementAsync(responseMessage, JsonValueKind.Array, "Developer key list")
            .ConfigureAwait(false);
      var keys = new List<DeveloperKey>();
      foreach (var entry in list.EnumerateArray()) {
        if (entry.ValueKind != JsonValueKind.Object) {
          continue;
        }

        var key = JsonUtils.Deserialize<DeveloperKey>(entry);
        if (key.KeyId.Length > 0) {
          keys.Add(key);
        }
      }

      return keys.ToArray();
    }

    /// <inheritdoc />
    public Task<DeveloperKey> DeactivateDeveloperKeyAsync(
          string keyId,
          CancellationToken cancellationToken = default) =>
          PutDeveloperKeyAsync(
                "v2/admin/developer-keys/deactivate",
                new Dictionary<string, object?> { ["key_id"] = CheckKeyId(keyId) },
                cancellationToken);

    /// <inheritdoc />
    public Task<DeveloperKey> DeactivateDeveloperKeyAsync(
          DeveloperKey key,
          CancellationToken cancellationToken = default) =>
          DeactivateDeveloperKeyAsync(CheckKey(key).KeyId, cancellationToken);

    /// <inheritdoc />
    public Task<DeveloperKey> RenameDeveloperKeyAsync(
          string keyId,
          string label,
          CancellationToken cancellationToken = default) =>
          PutDeveloperKeyAsync(
                "v2/admin/developer-keys/label",
                new Dictionary<string, object?> { ["key_id"] = CheckKeyId(keyId), ["label"] = label },
                cancellationToken);

    /// <inheritdoc />
    public Task<DeveloperKey> RenameDeveloperKeyAsync(
          DeveloperKey key,
          string label,
          CancellationToken cancellationToken = default) =>
          RenameDeveloperKeyAsync(CheckKey(key).KeyId, label, cancellationToken);

    /// <inheritdoc />
    public Task<DeveloperKey> SetDeveloperKeyUsageLimitsAsync(
          string keyId,
          DeveloperKeyUsageLimitsUpdate limits,
          CancellationToken cancellationToken = default) {
      var requestData = new Dictionary<string, object?> { ["key_id"] = CheckKeyId(keyId) };
      if (limits == null) {
        throw new ArgumentNullException(nameof(limits));
      }

      foreach (var limit in limits.AssignedLimits) {
        requestData[limit.Key] = limit.Value;
      }

      return PutDeveloperKeyAsync("v2/admin/developer-keys/limits", requestData, cancellationToken);
    }

    /// <inheritdoc />
    public Task<DeveloperKey> SetDeveloperKeyUsageLimitsAsync(
          DeveloperKey key,
          DeveloperKeyUsageLimitsUpdate limits,
          CancellationToken cancellationToken = default) =>
          SetDeveloperKeyUsageLimitsAsync(CheckKey(key).KeyId, limits, cancellationToken);

    /// <summary>Sends a JSON PUT request to a developer key endpoint and returns the resulting key.</summary>
    private async Task<DeveloperKey> PutDeveloperKeyAsync(
          string relativeUri,
          Dictionary<string, object?> requestData,
          CancellationToken cancellationToken) {
      using var responseMessage = await _client
            .ApiPutJsonAsync(relativeUri, cancellationToken, requestData).ConfigureAwait(false);
      await DeepLHttpClient.CheckStatusCodeAsync(responseMessage).ConfigureAwait(false);
      return await ReadKeyAsync(responseMessage).ConfigureAwait(false);
    }

    private static async Task<DeveloperKey> ReadKeyAsync(HttpResponseMessage responseMessage) {
      var element = await ReadElementAsync(responseMessage, JsonValueKind.Object, "Developer key")
            .ConfigureAwait(false);
      var key = JsonUtils.Deserialize<DeveloperKey>(element);
      return key.KeyId.Length > 0 ? key : throw new DeepLException("Developer key response is missing key_id");
    }

    /// <summary>Reads the JSON response; content of another top-level kind throws a <see cref="DeepLException" />.</summary>
    private static async Task<JsonElement> ReadElementAsync(
          HttpResponseMessage responseMessage,
          JsonValueKind kind,
          string what) {
      JsonElement element;
      try {
        element = await JsonUtils.DeserializeAsync<JsonElement>(responseMessage).ConfigureAwait(false);
      } catch (DeepLException exception) when (exception.InnerException is JsonException) {
        throw new DeepLException($"{what} response is not valid JSON", exception.InnerException);
      }

      return element.ValueKind == kind
            ? element
            : throw new DeepLException($"{what} response has an unexpected format");
    }

    private static DeveloperKey CheckKey(DeveloperKey key) => key ?? throw new ArgumentNullException(nameof(key));

    private static string CheckKeyId(string keyId) =>
          string.IsNullOrWhiteSpace(keyId)
                ? throw new ArgumentException($"Parameter {nameof(keyId)} must not be empty")
                : keyId;

    private const string DateFormat = "yyyy-MM-dd";

    private static List<(string Key, string Value)> CreateDateRangeParams(DateTime startDate, DateTime endDate) =>
          new List<(string Key, string Value)> {
            ("start_date", ToUtcDate(startDate)),
            ("end_date", ToUtcDate(endDate))
          };

    private static List<(string Key, string Value)> CreateDateRangeParams(string startDate, string endDate) =>
          new List<(string Key, string Value)> {
            ("start_date", CheckDate(startDate, nameof(startDate))),
            ("end_date", CheckDate(endDate, nameof(endDate)))
          };

    /// <summary>The UTC calendar date of a time; <see cref="DateTimeKind.Unspecified" /> counts as UTC.</summary>
    private static string ToUtcDate(DateTime value) =>
          (value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value)
          .ToString(DateFormat, CultureInfo.InvariantCulture);

    private static string CheckDate(string value, string paramName) {
      if (value == null) {
        throw new ArgumentNullException(paramName);
      }

      if (!DateTime.TryParseExact(
                value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)) {
        throw new ArgumentException($"Parameter {paramName} must be a date in the format yyyy-MM-dd", paramName);
      }

      return parsed.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>Class used for JSON-deserialization of <c>GET /v2/admin/analytics</c> results.</summary>
    private readonly struct UsageReportResult {
      /// <summary>Initializes a new instance of <see cref="UsageReportResult" />, used for JSON deserialization.</summary>
      [JsonConstructor]
      public UsageReportResult(AdminUsageReport? usageReport) {
        UsageReport = usageReport ?? new AdminUsageReport(null, null, null);
      }

      [JsonConverter(typeof(LenientObjectConverter<AdminUsageReport>))]
      public AdminUsageReport UsageReport { get; }
    }

    /// <summary>Class used for JSON-deserialization of <c>GET /v2/admin/analytics/custom-tags</c> results.</summary>
    private readonly struct CustomTagUsageReportResult {
      /// <summary>
      ///   Initializes a new instance of <see cref="CustomTagUsageReportResult" />, used for JSON deserialization.
      /// </summary>
      [JsonConstructor]
      public CustomTagUsageReportResult(CustomTagUsageReport? customTagUsageReport) {
        CustomTagUsageReport = customTagUsageReport ?? new CustomTagUsageReport(null, null, null, null, null);
      }

      [JsonConverter(typeof(LenientObjectConverter<CustomTagUsageReport>))]
      public CustomTagUsageReport CustomTagUsageReport { get; }
    }
  }
}
