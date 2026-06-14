using SoundSwitcher.Models;

namespace SoundSwitcher.ViewModels;

/// <summary>
/// ViewModel for an individual item in the device list.
/// </summary>
public class DeviceItemViewModel(AudioDeviceInfo device) : ViewModelBase
{
    public string DeviceId { get; } = device.DeviceId;

    public string Name { get; } = device.Name;

    public AudioDeviceType DeviceType { get; } = device.DeviceType;

    public bool IsDefault { get; } = device.IsDefault;

    public bool IsDefaultCommunication { get; } = device.IsDefaultCommunication;

    public bool IsPreferred
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string? CustomIconPath
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int OrderIndex
    {
        get;
        set => SetProperty(ref field, value);
    }
}
