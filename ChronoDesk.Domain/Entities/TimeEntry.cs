using System;

namespace ChronoDesk.Domain.Entities;

public class TimeEntry : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Notes { get; set; } = string.Empty;

    public TimeSpan Duration
    {
        get
        {
            if (EndTime.HasValue)
            {
                return EndTime.Value - StartTime;
            }
            return DateTime.UtcNow - StartTime;
        }
    }
}
