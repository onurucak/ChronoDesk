using System.Collections.Generic;
using System.Threading.Tasks;
using ChronoDesk.Domain.Entities;

namespace ChronoDesk.Application.Interfaces;

public interface ICsvExportService
{
    Task ExportAsync(IEnumerable<TimeEntry> timeEntries, string filePath);
}
