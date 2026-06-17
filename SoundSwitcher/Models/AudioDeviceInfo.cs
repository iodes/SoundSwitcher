using SoundSwitcher.ViewModels;
using System.Windows.Media;

namespace SoundSwitcher.Models;

/// <summary>
/// Model containing basic audio device information.
/// </summary>
public class AudioDeviceInfo : ViewModelBase
{
    private string _deviceId = string.Empty;
    private string _name = string.Empty;
    private AudioDeviceType _deviceType;
    private bool _isDefault;
    private bool _isDefaultCommunication;
    private ImageSource? _deviceIcon;
    private bool _isActive = true;

    /// <summary>
    /// Unique device ID assigned by the system (MMDevice ID).
    /// </summary>
    public string DeviceId
    {
        get => _deviceId;
        set => SetProperty(ref _deviceId, value);
    }

    /// <summary>
    /// Device name displayed to the user (FriendlyName).
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Device type: Playback (output) or Capture (input).
    /// </summary>
    public AudioDeviceType DeviceType
    {
        get => _deviceType;
        set => SetProperty(ref _deviceType, value);
    }

    /// <summary>
    /// Whether this is the current system default device.
    /// </summary>
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    /// <summary>
    /// Whether this is the current system default communication device.
    /// </summary>
    public bool IsDefaultCommunication
    {
        get => _isDefaultCommunication;
        set => SetProperty(ref _isDefaultCommunication, value);
    }

    /// <summary>
    /// Native system icon extracted from Windows (ImageSource).
    /// </summary>
    public ImageSource? DeviceIcon
    {
        get => _deviceIcon;
        set => SetProperty(ref _deviceIcon, value);
    }

    /// <summary>
    /// Whether this device is currently active and available.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
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
