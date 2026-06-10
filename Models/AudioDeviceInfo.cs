using System.Windows.Media;

namespace SoundSwitcher.Models;

/// <summary>
/// Model containing basic audio device information.
/// </summary>
public class AudioDeviceInfo
{
    /// <summary>
    /// Unique device ID assigned by the system (MMDevice ID).
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Device name displayed to the user (FriendlyName).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Device type: Playback (output) or Capture (input).
    /// </summary>
    public AudioDeviceType DeviceType { get; set; }

    /// <summary>
    /// Whether this is the current system default device.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether this is the current system default communication device.
    /// </summary>
    public bool IsDefaultCommunication { get; set; }

    /// <summary>
    /// Native system icon extracted from Windows (ImageSource).
    /// </summary>
    public ImageSource? DeviceIcon { get; set; }
}

/// <summary>
/// Audio device type.
/// </summary>
public enum AudioDeviceType
{
    Playback,
    Capture
}
