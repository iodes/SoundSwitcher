using SoundSwitcher.ViewModels;
using System.Windows.Media;

namespace SoundSwitcher.Models;

/// <summary>
/// Model containing basic audio device information.
/// </summary>
public class AudioDeviceInfo : ViewModelBase
{
    /// <summary>
    /// Unique device ID assigned by the system (MMDevice ID).
    /// </summary>
    public string DeviceId
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>
    /// Device name displayed to the user (FriendlyName).
    /// </summary>
    public string Name
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>
    /// Device type: Playback (output) or Capture (input).
    /// </summary>
    public AudioDeviceType DeviceType
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Whether this is the current system default device.
    /// </summary>
    public bool IsDefault
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Whether this is the current system default communication device.
    /// </summary>
    public bool IsDefaultCommunication
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Native system icon extracted from Windows (ImageSource).
    /// </summary>
    public ImageSource? DeviceIcon
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Whether this device is currently active and available.
    /// </summary>
    public bool IsActive
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    private string _state = string.Empty;

    /// <summary>
    /// Detailed device state (e.g. Active, Unplugged, Disabled).
    /// </summary>
    public string State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }
}

/// <summary>
/// Audio device type.
/// </summary>
public enum AudioDeviceType
{
    Playback,
    Capture
}
