using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

    private AdsConfig _adsConfig = AppConfigService.AppConfig.AdsConfig;
    private IAdsComService _adsComService;

    public MainViewModel(IEnumerable<ViewModelBase> viewBases, IAdsComService adsComService) 
    {
        NavViews = new AvaloniaList<ViewModelBase>(viewBases.OrderBy(x => x.Index)
            .ThenBy(x => x.DisplayName));
        _adsComService = adsComService;

        _netId = _adsConfig.NetId;
        _portId = _adsConfig.PortId.ToString();
    }

    [RelayCommand]
    private async Task OnConnectAdsServer()
    {
        if (string.IsNullOrEmpty(NetId) || string.IsNullOrEmpty(PortId)) return;
        _adsConfig.NetId = NetId;
        _adsConfig.PortId = int.Parse(PortId);
        await AppConfigService.SaveConfigAsync(AppConfig.ConfigFileFullName);
        _adsComService.ConnectAdsServer(_adsConfig);
    }

    [RelayCommand]
    private void OnDisconnectAdsServer()
    {
        _adsComService.DisconnectAdsServer();
    }
}
