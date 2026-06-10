using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SoundSwitcher.ViewModels;

namespace SoundSwitcher.Pages;

/// <summary>
/// Devices tab page — displays playback and capture device lists
/// bound to the MainViewModel via DataContext inheritance.
/// </summary>
public partial class DevicesPage : Page
{
    public DevicesPage()
    {
        InitializeComponent();
    }

    private void ProfileBorder_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Border border && border.DataContext is ViewModels.DeviceProfileViewModel vm)
        {
            if (vm.IsNew)
            {
                if (border.Resources["LoadStoryboard"] is System.Windows.Media.Animation.Storyboard sb)
                {
                    sb.Begin(border);
                }
            }
        }
    }

    private void ProfileBorder_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is Border border && border.DataContext is DeviceProfileViewModel vm)
        {
            if (DataContext is MainViewModel mainVm)
            {
                mainVm.FocusedProfileId = vm.Id;
            }
        }
    }

    private void ProfileBorder_ContextMenuClosing(object sender, ContextMenuEventArgs e)
    {
        if (sender is Border border && border.DataContext is DeviceProfileViewModel vm)
        {
            if (DataContext is MainViewModel mainVm)
            {
                // Only clear if the closing context menu belongs to the currently focused profile.
                // This prevents clearing focus if another profile's ContextMenuOpening already fired.
                if (mainVm.FocusedProfileId == vm.Id)
                {
                    mainVm.FocusedProfileId = null;
                }
            }
        }
    }
}
