using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SoundSwitcher.ViewModels;
using System.Windows.Media;

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

    private void ProfileBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var dObj = e.OriginalSource as DependencyObject;
        bool shouldIgnore = false;
        
        while (dObj != null)
        {
            if (dObj is ComboBox)
            {
                shouldIgnore = true;
                break;
            }

            if (dObj is FrameworkElement fe)
            {
                if (Behaviors.LiveReorderBehavior.GetIsReorderGrip(fe))
                {
                    shouldIgnore = true;
                    break;
                }
                
                if (fe.ToolTip?.ToString() == "아이콘 변경 (좌클릭)")
                {
                    shouldIgnore = true;
                    break;
                }
            }

            dObj = VisualTreeHelper.GetParent(dObj);
        }

        if (shouldIgnore)
            return;

        if (sender is Border border && border.DataContext is DeviceProfileViewModel vm)
        {
            if (vm.ApplyCommand.CanExecute(null))
            {
                vm.ApplyCommand.Execute(null);
            }
        }
    }
}
