using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SoundSwitcher.Behaviors;

public static class AnimatedPressBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(AnimatedPressBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty IgnorePressProperty =
        DependencyProperty.RegisterAttached(
            "IgnorePress",
            typeof(bool),
            typeof(AnimatedPressBehavior),
            new PropertyMetadata(false));

    public static bool GetIgnorePress(DependencyObject obj) => (bool)obj.GetValue(IgnorePressProperty);
    public static void SetIgnorePress(DependencyObject obj, bool value) => obj.SetValue(IgnorePressProperty, value);

    public static readonly DependencyProperty HoverBackgroundProperty =
        DependencyProperty.RegisterAttached(
            "HoverBackground",
            typeof(Brush),
            typeof(AnimatedPressBehavior),
            new PropertyMetadata(null));

    public static Brush GetHoverBackground(DependencyObject obj) => (Brush)obj.GetValue(HoverBackgroundProperty);
    public static void SetHoverBackground(DependencyObject obj, Brush value) => obj.SetValue(HoverBackgroundProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.PreviewMouseLeftButtonDown += Element_PreviewMouseLeftButtonDown;
                element.PreviewMouseLeftButtonUp += Element_PreviewMouseLeftButtonUp;
                element.MouseEnter += Element_MouseEnter;
                element.MouseLeave += Element_MouseLeave;
            }
            else
            {
                element.PreviewMouseLeftButtonDown -= Element_PreviewMouseLeftButtonDown;
                element.PreviewMouseLeftButtonUp -= Element_PreviewMouseLeftButtonUp;
                element.MouseEnter -= Element_MouseEnter;
                element.MouseLeave -= Element_MouseLeave;
            }
        }
    }

    private static void Element_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is System.Windows.Controls.Control control)
        {
            var hoverBrush = GetHoverBackground(control);
            if (hoverBrush != null)
            {
                control.SetCurrentValue(System.Windows.Controls.Control.BackgroundProperty, hoverBrush);
            }
        }
    }

    private static void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (ShouldIgnorePress(e.OriginalSource as DependencyObject, element))
                return;

            AnimateScale(element, 0.98);
        }
    }

    private static bool ShouldIgnorePress(DependencyObject? originalSource, DependencyObject rootElement)
    {
        DependencyObject? current = originalSource;
        while (current != null && current != rootElement)
        {
            if (GetIgnorePress(current))
            {
                return true;
            }

            // Automatically ignore presses originating from inner interactive controls
            if (current is System.Windows.Controls.Primitives.ButtonBase ||
                current is System.Windows.Controls.Primitives.TextBoxBase ||
                current is System.Windows.Controls.ComboBox ||
                current is System.Windows.Controls.Primitives.Thumb ||
                current is System.Windows.Controls.Slider)
            {
                return true;
            }

            DependencyObject? parent = VisualTreeHelper.GetParent(current);
            if (parent == null && current is FrameworkElement element)
            {
                parent = element.Parent;
            }
            current = parent;
        }
        return false;
    }

    private static void Element_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            AnimateScale(element, 1.0);
        }
    }

    private static void Element_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            AnimateScale(element, 1.0);

            if (element is System.Windows.Controls.Control control && GetHoverBackground(control) != null)
            {
                control.SetCurrentValue(System.Windows.Controls.Control.BackgroundProperty, DependencyProperty.UnsetValue);
            }
        }
    }

    private static void AnimateScale(FrameworkElement element, double targetScale)
    {
        // Ensure RenderTransformOrigin is center
        if (element.RenderTransformOrigin == new Point(0, 0))
        {
            element.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        if (element.RenderTransform == null || element.RenderTransform == Transform.Identity)
        {
            element.RenderTransform = new TransformGroup { Children = { new ScaleTransform(1, 1) } };
        }
        else if (element.RenderTransform.IsFrozen)
        {
            element.RenderTransform = element.RenderTransform.CloneCurrentValue();
        }

        ScaleTransform? scaleTransform = null;

        if (element.RenderTransform is ScaleTransform st)
        {
            scaleTransform = st;
        }
        else if (element.RenderTransform is TransformGroup group)
        {
            scaleTransform = group.Children.OfType<ScaleTransform>().FirstOrDefault();
            if (scaleTransform == null)
            {
                scaleTransform = new ScaleTransform(1, 1);
                group.Children.Add(scaleTransform);
            }
        }

        if (scaleTransform != null)
        {
            var anim = new DoubleAnimation
            {
                To = targetScale,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }
    }
}
