// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;

namespace DeepL {
  /// <summary>Options for grouping the usage in <see cref="IAdminManager.GetUsageAnalyticsAsync" />.</summary>
  public enum AdminUsageGroupBy {
    /// <summary>Group usage by API key.</summary>
    Key,

    /// <summary>Group usage by API key and usage date.</summary>
    KeyAndDay
  }

  /// <summary>Options for aggregating the usage in <see cref="IAdminManager.GetCustomTagUsageAnalyticsAsync" />.</summary>
  public enum CustomTagUsageAggregateBy {
    /// <summary>Aggregate usage over the entire date range.</summary>
    Period,

    /// <summary>Aggregate usage by individual day.</summary>
    Day
  }

  internal static class AdminUsageEnumExtensions {
    internal static string ToApiValue(this AdminUsageGroupBy groupBy) =>
          groupBy switch {
            AdminUsageGroupBy.Key => "key",
            AdminUsageGroupBy.KeyAndDay => "key_and_day",
            _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, "Unrecognized group by value")
          };

    internal static string ToApiValue(this CustomTagUsageAggregateBy aggregateBy) =>
          aggregateBy switch {
            CustomTagUsageAggregateBy.Period => "period",
            CustomTagUsageAggregateBy.Day => "day",
            _ => throw new ArgumentOutOfRangeException(
                  nameof(aggregateBy),
                  aggregateBy,
                  "Unrecognized aggregate by value")
          };
  }
}
