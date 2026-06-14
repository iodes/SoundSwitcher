using SoundSwitcher.Models;
using SoundSwitcher.Services;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

namespace SoundSwitcher.ViewModels;

/// <summary>
/// Main ViewModel that manages the entire settings window.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly AudioDeviceService _audioService;
    private readonly SettingsService _settingsService;
    private readonly DeviceSwitchingService _switchingService;

    private bool _switchCommunicationDevice;
    private bool _runAtStartup;
    private bool _showProfileIconInTray;
    private bool _showProfileChangeNotification;
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

        Profiles.CollectionChanged += (_, _) => SaveSettings();

        LoadSettings();
        RefreshDevices();

        // Auto-refresh when devices change
        _audioService.DevicesChanged += () =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(RefreshDevices);
        };
    }

    private void LoadSettings()
    {
        var settings = _settingsService.Load();
        _switchCommunicationDevice = settings.SwitchCommunicationDevice;
        _runAtStartup = settings.RunAtStartup;
        _showProfileIconInTray = settings.ShowProfileIconInTray;
        _showProfileChangeNotification = settings.ShowProfileChangeNotification;
        _language = settings.Language;
        OnPropertyChanged(nameof(Language));

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
        List<AudioDeviceInfo> currentPlayback = devices.Where(d => d.DeviceType == AudioDeviceType.Playback).OrderBy(d => d.Name).ToList();
        List<AudioDeviceInfo> currentCapture = devices.Where(d => d.DeviceType == AudioDeviceType.Capture).OrderBy(d => d.Name).ToList();

        SyncCollection(AvailablePlaybackDevices, currentPlayback);
        SyncCollection(AvailableCaptureDevices, currentCapture);

        var activeProfileModel = _switchingService.GetCurrentActiveProfile();

        foreach (var profile in Profiles)
        {
            profile.IsActive = activeProfileModel != null && profile.Id == activeProfileModel.Id;

            var playbackDevice = AvailablePlaybackDevices.FirstOrDefault(d => d.DeviceId == profile.PlaybackDeviceId);
            profile.FallbackDeviceIcon = playbackDevice?.DeviceIcon;
        }
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
        Profiles.Add(pvm); // Add to the end of the list
        SaveSettings();
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

        Profiles.Remove(pvm);

        var oldSettings = _settingsService.Load();

        bool oldSettingsModified = false;

        if (oldSettings.LastSelectedProfileId == pvm.Id)
        {
            oldSettings.LastSelectedProfileId = null;
            oldSettingsModified = true;
        }

        if (oldSettings.DefaultProfileId == pvm.Id)
        {
            oldSettings.DefaultProfileId = null;
            oldSettingsModified = true;
        }

        if (oldSettingsModified)
        {
            _settingsService.Save(oldSettings);
        }

        SaveSettings();
    }

    private void OnProfileToggleDefaultRequested(DeviceProfileViewModel pvm)
    {
        var oldSettings = _settingsService.Load();

        if (oldSettings.DefaultProfileId == pvm.Id)
        {
            oldSettings.DefaultProfileId = null;
        }
        else
        {
            oldSettings.DefaultProfileId = pvm.Id;
        }

        _settingsService.Save(oldSettings);

        foreach (var profile in Profiles)
        {
            profile.IsDefaultProfile = oldSettings.DefaultProfileId == profile.Id;
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
        // (e.g. a newly created profile), we proceed with the switching logic to update LastSelectedProfileId.
        if (!needsSwitch && activeProfile?.Id == model.Id)
            return;

        _switchingService.SwitchToProfile(model);
        App.NotifyProfileChanged(model);
    }

    public void SaveSettings()
    {
        // Update states before saving, in case PlaybackDeviceId changed and we need a new fallback icon
        var activeProfileModel = _switchingService.GetCurrentActiveProfile();

        foreach (var profile in Profiles)
        {
            profile.IsActive = activeProfileModel != null && profile.Id == activeProfileModel.Id;
            var playbackDevice = AvailablePlaybackDevices.FirstOrDefault(d => d.DeviceId == profile.PlaybackDeviceId);
            profile.FallbackDeviceIcon = playbackDevice?.DeviceIcon;
        }

        var oldSettings = _settingsService.Load();

        var settings = new AppSettings
        {
            DefaultProfileId = oldSettings.DefaultProfileId,
            LastSelectedProfileId = oldSettings.LastSelectedProfileId,
            SwitchCommunicationDevice = SwitchCommunicationDevice,
            RunAtStartup = RunAtStartup,
            ShowProfileIconInTray = ShowProfileIconInTray,
            ShowProfileChangeNotification = ShowProfileChangeNotification,
            Language = Language,
            DeviceProfiles = Profiles.Select(p => p.GetModel()).ToList()
        };

        _settingsService.Save(settings);
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
