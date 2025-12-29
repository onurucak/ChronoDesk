using System.Linq;
using System.Threading.Tasks;
using ChronoDesk.Domain.Entities;
using ChronoDesk.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoDesk.Infrastructure.Persistence.Repositories;

public class TimeEntryRepository : Repository<TimeEntry>, ITimeEntryRepository
{
    public TimeEntryRepository(ChronoDbContext context) : base(context)
    {
    }

    public async Task<TimeEntry?> GetActiveTimeEntryAsync()
    {
        return await _dbSet
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.EndTime == null);
    }
}
