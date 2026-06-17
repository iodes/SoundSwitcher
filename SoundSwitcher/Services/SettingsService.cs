using SoundSwitcher.Models;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace SoundSwitcher.Services;

/// <summary>
/// Settings save/load service based on local JSON files.
/// </summary>
public class SettingsService
{
    private static readonly string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SoundSwitcher");

    private static readonly string SettingsFilePath = Path.Combine(SettingsFolder, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // .NET 9+ modern Lock object for thread safety
    private static readonly Lock _fileLock = new();

    /// <summary>
    /// Loads settings from the settings file. Returns defaults if the file does not exist.
    /// </summary>
    public AppSettings Load()
    {
        lock (_fileLock)
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return new AppSettings();
                }

                var json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }
    }

    /// <summary>
    /// Saves settings to a JSON file.
    /// </summary>
    public void Save(AppSettings settings)
    {
        lock (_fileLock)
        {
            Directory.CreateDirectory(SettingsFolder);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsFilePath, json);
        }
    }

    /// <summary>
    /// Atomically loads, updates, and saves settings.
    /// </summary>
    public void Update(Action<AppSettings> modifier)
    {
        lock (_fileLock)
        {
            var settings = Load();
            modifier(settings);
            Save(settings);
        }
    }
}
