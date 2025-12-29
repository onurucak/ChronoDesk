using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Entities;

namespace ChronoDesk.Infrastructure.Services;

public class CsvExportService : ICsvExportService
{
    public async Task ExportAsync(IEnumerable<TimeEntry> timeEntries, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Project,Date,Start,End,Duration,Notes");

        foreach (var entry in timeEntries)
        {
            var project = entry.Project?.Name ?? "Unknown";
            var date = entry.StartTime.ToLocalTime().ToString("yyyy-MM-dd");
            var start = entry.StartTime.ToLocalTime().ToString("HH:mm:ss");
            var end = entry.EndTime.HasValue ? entry.EndTime.Value.ToLocalTime().ToString("HH:mm:ss") : "Running";
            var duration = entry.Duration.ToString(@"hh\:mm\:ss");
            var notes = entry.Notes?.Replace(",", " ") ?? ""; // Simple CSV escape

            sb.AppendLine($"{project},{date},{start},{end},{duration},{notes}");
        }

        await File.WriteAllTextAsync(filePath, sb.ToString());
    }
}
