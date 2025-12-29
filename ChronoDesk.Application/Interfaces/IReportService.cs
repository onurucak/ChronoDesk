using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChronoDesk.Application.Interfaces;

public interface IReportService
{
    Task<IEnumerable<ProjectSummaryDto>> GetProjectSummariesAsync(DateTime start, DateTime end);
    Task<IEnumerable<DailySummaryDto>> GetDailySummariesAsync(DateTime start, DateTime end);
}

public class ProjectSummaryDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
}

public class DailySummaryDto
{
    public DateTime Date { get; set; }
    public TimeSpan TotalDuration { get; set; }
}
