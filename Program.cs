// Print health report for past 14 days for each particular day, use IDateTimeProvider to get current date
// Format: {ServiceName} {Date} {Uptime} {UptimePercent} {UnhealthyPercent} {DegradedPercent}
// Consider health data could be unavailable, for example monitoring started 1 day ago, in that case display Unavailable for periods preceding

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Testing.HealthReport;

IDateTimeProvider dateProvider = new DateTimeProvider();
var healthData = new List<HealthDataItem>()
{
    new ("Service1", DateTimeOffset.Parse("2023-07-01 05:50:34 +03:00"), HealthStatus.Healthy),
    new ("Service1", DateTimeOffset.Parse("2023-07-02 05:50:34 +03:00"), HealthStatus.Unhealthy),
    new ("Service1", DateTimeOffset.Parse("2023-07-09 05:50:34 +03:00"), HealthStatus.Healthy),
    new ("Service1", DateTimeOffset.Parse("2023-07-10 03:50:34 +03:00"), HealthStatus.Degraded),
    new ("Service1", DateTimeOffset.Parse("2023-07-10 03:55:04 +03:00"), HealthStatus.Healthy),
    new ("Service1", DateTimeOffset.Parse("2023-07-11 03:55:04 +03:00"), HealthStatus.Unhealthy),
    new ("Service1", DateTimeOffset.Parse("2023-07-11 04:15:04 +03:00"), HealthStatus.Healthy)
};

const int daysToReport = 14;
const string service = "Service1";

DateTime reportStartDate = dateProvider.OffsetNow.AddDays(1 - daysToReport).Date;
DateTime reportEndDate = dateProvider.OffsetNow.Date;

// Creating a health status record for each day to report
healthData = healthData.Where(x => x.Service == service).ToList();
var dataOnDay = new List<HealthDataItem>[daysToReport];
for (int i = 0; i < daysToReport; i++)
    dataOnDay[i] = new ();
for (int i = 0; i < healthData.Count; i++)
{
    var dataItem = healthData[i];
    DateTime startDate = dataItem.Date.Date;
    DateTime endDate = (i + 1 == healthData.Count) ? reportEndDate : healthData[i + 1].Date.Date;
    int startDay = (int)(startDate - reportStartDate).TotalDays;
    int endDay = (int)(endDate - reportStartDate).TotalDays;
    for (int day = startDay; day <= endDay; day++)
        if (day >= 0 && day < daysToReport)
            dataOnDay[day].Add(dataItem);
}

// Combining daily records into logs
var log = new(DateTime, TimeSpan?, double?, double?, double?)[daysToReport];
for (int day = 0; day < daysToReport; day++)
{
    DateTime dayStart = reportStartDate.AddDays(day).Date;
    DateTime dayEnd = reportStartDate.AddDays(day + 1).Date;
    int daySeconds = (int)(dayEnd - dayStart).TotalSeconds;

    var dataEntries = dataOnDay[day];
    if (dataEntries.Count == 0)
    {
        log[day] = (dayStart, null, null, null, null);
        continue;
    }

    DateTime date = dayStart;
    TimeSpan uptime = TimeSpan.Zero;
    TimeSpan unhealthyTime = TimeSpan.Zero;
    TimeSpan degradedTime = TimeSpan.Zero;

    for (int i = 0; i < dataEntries.Count; i++)
    {
        var dataEntry = dataEntries[i];
        var timeSpan = (i + 1 == dataEntries.Count ? dayEnd : dataEntries[i + 1].Date) - (dataEntry.Date < dayStart ? dayStart : dataEntry.Date);
        switch (dataEntry.Status)
        {
            case HealthStatus.Unhealthy:
                unhealthyTime += timeSpan;
                break;
            case HealthStatus.Degraded:
                degradedTime += timeSpan;
                break;
            default:
                uptime += timeSpan;
                break;
        }
    }

    double unhealthyPercent = unhealthyTime.TotalSeconds / daySeconds;
    double degradedPercent = degradedTime.TotalSeconds / daySeconds;
    double uptimePercent = uptime.TotalSeconds / daySeconds;

    log[day] = (date, uptime, uptimePercent, unhealthyPercent, degradedPercent);
}

// Printing out the logs
Console.WriteLine($"Report for past {daysToReport} days for {service}");
for (int day = 0; day < daysToReport; day++)
{
    var entry = log[day];
    Console.WriteLine($"{service} {entry.Item1.ToShortDateString()} " + (entry.Item2 != null ? $"{(int)((TimeSpan)entry.Item2).TotalHours + ((TimeSpan)entry.Item2).ToString(@"\:mm\:ss")} {entry.Item3:0.00%} {entry.Item4:0.00%} {entry.Item5:0.00%}" : "Unavailable"));
}