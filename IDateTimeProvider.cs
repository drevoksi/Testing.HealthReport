namespace Testing.HealthReport;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTimeOffset OffsetNow { get; }
}


public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => new (2023, 07, 12, 10, 30, 1);
    public DateTimeOffset OffsetNow => new (Now);
}