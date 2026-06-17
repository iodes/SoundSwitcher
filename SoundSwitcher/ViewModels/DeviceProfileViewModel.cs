using Serilog;
using SoundSwitcher.Models;
using SoundSwitcher.Services;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using SoundSwitcher.Helpers;

namespace SoundSwitcher.ViewModels;

public class DeviceProfileViewModel : ViewModelBase
{
    private readonly DeviceProfile _profile;
    private string? _playbackDeviceId;
    private string? _captureDeviceId;
    private string? _iconPath;

    public Guid Id => _profile.Id;

    public string? PlaybackDeviceId
    {
        get => _playbackDeviceId;
        set
        {
            if (value == null && _playbackDeviceId != null)
                return;

            if (SetProperty(ref _playbackDeviceId, value))
            {
                _profile.PlaybackDeviceId = value;
                OnPropertyChanged(nameof(DisplayName));

                if (IsActive || IsPending)
                    DeviceApplyRequested?.Invoke(this);

                ProfileChanged?.Invoke();
            }
        }
    }

    public string? CaptureDeviceId
    {
        get => _captureDeviceId;
        set
        {
            if (value == null && _captureDeviceId != null)
                return;

            if (SetProperty(ref _captureDeviceId, value))
            {
                bool wasActive = IsActive || IsPending;
                _profile.CaptureDeviceId = value;
                OnPropertyChanged(nameof(DisplayName));

                if (wasActive && value != null)
                    DeviceApplyRequested?.Invoke(this);

                ProfileChanged?.Invoke();
            }
        }
    }

    public string? IconPath
    {
        get => _iconPath;
        set
        {
            if (SetProperty(ref _iconPath, value))
            {
                _profile.IconPath = value;
                OnPropertyChanged(nameof(IconImage));
                ProfileChanged?.Invoke();
            }
        }
    }

    public ImageSource? IconImage
    {
        get
        {
            string? fullPath = IconCacheService.GetIconFullPath(_iconPath);

            if (fullPath == null)
                return null;

            try
            {
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(fullPath);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }

    public string DisplayName
    {
        get
        {
            if (string.IsNullOrEmpty(PlaybackDeviceId) && string.IsNullOrEmpty(CaptureDeviceId))
                return Localization.LocalizationManager.Instance["DeviceNotSet"];

            return Localization.LocalizationManager.Instance["Profile"];
        }
    }

    public DeviceProfile GetModel() => _profile;

    public event Action? ProfileChanged;

    public event Action<DeviceProfileViewModel>? DeleteRequested;

    public event Action<DeviceProfileViewModel>? DeviceApplyRequested;

    public event Action<DeviceProfileViewModel>? ToggleDefaultRequested;

    public ICommand DeleteCommand { get; }

    public ICommand ChangeIconCommand { get; }

    public ICommand ResetIconCommand { get; }

    public ICommand ApplyCommand { get; }

    public ICommand ToggleDefaultCommand { get; }

    public ICommand OpenOutputDevicePropertiesCommand { get; }

    public ICommand OpenInputDevicePropertiesCommand { get; }

    public bool IsDeleting
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsFocused
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsActive
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsPending
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsDefaultProfile
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string ToggleDefaultMenuText => IsDefaultProfile
        ? Localization.LocalizationManager.Instance["UnsetDefaultProfile"]
        : Localization.LocalizationManager.Instance["SetDefaultProfile"];

    public Wpf.Ui.Controls.SymbolRegular ToggleDefaultIcon => IsDefaultProfile
        ? Wpf.Ui.Controls.SymbolRegular.StarOff24
        : Wpf.Ui.Controls.SymbolRegular.Star24;

    public void NotifyMenuState()
    {
        OnPropertyChanged(nameof(ToggleDefaultMenuText));
        OnPropertyChanged(nameof(ToggleDefaultIcon));
    }

    public ImageSource? FallbackDeviceIcon
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsNew
    {
        get;
        set => SetProperty(ref field, value);
    }

    public DeviceProfileViewModel(DeviceProfile profile, bool isNew = false)
    {
        _profile = profile;
        IsNew = isNew;

        if (IsNew)
        {
            Task.Delay(400).ContinueWith(_ => IsNew = false);
        }

        _playbackDeviceId = profile.PlaybackDeviceId;
        _captureDeviceId = profile.CaptureDeviceId;
        _iconPath = profile.IconPath;

        ToggleDefaultCommand = new RelayCommand(() => ToggleDefaultRequested?.Invoke(this));

        DeleteCommand = new RelayCommand(async void () =>
        {
            try
            {
                IsDeleting = true;
                await Task.Delay(350);
                DeleteRequested?.Invoke(this);
            }
            catch
            {
                // ignored
            }
        });

        ChangeIconCommand = new RelayCommand(() =>
        {
            var dialog = new OpenFileDialog
            {
                Title = Localization.LocalizationManager.Instance["SelectProfileIcon"],
                Filter = Localization.LocalizationManager.Instance["ImageFilesFilter"]
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // If replacing an existing custom icon, delete the old cache
                    if (!string.IsNullOrEmpty(IconPath))
                    {
                        IconCacheService.DeleteIcon(IconPath);
                    }

                    string newCachePath = IconCacheService.CacheIcon(dialog.FileName);
                    IconPath = newCachePath;
                    Log.Information("Changed icon for profile {ProfileId}", Id);
                }
                catch (Exception)
                {
                    // Fallback or ignore on error
                }
            }
        });

        ResetIconCommand = new RelayCommand(() =>
        {
            if (!string.IsNullOrEmpty(IconPath))
            {
                IconCacheService.DeleteIcon(IconPath);
                IconPath = null;
                Log.Information("Reset icon for profile {ProfileId}", Id);
            }
        });

        ApplyCommand = new RelayCommand(() =>
        {
            DeviceApplyRequested?.Invoke(this);
        });

        OpenOutputDevicePropertiesCommand = new RelayCommand(async void () =>
        {
            try
            {
                if (!string.IsNullOrEmpty(PlaybackDeviceId))
                {
                    if (!SoundControlPanelHelper.OpenDeviceProperties(PlaybackDeviceId))
                    {
                        await DialogHelper.ShowDialogAsync(
                            Localization.LocalizationManager.Instance["PropertiesWindowAlreadyOpenTitle"],
                            Localization.LocalizationManager.Instance["PropertiesWindowAlreadyOpenMessage"],
                            System.Windows.MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }, () => !string.IsNullOrEmpty(PlaybackDeviceId));

        OpenInputDevicePropertiesCommand = new RelayCommand(async void () =>
        {
            try
            {
                if (!string.IsNullOrEmpty(CaptureDeviceId))
                {
                    if (!SoundControlPanelHelper.OpenDeviceProperties(CaptureDeviceId))
                    {
                        await DialogHelper.ShowDialogAsync(
                            Localization.LocalizationManager.Instance["PropertiesWindowAlreadyOpenTitle"],
                            Localization.LocalizationManager.Instance["PropertiesWindowAlreadyOpenMessage"],
                            System.Windows.MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }, () => !string.IsNullOrEmpty(CaptureDeviceId));
    }
}
