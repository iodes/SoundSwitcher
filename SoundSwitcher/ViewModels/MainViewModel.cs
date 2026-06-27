using Serilog;
using SoundSwitcher.Models;
using SoundSwitcher.Services;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SoundSwitcher.ViewModels;

/// <summary>
/// Main ViewModel that manages the entire settings window.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly AudioDeviceService _audioService;
    private readonly DeviceSwitchingService _switchingService;
    private readonly SettingsService _settingsService;

    private bool _switchCommunicationDevice;
    private bool _runAtStartup;
    private bool _showProfileIconInTray;
    private bool _showProfileChangeNotification;
    private bool _showUnavailableDevices;
    private string _language = string.Empty;

    public class LanguageOption(string code, string name) : ViewModelBase
    {
        public string Code { get; } = code;

        public string Name
        {
            get;
            set => SetProperty(ref field, value);
        } = name;
    }

    public ObservableCollection<LanguageOption> AvailableLanguages { get; } =
    [
        new("", Localization.LocalizationManager.Instance["LanguageAuto"]),
        new("en-US", "English"),
        new("ko-KR", "한국어"),
        new("ja-JP", "日本語"),
        new("zh-CN", "中文(简体)"),
        new("zh-TW", "中文(繁體)")
    ];

    public ObservableCollection<DeviceProfileViewModel> Profiles { get; } = [];

    public ObservableCollection<AudioDeviceInfo> AvailablePlaybackDevices { get; } = [];

    public ObservableCollection<AudioDeviceInfo> AvailableCaptureDevices { get; } = [];

    public bool SwitchCommunicationDevice
    {
        get => _switchCommunicationDevice;
        set
        {
            if (SetProperty(ref _switchCommunicationDevice, value))
                SaveSettings();
        }
    }

    public bool RunAtStartup
    {
        get => _runAtStartup;
        set
        {
            if (SetProperty(ref _runAtStartup, value))
            {
                StartupService.SetAutoStartup(value);
                SaveSettings();
            }
        }
    }

    public bool ShowProfileIconInTray
    {
        get => _showProfileIconInTray;
        set
        {
            if (SetProperty(ref _showProfileIconInTray, value))
                SaveSettings();
        }
    }

    public bool ShowProfileChangeNotification
    {
        get => _showProfileChangeNotification;
        set
        {
            if (SetProperty(ref _showProfileChangeNotification, value))
                SaveSettings();
        }
    }

    public bool ShowUnavailableDevices
    {
        get => _showUnavailableDevices;
        set
        {
            if (SetProperty(ref _showUnavailableDevices, value))
            {
                SaveSettings();
                Application.Current.Dispatcher.Invoke(RefreshDevices);
            }
        }
    }

    public string Language
    {
        get => _language;
        set
        {
            if (SetProperty(ref _language, value))
            {
                Localization.LocalizationManager.Instance.ApplyFromSettings(value);
                SaveSettings();
                AvailableLanguages[0].Name = Localization.LocalizationManager.Instance["LanguageAuto"];
            }
        }
    }

    public Guid? FocusedProfileId
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                foreach (var profile in Profiles)
                {
                    profile.IsFocused = profile.Id == value;
                }
            }
        }
    }

    public bool IsReordering
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string AppVersion => $"{Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0"}";

    public ICommand AddProfileCommand { get; }

    public ICommand ClearFocusCommand { get; }

    /// <summary>
    /// Event raised when settings are changed.
    /// </summary>
    public event Action? SettingsChanged;

    public MainViewModel(AudioDeviceService audioService, SettingsService settingsService, DeviceSwitchingService switchingService)
    {
        _audioService = audioService;
        _settingsService = settingsService;
        _switchingService = switchingService;

        AddProfileCommand = new RelayCommand(AddProfile);
        ClearFocusCommand = new RelayCommand(() => FocusedProfileId = null);

        LoadSettings();

        Profiles.CollectionChanged += (_, _) => SaveSettings();
        RefreshDevices();

        // Auto-refresh when devices change
        _audioService.DevicesChanged += () =>
        {
            Application.Current.Dispatcher.Invoke(RefreshDevices);
        };

        _switchingService.PendingProfileChanged += () =>
        {
            Application.Current.Dispatcher.Invoke(RefreshDevices);
        };
    }

    private void LoadSettings()
    {
        var settings = _settingsService.Load();
        _switchCommunicationDevice = settings.SwitchCommunicationDevice;
        _runAtStartup = settings.RunAtStartup;
        _showProfileIconInTray = settings.ShowProfileIconInTray;
        _showProfileChangeNotification = settings.ShowProfileChangeNotification;
        _showUnavailableDevices = settings.ShowUnavailableDevices;
        _language = settings.Language;
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(ShowUnavailableDevices));

        Profiles.Clear();

        foreach (var pvm in settings.DeviceProfiles.Select(profile => new DeviceProfileViewModel(profile, isNew: false)))
        {
            pvm.IsDefaultProfile = settings.DefaultProfileId == pvm.Id;
            pvm.ProfileChanged += SaveSettings;
            pvm.DeleteRequested += OnProfileDeleteRequested;
            pvm.DeviceApplyRequested += OnProfileApplyRequested;
            pvm.ToggleDefaultRequested += OnProfileToggleDefaultRequested;
            Profiles.Add(pvm);
        }
    }

    public void RefreshDevices()
    {
        List<AudioDeviceInfo> devices = _audioService.GetActiveDevices();
        bool deviceSnapshotChanged = RememberSelectedDeviceNames(devices);

        var selectedDeviceIds = new HashSet<string>(
            Profiles.Select(p => p.PlaybackDeviceId).Concat(Profiles.Select(p => p.CaptureDeviceId)).OfType<string>()
        );

        List<AudioDeviceInfo> currentPlayback = devices
            .Where(d => d.DeviceType == AudioDeviceType.Playback && (d.IsActive || ShowUnavailableDevices || selectedDeviceIds.Contains(d.DeviceId)))
            .ToList();

        List<AudioDeviceInfo> currentCapture = devices
            .Where(d => d.DeviceType == AudioDeviceType.Capture && (d.IsActive || ShowUnavailableDevices || selectedDeviceIds.Contains(d.DeviceId)))
            .ToList();

        AddMissingSelectedDevices(currentPlayback, AudioDeviceType.Playback);
        AddMissingSelectedDevices(currentCapture, AudioDeviceType.Capture);

        currentPlayback = currentPlayback
            .OrderBy(d => ParseDeviceName(d.Name).DeviceDescription)
            .ThenBy(d => ParseDeviceName(d.Name).EndpointName)
            .ToList();

        currentCapture = currentCapture
            .OrderBy(d => ParseDeviceName(d.Name).DeviceDescription)
            .ThenBy(d => ParseDeviceName(d.Name).EndpointName)
            .ToList();

        SyncCollection(AvailablePlaybackDevices, currentPlayback);
        SyncCollection(AvailableCaptureDevices, currentCapture);

        var activeProfileModel = _switchingService.GetCurrentActiveProfile();

        foreach (var profile in Profiles)
        {
            profile.IsActive = activeProfileModel != null && profile.Id == activeProfileModel.Id;
            profile.IsPending = _switchingService.PendingProfileId == profile.Id;

            var playbackDevice = AvailablePlaybackDevices.FirstOrDefault(d => d.DeviceId == profile.PlaybackDeviceId);
            profile.FallbackDeviceIcon = playbackDevice?.DeviceIcon;
        }

        if (deviceSnapshotChanged)
        {
            SaveSettings();
        }
    }

    private bool RememberSelectedDeviceNames(List<AudioDeviceInfo> devices)
    {
        bool changed = false;

        foreach (var profile in Profiles)
        {
            var model = profile.GetModel();

            if (!string.IsNullOrEmpty(model.PlaybackDeviceId))
            {
                var playbackDevice = devices.FirstOrDefault(d => d.DeviceId == model.PlaybackDeviceId);

                if (playbackDevice != null && !string.IsNullOrWhiteSpace(playbackDevice.Name) && model.LastKnownPlaybackDeviceName != playbackDevice.Name)
                {
                    model.LastKnownPlaybackDeviceName = playbackDevice.Name;
                    changed = true;
                }
            }

            if (!string.IsNullOrEmpty(model.CaptureDeviceId))
            {
                var captureDevice = devices.FirstOrDefault(d => d.DeviceId == model.CaptureDeviceId);

                if (captureDevice != null && !string.IsNullOrWhiteSpace(captureDevice.Name) && model.LastKnownCaptureDeviceName != captureDevice.Name)
                {
                    model.LastKnownCaptureDeviceName = captureDevice.Name;
                    changed = true;
                }
            }
        }

        return changed;
    }

    private void AddMissingSelectedDevices(List<AudioDeviceInfo> target, AudioDeviceType deviceType)
    {
        foreach (var profile in Profiles)
        {
            var model = profile.GetModel();
            string? deviceId = deviceType == AudioDeviceType.Playback ? model.PlaybackDeviceId : model.CaptureDeviceId;

            if (string.IsNullOrEmpty(deviceId) || target.Any(d => d.DeviceId == deviceId))
                continue;

            string? lastKnownName = deviceType == AudioDeviceType.Playback
                ? model.LastKnownPlaybackDeviceName
                : model.LastKnownCaptureDeviceName;

            target.Add(new AudioDeviceInfo
            {
                DeviceId = deviceId,
                Name = string.IsNullOrWhiteSpace(lastKnownName) ? $"Unknown ({deviceId})" : lastKnownName,
                DeviceType = deviceType,
                IsActive = false,
                State = "Unavailable"
            });
        }
    }

    private (string EndpointName, string DeviceDescription) ParseDeviceName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return (string.Empty, string.Empty);

        int lastOpen = fullName.LastIndexOf('(');
        int lastClose = fullName.LastIndexOf(')');

        if (lastOpen >= 0 && lastClose > lastOpen)
        {
            string endpoint = fullName.Substring(0, lastOpen).Trim();
            string description = fullName.Substring(lastOpen + 1, lastClose - lastOpen - 1).Trim();
            return (endpoint, description);
        }

        return (fullName, string.Empty);
    }

    private void SyncCollection(ObservableCollection<AudioDeviceInfo> target, List<AudioDeviceInfo> source)
    {
        // Remove items not in source
        for (int i = target.Count - 1; i >= 0; i--)
        {
            if (source.All(d => d.DeviceId != target[i].DeviceId))
            {
                target.RemoveAt(i);
            }
        }

        // Add or reorder items to match source
        for (int i = 0; i < source.Count; i++)
        {
            var item = source[i];
            var existing = target.FirstOrDefault(d => d.DeviceId == item.DeviceId);

            if (existing == null)
            {
                target.Insert(i, item);
            }
            else
            {
                // Update properties in case they changed (e.g. IsActive, Name, etc.)
                existing.Name = item.Name;
                existing.IsActive = item.IsActive;
                existing.DeviceIcon = item.DeviceIcon;
                existing.IsDefault = item.IsDefault;
                existing.IsDefaultCommunication = item.IsDefaultCommunication;
                existing.State = item.State;

                int oldIndex = target.IndexOf(existing);

                if (oldIndex != i)
                {
                    target.Move(oldIndex, i);
                }
            }
        }
    }

    private void AddProfile()
    {
        var newProfile = new DeviceProfile
        {
            Id = Guid.NewGuid(),
            IconPath = null // Default to null so it uses fallback
        };

        // Default to the first available devices
        if (AvailablePlaybackDevices.Count > 0)
        {
            newProfile.PlaybackDeviceId = AvailablePlaybackDevices[0].DeviceId;
        }

        if (AvailableCaptureDevices.Count > 0)
        {
            newProfile.CaptureDeviceId = AvailableCaptureDevices[0].DeviceId;
        }

        var pvm = new DeviceProfileViewModel(newProfile, isNew: true);
        pvm.ProfileChanged += SaveSettings;
        pvm.DeleteRequested += OnProfileDeleteRequested;
        pvm.DeviceApplyRequested += OnProfileApplyRequested;
        pvm.ToggleDefaultRequested += OnProfileToggleDefaultRequested;
        Profiles.Add(pvm); // Add to the end of the list (triggers SaveSettings)
        Log.Information("Created new profile {ProfileId}", newProfile.Id);
    }

    private void OnProfileDeleteRequested(DeviceProfileViewModel pvm)
    {
        pvm.ProfileChanged -= SaveSettings;
        pvm.DeleteRequested -= OnProfileDeleteRequested;
        pvm.DeviceApplyRequested -= OnProfileApplyRequested;
        pvm.ToggleDefaultRequested -= OnProfileToggleDefaultRequested;

        if (!string.IsNullOrEmpty(pvm.IconPath))
        {
            IconCacheService.DeleteIcon(pvm.IconPath);
        }

        Log.Information("Deleted profile {ProfileId}", pvm.Id);

        _switchingService.ClearPendingProfile(pvm.Id);

        _settingsService.Update(settings =>
        {
            if (settings.LastSelectedProfileId == pvm.Id)
            {
                settings.LastSelectedProfileId = null;
            }

            if (settings.DefaultProfileId == pvm.Id)
            {
                settings.DefaultProfileId = null;
            }
        });

        // This triggers CollectionChanged -> SaveSettings, handling the final save
        Profiles.Remove(pvm);
    }

    private void OnProfileToggleDefaultRequested(DeviceProfileViewModel pvm)
    {
        _settingsService.Update(settings =>
        {
            if (settings.DefaultProfileId == pvm.Id)
            {
                settings.DefaultProfileId = null;
                Log.Information("Unset default profile (was {ProfileId})", pvm.Id);
            }
            else
            {
                settings.DefaultProfileId = pvm.Id;
                Log.Information("Set default profile to {ProfileId}", pvm.Id);
            }
        });

        var updatedSettings = _settingsService.Load();

        foreach (var profile in Profiles)
        {
            profile.IsDefaultProfile = updatedSettings.DefaultProfileId == profile.Id;
        }
    }

    private void OnProfileApplyRequested(DeviceProfileViewModel pvm)
    {
        var model = pvm.GetModel();

        var currentPlayback = _audioService.GetDefaultDevice(NAudio.CoreAudioApi.DataFlow.Render)?.ID;
        var currentCapture = _audioService.GetDefaultDevice(NAudio.CoreAudioApi.DataFlow.Capture)?.ID;

        bool needsSwitch = (model.PlaybackDeviceId != null && model.PlaybackDeviceId != currentPlayback) ||
                           (model.CaptureDeviceId != null && model.CaptureDeviceId != currentCapture);

        var activeProfile = _switchingService.GetCurrentActiveProfile();

        // Even if the devices are already matching, if the logical 'active profile' does not match 
        // (e.g. a newly created profile), or if there is a pending profile that needs to be cleared,
        // we proceed with the switching logic.
        if (!needsSwitch && activeProfile?.Id == model.Id && _switchingService.PendingProfileId == null)
            return;

        _switchingService.SwitchToProfile(model);
    }

    public void SaveSettings()
    {
        _settingsService.Update(settings =>
        {
            settings.SwitchCommunicationDevice = SwitchCommunicationDevice;
            settings.RunAtStartup = RunAtStartup;
            settings.ShowProfileIconInTray = ShowProfileIconInTray;
            settings.ShowProfileChangeNotification = ShowProfileChangeNotification;
            settings.ShowUnavailableDevices = ShowUnavailableDevices;
            settings.Language = Language;
            settings.DeviceProfiles = Profiles.Select(p => p.GetModel()).ToList();
        });

        var activeProfileModel = _switchingService.GetCurrentActiveProfile();

        foreach (var profile in Profiles)
        {
            profile.IsActive = activeProfileModel != null && profile.Id == activeProfileModel.Id;
            profile.IsPending = _switchingService.PendingProfileId == profile.Id;
            var playbackDevice = AvailablePlaybackDevices.FirstOrDefault(d => d.DeviceId == profile.PlaybackDeviceId);
            profile.FallbackDeviceIcon = playbackDevice?.DeviceIcon;
        }

        SettingsChanged?.Invoke();
    }

    public void ReorderProfile(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= Profiles.Count || newIndex < 0 || newIndex >= Profiles.Count)
            return;

        var item = Profiles[oldIndex];
        Profiles.RemoveAt(oldIndex);
        Profiles.Insert(newIndex, item);

        SaveSettings();
    }
}
