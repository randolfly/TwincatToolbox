using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;


using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Material.Icons;

using SukiUI.Controls;

using TwinCAT.Ads.TypeSystem;

using TwincatToolbox.Extensions;
using TwincatToolbox.Models;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.ViewModels;

public partial class DataLogViewModel : ViewModelBase
{
    private readonly IAdsComService _adsComService;

    [NotifyPropertyChangedFor(nameof(SearchResultSymbols))]
    [ObservableProperty] 
    private string _searchText = string.Empty;

    [NotifyPropertyChangedFor(nameof(SearchResultSymbols))]
    [ObservableProperty]
    private List<SymbolInfo> _availableSymbols = new();

    private List<SymbolInfo> _searchResultSymbols = new();
    public List<SymbolInfo> SearchResultSymbols { 
        get {
            _searchResultSymbols = SearchSymbols(AvailableSymbols);

            SearchResultSelectedSymbols.CollectionChanged -= OnSearchResultSelectedSymbolsChanged;
            SearchResultSelectedSymbols.Clear();
            var initSearchResultSelectedSymbols = _searchResultSymbols
                .Intersect(LogSymbols, SymbolInfoComparer.Instance);
            foreach (var symbol in initSearchResultSelectedSymbols) SearchResultSelectedSymbols.Add(symbol);
            SearchResultSelectedSymbols.CollectionChanged += OnSearchResultSelectedSymbolsChanged;

            return _searchResultSymbols;
        }
    }
      
    [ObservableProperty]
    private ObservableCollection<SymbolInfo> _searchResultSelectedSymbols = new();
    public ObservableCollection<SymbolInfo> LogSymbols { get; set; } = [];
    public ObservableCollection<SymbolInfo> PlotSymbols { get; } = [];


    public DataLogViewModel(IAdsComService adsComService)  : base("DataLog", MaterialIconKind.Blog) {
        _adsComService = adsComService;
        SearchResultSelectedSymbols.CollectionChanged += OnSearchResultSelectedSymbolsChanged;
    }

    private void OnSearchResultSelectedSymbolsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        UpdateLogSymbol();
    }

    [RelayCommand]
    private void OnGetAvailableSymbols()
    {
        if (_adsComService.GetAdsState() == TwinCAT.Ads.AdsState.Invalid)
        {
            Debug.WriteLine("Ads server is not connected.");
            return;
        }
        AvailableSymbols = _adsComService.GetAvailableSymbols();
        AvailableSymbols.Sort((a, b) => a.Name.CompareTo(b.Name));
        Debug.WriteLine("Available symbols: {0}", AvailableSymbols.Count());
    }

    public List<SymbolInfo> SearchSymbols(IList<SymbolInfo> sourceList)
    {
        // todo: ²¹³äÄ£ºýËÑË÷Âß¼­
        var searchResults = sourceList
            .Where(s => s.Name.Contains(SearchText))
            .ToList();
        Debug.WriteLine("Search results: {0}", searchResults.Count());

        return searchResults;
    }

    public void UpdateLogSymbol() {
        var logSearchSymbols = SearchSymbols(LogSymbols);
        foreach (var symbol in logSearchSymbols) LogSymbols.Remove(symbol);
        foreach (var symbol in SearchResultSelectedSymbols) LogSymbols.AddSorted(symbol, SymbolInfoComparer.Instance);
    }

    [RelayCommand]
    private void OpenLogConfigDialog() {
        SukiHost.ShowDialog(new LogConfigControl
        {
            DataContext = new LogConfigViewModel()
        });
    }
}