using System.Threading.Tasks;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoDesk.Infrastructure.Services;

public class DataMaintenanceService : IDataMaintenanceService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DataMaintenanceService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ClearAllDataAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChronoDbContext>();

        // Execute raw SQL delete for efficiency and to bypass potential cascade issues if any,
        // though EF Core RemoveRange is also fine. We'll use EF Core for safety.
        
        var timeEntries = await context.TimeEntries.ToListAsync();
        context.TimeEntries.RemoveRange(timeEntries);
        
        var projects = await context.Projects.ToListAsync();
        context.Projects.RemoveRange(projects);

        await context.SaveChangesAsync();
    }
}
