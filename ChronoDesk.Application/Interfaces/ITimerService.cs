using System;
using System.Threading.Tasks;
using ChronoDesk.Domain.Entities;

namespace ChronoDesk.Application.Interfaces;

public interface ITimerService
{
    // Returns the currently running entry if any
    Task<TimeEntry?> GetCurrentTimerAsync();
    
    // Starts a new timer for project (stops existing if any)
    Task<TimeEntry> StartTimerAsync(Guid projectId);
    
    // Stops the current timer
    Task StopTimerAsync();
    
    // Updates the current timer (e.g. notes)
    Task UpdateCurrentTimerAsync(string notes);
    
    // Manual entry
    Task AddManualEntryAsync(Guid projectId, DateTime start, DateTime end, string notes);
}
