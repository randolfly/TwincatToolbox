using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using Avalonia;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Material.Icons;

using SukiUI.Controls;

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
    public ObservableCollection<SymbolInfo> LogSymbols { get; set; } = [];
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
        _availableSymbols.Sort((a, b) => a.Name.CompareTo(b.Name));
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
        Debug.WriteLine("Search results: {0}", searchResults.Count());

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
        
        //var sortedLogSymbols = LogSymbols
        //    //.Select(s=>s.DeepCopy())
        //    .Order(SymbolInfoComparer.Instance);
        //foreach (var symbol in sortedLogSymbols)
        //{
        //    LogSymbols.Add(symbol);
        //    LogSymbols.RemoveAt(0);
        //}
    }

    private void SearchResultSelectedSymbols_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        OnSearchResultSelectedSymbolsChanged();
    }

    [RelayCommand]
    private void OpenLogConfigDialog() {
        SukiHost.ShowDialog(new LogConfigControl
        {
            DataContext = new LogConfigViewModel()
        });
    }
}