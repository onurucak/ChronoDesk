using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Entities;

namespace ChronoDesk.UI.ViewModels;

public class TimerViewModel : ViewModelBase
{
    private readonly ITimerService _timerService;
    private readonly IProjectService _projectService;
    private readonly DispatcherTimer _uiTimer;

    private ObservableCollection<Project> _projects = new();
    public ObservableCollection<Project> Projects
    {
        get => _projects;
        set => SetField(ref _projects, value);
    }

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

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand UpdateNotesCommand { get; }

    public TimerViewModel(ITimerService timerService, IProjectService projectService)
    {
        _timerService = timerService;
        _projectService = projectService;

        StartCommand = new RelayCommand(async _ => await StartTimerAsync(), _ => !IsTimerRunning && SelectedProject != null);
        StopCommand = new RelayCommand(async _ => await StopTimerAsync(), _ => IsTimerRunning);
        UpdateNotesCommand = new RelayCommand(async _ => await UpdateNotesAsync());

        _uiTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _uiTimer.Tick += UiTimer_Tick;

        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        Projects = new ObservableCollection<Project>(projects.Where(p => !p.IsArchived));

        var active = await _timerService.GetCurrentTimerAsync();
        if (active != null)
        {
            IsTimerRunning = true;
            SelectedProject = Projects.FirstOrDefault(p => p.Id == active.ProjectId);
            CurrentNotes = active.Notes;
            _uiTimer.Start();
        }
    }

    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        // We technically should re-fetch start time to be accurate or cache it
        // For simple display, we can ask service or just calc here if we stored start time.
        // Better: store StartTime locally in VM when found/started.
        UpdateDurationDisplay();
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
    }

    private async Task StopTimerAsync()
    {
        await _timerService.StopTimerAsync();
        IsTimerRunning = false;
        _uiTimer.Stop();
        CurrentDuration = "00:00:00";
        CurrentNotes = string.Empty;
    }

    private async Task UpdateNotesAsync()
    {
        await _timerService.UpdateCurrentTimerAsync(CurrentNotes);
    }
}
