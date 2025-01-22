using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuzzySharp;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Dialogs;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwincatToolbox.Extensions;
using TwincatToolbox.Models;
using TwincatToolbox.Services;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.ViewModels;

public partial class DataLogViewModel : ViewModelBase
{
    private LogConfig _logConfig;

    private readonly IAdsComService _adsComService;
    private readonly ILogDataService _logDataService;
    private readonly ILogPlotService _logPlotService;

    #region log and plot symbols

    [NotifyPropertyChangedFor(nameof(SearchResultSymbols))] [ObservableProperty]
    private string _searchText = string.Empty;

    [NotifyPropertyChangedFor(nameof(SearchResultSymbols))] [ObservableProperty]
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

    [ObservableProperty] private ObservableCollection<SymbolInfo> _searchResultSelectedSymbols = new();
    public ObservableCollection<SymbolInfo> LogSymbols { get; set; } = [];
    public ObservableCollection<SymbolInfo> PlotSymbols { get; } = [];
    private bool _isFirstGetAvailableSymbols = true;

    [ObservableProperty] private bool _isLoggingStopped = true;

    #endregion

    private Dictionary<uint, SymbolInfo> _symbolsDict = [];

    public ISukiDialogManager DialogManager { get; } = DialogManageService.DialogManager;

    public DataLogViewModel(IAdsComService adsComService,
        ILogDataService logDataService, ILogPlotService logPlotService)
        : base("DataLog", MaterialIconKind.Blog, index: -1)
    {
        _adsComService = adsComService;
        _logDataService = logDataService;
        _logPlotService = logPlotService;

        _logConfig = AppConfigService.AppConfig.LogConfig;

        SearchResultSelectedSymbols.CollectionChanged += OnSearchResultSelectedSymbolsChanged;
    }

    private void OnSearchResultSelectedSymbolsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
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

        if (_isFirstGetAvailableSymbols)
        {
            InitLogAndPlotSymbolsWithConfig(_logConfig);
            _isFirstGetAvailableSymbols = false;
        }
    }

    private void InitLogAndPlotSymbolsWithConfig(LogConfig logConfig)
    {
        LogSymbols.Clear();
        foreach (var symbolName in logConfig.LogSymbols)
        {
            var symbol = AvailableSymbols.Find(s => s.Name == symbolName);
            if (symbol is null) break;
            LogSymbols.AddSorted(symbol, SymbolInfoComparer.Instance);
        }

        PlotSymbols.Clear();
        foreach (var symbolName in logConfig.PlotSymbols)
        {
            var symbol = AvailableSymbols.Find(s => s.Name == symbolName);
            if (symbol is null) break;
            PlotSymbols.AddSorted(symbol, SymbolInfoComparer.Instance);
        }
    }

    public List<SymbolInfo> SearchSymbols(IList<SymbolInfo> sourceList)
    {
        if (string.IsNullOrEmpty((SearchText))) return sourceList.ToList();
        var searchResults = sourceList
            .OrderByDescending(s=>GetSimilarityScore(SearchText, s))
            .Take(20).ToList();
        // Debug.WriteLine("Search results: {0}", searchResults.Count());
        
        return searchResults;
    }

    public int GetSimilarityScore(string searchText, SymbolInfo symbolInfo) {
        return Fuzz.PartialTokenSetRatio(searchText, symbolInfo.Name.ToLower());
    }

    private void UpdateLogSymbol()
    {
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
    private void OpenLogConfigDialog()
    {
        DialogManager.CreateDialog()
            .WithContent(new LogConfigControl
            {
                DataContext = new LogConfigViewModel()
            })
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    [RelayCommand]
    private void StartLog()
    {
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
            // show 3s data
            _logPlotService.AddChannel(symbol.Name,
                (int)Math.Floor(3000.0 / _logConfig.Period));
        }

        ExportLogConfig();
        IsLoggingStopped = false;

        _adsComService.AddNotificationHandler(AdsNotificationHandler);
    }

    private void ExportLogConfig()
    {
        _logConfig.LogSymbols = LogSymbols.Select(s => s.Name).ToList();
        _logConfig.PlotSymbols = PlotSymbols.Select(s => s.Name).ToList();
        AppConfigService.SaveConfig(AppConfig.ConfigFileFullName);
    }

    [RelayCommand]
    private async Task StopLogAsync()
    {
        _adsComService.RemoveNotificationHandler(AdsNotificationHandler);

        _symbolsDict.Keys.ToList().ForEach(handle =>
            _adsComService.RemoveDeviceNotification(handle));
        _symbolsDict.Clear();

        // load data from tmp folder
        var logResult = await _logDataService.LoadAllChannelsAsync();
        _logDataService.DeleteTmpFiles();
        // export
        await _logDataService.ExportDataAsync(logResult, _logConfig.FileFullName, _logConfig.FileType);
        // plot
        _logPlotService.ShowAllChannelsWithNewData(logResult, _logConfig.Period);
        IsLoggingStopped = true;
    }

    private async void AdsNotificationHandler(object? sender, AdsNotificationEventArgs e)
    {
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

        await _logDataService.AddDataAsync(symbol.Name, data);
        _logPlotService.AddData(symbol.Name, data);
    }
}