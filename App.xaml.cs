using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using SoundSwitcher.Services;
using SoundSwitcher.ViewModels;
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
    private MainWindow _mainWindow = null!;
    private TaskbarIcon _taskbarIcon = null!;
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
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        // Initialise services
        AudioService = new AudioDeviceService();
        SettingsService = new SettingsService();
        SwitchingService = new DeviceSwitchingService(AudioService, SettingsService);

        // Initialise ViewModel
        ViewModel = new MainViewModel(AudioService, SettingsService, SwitchingService);
        ViewModel.SettingsChanged += () => NotifySettingsChanged();

        AudioService.DevicesChanged += () =>
        {
            Dispatcher.Invoke(() => UpdateTrayIcon());
        };

        InitializeUserInterface();
    }

    private void App_OnExit(object sender, ExitEventArgs e)
    {
        _mainWindow?.Close();

        if (_taskbarIcon is null) return;

        _taskbarIcon.IsEnabled = false;
        _taskbarIcon.Dispose();
    }
    #endregion

    #region UI Initialisation
    private void ShowWithActivate()
    {
        _mainWindow?.ShowAndActivate();
    }

    private void InitializeUserInterface()
    {
        _mainWindow = new MainWindow { DataContext = ViewModel };

        _taskbarIcon = new TaskbarIcon
        {
            IsEnabled = true,
            ContextMenu = new ContextMenu(),
            Icon = GetAppIcon(),
            ToolTipText = "SoundSwitcher"
        };

        // Left-click: switch to next preferred device
        _taskbarIcon.TrayLeftMouseUp += OnTrayLeftClick;

        // Context menu
        var menuSettings = CreateMenuItem("설정", ShowWithActivate, SymbolRegular.Settings24);
        var menuExit = CreateMenuItem("종료", () => Current.Shutdown(), SymbolRegular.ArrowExit20);

        _taskbarIcon.ContextMenu.Items.Add(menuSettings);
        _taskbarIcon.ContextMenu.Items.Add(new Separator());
        _taskbarIcon.ContextMenu.Items.Add(menuExit);

        UpdateTrayIcon();
    }
    #endregion

    #region Tray Icon
    private void OnTrayLeftClick(object sender, RoutedEventArgs e)
    {
        var switchedProfile = SwitchingService.SwitchToNextProfile();
        if (switchedProfile == null) return;

        UpdateTrayIcon();
        
        string msg = "";
        if (switchedProfile.PlaybackDeviceId != null)
        {
            var pName = AudioService.GetDeviceName(switchedProfile.PlaybackDeviceId);
            if (!string.IsNullOrEmpty(pName)) msg += $"🔊 {pName}\n";
        }
        if (switchedProfile.CaptureDeviceId != null)
        {
            var cName = AudioService.GetDeviceName(switchedProfile.CaptureDeviceId);
            if (!string.IsNullOrEmpty(cName)) msg += $"🎤 {cName}";
        }

        _taskbarIcon.ShowBalloonTip(
            "장치 전환됨",
            msg.TrimEnd(),
            BalloonIcon.Info);
    }

    private void UpdateTrayIcon()
    {
        var activeProfile = SwitchingService.GetCurrentActiveProfile();
        if (activeProfile == null)
        {
            _taskbarIcon.ToolTipText = "SoundSwitcher - 장치 없음";
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
        if (activeProfile.IconPath != null && File.Exists(activeProfile.IconPath))
        {
            try
            {
                _taskbarIcon.IconSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(activeProfile.IconPath));
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
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "app.ico");
            if (File.Exists(iconPath))
                return new Icon(iconPath);
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
