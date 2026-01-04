using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Interfaces;
using Microsoft.Win32;

using ChronoDesk.UI.Views;

namespace ChronoDesk.UI.ViewModels;

public class SummaryViewModel : ViewModelBase
{
    private readonly IReportService _reportService;
    private readonly ICsvExportService _csvExportService;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ChronoDesk.Application.Interfaces.IProjectService _projectService;

    private DateTime _startDate = DateTime.Today.AddDays(-7);
    public DateTime StartDate
    {
        get => _startDate;
        set => SetField(ref _startDate, value);
    }

    private DateTime _endDate = DateTime.Today.AddDays(1).AddTicks(-1);
    public DateTime EndDate
    {
        get => _endDate;
        set => SetField(ref _endDate, value);
    }

    private ObservableCollection<DailyProjectSummary> _projectBreakdown = new();
    public ObservableCollection<DailyProjectSummary> ProjectBreakdown
    {
        get => _projectBreakdown;
        set
        {
            SetField(ref _projectBreakdown, value);
            OnPropertyChanged(nameof(IsProjectBreakdownEmpty));
        }
    }

    public bool IsProjectBreakdownEmpty => ProjectBreakdown.Count == 0;

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            SetField(ref _errorMessage, value);
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ICommand RefreshCommand { get; }
    public ICommand ExportCommand { get; }

    // Removed DeleteSessionCommand, EditSessionCommand as they were for Recent Sessions

    public SummaryViewModel(
        IReportService reportService, 
        ICsvExportService csvExportService, 
        ITimeEntryRepository timeEntryRepository,
        ChronoDesk.Application.Interfaces.IProjectService projectService)
    {
        _reportService = reportService;
        _csvExportService = csvExportService;
        _timeEntryRepository = timeEntryRepository;
        _projectService = projectService;

        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
        ExportCommand = new RelayCommand(async _ => await ExportCsvAsync());

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            
            // Fetch all entries for the range
            var entries = await _timeEntryRepository.FindAsync(t => t.StartTime >= StartDate && t.StartTime <= EndDate);
            
            if (entries == null || !entries.Any())
            {
                ProjectBreakdown = new ObservableCollection<DailyProjectSummary>();
                return;
            }

            var dailyGroups = entries
                .GroupBy(t => t.StartTime.Date)
                .OrderByDescending(g => g.Key)
                .Select(dayGroup => 
                {
                   var dayProjects = dayGroup
                       .GroupBy(t => t.ProjectId)
                       .Select(projGroup => new ProjectSummaryDto
                       {
                           // Assuming we can get ProjectName here or need to join. 
                           // Repository entities might have Project navigation property loaded? 
                           // If not, we might need to lookup project names. 
                           // For now assuming Project property is loaded or we use a separate lookup if needed.
                           // Actually _timeEntryRepository.FindAsync returns entities. 
                           // If they have Project navigation prop populated, good.
                           // If not, we use the ProjectService.
                           ProjectName = projGroup.First().Project?.Name ?? "Unknown Project", 
                           TotalDuration = TimeSpan.FromTicks(projGroup.Sum(t => (t.EndTime ?? DateTime.Now).Ticks - t.StartTime.Ticks))
                       })
                       .OrderByDescending(p => p.TotalDuration)
                       .ToList();

                   return new DailyProjectSummary
                   {
                       Date = dayGroup.Key,
                       TotalDuration = TimeSpan.FromTicks(dayGroup.Sum(t => (t.EndTime ?? DateTime.Now).Ticks - t.StartTime.Ticks)),
                       Projects = dayProjects
                   };
                });

            ProjectBreakdown = new ObservableCollection<DailyProjectSummary>(dailyGroups);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load summary data: {ex.Message}";
        }
    }

    public class DailyProjectSummary
    {
        public DateTime Date { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public System.Collections.Generic.List<ProjectSummaryDto> Projects { get; set; } = new();
    }

    private async Task ExportCsvAsync()
    {
        try
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"ChronoDesk_Export_{DateTime.Now:yyyyMMdd}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var entries = await _timeEntryRepository.FindAsync(t => t.StartTime >= StartDate && t.StartTime <= EndDate);
                await _csvExportService.ExportAsync(entries, saveFileDialog.FileName);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to export data: {ex.Message}";
        }
    }

    private async Task DeleteSessionAsync(object? parameter)
    {
        if (parameter is not TimeEntryDto entry) return;

        if (entry.EndTime == null)
        {
            ErrorMessage = "Cannot delete an active session. Please stop the timer first.";
            return;
        }

        try
        {
            await _timeEntryRepository.DeleteAsync(entry.Id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete session: {ex.Message}";
        }
    }

    private async Task EditSessionAsync(object? parameter)
    {
        if (parameter is not TimeEntryDto dto) return;

        try
        {
            var entry = await _timeEntryRepository.GetByIdAsync(dto.Id);
            if (entry == null) return;

            var projects = await _projectService.GetAllProjectsAsync();

            var vm = new EditSessionViewModel(entry, projects);
            var window = new Views.EditSessionWindow
            {
                DataContext = vm,
                Owner = System.Windows.Application.Current.MainWindow 
            };

            if (window.ShowDialog() == true)
            {
                await _timeEntryRepository.UpdateAsync(entry);
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to edit session: {ex.Message}";
        }
    }
}
