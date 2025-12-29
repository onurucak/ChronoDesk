using System;
using System.IO;
using System.Windows;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Application.Services;
using ChronoDesk.Domain.Interfaces;
using ChronoDesk.Infrastructure.Persistence;
using ChronoDesk.Infrastructure.Persistence.Repositories;
using ChronoDesk.Infrastructure.Services;
using ChronoDesk.UI.ViewModels;
using ChronoDesk.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChronoDesk.UI;

public partial class App : System.Windows.Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure
        services.AddDbContext<ChronoDbContext>(options =>
            options.UseSqlite("Data Source=chronodesk.db"));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddSingleton<ICsvExportService, CsvExportService>();

        // Application
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITimerService, TimerService>();
        services.AddScoped<IReportService, ReportService>();

        // UI - ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<ProjectListViewModel>();
        services.AddTransient<TimerViewModel>();
        services.AddTransient<SummaryViewModel>();
        services.AddTransient<SettingsViewModel>();

        // UI - Views
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure database is created/migrated
        using (var scope = ServiceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ChronoDbContext>();
            await context.Database.MigrateAsync();
        }

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
