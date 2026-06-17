using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using SoundSwitcher.Models;
using SoundSwitcher.Helpers;
using System.Runtime.InteropServices;

namespace SoundSwitcher.Services;

/// <summary>
/// Audio device management service using the Windows Core Audio API.
/// </summary>
public class AudioDeviceService : IMMNotificationClient
{
    private readonly MMDeviceEnumerator _enumerator;

    public event Action? DevicesChanged;

    public AudioDeviceService()
    {
        _enumerator = new MMDeviceEnumerator();
        _enumerator.RegisterEndpointNotificationCallback(this);
    }

    /// <summary>
    /// Retrieves all active audio devices, categorized by playback and capture.
    /// </summary>
    public List<AudioDeviceInfo> GetActiveDevices()
    {
        var devices = new List<AudioDeviceInfo>();

        // Playback (output) devices
        var defaultPlayback = GetDefaultDevice(DataFlow.Render);
        var defaultPlaybackComm = GetDefaultCommunicationDevice(DataFlow.Render);
        var iconPropertyKey = new PropertyKey(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 12);

        const DeviceState allStates = DeviceState.Active | DeviceState.Unplugged | DeviceState.Disabled;

        foreach (var device in _enumerator.EnumerateAudioEndPoints(DataFlow.Render, allStates))
        {
            string? iconPath = null;

            try { iconPath = device.Properties[iconPropertyKey].Value as string; }
            catch
            {
                // ignored
            }

            devices.Add(new AudioDeviceInfo
            {
                DeviceId = device.ID,
                Name = device.FriendlyName,
                DeviceType = AudioDeviceType.Playback,
                IsDefault = defaultPlayback != null && device.ID == defaultPlayback.ID,
                IsDefaultCommunication = defaultPlaybackComm != null && device.ID == defaultPlaybackComm.ID,
                DeviceIcon = NativeIconHelper.ExtractDeviceIcon(iconPath),
                IsActive = device.State == DeviceState.Active
            });
        }

        // Capture (input) devices
        var defaultCapture = GetDefaultDevice(DataFlow.Capture);
        var defaultCaptureComm = GetDefaultCommunicationDevice(DataFlow.Capture);

        foreach (var device in _enumerator.EnumerateAudioEndPoints(DataFlow.Capture, allStates))
        {
            string? iconPath = null;

            try { iconPath = device.Properties[iconPropertyKey].Value as string; }
            catch
            {
                // ignored
            }

            devices.Add(new AudioDeviceInfo
            {
                DeviceId = device.ID,
                Name = device.FriendlyName,
                DeviceType = AudioDeviceType.Capture,
                IsDefault = defaultCapture != null && device.ID == defaultCapture.ID,
                IsDefaultCommunication = defaultCaptureComm != null && device.ID == defaultCaptureComm.ID,
                DeviceIcon = NativeIconHelper.ExtractDeviceIcon(iconPath),
                IsActive = device.State == DeviceState.Active
            });
        }

        return devices;
    }

    /// <summary>
    /// Gets the current system default device.
    /// </summary>
    public MMDevice? GetDefaultDevice(DataFlow dataFlow)
    {
        try
        {
            return _enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia);
        }
        catch (COMException)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the current system default communication device.
    /// </summary>
    public MMDevice? GetDefaultCommunicationDevice(DataFlow dataFlow)
    {
        try
        {
            return _enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Communications);
        }
        catch (COMException)
        {
            return null;
        }
    }

    /// <summary>
    /// Sets a specific device as the system default device.
    /// </summary>
    /// <param name="deviceId">The device ID to set as default.</param>
    /// <param name="alsoSetCommunication">Whether to also set as the communication device.</param>
    public void SetDefaultDevice(string deviceId, bool alsoSetCommunication = true)
    {
        var clsid = new Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9");
        var type = Type.GetTypeFromCLSID(clsid)!;
        var comObject = Activator.CreateInstance(type)!;

        var wrappers = new System.Runtime.InteropServices.Marshalling.StrategyBasedComWrappers();
        var ptr = Marshal.GetIUnknownForObject(comObject);
        var policyConfig = (IPolicyConfig)wrappers.GetOrCreateObjectForComInstance(ptr, CreateObjectFlags.None);

        // Set as default console/multimedia device
        policyConfig.SetDefaultEndpoint(deviceId, ERole.eConsole);
        policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia);

        // Also set as communication device
        if (alsoSetCommunication)
        {
            policyConfig.SetDefaultEndpoint(deviceId, ERole.eCommunications);
        }
    }

    /// <summary>
    /// Checks whether the device with the specified ID is currently active.
    /// </summary>
    public bool IsDeviceActive(string deviceId)
    {
        try
        {
            var device = _enumerator.GetDevice(deviceId);
            return device.State == DeviceState.Active;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the FriendlyName of a device by its device ID.
    /// </summary>
    public string? GetDeviceName(string deviceId)
    {
        try
        {
            var device = _enumerator.GetDevice(deviceId);
            return device.FriendlyName;
        }
        catch
        {
            return null;
        }
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState) => DevicesChanged?.Invoke();

    public void OnDeviceAdded(string pwstrDeviceId) => DevicesChanged?.Invoke();

    public void OnDeviceRemoved(string deviceId) => DevicesChanged?.Invoke();

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) => DevicesChanged?.Invoke();

    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
    {
    }
}
