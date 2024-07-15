using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Testing.HealthReport;

internal record HealthDataItem(string Service, DateTimeOffset Date, HealthStatus Status);