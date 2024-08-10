using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Material.Icons;

using TwinCAT.Ads.TypeSystem;

using TwincatToolbox.Models;
using TwincatToolbox.Services;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.ViewModels;

public partial class DataLogViewModel(IAdsComService adsComService) : ViewModelBase("DataLog", MaterialIconKind.Blog)
{
    private readonly IAdsComService _adsComService = adsComService;

    public ObservableCollection<SymbolNode> AvailableSymbols { get; } = [];
    public ObservableCollection<SymbolNode> RecordSymbols { get; } = [];
    public ObservableCollection<SymbolNode> PlotSymbols { get; } = [];

    [RelayCommand]
    private void OnGetAvailableSymbols()
    {
        AvailableSymbols.Clear();
        if (_adsComService.GetAdsState() == TwinCAT.Ads.AdsState.Invalid)
        {
            Debug.WriteLine("Ads server is not connected.");
            return;
        }
        var symbols = _adsComService.GetAvailableSymbols();
        foreach (var symbol in symbols)
        {
            AvailableSymbols.Add(symbol);
        }
        // 更新记录符号
        Debug.WriteLine("Available symbols: {0}", symbols.Count());
        // 更新绘图符号
    }
}