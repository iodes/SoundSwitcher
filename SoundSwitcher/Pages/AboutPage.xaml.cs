using System.Diagnostics;
using System.Windows;
using SoundSwitcher.Helpers;

namespace SoundSwitcher.Pages;

/// <summary>
/// About tab page — displays app version, and provides update and homepage links.
/// AppVersion is bound from the MainViewModel.
/// </summary>
public partial class AboutPage
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private void BtnCheckUpdate_OnClick(object sender, RoutedEventArgs e)
    {
#if RELEASE
        try
        {
            WinSparkleNative.win_sparkle_check_update_with_ui();
        }
        catch
        {
            // ignored
        }
#endif
    }

    private void BtnOpenHomepage_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/iodes/SoundSwitcher",
                UseShellExecute = true
            });
        }
        catch
        {
            // ignored
        }
    }
}
