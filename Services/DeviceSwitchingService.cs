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
    }

    /// <summary>
    /// Determines the next profile to switch to and performs the switch.
    /// </summary>
    /// <returns>The switched profile, or null if no profiles exist.</returns>
    public DeviceProfile? SwitchToNextProfile()
    {
        var settings = _settingsService.Load();
        var profiles = settings.DeviceProfiles;

        if (profiles.Count == 0)
            return null;

        if (profiles.Count == 1)
        {
            ApplyProfile(profiles[0], settings.SwitchCommunicationDevice);
            UpdateLastSelectedProfileId(profiles[0].Id);
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

        ApplyProfile(nextProfile, settings.SwitchCommunicationDevice);
        UpdateLastSelectedProfileId(nextProfile.Id);

        return nextProfile;
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
        var profiles = settings.DeviceProfiles;

        var currentPlayback = _audioService.GetDefaultDevice(NAudio.CoreAudioApi.DataFlow.Render);
        var currentCapture = _audioService.GetDefaultDevice(NAudio.CoreAudioApi.DataFlow.Capture);

        var matchingProfiles = profiles.Where(p => 
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

        return matchingProfiles.First();
    }
}
