using SoundSwitcher.Models;
using SoundSwitcher.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;

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
    private Guid? _focusedProfileId;
    private bool _isReordering;

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

    public Guid? FocusedProfileId
    {
        get => _focusedProfileId;
        set
        {
            if (SetProperty(ref _focusedProfileId, value))
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
        get => _isReordering;
        set => SetProperty(ref _isReordering, value);
    }

    public string AppVersion =>
        $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"}";

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
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RefreshDevices();
            });
        };
    }

    private void LoadSettings()
    {
        var settings = _settingsService.Load();
        _switchCommunicationDevice = settings.SwitchCommunicationDevice;
        _runAtStartup = settings.RunAtStartup;
        _showProfileIconInTray = settings.ShowProfileIconInTray;

        Profiles.Clear();
        foreach (var profile in settings.DeviceProfiles)
        {
            var pvm = new DeviceProfileViewModel(profile, isNew: false);
            pvm.ProfileChanged += SaveSettings;
            pvm.DeleteRequested += OnProfileDeleteRequested;
            pvm.DeviceApplyRequested += OnProfileApplyRequested;
            Profiles.Add(pvm);
        }
    }

    public void RefreshDevices()
    {
        var devices = _audioService.GetActiveDevices();

        var currentPlayback = devices.Where(d => d.DeviceType == AudioDeviceType.Playback).ToList();
        var currentCapture = devices.Where(d => d.DeviceType == AudioDeviceType.Capture).ToList();

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
            if (!source.Any(d => d.DeviceId == target[i].DeviceId))
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
        Profiles.Add(pvm); // Add to the end of the list
        SaveSettings();
    }

    private void OnProfileDeleteRequested(DeviceProfileViewModel pvm)
    {
        pvm.ProfileChanged -= SaveSettings;
        pvm.DeleteRequested -= OnProfileDeleteRequested;
        pvm.DeviceApplyRequested -= OnProfileApplyRequested;

        if (!string.IsNullOrEmpty(pvm.IconPath))
        {
            IconCacheService.DeleteIcon(pvm.IconPath);
        }

        Profiles.Remove(pvm);

        var oldSettings = _settingsService.Load();
        if (oldSettings.LastSelectedProfileId == pvm.Id)
        {
            oldSettings.LastSelectedProfileId = null;
            _settingsService.Save(oldSettings);
        }

        SaveSettings();
    }

    private void OnProfileApplyRequested(DeviceProfileViewModel pvm)
    {
        if (!string.IsNullOrEmpty(pvm.PlaybackDeviceId))
        {
            if (_audioService.IsDeviceActive(pvm.PlaybackDeviceId))
                _audioService.SetDefaultDevice(pvm.PlaybackDeviceId, SwitchCommunicationDevice);
        }
        if (!string.IsNullOrEmpty(pvm.CaptureDeviceId))
        {
            if (_audioService.IsDeviceActive(pvm.CaptureDeviceId))
                _audioService.SetDefaultDevice(pvm.CaptureDeviceId, SwitchCommunicationDevice);
        }
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
            LastSelectedProfileId = oldSettings.LastSelectedProfileId,
            SwitchCommunicationDevice = SwitchCommunicationDevice,
            RunAtStartup = RunAtStartup,
            ShowProfileIconInTray = ShowProfileIconInTray,
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
