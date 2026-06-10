using SoundSwitcher.Models;

namespace SoundSwitcher.ViewModels;

/// <summary>
/// ViewModel for an individual item in the device list.
/// </summary>
public class DeviceItemViewModel : ViewModelBase
{
    private bool _isPreferred;
    private string? _customIconPath;
    private int _orderIndex;

    public string DeviceId { get; }
    public string Name { get; }
    public AudioDeviceType DeviceType { get; }
    public bool IsDefault { get; }
    public bool IsDefaultCommunication { get; }

    public bool IsPreferred
    {
        get => _isPreferred;
        set => SetProperty(ref _isPreferred, value);
    }

    public string? CustomIconPath
    {
        get => _customIconPath;
        set => SetProperty(ref _customIconPath, value);
    }

    public int OrderIndex
    {
        get => _orderIndex;
        set => SetProperty(ref _orderIndex, value);
    }

    public DeviceItemViewModel(AudioDeviceInfo device)
    {
        DeviceId = device.DeviceId;
        Name = device.Name;
        DeviceType = device.DeviceType;
        IsDefault = device.IsDefault;
        IsDefaultCommunication = device.IsDefaultCommunication;
    }
}
