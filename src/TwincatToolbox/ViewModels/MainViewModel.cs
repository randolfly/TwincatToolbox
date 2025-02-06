using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using SukiUI.Dialogs;

using TwinCAT.Ads;

using TwincatToolbox.Models;
using TwincatToolbox.Services;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public IAvaloniaReadOnlyList<ViewModelBase> NavViews { get; }
    [ObservableProperty] private ViewModelBase? _activeView;

    [ObservableProperty] private string _netId = string.Empty;
    [ObservableProperty] private string _portId = string.Empty;

    private AdsConfig _adsConfig;
    private IAdsComService _adsComService;

    [ObservableProperty] private string _adsStateText = string.Empty;
    public ISukiDialogManager DialogManager { get; } = DialogManageService.DialogManager;

    public MainViewModel(IEnumerable<ViewModelBase> viewBases, IAdsComService adsComService) 
    {
        NavViews = new AvaloniaList<ViewModelBase>(viewBases.OrderBy(x => x.Index)
            .ThenBy(x => x.DisplayName));
        _adsComService = adsComService;
        _adsConfig = AppConfigService.AppConfig.AdsConfig;

        _netId = _adsConfig.NetId;
        _portId = _adsConfig.PortId.ToString();
    }

    [RelayCommand(CanExecute = nameof(CanConnectAdsServer))]
    private void OnConnectAdsServer()
    {
        _adsConfig.NetId = NetId;
        _adsConfig.PortId = int.Parse(PortId);
        AppConfigService.SaveConfig(AppConfig.ConfigFileFullName);
        _adsComService.ConnectAdsServer(_adsConfig);
        OnCheckAdsState();
    }
    private bool CanConnectAdsServer() {
        return !string.IsNullOrEmpty(NetId) && !string.IsNullOrEmpty(PortId);
    }

    [RelayCommand]
    private void OnDisconnectAdsServer()
    {
        _adsComService.DisconnectAdsServer();
        OnCheckAdsState();
    }

    [RelayCommand]
    private void OnCheckAdsState() {
        AdsStateText = _adsComService.GetAdsState().ToString();
    }

    [RelayCommand]
    private async Task ScanAndSelectAdsRouteAsync() {
        var dialog = new ScanAdsRouteControl
        {
            DataContext = new ScanAdsRouteViewModel()
        };
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialogResult = await dialog.ShowDialog<string>(desktop?.MainWindow);
            if (dialogResult != null) NetId = dialogResult;
            Debug.WriteLine($"Selected NetId: {NetId}");
        }
    }

    [RelayCommand]
    private void OpenConfigFolder() {
        Process.Start(new ProcessStartInfo
        {
            FileName = AppConfig.FolderName,
            UseShellExecute = true
        });
    }
}