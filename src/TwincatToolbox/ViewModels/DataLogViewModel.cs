using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Material.Icons;

using TwincatToolbox.Extensions;
using TwincatToolbox.Models;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.ViewModels;

public partial class DataLogViewModel : ViewModelBase
{
    private readonly IAdsComService _adsComService;
    [ObservableProperty] private string _searchText = string.Empty;

    private List<SymbolInfo> _availableSymbols = new();
    public ObservableCollection<SymbolInfo> SearchResultSymbols { get; } = [];
    public ObservableCollection<SymbolInfo> SearchResultSelectedSymbols { get; } = [];
    public ObservableCollection<SymbolInfo> LogSymbols { get; } = [];
    public ObservableCollection<SymbolInfo> PlotSymbols { get; } = [];

    public DataLogViewModel(IAdsComService adsComService)  : base("DataLog", MaterialIconKind.Blog) {
        _adsComService = adsComService;
        SearchResultSelectedSymbols.CollectionChanged += SearchResultSelectedSymbols_CollectionChanged;
    }


    [RelayCommand]
    private void OnGetAvailableSymbols()
    {
        if (_adsComService.GetAdsState() == TwinCAT.Ads.AdsState.Invalid)
        {
            Debug.WriteLine("Ads server is not connected.");
            return;
        }
        _availableSymbols = _adsComService.GetAvailableSymbols();
        Debug.WriteLine("Available symbols: {0}", _availableSymbols.Count());
        // 更新搜索结果的Symbols
        SearchSymbols();
        Debug.WriteLine("Available symbols: {0}", SearchResultSymbols.Count());
    }

    public void SearchSymbols()
    {
        // todo: 补充模糊搜索逻辑
        var searchResults = _availableSymbols
            .Where(s => s.Name.Contains(SearchText));
        SearchResultSymbols.Clear();
        foreach(var symbol in searchResults)
        {
            SearchResultSymbols.Add(symbol);
        }
        SearchResultSelectedSymbols.CollectionChanged -= SearchResultSelectedSymbols_CollectionChanged;
        var idealSelectedItems = SearchResultSymbols.Intersect(LogSymbols, SymbolInfoComparer.Instance);
        SearchResultSelectedSymbols.Clear();
        foreach (var symbol in idealSelectedItems)
        {
            SearchResultSelectedSymbols.Add(symbol);
        }
        SearchResultSelectedSymbols.CollectionChanged += SearchResultSelectedSymbols_CollectionChanged;
    }

    private void OnSearchResultSelectedSymbolsChanged() {
        Debug.WriteLine("SearchResultSelectedSymbols has changed.");
        foreach(var symbol in SearchResultSymbols)
        {
            if(!SearchResultSelectedSymbols.Contains(symbol, SymbolInfoComparer.Instance) && 
                LogSymbols.Contains(symbol, SymbolInfoComparer.Instance))
            {
                LogSymbols.Remove(symbol);
            }
            if (SearchResultSelectedSymbols.Contains(symbol, SymbolInfoComparer.Instance) &&
               !LogSymbols.Contains(symbol, SymbolInfoComparer.Instance))
            {
                LogSymbols.Add(symbol);
            }
        }
    }

    private void SearchResultSelectedSymbols_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        OnSearchResultSelectedSymbolsChanged();
    }
}