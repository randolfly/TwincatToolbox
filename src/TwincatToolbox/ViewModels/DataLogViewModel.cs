using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Material.Icons;

using TwinCAT.Ads.TypeSystem;

using TwincatToolbox.Models;
using TwincatToolbox.Services;
using TwincatToolbox.Services.IService;
using TwincatToolbox.Extensions;

namespace TwincatToolbox.ViewModels;

public partial class DataLogViewModel(IAdsComService adsComService) : ViewModelBase("DataLog", MaterialIconKind.Blog)
{
    private readonly IAdsComService _adsComService = adsComService;
   
    [ObservableProperty] private string _searchText = string.Empty;
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

    public IEnumerable<SymbolNode> SearchSymbol() {
        //todo: 使用模糊搜索库搜索需要隐藏的符号
        List<SymbolNode> flattedSymbolNodes = [];
        foreach (var symbol in AvailableSymbols)
        {
            symbol.ToIEnumerable(ref flattedSymbolNodes);
        }
        var filteredSymbols = flattedSymbolNodes
            .Where(s => !s.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        return filteredSymbols;
    }
}