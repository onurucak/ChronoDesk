using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Entities;
using ChronoDesk.UI.Services;

namespace ChronoDesk.UI.ViewModels;

public class TimerViewModel : ViewModelBase
{
    private readonly ITimerService _timerService;
    private readonly IProjectService _projectService;
    private readonly ProjectStore _projectStore;
    private readonly ISettingsService _settingsService;
    private readonly IReportService _reportService;
    private readonly ChronoDesk.Domain.Interfaces.ITimeEntryRepository _timeEntryRepository;
    private readonly DispatcherTimer _uiTimer;
    private int _autoSaveCounter;

    public ObservableCollection<Project> Projects => _projectStore.Projects;
    
    private ObservableCollection<TimeEntryDto> _recentSessions = new();
    public ObservableCollection<TimeEntryDto> RecentSessions
    {
        get => _recentSessions;
        set => SetField(ref _recentSessions, value);
    }
    
    public bool IsRecentSessionsEmpty => RecentSessions.Count == 0;

    private Project? _selectedProject;
    public Project? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (SetField(ref _selectedProject, value))
            {
               ((RelayCommand)StartCommand).RaiseCanExecuteChanged(); 
            }
        }
    }

    private string _currentDuration = "00:00:00";
    public string CurrentDuration
    {
        get => _currentDuration;
        set => SetField(ref _currentDuration, value);
    }

    private string _currentNotes = string.Empty;
    public string CurrentNotes
    {
        get => _currentNotes;
        set => SetField(ref _currentNotes, value);
    }

    private bool _isTimerRunning;
    public bool IsTimerRunning
    {
        get => _isTimerRunning;
        set
        {
            SetField(ref _isTimerRunning, value);
            ((RelayCommand)StartCommand).RaiseCanExecuteChanged();
            ((RelayCommand)StopCommand).RaiseCanExecuteChanged();
        }
    }
    
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

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand UpdateNotesCommand { get; }
    public ICommand DeleteSessionCommand { get; }
    public ICommand EditSessionCommand { get; }

    public TimerViewModel(
        ITimerService timerService, 
        IProjectService projectService, 
        ProjectStore projectStore, 
        ISettingsService settingsService,
        IReportService reportService,
        ChronoDesk.Domain.Interfaces.ITimeEntryRepository timeEntryRepository)
    {
        _timerService = timerService;
        _projectService = projectService;
        _projectStore = projectStore;
        _settingsService = settingsService;
        _reportService = reportService;
        _timeEntryRepository = timeEntryRepository;

        StartCommand = new RelayCommand(async _ => await StartTimerAsync(), _ => !IsTimerRunning && SelectedProject != null);
        StopCommand = new RelayCommand(async _ => await StopTimerAsync(), _ => IsTimerRunning);
        UpdateNotesCommand = new RelayCommand(async _ => await UpdateNotesAsync());
        DeleteSessionCommand = new RelayCommand(async parameter => await DeleteSessionAsync(parameter));
        EditSessionCommand = new RelayCommand(async parameter => await EditSessionAsync(parameter));

        _uiTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _uiTimer.Tick += UiTimer_Tick;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        // Projects loaded by App.xaml.cs via Store, so we just check active timer
        // We might need to ensure SelectedProject is set if running
        var active = await _timerService.GetCurrentTimerAsync();
        if (active != null)
        {
            IsTimerRunning = true;
            SelectedProject = Projects.FirstOrDefault(p => p.Id == active.ProjectId);
            CurrentNotes = active.Notes;
            _uiTimer.Start();
        }
        
        await LoadRecentSessionsAsync();
    }
    
    private async Task LoadRecentSessionsAsync()
    {
        try
        {
             var recents = await _reportService.GetRecentEntriesAsync(20); // Fetch a bit more for the main list
            if (recents != null)
            {
                RecentSessions = new ObservableCollection<TimeEntryDto>(recents.Where(x => x.EndTime != null));
                OnPropertyChanged(nameof(IsRecentSessionsEmpty));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load recent sessions: {ex.Message}";
        }
    }

    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        UpdateDurationDisplay();
        
        if (_settingsService.IsAutoSaveEnabled)
        {
            _autoSaveCounter++;
            if (_autoSaveCounter >= 30) // Auto-save every 30 seconds
            {
                _ = UpdateNotesAsync(); // Fire and forget update
                _autoSaveCounter = 0;
            }
        }
    }

    private async void UpdateDurationDisplay()
    {
        var active = await _timerService.GetCurrentTimerAsync();
        if (active != null)
        {
            CurrentDuration = active.Duration.ToString(@"hh\:mm\:ss");
        }
    }

    private async Task StartTimerAsync()
    {
        if (SelectedProject == null) return;

        await _timerService.StartTimerAsync(SelectedProject.Id);
        IsTimerRunning = true;
        _uiTimer.Start();
        UpdateDurationDisplay();
        
        // Refresh list immediately (though active isn't usually in history until stopped, unless we want to show active too)
        // Usually recent list shows *completed* or *historical* sessions + active at top. 
        // Our GetRecentEntriesAsync implementation probably fetches all. 
        // Let's refresh just in case.
        await LoadRecentSessionsAsync();
    }

    private async Task StopTimerAsync()
    {
        // Ensure notes are saved before stopping
        await UpdateNotesAsync();

        await _timerService.StopTimerAsync();
        IsTimerRunning = false;
        _uiTimer.Stop();
        CurrentDuration = "00:00:00";
        CurrentNotes = string.Empty;
        
        // Refresh list to show the newly stopped session
        await LoadRecentSessionsAsync();
    }

    private async Task UpdateNotesAsync()
    {
        await _timerService.UpdateCurrentTimerAsync(CurrentNotes);
    }
    
    private async Task DeleteSessionAsync(object? parameter)
    {
        if (parameter is not TimeEntryDto entry) return;

        if (entry.EndTime == null)
        {
            ErrorMessage = "Cannot delete an active session. Please stop the timer first.";
            return;
        }

        var result = System.Windows.MessageBox.Show(
            "Are you sure you want to delete this session?", 
            "Confirm Delete", 
            System.Windows.MessageBoxButton.YesNo, 
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _timeEntryRepository.DeleteAsync(entry.Id);
            await LoadRecentSessionsAsync();
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

            // Simple re-fetch projects just in case needed or rely on Store
            // Store has them, but VM might need fresh list. We used service in SummaryViewModel.
            // Let's use _projectStore directly or service. Using service is cleaner for "fresh" data but store is cached.
            // SummaryViewModel used _projectService.GetAllProjectsAsync(). Let's stick to that pattern or use Store if valid.
            // Using Store.Projects might be easier since it's already here.
            
            var vm = new EditSessionViewModel(entry, Projects); // Using cached projects from Store
            var window = new Views.EditSessionWindow
            {
                DataContext = vm,
                Owner = System.Windows.Application.Current.MainWindow 
            };

            if (window.ShowDialog() == true)
            {
                await _timeEntryRepository.UpdateAsync(entry);
                await LoadRecentSessionsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to edit session: {ex.Message}";
        }
    }
}
