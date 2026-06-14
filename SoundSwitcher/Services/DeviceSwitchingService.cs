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

    /// <summary>
    /// Determines the next profile to switch to and performs the switch.
    /// </summary>
    /// <returns>The switched profile, or null if no profiles exist.</returns>
    public DeviceProfile? SwitchToNextProfile()
    {
        var settings = _settingsService.Load();
        List<DeviceProfile> profiles = settings.DeviceProfiles;

        switch (profiles.Count)
        {
            case 0:
                return null;

            case 1:
                SwitchToProfile(profiles[0]);
                return profiles[0];
        }

        // Try to find the current active profile
        var currentActive = GetCurrentActiveProfile();
        int currentIndex = -1;

        if (currentActive != null)
        {
            currentIndex = profiles.FindIndex(p => p.Id == currentActive.Id);
        }

        int nextIndex = (currentIndex + 1) % profiles.Count;
        var nextProfile = profiles[nextIndex];

        SwitchToProfile(nextProfile);

        return nextProfile;
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
            {
                return lastApplied;
            }
        }

        return null;
    }
}
