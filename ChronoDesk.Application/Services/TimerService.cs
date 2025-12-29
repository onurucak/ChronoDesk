using System;
using System.Threading.Tasks;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Entities;
using ChronoDesk.Domain.Interfaces;

namespace ChronoDesk.Application.Services;

public class TimerService : ITimerService
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    
    public TimerService(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<TimeEntry?> GetCurrentTimerAsync()
    {
        return await _timeEntryRepository.GetActiveTimeEntryAsync();
    }

    public async Task<TimeEntry> StartTimerAsync(Guid projectId)
    {
        // Stop any running timer first
        await StopTimerAsync();

        var newEntry = new TimeEntry
        {
            ProjectId = projectId,
            StartTime = DateTime.UtcNow,
            EndTime = null
        };

        await _timeEntryRepository.AddAsync(newEntry);
        return newEntry;
    }

    public async Task StopTimerAsync()
    {
        var active = await _timeEntryRepository.GetActiveTimeEntryAsync();
        if (active != null)
        {
            active.EndTime = DateTime.UtcNow;
            active.UpdatedAt = DateTime.UtcNow;
            await _timeEntryRepository.UpdateAsync(active);
        }
    }

    public async Task UpdateCurrentTimerAsync(string notes)
    {
        var active = await _timeEntryRepository.GetActiveTimeEntryAsync();
        if (active != null)
        {
            active.Notes = notes;
            active.UpdatedAt = DateTime.UtcNow;
            await _timeEntryRepository.UpdateAsync(active);
        }
    }

    public async Task AddManualEntryAsync(Guid projectId, DateTime start, DateTime end, string notes)
    {
        var entry = new TimeEntry
        {
            ProjectId = projectId,
            StartTime = start,
            EndTime = end,
            Notes = notes
        };
        await _timeEntryRepository.AddAsync(entry);
    }
}
