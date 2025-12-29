using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Interfaces;
using Microsoft.Win32;

namespace ChronoDesk.UI.ViewModels;

public class SummaryViewModel : ViewModelBase
{
    private readonly IReportService _reportService;
    private readonly ICsvExportService _csvExportService;
    private readonly ITimeEntryRepository _timeEntryRepository; // Direct access for export convenience

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

    private ObservableCollection<ProjectSummaryDto> _projectSummaries = new();
    public ObservableCollection<ProjectSummaryDto> ProjectSummaries
    {
        get => _projectSummaries;
        set
        {
            SetField(ref _projectSummaries, value);
            OnPropertyChanged(nameof(IsProjectSummariesEmpty));
        }
    }

    public bool IsProjectSummariesEmpty => ProjectSummaries.Count == 0;

    private ObservableCollection<TimeEntryDto> _recentSessions = new();
    public ObservableCollection<TimeEntryDto> RecentSessions
    {
        get => _recentSessions;
        set
        {
            SetField(ref _recentSessions, value);
            OnPropertyChanged(nameof(IsRecentSessionsEmpty));
        }
    }

    public bool IsRecentSessionsEmpty => RecentSessions.Count == 0;

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

    public SummaryViewModel(IReportService reportService, ICsvExportService csvExportService, ITimeEntryRepository timeEntryRepository)
    {
        _reportService = reportService;
        _csvExportService = csvExportService;
        _timeEntryRepository = timeEntryRepository;

        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
        ExportCommand = new RelayCommand(async _ => await ExportCsvAsync());

        LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            var summaries = await _reportService.GetProjectSummariesAsync(StartDate, EndDate);
            if (summaries != null)
            {
                ProjectSummaries = new ObservableCollection<ProjectSummaryDto>(summaries);
            }
            else
            {
                ProjectSummaries = new ObservableCollection<ProjectSummaryDto>();
            }

            var recents = await _reportService.GetRecentEntriesAsync(10);
            if (recents != null)
            {
                RecentSessions = new ObservableCollection<TimeEntryDto>(recents);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load summary data: {ex.Message}";
        }
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
}
