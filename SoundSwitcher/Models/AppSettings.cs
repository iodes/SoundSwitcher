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
    /// The ID of the last profile explicitly selected by the user.
    /// Used as a tie-breaker when multiple profiles match current devices.
    /// </summary>
    public Guid? LastSelectedProfileId { get; set; }

    /// <summary>
    /// Whether to also switch the default communication device.
    /// </summary>
    public bool SwitchCommunicationDevice { get; set; } = false;

    /// <summary>
    /// Whether to show a notification when the profile is changed.
    /// </summary>
    public bool ShowProfileChangeNotification { get; set; } = true;

    /// <summary>
    /// Whether to show the active profile icon in the system tray.
    /// </summary>
    public bool ShowProfileIconInTray { get; set; } = true;

    /// <summary>
    /// Whether to automatically run at Windows startup.
    /// </summary>
    public bool RunAtStartup { get; set; } = false;

    /// <summary>
    /// The language code (e.g. "ko-KR", "en-US") for the application UI.
    /// Empty string indicates system default.
    /// </summary>
    public string Language { get; set; } = string.Empty;
}
