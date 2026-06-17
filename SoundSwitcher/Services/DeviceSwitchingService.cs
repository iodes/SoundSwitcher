using SoundSwitcher.Models;

namespace SoundSwitcher.Services;

/// <summary>
/// Round-robin logic that determines the next profile to switch to.
/// </summary>
public class DeviceSwitchingService
{
    private readonly AudioDeviceService _audioService;
    private readonly SettingsService _settingsService;

    public DeviceSwitchingService(AudioDeviceService audioService, SettingsService settingsService)
    {
        _audioService = audioService;
        _settingsService = settingsService;

        _audioService.DevicesChanged += OnDevicesChanged;
    }

    private void OnDevicesChanged()
    {
        var settings = _settingsService.Load();

        if (settings.LastSelectedProfileId != null)
        {
            var activeProfile = GetCurrentActiveProfile();

            if (activeProfile == null)
            {
                settings.LastSelectedProfileId = null;
                _settingsService.Save(settings);
            }
        }
    }

    private bool IsProfileValid(DeviceProfile profile)
    {
        bool hasDevice = false;

        if (!string.IsNullOrEmpty(profile.PlaybackDeviceId))
        {
            hasDevice = true;

            if (!_audioService.IsDeviceActive(profile.PlaybackDeviceId))
                return false;
        }

        if (!string.IsNullOrEmpty(profile.CaptureDeviceId))
        {
            hasDevice = true;

            if (!_audioService.IsDeviceActive(profile.CaptureDeviceId))
                return false;
        }

        return hasDevice;
    }

    /// <summary>
    /// Determines the next profile to switch to and performs the switch.
    /// </summary>
    /// <returns>The switched profile, or null if no profiles exist or none are valid.</returns>
    public DeviceProfile? SwitchToNextProfile()
    {
        var settings = _settingsService.Load();
        List<DeviceProfile> profiles = settings.DeviceProfiles;

        if (profiles.Count == 0)
            return null;

        var currentActive = GetCurrentActiveProfile();
        int currentIndex = currentActive != null ? profiles.FindIndex(p => p.Id == currentActive.Id) : -1;

        int startIndex = (currentIndex + 1) % profiles.Count;
        int nextIndex = startIndex;

        do
        {
            var nextProfile = profiles[nextIndex];

            if (IsProfileValid(nextProfile))
            {
                SwitchToProfile(nextProfile);
                return nextProfile;
            }

            nextIndex = (nextIndex + 1) % profiles.Count;
        } while (nextIndex != startIndex);

        return null;
    }

    /// <summary>
    /// Switches to the specified profile and updates the last selected profile ID.
    /// </summary>
    public void SwitchToProfile(DeviceProfile profile)
    {
        var settings = _settingsService.Load();
        ApplyProfile(profile, settings.SwitchCommunicationDevice);
        UpdateLastSelectedProfileId(profile.Id);
    }

    private void UpdateLastSelectedProfileId(Guid id)
    {
        var settings = _settingsService.Load();
        settings.LastSelectedProfileId = id;
        _settingsService.Save(settings);
    }

    private void ApplyProfile(DeviceProfile profile, bool switchCommunication)
    {
        if (!string.IsNullOrEmpty(profile.PlaybackDeviceId))
        {
            // Ignore exception if device not found (e.g. unplugged)
            if (_audioService.IsDeviceActive(profile.PlaybackDeviceId))
            {
                _audioService.SetDefaultDevice(profile.PlaybackDeviceId, switchCommunication);
            }
        }

        if (!string.IsNullOrEmpty(profile.CaptureDeviceId))
        {
            if (_audioService.IsDeviceActive(profile.CaptureDeviceId))
            {
                _audioService.SetDefaultDevice(profile.CaptureDeviceId, switchCommunication);
            }
        }
    }

    /// <summary>
    /// Gets the currently active profile by checking if current default devices match any profile.
    /// </summary>
    public DeviceProfile? GetCurrentActiveProfile()
    {
        var settings = _settingsService.Load();
        List<DeviceProfile> profiles = settings.DeviceProfiles;

        var currentPlayback = _audioService.GetDefaultDevice(NAudio.CoreAudioApi.DataFlow.Render);
        var currentCapture = _audioService.GetDefaultDevice(NAudio.CoreAudioApi.DataFlow.Capture);

        List<DeviceProfile> matchingProfiles = profiles.Where(p =>
            (p.PlaybackDeviceId == null || p.PlaybackDeviceId == currentPlayback?.ID) &&
            (p.CaptureDeviceId == null || p.CaptureDeviceId == currentCapture?.ID) &&
            (p.PlaybackDeviceId != null || p.CaptureDeviceId != null)).ToList();

        if (matchingProfiles.Count == 0)
            return null;

        if (settings.LastSelectedProfileId != null)
        {
            var lastApplied = matchingProfiles.FirstOrDefault(p => p.Id == settings.LastSelectedProfileId);

            if (lastApplied != null)
                return lastApplied;
        }

        return null;
    }
}
