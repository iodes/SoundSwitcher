using System.ComponentModel;
using System.Windows;
using SoundSwitcher.Pages;
using Wpf.Ui.Controls;

namespace SoundSwitcher;

/// <summary>
/// Main settings window. Uses WPF-UI NavigationView with collapsed pane
/// (GestureWheel pattern). DataContext is set to MainViewModel by App.
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    /// <summary>
    /// Shows and activates the window from the tray icon.
    /// Refreshes devices to reflect any changes since last shown.
    /// </summary>
    public void ShowAndActivate()
    {
        if (DataContext is ViewModels.MainViewModel vm)
            vm.RefreshDevices();

        Show();
        Activate();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RootNavigation.Navigate(typeof(DevicesPage));
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    /// <summary>
    /// Propagates the MainWindow's DataContext to newly navigated pages,
    /// since NavigationView creates page instances without inheriting DataContext.
    /// </summary>
    private void RootNavigation_OnNavigated(NavigationView sender, NavigatedEventArgs args)
    {
        if (args.Page is FrameworkElement page && page.DataContext == null)
        {
            page.DataContext = DataContext;
        }
    }
}