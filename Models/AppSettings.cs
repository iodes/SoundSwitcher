namespace SoundSwitcher.Models;

/// <summary>
/// Device profile representing a pair of playback and capture devices.
/// </summary>
public class DeviceProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? IconPath { get; set; }
    public string? PlaybackDeviceId { get; set; }
    public string? CaptureDeviceId { get; set; }
}

/// <summary>
/// Model containing application-wide user settings.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// List of device profiles (pairs of playback and capture devices).
    /// </summary>
    public List<DeviceProfile> DeviceProfiles { get; set; } = [];

    /// <summary>
    /// Whether to also switch the default communication device.
    /// </summary>
    public bool SwitchCommunicationDevice { get; set; } = true;

    /// <summary>
    /// Whether to automatically run at Windows startup.
    /// </summary>
    public bool RunAtStartup { get; set; } = false;

    /// <summary>
    /// Whether to show the active profile icon in the system tray.
    /// </summary>
    public bool ShowProfileIconInTray { get; set; } = true;
}
