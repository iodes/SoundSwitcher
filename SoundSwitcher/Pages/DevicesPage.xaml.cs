using System.Windows;
using System.Windows.Controls;
using SoundSwitcher.ViewModels;

namespace SoundSwitcher.Pages;

/// <summary>
/// Devices tab page — displays playback and capture device lists
/// bound to the MainViewModel via DataContext inheritance.
/// </summary>
public partial class DevicesPage
{
    public DevicesPage()
    {
        InitializeComponent();
    }

    private void ProfileBorder_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: DeviceProfileViewModel vm })
        {
            if (DataContext is MainViewModel mainVm)
            {
                mainVm.FocusedProfileId = vm.Id;
            }

            vm.NotifyMenuState();
        }
    }

    private void ProfileBorder_ContextMenuClosing(object sender, ContextMenuEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: DeviceProfileViewModel vm })
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
