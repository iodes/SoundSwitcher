using Serilog;
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
    private Guid? _pendingProfileId;
    private readonly System.Threading.Lock _stateLock = new();

    public Guid? PendingProfileId
    {
        get
        {
            lock (_stateLock) return _pendingProfileId;
        }
        private set
        {
            lock (_stateLock) _pendingProfileId = value;
        }
    }

    public event Action<DeviceProfile>? ProfileSwitched;

    public event Action? PendingProfileChanged;

    public DeviceSwitchingService(AudioDeviceService audioService, SettingsService settingsService)
    {
        _audioService = audioService;
        _settingsService = settingsService;
        _audioService.DevicesChanged += OnDevicesChanged;
    }

    /// <summary>
    /// Restores the pending state or clears inactive profile on startup.
    /// To be called if no Default Profile overrides it.
    /// </summary>
    public void RestoreStartupState()
    {
        CheckAndRestoreLastSelectedProfile(isStartup: true);
    }

    /// <summary>
    /// Clears the pending state if the given profile is currently pending.
    /// </summary>
    public void ClearPendingProfile(Guid profileId)
    {
        bool raisePendingChanged = false;
        lock (_stateLock)
        {
            if (_pendingProfileId == profileId)
            {
                _pendingProfileId = null;
                raisePendingChanged = true;
            }
        }

        if (raisePendingChanged)
            PendingProfileChanged?.Invoke();
    }

    private void OnDevicesChanged()
    {
        DeviceProfile? profileToResume = null;
        bool raisePendingChanged = false;

        lock (_stateLock)
        {
            if (_isSwitching)
                return;

            var settings = _settingsService.Load();

            if (_pendingProfileId != null)
            {
                var pendingProfile = settings.DeviceProfiles.FirstOrDefault(p => p.Id == _pendingProfileId);

                if (pendingProfile != null && IsProfileFullyActive(pendingProfile))
                {
                    _pendingProfileId = null;
                    profileToResume = pendingProfile;
                    raisePendingChanged = true;
                }
            }
        }

        if (profileToResume != null)
        {
            if (raisePendingChanged)
                PendingProfileChanged?.Invoke();

            SwitchToProfile(profileToResume);
            return;
        }

        CheckAndRestoreLastSelectedProfile(isStartup: false);
    }

    private void CheckAndRestoreLastSelectedProfile(bool isStartup)
    {
        bool raisePendingChanged = false;

        lock (_stateLock)
        {
            var settings = _settingsService.Load();

            if (settings.LastSelectedProfileId != null && _pendingProfileId == null)
            {
                var activeProfile = GetCurrentActiveProfile();

                if (activeProfile == null)
                {
                    var lastProfile = settings.DeviceProfiles.FirstOrDefault(p => p.Id == settings.LastSelectedProfileId);

                    // If the profile became inactive (e.g. device unplugged), put it into pending state.
                    if (lastProfile != null && !IsProfileFullyActive(lastProfile))
                    {
                        var details = GetProfileDeviceDetails(lastProfile);

                        string actionVerb = isStartup ? "Restoring pending profile" : "Suspending profile";
                        string reason = isStartup ? "App startup" : "Device disconnected";

                        Log.Information("{ActionVerb} {ProfileId} ({Reason}) -> Playback: {PlaybackName} [{@PlaybackId}] {PlaybackStatus}, Capture: {CaptureName} [{@CaptureId}] {CaptureStatus}",
                            actionVerb, lastProfile.Id, reason, details.PlaybackName, lastProfile.PlaybackDeviceId, details.PlaybackStatus, details.CaptureName, lastProfile.CaptureDeviceId, details.CaptureStatus);

                        _pendingProfileId = lastProfile.Id;
                        raisePendingChanged = true;
                    }
                    else
                    {
                        // Otherwise (e.g. user manually changed default device via Windows), clear the selected profile.
                        _settingsService.Update(s => s.LastSelectedProfileId = null);
                    }
                }
            }
        }

        if (raisePendingChanged)
        {
            PendingProfileChanged?.Invoke();
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

        Guid? currentActiveId;

        lock (_stateLock)
        {
            currentActiveId = _pendingProfileId ?? GetCurrentActiveProfile()?.Id;
        }

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
        bool raiseProfileSwitched = false;
        bool raisePendingChanged = false;

        lock (_stateLock)
        {
            _isSwitching = true;

            try
            {
                var settings = _settingsService.Load();

                if (IsProfileFullyActive(profile))
                {
                    UpdateLastSelectedProfileId(profile.Id);
                    ApplyProfile(profile, settings.SwitchCommunicationDevice);

                    _pendingProfileId = null;
                    raisePendingChanged = true;
                    raiseProfileSwitched = true;
                }
                else
                {
                    var details = GetProfileDeviceDetails(profile);

                    Log.Information("Pending profile {ProfileId} (Device not found) -> Playback: {PlaybackName} [{@PlaybackId}] {PlaybackStatus}, Capture: {CaptureName} [{@CaptureId}] {CaptureStatus}",
                        profile.Id, details.PlaybackName, profile.PlaybackDeviceId, details.PlaybackStatus, details.CaptureName, profile.CaptureDeviceId, details.CaptureStatus);

                    UpdateLastSelectedProfileId(profile.Id);
                    _pendingProfileId = profile.Id;
                    raisePendingChanged = true;
                }
            }
            finally
            {
                _isSwitching = false;
            }
        }

        if (raisePendingChanged)
            PendingProfileChanged?.Invoke();

        if (raiseProfileSwitched)
            ProfileSwitched?.Invoke(profile);
    }

    private void UpdateLastSelectedProfileId(Guid id)
    {
        _settingsService.Update(s => s.LastSelectedProfileId = id);
    }

    private void ApplyProfile(DeviceProfile profile, bool switchCommunication)
    {
        var details = GetProfileDeviceDetails(profile);

        Log.Information("Switched to profile {ProfileId} -> Playback: {PlaybackName} [{@PlaybackId}] {PlaybackStatus}, Capture: {CaptureName} [{@CaptureId}] {CaptureStatus}",
            profile.Id, details.PlaybackName, profile.PlaybackDeviceId, details.PlaybackStatus, details.CaptureName, profile.CaptureDeviceId, details.CaptureStatus);

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

    private (string PlaybackName, string PlaybackStatus, string CaptureName, string CaptureStatus) GetProfileDeviceDetails(DeviceProfile profile)
    {
        var playbackName = !string.IsNullOrEmpty(profile.PlaybackDeviceId) ? (_audioService.GetDeviceName(profile.PlaybackDeviceId) ?? profile.LastKnownPlaybackDeviceName ?? "Unknown") : "None";
        var playbackStatus = string.IsNullOrEmpty(profile.PlaybackDeviceId) ? "" : (_audioService.IsDeviceActive(profile.PlaybackDeviceId) ? "Active" : "Pending");

        var captureName = !string.IsNullOrEmpty(profile.CaptureDeviceId) ? (_audioService.GetDeviceName(profile.CaptureDeviceId) ?? profile.LastKnownCaptureDeviceName ?? "Unknown") : "None";
        var captureStatus = string.IsNullOrEmpty(profile.CaptureDeviceId) ? "" : (_audioService.IsDeviceActive(profile.CaptureDeviceId) ? "Active" : "Pending");

        return (playbackName, playbackStatus, captureName, captureStatus);
    }
}
