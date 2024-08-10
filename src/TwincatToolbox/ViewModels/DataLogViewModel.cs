using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Material.Icons;
using TwincatToolbox.Models;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.ViewModels;

public partial class DataLogViewModel(IAdsComService adsComService) : ViewModelBase("DataLog", MaterialIconKind.Blog)
{
    private readonly IAdsComService _adsComService = adsComService;
    [ObservableProperty] private string _searchText = string.Empty;

    public ObservableCollection<SymbolInfo> AvailableSymbols { get; } = [];
    public ObservableCollection<SymbolInfo> RecordSymbols { get; } = [];
    public ObservableCollection<SymbolInfo> PlotSymbols { get; } = [];

    [RelayCommand]
    private void OnGetAvailableSymbols()
    {
        if (_adsComService.GetAdsState() == TwinCAT.Ads.AdsState.Invalid)
        {
            Debug.WriteLine("Ads server is not connected.");
            return;
        }
        AvailableSymbols.Clear();
        var symbols = _adsComService.GetAvailableSymbols();
        foreach (var symbol in symbols)
        {
            AvailableSymbols.Add(symbol);
        }

        Debug.WriteLine("Available symbols: {0}", symbols.Count());
    }

    public IEnumerable<bool> SearchSymbols()
    {
        // todo: 补充模糊搜索逻辑
        var searchResults = AvailableSymbols
            .Select(s => s.Name.Contains(SearchText));
        // 返回对应子项显示与否的列表
        return searchResults;
    }
}