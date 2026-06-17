using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SoundSwitcher.Localization;
using Wpf.Ui.Controls;

namespace SoundSwitcher.Helpers;

public static class DialogHelper
{
    public static async Task ShowDialogAsync(string title, string message, MessageBoxImage image = MessageBoxImage.Information)
    {
        var iconSymbol = image switch
        {
            MessageBoxImage.Warning => SymbolRegular.Warning24,
            MessageBoxImage.Error => SymbolRegular.ErrorCircle24,
            MessageBoxImage.Question => SymbolRegular.QuestionCircle24,
            _ => SymbolRegular.Info24
        };

        Brush iconColor = image switch
        {
            MessageBoxImage.Warning => Brushes.Orange,
            MessageBoxImage.Error => Brushes.Red,
            MessageBoxImage.Question => Brushes.Gray,
            _ => Brushes.DodgerBlue
        };

        var icon = new SymbolIcon
        {
            Symbol = iconSymbol,
            FontSize = 28,
            Margin = new Thickness(0, 0, 16, 0),
            Foreground = iconColor,
            VerticalAlignment = VerticalAlignment.Top
        };

        Grid.SetColumn(icon, 0);

        var textBlock = new System.Windows.Controls.TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Top,
            LineHeight = 22,
            FontSize = 14
        };

        Grid.SetColumn(textBlock, 1);

        var grid = new Grid
        {
            Margin = new Thickness(0, 10, 0, 16)
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.Children.Add(icon);
        grid.Children.Add(textBlock);

        var uiMessageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = title,
            Content = grid,
            CloseButtonText = LocalizationManager.Instance["OKButton"]
        };

        await uiMessageBox.ShowDialogAsync();
    }
}
