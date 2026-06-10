using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace SoundSwitcher.Controls;

/// <summary>
/// A toggle switch card that inherits from CardControl.
/// Clicking anywhere on the card toggles the switch.
/// Supports IsChecked as a DependencyProperty for XAML binding.
/// </summary>
public class CardToggleSwitch : CardControl
{
    #region DependencyProperty
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(CardToggleSwitch),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardToggleSwitch control)
        {
            control._toggleSwitch.IsChecked = (bool)e.NewValue;
        }
    }
    #endregion

    #region Fields
    private readonly ToggleSwitch _toggleSwitch = new();
    private bool _suppressToggleEvent;
    #endregion

    #region Constructor
    public CardToggleSwitch()
    {
        _toggleSwitch.IsHitTestVisible = false;

        SetResourceReference(StyleProperty, typeof(CardControl));
        AddChild(_toggleSwitch);

        Click += OnClick;
        _toggleSwitch.Checked += ToggleSwitch_CheckChanged;
        _toggleSwitch.Unchecked += ToggleSwitch_CheckChanged;
    }
    #endregion

    #region Private Events
    private void OnClick(object sender, RoutedEventArgs e)
    {
        IsChecked = !IsChecked;
    }

    private void ToggleSwitch_CheckChanged(object sender, RoutedEventArgs e)
    {
        if (_suppressToggleEvent) return;

        _suppressToggleEvent = true;
        IsChecked = _toggleSwitch.IsChecked ?? false;
        _suppressToggleEvent = false;
    }
    #endregion
}
