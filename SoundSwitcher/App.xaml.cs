using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using SoundSwitcher.Services;
using SoundSwitcher.ViewModels;
using SoundSwitcher.Controls;
using SoundSwitcher.Helpers;
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

    /// <summary>
    /// Called when profile changes to show the notification toast.
    /// </summary>
    public static void NotifyProfileChanged(Models.DeviceProfile profile)
    {
        if (Current is App app)
            app.ShowProfileNotification(profile);
    }
    #endregion

    #region Startup / Exit
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
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

        // Initialise Localization
        Localization.LocalizationManager.Instance.ApplyFromSettings(SettingsService.Load().Language);

        // Initialise ViewModel
        ViewModel = new MainViewModel(AudioService, SettingsService, SwitchingService);
        ViewModel.SettingsChanged += NotifySettingsChanged;

        AudioService.DevicesChanged += () =>
        {
            Dispatcher.Invoke(UpdateTrayIcon);
        };

        InitializeUserInterface();

        if (Environment.GetCommandLineArgs().Contains("/Activate", StringComparer.OrdinalIgnoreCase))
        {
            ShowWithActivate();
        }
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
        }
        finally
        {
            _mutex.Dispose();
            _activateEvent.Dispose();
        }
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
        var menuSettings = CreateMenuItem(Localization.LocalizationManager.Instance["TrayMenuSettings"], ShowWithActivate, SymbolRegular.Settings24);
        var menuSystemSoundSettings = CreateMenuItem(Localization.LocalizationManager.Instance["TrayMenuSystemSound"], OpenSystemSoundSettings, SymbolRegular.Speaker224);

        var menuExit = CreateMenuItem(Localization.LocalizationManager.Instance["TrayMenuExit"], () => Current.Shutdown(), SymbolRegular.ArrowExit20);

        _trayContextMenu.Items.Add(menuSettings);
        _trayContextMenu.Items.Add(menuSystemSoundSettings);

        _trayContextMenu.Items.Add(new Separator());
        _trayContextMenu.Items.Add(menuExit);

        Localization.LocalizationManager.Instance.PropertyChanged += (s, e) =>
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

        var switchedProfile = SwitchingService.SwitchToNextProfile();

        if (switchedProfile == null)
            return;

        UpdateTrayIcon();
        ShowProfileNotification(switchedProfile);
    }

    private void ShowProfileNotification(Models.DeviceProfile profile)
    {
        var settings = SettingsService.Load();

        if (!settings.ShowProfileChangeNotification)
            return;

        string msg = "";

        if (profile.PlaybackDeviceId != null)
        {
            var pName = AudioService.GetDeviceName(profile.PlaybackDeviceId);
            if (!string.IsNullOrEmpty(pName)) msg += $"🔊 {pName}\n";
        }

        if (profile.CaptureDeviceId != null)
        {
            var cName = AudioService.GetDeviceName(profile.CaptureDeviceId);
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
        var activeProfile = SwitchingService.GetCurrentActiveProfile();

        if (activeProfile == null)
        {
            _taskbarIcon.ToolTipText = "SoundSwitcher - " + Localization.LocalizationManager.Instance["TrayNoDevice"];
            _taskbarIcon.ClearValue(TaskbarIcon.IconSourceProperty);
            _taskbarIcon.Icon = GetAppIcon();
            return;
        }

        string tooltip = "SoundSwitcher";

        if (activeProfile.PlaybackDeviceId != null)
        {
            var pName = AudioService.GetDeviceName(activeProfile.PlaybackDeviceId);
            if (!string.IsNullOrEmpty(pName)) tooltip += $"\n🔊 {pName}";
        }

        if (activeProfile.CaptureDeviceId != null)
        {
            var cName = AudioService.GetDeviceName(activeProfile.CaptureDeviceId);
            if (!string.IsNullOrEmpty(cName)) tooltip += $"\n🎤 {cName}";
        }

        _taskbarIcon.ToolTipText = tooltip;

        // Check if user enabled profile icon in tray
        var settings = SettingsService.Load();

        if (!settings.ShowProfileIconInTray)
        {
            _taskbarIcon.ClearValue(TaskbarIcon.IconSourceProperty);
            _taskbarIcon.Icon = GetAppIcon();
            return;
        }

        // Try loading user-configured custom icon
        string? fullIconPath = IconCacheService.GetIconFullPath(activeProfile.IconPath);

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
        var fallbackDevice = ViewModel.AvailablePlaybackDevices.FirstOrDefault(d => d.DeviceId == activeProfile.PlaybackDeviceId);

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
