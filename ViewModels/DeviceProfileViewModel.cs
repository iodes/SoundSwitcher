using SoundSwitcher.Models;
using SoundSwitcher.Services;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace SoundSwitcher.ViewModels;

public class DeviceProfileViewModel : ViewModelBase
{
    private readonly DeviceProfile _profile;
    private string? _playbackDeviceId;
    private string? _captureDeviceId;
    private string? _iconPath;
    private bool _isFocused;
    private bool _isActive;
    private ImageSource? _fallbackDeviceIcon;

    public Guid Id => _profile.Id;

    public string? PlaybackDeviceId
    {
        get => _playbackDeviceId;
        set
        {
            if (value == null && _playbackDeviceId != null)
                return; // Prevent WPF ComboBox from resetting to null when items source refreshes or device is unplugged

            if (SetProperty(ref _playbackDeviceId, value))
            {
                bool wasActive = IsActive;
                _profile.PlaybackDeviceId = value;
                OnPropertyChanged(nameof(DisplayName));
                
                if (wasActive && value != null)
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
                return; // Prevent WPF ComboBox from resetting to null when items source refreshes or device is unplugged

            if (SetProperty(ref _captureDeviceId, value))
            {
                bool wasActive = IsActive;
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
                ProfileChanged?.Invoke();
            }
        }
    }

    public string DisplayName
    {
        get
        {
            if (string.IsNullOrEmpty(PlaybackDeviceId) && string.IsNullOrEmpty(CaptureDeviceId))
                return "장치 미설정";

            return "프로파일";
        }
    }

    public DeviceProfile GetModel() => _profile;

    public event Action? ProfileChanged;
    public event Action<DeviceProfileViewModel>? DeleteRequested;
    public event Action<DeviceProfileViewModel>? DeviceApplyRequested;

    public ICommand DeleteCommand { get; }
    public ICommand ChangeIconCommand { get; }
    public ICommand ResetIconCommand { get; }

    private bool _isDeleting;
    public bool IsDeleting
    {
        get => _isDeleting;
        set => SetProperty(ref _isDeleting, value);
    }

    public bool IsFocused
    {
        get => _isFocused;
        set => SetProperty(ref _isFocused, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public ImageSource? FallbackDeviceIcon
    {
        get => _fallbackDeviceIcon;
        set => SetProperty(ref _fallbackDeviceIcon, value);
    }

    private bool _isNew;
    public bool IsNew
    {
        get => _isNew;
        set => SetProperty(ref _isNew, value);
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

        DeleteCommand = new RelayCommand(async () => 
        {
            IsDeleting = true;
            await Task.Delay(350);
            DeleteRequested?.Invoke(this);
        });

        ChangeIconCommand = new RelayCommand(() =>
        {
            var dialog = new OpenFileDialog
            {
                Title = "프로파일 아이콘 선택",
                Filter = "이미지 파일|*.ico;*.png;*.jpg;*.jpeg"
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
            }
        });
    }
}
