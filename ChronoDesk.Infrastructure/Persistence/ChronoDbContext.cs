using ChronoDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChronoDesk.Infrastructure.Persistence;

public class ChronoDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }

    public ChronoDbContext(DbContextOptions<ChronoDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=chronodesk.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project
        modelBuilder.Entity<Project>()
            .Property(p => p.Name)
            .IsRequired();

        // TimeEntry
        modelBuilder.Entity<TimeEntry>()
            .HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
