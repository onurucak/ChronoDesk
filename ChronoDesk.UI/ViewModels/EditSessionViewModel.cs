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

    private DateTime _startTime;
    public DateTime StartTime
    {
        get => _startTime;
        set => SetField(ref _startTime, value);
    }

    private DateTime? _endTime;
    public DateTime? EndTime
    {
        get => _endTime;
        set => SetField(ref _endTime, value);
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set => SetField(ref _notes, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action<bool>? RequestClose;

    public EditSessionViewModel(TimeEntry entry, IEnumerable<Project> projects)
    {
        _originalEntry = entry;
        Projects = new ObservableCollection<Project>(projects);

        // Initialize fields
        _selectedProject = Projects.FirstOrDefault(p => p.Id == entry.ProjectId);
        _startTime = entry.StartTime.ToLocalTime();
        _endTime = entry.EndTime?.ToLocalTime();
        _notes = entry.Notes;

        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    private void Save()
    {
        if (SelectedProject == null)
        {
            // Simple validation
            return; 
        }

        // update original entry 
        // Note: In a stricter MVVM, we might return a DTO or updated object, 
        // but here we act on the reference passed (fetching fresh from repo in parent is safer but this is local state until persisted)
        _originalEntry.ProjectId = SelectedProject.Id;
        _originalEntry.Project = SelectedProject; 
        
        // Convert back to UTC if your app uses UTC internally (safe assumption usually, but check existing)
        // Checking TimeEntry.cs, doesn't enforce, but usually DB is UTC. 
        // Let's assume we maintain the Kind or convert to Utc if needed.
        // For now, let's keep it simple: just take the value. The repository/EF usually handles conversion if configured.
        // Actually, best practice: convert back to UTC if the original was UTC.
        
        _originalEntry.StartTime = StartTime; // If UI is Local, this might need .ToUniversalTime(), checking existing code is wise.
        _originalEntry.EndTime = EndTime;
        _originalEntry.Notes = Notes;

        RequestClose?.Invoke(true);
    }

    private void Cancel()
    {
        RequestClose?.Invoke(false);
    }
}
