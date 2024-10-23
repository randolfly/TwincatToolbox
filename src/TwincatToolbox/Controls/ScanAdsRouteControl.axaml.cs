using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using SukiUI.Controls;

using TwincatToolbox.ViewModels;

namespace TwincatToolbox;

public partial class ScanAdsRouteControl : SukiWindow
{
    public ScanAdsRouteControl()
    {
        InitializeComponent();
    }

    private void CloseDialog(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Close(((ScanAdsRouteViewModel)DataContext!).SelectedAdsRoute?.NetId);
    }
}