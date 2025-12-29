using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChronoDesk.Infrastructure.Persistence;

public class ChronoDbContextFactory : IDesignTimeDbContextFactory<ChronoDbContext>
{
    public ChronoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChronoDbContext>();
        optionsBuilder.UseSqlite("Data Source=chronodesk.db");

        return new ChronoDbContext(optionsBuilder.Options);
    }
}
