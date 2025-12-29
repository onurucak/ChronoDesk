using System;
using System.Threading.Tasks;
using ChronoDesk.Domain.Entities;

namespace ChronoDesk.Domain.Interfaces;

public interface ITimeEntryRepository : IRepository<TimeEntry>
{
    Task<TimeEntry?> GetActiveTimeEntryAsync();
}
