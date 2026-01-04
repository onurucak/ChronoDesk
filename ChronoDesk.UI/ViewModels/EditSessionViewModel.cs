using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ChronoDesk.Domain.Entities;

namespace ChronoDesk.UI.ViewModels;

public class EditSessionViewModel : ViewModelBase
{
    private readonly TimeEntry _originalEntry;

    public ObservableCollection<Project> Projects { get; }

    private Project? _selectedProject;
    public Project? SelectedProject
    {
        get => _selectedProject;
        set => SetField(ref _selectedProject, value);
    }

    private DateTime _startDate;
    public DateTime StartDate
    {
        get => _startDate;
        set => SetField(ref _startDate, value);
    }

    private string _startTimeStr = string.Empty;
    public string StartTimeStr
    {
        get => _startTimeStr;
        set => SetField(ref _startTimeStr, value);
    }

    private DateTime? _endDate;
    public DateTime? EndDate
    {
        get => _endDate;
        set => SetField(ref _endDate, value);
    }

    private string _endTimeStr = string.Empty;
    public string EndTimeStr
    {
        get => _endTimeStr;
        set => SetField(ref _endTimeStr, value);
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set => SetField(ref _notes, value);
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

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action<bool>? RequestClose;

    public EditSessionViewModel(TimeEntry entry, IEnumerable<Project> projects)
    {
        _originalEntry = entry;
        Projects = new ObservableCollection<Project>(projects);

        // Initialize fields
        _selectedProject = Projects.FirstOrDefault(p => p.Id == entry.ProjectId);
        
        var localStart = entry.StartTime.ToLocalTime();
        _startDate = localStart.Date;
        _startTimeStr = localStart.ToString("HH:mm");

        if (entry.EndTime.HasValue)
        {
            var localEnd = entry.EndTime.Value.ToLocalTime();
            _endDate = localEnd.Date;
            _endTimeStr = localEnd.ToString("HH:mm");
        }
        else
        {
            _endDate = null; // or DateTime.Today if we want defaults, but null is safer for "active" logic
            _endTimeStr = string.Empty;
        }

        _notes = entry.Notes;

        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    private void Save()
    {
        ErrorMessage = string.Empty;

        if (SelectedProject == null)
        {
            ErrorMessage = "Please select a project.";
            return; 
        }

        // Parse Start Time
        if (!TimeSpan.TryParse(StartTimeStr, out var startTimeSpan))
        {
            ErrorMessage = "Invalid Start Time format (use HH:mm).";
            return;
        }
        var startDateTime = StartDate.Date + startTimeSpan;

        // Parse End Time
        DateTime? endDateTime = null;
        if (!string.IsNullOrWhiteSpace(EndTimeStr))
        {
            if (EndDate == null)
            {
                ErrorMessage = "Please select an End Date if End Time is provided.";
                return;
            }

            if (!TimeSpan.TryParse(EndTimeStr, out var endTimeSpan))
            {
                ErrorMessage = "Invalid End Time format (use HH:mm).";
                return;
            }
            endDateTime = EndDate.Value.Date + endTimeSpan;

            if (endDateTime < startDateTime)
            {
                ErrorMessage = "End Time cannot be before Start Time.";
                return;
            }
        }

        // Update original entry
        _originalEntry.ProjectId = SelectedProject.Id;
        _originalEntry.Project = SelectedProject; 
        
        // Convert to UTC before saving (assuming repository expects UTC)
        _originalEntry.StartTime = startDateTime.ToUniversalTime();
        _originalEntry.EndTime = endDateTime?.ToUniversalTime();
        _originalEntry.Notes = Notes;

        RequestClose?.Invoke(true);
    }

    private void Cancel()
    {
        RequestClose?.Invoke(false);
    }
}
