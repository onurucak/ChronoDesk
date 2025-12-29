using ChronoDesk.UI.Services;

namespace ChronoDesk.UI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public string Version => "1.0.0";

    public bool IsDarkModeEnabled
    {
        get => _settingsService.IsDarkModeEnabled;
        set
        {
            if (_settingsService.IsDarkModeEnabled != value)
            {
                _settingsService.IsDarkModeEnabled = value;
                OnPropertyChanged();
                SaveSettingsAsync();
            }
        }
    }

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

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    private async void SaveSettingsAsync()
    {
        await _settingsService.SaveSettingsAsync();
    }
}
