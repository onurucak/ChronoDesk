using System.Threading.Tasks;
using System.Windows.Input;
using ChronoDesk.Application.Interfaces;
using System.Reflection;
using ChronoDesk.UI.Services;

namespace ChronoDesk.UI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly IDataMaintenanceService _dataMaintenanceService;
    private readonly ProjectStore _projectStore;

    public string Version => Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "1.0.1";



    public bool IsAutoSaveEnabled
    {
        get => _settingsService.IsAutoSaveEnabled;
        set
        {
            if (_settingsService.IsAutoSaveEnabled != value)
            {
                _settingsService.IsAutoSaveEnabled = value;
                OnPropertyChanged();
                SaveSettingsAsync();
            }
        }
    }

    private string _deleteConfirmationText = string.Empty;
    public string DeleteConfirmationText
    {
        get => _deleteConfirmationText;
        set
        {
            if (SetField(ref _deleteConfirmationText, value))
            {
                ((RelayCommand)ClearDatabaseCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand ClearDatabaseCommand { get; }

    public SettingsViewModel(ISettingsService settingsService, IDataMaintenanceService dataMaintenanceService, ProjectStore projectStore)
    {
        _settingsService = settingsService;
        _dataMaintenanceService = dataMaintenanceService;
        _projectStore = projectStore;

        ClearDatabaseCommand = new RelayCommand(async _ => await ClearDatabaseAsync(), _ => DeleteConfirmationText == "delete me");
    }

    private async void SaveSettingsAsync()
    {
        await _settingsService.SaveSettingsAsync();
    }

    private async Task ClearDatabaseAsync()
    {
        await _dataMaintenanceService.ClearAllDataAsync();
        await _projectStore.LoadProjectsAsync(); // Reload to clear the list in UI
        DeleteConfirmationText = string.Empty; // Reset text
    }
}
