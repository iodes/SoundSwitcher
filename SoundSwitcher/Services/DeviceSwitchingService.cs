using SoundSwitcher.Models;

namespace SoundSwitcher.Services;

/// <summary>
/// Round-robin logic that determines the next profile to switch to.
/// </summary>
public class DeviceSwitchingService
{
    private readonly AudioDeviceService _audioService;
    private readonly SettingsService _settingsService;
    private bool _isSwitching;

    public Guid? PendingProfileId { get; private set; }

    public event Action<DeviceProfile>? ProfileSwitched;

    public event Action? PendingProfileChanged;

    public DeviceSwitchingService(AudioDeviceService audioService, SettingsService settingsService)
    {
        _audioService = audioService;
        _settingsService = settingsService;

        _audioService.DevicesChanged += OnDevicesChanged;
    }

    private void OnDevicesChanged()
    {
        if (_isSwitching)
            return;

        var settings = _settingsService.Load();

        if (PendingProfileId != null)
        {
            var pendingProfile = settings.DeviceProfiles.FirstOrDefault(p => p.Id == PendingProfileId);

            if (pendingProfile != null && IsProfileFullyActive(pendingProfile))
            {
                // The pending profile is now fully available. Apply it!
                PendingProfileId = null;
                PendingProfileChanged?.Invoke();

                ApplyProfile(pendingProfile, settings.SwitchCommunicationDevice);
                UpdateLastSelectedProfileId(pendingProfile.Id);
                ProfileSwitched?.Invoke(pendingProfile);
            }
        }

        if (settings.LastSelectedProfileId != null && PendingProfileId == null)
        {
            var activeProfile = GetCurrentActiveProfile();

            if (activeProfile == null)
            {
                var lastProfile = settings.DeviceProfiles.FirstOrDefault(p => p.Id == settings.LastSelectedProfileId);

                // If the profile became inactive (e.g. device unplugged), put it into pending state.
                if (lastProfile != null && !IsProfileFullyActive(lastProfile))
                {
                    PendingProfileId = lastProfile.Id;
                    PendingProfileChanged?.Invoke();
                }
                else
                {
                    // Otherwise (e.g. user manually changed default device via Windows), clear the selected profile.
                    _settingsService.Update(s => s.LastSelectedProfileId = null);
                }
            }
        }
    }

    private bool IsProfileFullyActive(DeviceProfile profile)
    {
        if (!string.IsNullOrEmpty(profile.PlaybackDeviceId) && !_audioService.IsDeviceActive(profile.PlaybackDeviceId))
            return false;

        return string.IsNullOrEmpty(profile.CaptureDeviceId) || _audioService.IsDeviceActive(profile.CaptureDeviceId);
    }

    /// <summary>
    /// Determines the next profile to switch to and performs the switch or sets it as pending.
    /// </summary>
    /// <returns>The switched or pending profile, or null if no profiles exist.</returns>
    public DeviceProfile? SwitchToNextProfile()
    {
        var settings = _settingsService.Load();
        List<DeviceProfile> profiles = settings.DeviceProfiles;

        if (profiles.Count == 0)
            return null;

        var currentActiveId = PendingProfileId ?? GetCurrentActiveProfile()?.Id;
        int currentIndex = currentActiveId != null ? profiles.FindIndex(p => p.Id == currentActiveId) : -1;

        int nextIndex = (currentIndex + 1) % profiles.Count;
        var nextProfile = profiles[nextIndex];

        SwitchToProfile(nextProfile);
        return nextProfile;
    }

    /// <summary>
    /// Switches to the specified profile, or marks it as pending if devices are inactive.
    /// </summary>
    public void SwitchToProfile(DeviceProfile profile)
    {
        _isSwitching = true;

        try
        {
            var settings = _settingsService.Load();

            if (IsProfileFullyActive(profile))
            {
                UpdateLastSelectedProfileId(profile.Id);
                ApplyProfile(profile, settings.SwitchCommunicationDevice);

                PendingProfileId = null;
                PendingProfileChanged?.Invoke();
                ProfileSwitched?.Invoke(profile);
            }
            else
            {
                UpdateLastSelectedProfileId(profile.Id);
                PendingProfileId = profile.Id;
                PendingProfileChanged?.Invoke();
            }
        }
        finally
        {
            _isSwitching = false;
        }
    }

    private void UpdateLastSelectedProfileId(Guid id)
    {
        _settingsService.Update(s => s.LastSelectedProfileId = id);
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
