using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Interfaces;


namespace ChronoDesk.Application.Services;

public class ReportService : IReportService
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IProjectRepository _projectRepository;

    public ReportService(ITimeEntryRepository timeEntryRepository, IProjectRepository projectRepository)
    {
        _timeEntryRepository = timeEntryRepository;
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<ProjectSummaryDto>> GetProjectSummariesAsync(DateTime start, DateTime end)
    {
        // Ideally handled via more complex query in repo, but for simplicity/portability:
        // Fetch all in range
        var entries = await _timeEntryRepository.FindAsync(t => t.StartTime >= start && t.StartTime <= end);
        
        // Group by Project
        var grouped = entries.GroupBy(t => t.ProjectId);
        
        var summaries = new List<ProjectSummaryDto>();
        foreach (var group in grouped)
        {
            var project = await _projectRepository.GetByIdAsync(group.Key);
            var totalDuration = TimeSpan.Zero;
            foreach(var item in group)
            {
                totalDuration += item.Duration;
            }

            summaries.Add(new ProjectSummaryDto
            {
                ProjectId = group.Key,
                ProjectName = project?.Name ?? "Unknown Project",
                TotalDuration = totalDuration
            });
        }

        return summaries;
    }

    public async Task<IEnumerable<DailySummaryDto>> GetDailySummariesAsync(DateTime start, DateTime end)
    {
        var entries = await _timeEntryRepository.FindAsync(t => t.StartTime >= start && t.StartTime <= end);
        
        return entries
            .GroupBy(t => t.StartTime.Date)
            .Select(g => new DailySummaryDto
            {
                Date = g.Key,
                TotalDuration = TimeSpan.FromTicks(g.Sum(t => t.Duration.Ticks))
            })
            .OrderBy(d => d.Date);
    }

    public async Task<IEnumerable<TimeEntryDto>> GetRecentEntriesAsync(int limit = 10)
    {
        // Simple fetch all and take last N (efficient enough for local SQLite)
        // Ideally repo should support Take/Skip/OrderBy
        var all = await _timeEntryRepository.GetAllAsync(); 
        
        var recent = all.OrderByDescending(t => t.StartTime).Take(limit);
        
        var dtos = new List<TimeEntryDto>();
        foreach(var item in recent)
        {
            var project = await _projectRepository.GetByIdAsync(item.ProjectId);
            dtos.Add(new TimeEntryDto
            {
                Id = item.Id,
                ProjectName = project?.Name ?? "Unknown",
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                Duration = item.Duration,
                Notes = item.Notes
            });
        }
        return dtos;
    }
}
