using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChronoDesk.UI.Services;

public interface ISettingsService
{
    bool IsDarkModeEnabled { get; set; }
    bool IsAutoSaveEnabled { get; set; }
    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
}

public class SettingsService : ISettingsService
{
    private const string SettingsFileName = "settings.json";
    private readonly string _filePath;

    public bool IsDarkModeEnabled { get; set; } = true;
    public bool IsAutoSaveEnabled { get; set; } = true;

    public SettingsService()
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
    }

    public async Task LoadSettingsAsync()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                var settings = JsonSerializer.Deserialize<SettingsModel>(json);
                if (settings != null)
                {
                    IsDarkModeEnabled = settings.IsDarkModeEnabled;
                    IsAutoSaveEnabled = settings.IsAutoSaveEnabled;
                }
            }
            catch
            {
                // Ignore load errors, stick to defaults
            }
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            var settings = new SettingsModel
            {
                IsDarkModeEnabled = IsDarkModeEnabled,
                IsAutoSaveEnabled = IsAutoSaveEnabled
            };
            var json = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(_filePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    private class SettingsModel
    {
        public bool IsDarkModeEnabled { get; set; }
        public bool IsAutoSaveEnabled { get; set; }
    }
}
