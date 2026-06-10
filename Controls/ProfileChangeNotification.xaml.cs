using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace SoundSwitcher.Controls;

public partial class ProfileChangeNotification : UserControl
{
    private DispatcherTimer? _closeTimer;
    private Action _onCloseAction;
    private bool _isClosing = false;

    public bool IsClosed => _isClosing;

    #region P/Invoke for Click-Through
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    private static int GetWindowLong(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 8) return (int)GetWindowLongPtr64(hWnd, nIndex);
        else return GetWindowLong32(hWnd, nIndex);
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
    {
        if (IntPtr.Size == 8) return (int)SetWindowLongPtr64(hWnd, nIndex, new IntPtr(dwNewLong));
        else return SetWindowLong32(hWnd, nIndex, dwNewLong);
    }
    #endregion

    public ProfileChangeNotification(string message, Action onCloseAction)
    {
        InitializeComponent();
        MessageText.Text = message;
        _onCloseAction = onCloseAction;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Apply Click-Through at the OS level (using deferred execution to ensure HWND is available)
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                var hwnd = hwndSource.Handle;
                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
            }
        }), DispatcherPriority.Loaded);

        // Start Fade-in
        var fadeIn = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(0.2)));
        RootBorder.BeginAnimation(UIElement.OpacityProperty, fadeIn);

        var moveAnim = new DoubleAnimation(20.0, 0.0, new Duration(TimeSpan.FromSeconds(0.3)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        if (RootBorder.RenderTransform is TranslateTransform transform)
        {
            transform.BeginAnimation(TranslateTransform.YProperty, moveAnim);
        }

        // Start timer to close after 1.5 seconds
        _closeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.5)
        };
        _closeTimer.Tick += CloseTimer_Tick;
        _closeTimer.Start();
    }

    private void CloseTimer_Tick(object? sender, EventArgs e)
    {
        Close();
    }

    public void UpdateMessage(string message)
    {
        MessageText.Text = message;

        // Reset the timeout
        _closeTimer?.Stop();
        _closeTimer?.Start();
    }

    public void Close()
    {
        if (_isClosing) return;
        _isClosing = true;

        _closeTimer?.Stop();

        var fadeOut = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.15)));
        fadeOut.Completed += (s, ev) => _onCloseAction?.Invoke();
        RootBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);
    }
}
