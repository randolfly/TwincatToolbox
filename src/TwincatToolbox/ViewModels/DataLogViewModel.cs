using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Timers;

using Avalonia.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Material.Icons;

using SukiUI.Controls;

using TwinCAT.Ads;

using TwincatToolbox.Extensions;
using TwincatToolbox.Models;
using TwincatToolbox.Services;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.ViewModels;

public partial class DataLogViewModel : ViewModelBase
{
    private readonly IAdsComService _adsComService;
    private readonly ILogDataService _logDataService;
    private readonly ILogPlotService _logPlotService;

    #region log and plot symbols

    [NotifyPropertyChangedFor(nameof(SearchResultSymbols))]
    [ObservableProperty]
    private string _searchText = string.Empty;

    [NotifyPropertyChangedFor(nameof(SearchResultSymbols))]
    [ObservableProperty]
    private List<SymbolInfo> _availableSymbols = new();

    private List<SymbolInfo> _searchResultSymbols = new();
    public List<SymbolInfo> SearchResultSymbols
    {
        get
        {
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

    #endregion

    private Dictionary<uint, SymbolInfo> _symbolsDict = [];

    public DataLogViewModel(IAdsComService adsComService,
        ILogDataService logDataService, ILogPlotService logPlotService)
        : base("DataLog", MaterialIconKind.Blog) {
        _adsComService = adsComService;
        _logDataService = logDataService;
        _logPlotService = logPlotService;
        SearchResultSelectedSymbols.CollectionChanged += OnSearchResultSelectedSymbolsChanged;
    }

    private void OnSearchResultSelectedSymbolsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        UpdateLogSymbol();
    }

    [RelayCommand]
    private void OnGetAvailableSymbols() {
        if (_adsComService.GetAdsState() == TwinCAT.Ads.AdsState.Invalid)
        {
            Debug.WriteLine("Ads server is not connected.");
            return;
        }
        AvailableSymbols = _adsComService.GetAvailableSymbols();
        AvailableSymbols.Sort((a, b) => a.Name.CompareTo(b.Name));
        Debug.WriteLine("Available symbols: {0}", AvailableSymbols.Count());
    }

    public List<SymbolInfo> SearchSymbols(IList<SymbolInfo> sourceList) {
        // todo: ²¹³äÄ£ºýËÑË÷Âß¼­
        var searchResults = sourceList
            .Where(s => s.Name.Contains(SearchText))
            .ToList();
        Debug.WriteLine("Search results: {0}", searchResults.Count());

        return searchResults;
    }

    private void UpdateLogSymbol() {
        var logSearchSymbols = SearchSymbols(LogSymbols);
        if (logSearchSymbols.Count > SearchResultSelectedSymbols.Count)
        {
            var distinctSymbols = logSearchSymbols
                .Except(SearchResultSelectedSymbols, SymbolInfoComparer.Instance);
            foreach (var symbol in distinctSymbols) LogSymbols.Remove(symbol);
        }
        else if (logSearchSymbols.Count < SearchResultSelectedSymbols.Count)
        {
            var distinctSymbols = SearchResultSelectedSymbols
                .Except(logSearchSymbols, SymbolInfoComparer.Instance);
            foreach (var symbol in distinctSymbols) LogSymbols.AddSorted(symbol, SymbolInfoComparer.Instance);
        }
    }

    [RelayCommand]
    private void OpenLogConfigDialog() {
        SukiHost.ShowDialog(new LogConfigControl
        {
            DataContext = new LogConfigViewModel()
        });
    }

    [RelayCommand]
    private void StartLog() {
        if (LogSymbols.Count == 0)
        {
            Debug.WriteLine("No symbol selected for logging.");
            return;
        }
        _symbolsDict.Clear();
        _logDataService.RemoveAllChannels();
        foreach (var symbol in LogSymbols)
        {
            var notificationHandle = _adsComService.AddDeviceNotification(
                symbol.Symbol.InstancePath,
                symbol.Symbol.ByteSize,
                new NotificationSettings(AdsTransMode.Cyclic,
                AppConfigService.AppConfig.LogConfig.Period, 0));
            _symbolsDict.Add(notificationHandle, symbol);
            _logDataService.AddChannel(symbol.Name);
        }

        _logPlotService.RemoveAllChannels();
        foreach (var symbol in PlotSymbols)
        {
            _logPlotService.AddChannel(symbol.Name);
        }

        _adsComService.AddNotificationHandler(AdsNotificationHandler);
    }

    private void AdsNotificationHandler(object? sender, AdsNotificationEventArgs e) {
        double data = 0.0d;
        data = e.Data.Length switch
        {
            1 => BitConverter.ToBoolean(e.Data.Span) ? 1 : 0,
            2 => BinaryPrimitives.ReadInt16LittleEndian(e.Data.Span),
            4 => BinaryPrimitives.ReadInt32LittleEndian(e.Data.Span),
            8 => BinaryPrimitives.ReadDoubleLittleEndian(e.Data.Span),
            _ => 0
        };
        var symbol = _symbolsDict[e.Handle];
        _logDataService.AddData(symbol.Name, data);
        _logPlotService.AddData(symbol.Name, data);
    }
}