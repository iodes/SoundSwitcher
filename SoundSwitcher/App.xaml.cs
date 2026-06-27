using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Serilog;
using SoundSwitcher.Services;
using SoundSwitcher.ViewModels;
using SoundSwitcher.Controls;
using SoundSwitcher.Helpers;
using SoundSwitcher.Models;
using System.Linq;
using MenuItem = Wpf.Ui.Controls.MenuItem;
using SymbolIcon = Wpf.Ui.Controls.SymbolIcon;
using SymbolRegular = Wpf.Ui.Controls.SymbolRegular;

namespace SoundSwitcher;

/// <summary>
/// Application entry point. Initialises services, ViewModel, tray icon,
/// and main window. Follows the GestureWheel startup pattern with
/// Startup/Exit event handlers and TaskbarIcon tray management.
/// </summary>
public partial class App
{
    #region Fields
    private Mutex _mutex = null!;
    private EventWaitHandle _activateEvent = null!;
    private MainWindow _mainWindow = null!;
    private TaskbarIcon _taskbarIcon = null!;
    private ContextMenu _trayContextMenu = null!;
    private ProfileChangeNotification? _currentNotification;
    #endregion

    #region Static Services
    public static AudioDeviceService AudioService { get; private set; } = null!;

    public static SettingsService SettingsService { get; private set; } = null!;

    public static DeviceSwitchingService SwitchingService { get; private set; } = null!;

    public static MainViewModel ViewModel { get; private set; } = null!;

    /// <summary>
    /// Called by ViewModels/Pages after settings change to refresh the tray icon.
    /// </summary>
    public static void NotifySettingsChanged()
    {
        if (Current is App app)
            app.UpdateTrayIcon();
    }
    #endregion

    #region Startup / Exit
    private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            Log.Error(exception, "Unhandled exception occurred");
            MessageBox.Show(exception.Message, "SoundSwitcher", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

        var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SoundSwitcher", "logs", "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("Application started");

        _mutex = new Mutex(true, "SoundSwitcher.App.Mutex", out bool createdNew);

        if (!createdNew)
        {
            if (EventWaitHandle.TryOpenExisting("SoundSwitcher.App.ActivateEvent", out var ewh))
            {
                ewh.Set();
            }

            Current.Shutdown();
            return;
        }

        _activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "SoundSwitcher.App.ActivateEvent");

        Task.Factory.StartNew(() =>
        {
            try
            {
                while (true)
                {
                    _activateEvent.WaitOne();
                    Dispatcher.Invoke(ShowWithActivate);
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
        }, TaskCreationOptions.LongRunning);

#if RELEASE
        // Initialise WinSparkle only in Release builds
        try
        {
            var attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyMetadataAttribute), false);
            var appcastAttr = attributes.OfType<System.Reflection.AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "AppcastUrl");

            if (appcastAttr != null && !string.IsNullOrEmpty(appcastAttr.Value))
            {
                WinSparkleNative.win_sparkle_set_appcast_url(appcastAttr.Value);
                WinSparkleNative.win_sparkle_set_eddsa_public_key("8JV9mpX07duHEHUmrM7/Ref8TIuzh9IWZc+Of4dL0PI=");

                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                var versionString = version?.ToString() ?? "1.0.0.0";
                WinSparkleNative.win_sparkle_set_app_details("Kevin", "SoundSwitcher", versionString);

                WinSparkleNative.win_sparkle_init();
            }
        }
        catch
        {
            /* Ignore if DLL not found */
        }
#endif

        // Initialise services
        AudioService = new AudioDeviceService();
        SettingsService = new SettingsService();
        SwitchingService = new DeviceSwitchingService(AudioService, SettingsService);

        LogAudioDevices();

        // Initialise Localization
        Localization.LocalizationManager.Instance.ApplyFromSettings(SettingsService.Load().Language);

        // Initialise ViewModel
        ViewModel = new MainViewModel(AudioService, SettingsService, SwitchingService);
        ViewModel.SettingsChanged += NotifySettingsChanged;

        AudioService.DevicesChanged += () =>
        {
            Dispatcher.Invoke(UpdateTrayIcon);
        };

        SwitchingService.ProfileSwitched += profile =>
        {
            Dispatcher.Invoke(() => ShowProfileNotification(profile));
        };

        SwitchingService.PendingProfileChanged += () =>
        {
            Dispatcher.Invoke(UpdateTrayIcon);
        };

        // Initialize startup state (Default Profile vs Restoring Last Session)
        var startupSettings = SettingsService.Load();
        bool defaultProfileApplied = false;

        if (startupSettings.DefaultProfileId.HasValue)
        {
            var defaultProfile = startupSettings.DeviceProfiles.FirstOrDefault(p => p.Id == startupSettings.DefaultProfileId.Value);

            if (defaultProfile != null)
            {
                var activeProfile = SwitchingService.GetCurrentActiveProfile();

                if (activeProfile == null || activeProfile.Id != defaultProfile.Id)
                {
                    SwitchingService.SwitchToProfile(defaultProfile);
                }

                defaultProfileApplied = true;
            }
        }

        if (!defaultProfileApplied)
        {
            SwitchingService.RestoreStartupState();
        }

        InitializeUserInterface();

