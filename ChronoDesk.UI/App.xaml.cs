using System;
using System.IO;
using System.Threading;
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
using ChronoDesk.UI.Services;

namespace ChronoDesk.UI;

public partial class App : System.Windows.Application
{
    private static Mutex? _mutex;
    private const string AppGuid = "2F6E1BD6-31E6-48EA-AF26-549BB8FB7227"; // Unique identifier for ChronoDesk

    public IServiceProvider ServiceProvider { get; private set; } = null!;

    public App()
    {
        // Single Instance Check
        _mutex = new Mutex(true, AppGuid, out bool createdNew);
        if (!createdNew)
        {
            // Another instance is already running
            System.Windows.MessageBox.Show("ChronoDesk is already running.", "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
            System.Windows.Application.Current.Shutdown();
            return;
        }

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
        services.AddSingleton<ITimerService, TimerService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddSingleton<IDataMaintenanceService, DataMaintenanceService>();

        // UI - Services
        services.AddSingleton<ProjectStore>();
        services.AddSingleton<ISettingsService, SettingsService>();

        // UI - ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<ProjectListViewModel>();
        services.AddSingleton<TimerViewModel>();
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

        // Initialize Stores/Settings
        var settings = ServiceProvider.GetRequiredService<ISettingsService>();
        await settings.LoadSettingsAsync();

        var projectStore = ServiceProvider.GetRequiredService<ProjectStore>();
        await projectStore.LoadProjectsAsync();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