#if DEBUG
        ShowWithActivate();
#else
        if (Environment.GetCommandLineArgs().Contains("/Activate", StringComparer.OrdinalIgnoreCase))
        {
            ShowWithActivate();
        }
#endif
    }

    private void App_OnExit(object sender, ExitEventArgs e)
    {
        try
        {
#if RELEASE
            try { WinSparkleNative.win_sparkle_cleanup(); }
            catch
            {
                // ignored
            }
#endif
            _mainWindow.Close();
            _taskbarIcon.IsEnabled = false;
            _taskbarIcon.Dispose();
            Log.Information("Application exited");
        }
        finally
        {
            _mutex.Dispose();
            _activateEvent.Dispose();
            Log.CloseAndFlush();
        }
    }

    private void LogAudioDevices()
    {
        Log.Information("--- Audio Devices ---");
        var activeDevices = AudioService.GetActiveDevices();

        var defaultPlayback = AudioService.GetDefaultDevice(NAudio.CoreAudioApi.DataFlow.Render);
        var defaultPlaybackComm = AudioService.GetDefaultCommunicationDevice(NAudio.CoreAudioApi.DataFlow.Render);
        var defaultCapture = AudioService.GetDefaultDevice(NAudio.CoreAudioApi.DataFlow.Capture);
        var defaultCaptureComm = AudioService.GetDefaultCommunicationDevice(NAudio.CoreAudioApi.DataFlow.Capture);

        Log.Information("Default Playback Device: {0}", defaultPlayback?.FriendlyName ?? "None");
        Log.Information("Default Communication Playback Device: {0}", defaultPlaybackComm?.FriendlyName ?? "None");
        Log.Information("Default Capture Device: {0}", defaultCapture?.FriendlyName ?? "None");
        Log.Information("Default Communication Capture Device: {0}", defaultCaptureComm?.FriendlyName ?? "None");

        Log.Information("Available Playback Devices:");
        foreach (var device in activeDevices.Where(d => d.DeviceType == AudioDeviceType.Playback))
        {
            Log.Information(" - {0} [{1}]", device.Name, device.State);
        }

        Log.Information("Available Capture Devices:");
        foreach (var device in activeDevices.Where(d => d.DeviceType == AudioDeviceType.Capture))
        {
            Log.Information(" - {0} [{1}]", device.Name, device.State);
        }
        Log.Information("-----------------------");
    }
    #endregion

    #region UI Initialisation
    private void ShowWithActivate()
    {
        _mainWindow.ShowAndActivate();
    }

    private void OpenSystemSoundSettings()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "mmsys.cpl",
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore if it fails to launch
        }
    }

    private void InitializeUserInterface()
    {
        _mainWindow = new MainWindow { DataContext = ViewModel };

        _taskbarIcon = new TaskbarIcon
        {
            IsEnabled = true,
            Icon = GetAppIcon(),
            ToolTipText = "SoundSwitcher"
        };

        // Primary action (Click / Double-click): switch to next preferred device
        _taskbarIcon.TrayLeftMouseDown += OnTrayPrimaryAction;
        _taskbarIcon.TrayMouseDoubleClick += OnTrayPrimaryAction;

        // Context menu
        _trayContextMenu = new ContextMenu();
        _trayContextMenu.Style = (Style)Current.Resources["TrayContextMenuStyle"];
        var menuSettings = CreateMenuItem(Localization.LocalizationManager.Instance["TrayMenuSettings"], ShowWithActivate, SymbolRegular.Settings24);

        var menuSystemSoundSettings = CreateMenuItem(Localization.LocalizationManager.Instance["TrayMenuSystemSound"], OpenSystemSoundSettings, SymbolRegular.Speaker224);

        var menuExit = CreateMenuItem(Localization.LocalizationManager.Instance["TrayMenuExit"], () => Current.Shutdown(), SymbolRegular.ArrowExit20);

        _trayContextMenu.Items.Add(menuSettings);
        _trayContextMenu.Items.Add(menuSystemSoundSettings);

        _trayContextMenu.Items.Add(new Separator());
        _trayContextMenu.Items.Add(menuExit);

        Localization.LocalizationManager.Instance.PropertyChanged += (_, _) =>
        {
            menuSettings.Header = Localization.LocalizationManager.Instance["TrayMenuSettings"];
            menuSystemSoundSettings.Header = Localization.LocalizationManager.Instance["TrayMenuSystemSound"];
            menuExit.Header = Localization.LocalizationManager.Instance["TrayMenuExit"];
            UpdateTrayIcon();
        };

        _taskbarIcon.TrayRightMouseUp += OnTrayRightMouseUp;

        UpdateTrayIcon();
    }
    #endregion

    #region Tray Icon
    private void OnTrayRightMouseUp(object sender, RoutedEventArgs e)
    {
        global::Windows.Win32.PInvoke.GetCursorPos(out var point);

        var helper = new System.Windows.Interop.WindowInteropHelper(_mainWindow);
        _ = global::Windows.Win32.PInvoke.SetForegroundWindow(new Windows.Win32.Foundation.HWND(helper.EnsureHandle()));

        double dpiX;
        double dpiY;

        using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            dpiX = graphics.DpiX / 96.0;
            dpiY = graphics.DpiY / 96.0;
        }

        _trayContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
        _trayContextMenu.HorizontalOffset = point.X / dpiX;
        _trayContextMenu.VerticalOffset = point.Y / dpiY;
        _trayContextMenu.IsOpen = true;
    }

    private void OnTrayPrimaryAction(object sender, RoutedEventArgs e)
    {
        var profileCount = ViewModel.Profiles.Count;

        switch (profileCount)
        {
            case 0:
                ShowWithActivate();
                return;

            case 1:
            {
                var activeProfile = SwitchingService.GetCurrentActiveProfile();

                if (activeProfile != null && activeProfile.Id == ViewModel.Profiles[0].Id)
                {
                    return;
                }

                break;
            }
        }

        SwitchingService.SwitchToNextProfile();
    }

    private void ShowProfileNotification(Models.DeviceProfile profile)
    {
        if (_taskbarIcon == null)
            return;

        var settings = SettingsService.Load();

        if (!settings.ShowProfileChangeNotification)
            return;

        string msg = "";

        if (profile.PlaybackDeviceId != null)
        {
            var pName = GetDeviceName(profile.PlaybackDeviceId, profile.LastKnownPlaybackDeviceName);
            if (!string.IsNullOrEmpty(pName)) msg += $"🔊 {pName}\n";
        }

        if (profile.CaptureDeviceId != null)
        {
            var cName = GetDeviceName(profile.CaptureDeviceId, profile.LastKnownCaptureDeviceName);
            if (!string.IsNullOrEmpty(cName)) msg += $"🎤 {cName}";
        }

        if (_currentNotification is { IsClosed: false })
        {
            // If an existing popup is open, update its content only (extend timer without resetting animation)
            _currentNotification.UpdateMessage(msg.TrimEnd());
        }
        else
        {
            _taskbarIcon.CloseBalloon();

            _currentNotification = new ProfileChangeNotification(msg.TrimEnd(), () =>
            {
                _taskbarIcon.CloseBalloon();
                _currentNotification = null;
            });

            // Apply margins based on Taskbar position to mimic native Windows 11 notification placement
            var tbPos = TaskbarHelper.GetTaskbarPosition();

            _currentNotification.Margin = tbPos switch
            {
                TaskbarPosition.Top => new Thickness(0, 10, 10, 0),
                TaskbarPosition.Left => new Thickness(10, 0, 0, 10),
                _ => new Thickness(0, 0, 10, 10)
            };

            _taskbarIcon.ShowCustomBalloon(_currentNotification, System.Windows.Controls.Primitives.PopupAnimation.None, null);
        }
    }

    private void UpdateTrayIcon()
    {
        if (_taskbarIcon == null)
            return;

        var activeProfile = SwitchingService.GetCurrentActiveProfile();

        if (activeProfile == null && SwitchingService.PendingProfileId == null)
        {
            _taskbarIcon.ToolTipText = "SoundSwitcher - " + Localization.LocalizationManager.Instance["TrayNoDevice"];
            _taskbarIcon.ClearValue(TaskbarIcon.IconSourceProperty);
            _taskbarIcon.Icon = GetAppIcon();
            return;
        }

        string tooltip = "SoundSwitcher";

        if (SwitchingService.PendingProfileId != null)
        {
            // If there is a pending profile, show the pending profile's devices instead of the active profile.
            // Use the hourglass icon (⏳) only for devices that are actually inactive (pending).
            var settings = SettingsService.Load();
            var pendingProfile = settings.DeviceProfiles.FirstOrDefault(p => p.Id == SwitchingService.PendingProfileId);

            if (pendingProfile != null)
            {
                if (pendingProfile.PlaybackDeviceId != null)
                {
                    var pName = GetDeviceName(pendingProfile.PlaybackDeviceId, pendingProfile.LastKnownPlaybackDeviceName);

                    if (!string.IsNullOrEmpty(pName))
                    {
                        string icon = AudioService.IsDeviceActive(pendingProfile.PlaybackDeviceId) ? "🔊" : "⏳";
                        tooltip += $"\n{icon} {pName}";
                    }
                }

                if (pendingProfile.CaptureDeviceId != null)
                {
                    var cName = GetDeviceName(pendingProfile.CaptureDeviceId, pendingProfile.LastKnownCaptureDeviceName);

                    if (!string.IsNullOrEmpty(cName))
                    {
                        string icon = AudioService.IsDeviceActive(pendingProfile.CaptureDeviceId) ? "🎤" : "⏳";
                        tooltip += $"\n{icon} {cName}";
                    }
                }
            }
        }
        else if (activeProfile != null)
        {
            // Show the currently active profile's devices when not pending
            if (activeProfile.PlaybackDeviceId != null)
            {
                var pName = GetDeviceName(activeProfile.PlaybackDeviceId, activeProfile.LastKnownPlaybackDeviceName);
                if (!string.IsNullOrEmpty(pName)) tooltip += $"\n🔊 {pName}";
            }

            if (activeProfile.CaptureDeviceId != null)
            {
                var cName = GetDeviceName(activeProfile.CaptureDeviceId, activeProfile.LastKnownCaptureDeviceName);
                if (!string.IsNullOrEmpty(cName)) tooltip += $"\n🎤 {cName}";
            }
        }

        _taskbarIcon.ToolTipText = tooltip;

        // Check if user enabled profile icon in tray
        var finalSettings = SettingsService.Load();

        if (!finalSettings.ShowProfileIconInTray || (activeProfile == null && SwitchingService.PendingProfileId == null))
        {
            _taskbarIcon.ClearValue(TaskbarIcon.IconSourceProperty);
            _taskbarIcon.Icon = GetAppIcon();
            return;
        }

        var profileForIcon = activeProfile ?? finalSettings.DeviceProfiles.FirstOrDefault(p => p.Id == SwitchingService.PendingProfileId);

        if (profileForIcon == null)
        {
            _taskbarIcon.ClearValue(TaskbarIcon.IconSourceProperty);
            _taskbarIcon.Icon = GetAppIcon();
            return;
        }

        // Try loading user-configured custom icon
        string? fullIconPath = IconCacheService.GetIconFullPath(profileForIcon.IconPath);

        if (fullIconPath != null)
        {
            try
            {
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(fullIconPath);
                bitmap.EndInit();
                bitmap.Freeze();

                _taskbarIcon.IconSource = bitmap;
                return;
            }
            catch
            {
                // Fall through
            }
        }

        // Fallback to Native Device Icon
        var fallbackDevice = ViewModel.AvailablePlaybackDevices.FirstOrDefault(d => d.DeviceId == profileForIcon.PlaybackDeviceId);

        if (fallbackDevice?.DeviceIcon != null)
        {
            _taskbarIcon.IconSource = fallbackDevice.DeviceIcon;
            return;
        }

        // Fallback to default app icon
        _taskbarIcon.ClearValue(TaskbarIcon.IconSourceProperty);
        _taskbarIcon.Icon = GetAppIcon();
    }

    private static Icon GetAppIcon()
    {
        try
        {
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;

            if (!string.IsNullOrEmpty(exePath))
            {
                var extracted = Icon.ExtractAssociatedIcon(exePath);

                if (extracted != null)
                    return extracted;
            }
        }
        catch
        {
            // Ignore
        }

        return SystemIcons.Application;
    }

    private static string? GetDeviceName(string deviceId, string? fallbackName)
    {
        return AudioService.GetDeviceName(deviceId) ?? fallbackName;
    }
    #endregion

    #region Helpers
    private static MenuItem CreateMenuItem(string header, Action action, SymbolRegular? icon = null)
    {
        var item = new MenuItem { Header = header };

        if (icon is not null)
            item.Icon = new SymbolIcon(icon.Value);

        item.Click += delegate { action(); };
        return item;
    }
    #endregion
}
