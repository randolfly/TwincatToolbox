using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
            UpdateSearchResultSymbols();
            return _searchResultSymbols;
        }
    }

    private void UpdateSearchResultSymbols()
    {
        _searchResultSymbols = SearchSymbols(AvailableSymbols);

        SearchResultSelectedSymbols.CollectionChanged -= OnSearchResultSelectedSymbolsChanged;
        SearchResultSelectedSymbols.Clear();
        var initSearchResultSelectedSymbols = _searchResultSymbols
            .Intersect(LogSymbols, SymbolInfoComparer.Instance);
        foreach (var symbol in initSearchResultSelectedSymbols) SearchResultSelectedSymbols.Add(symbol);
        SearchResultSelectedSymbols.CollectionChanged += OnSearchResultSelectedSymbolsChanged;
    }

    [ObservableProperty] private ObservableCollection<SymbolInfo> _searchResultSelectedSymbols = new();
    public ObservableCollection<SymbolInfo> LogSymbols { get; set; } = [];
    public ObservableCollection<SymbolInfo> PlotSymbols { get; } = [];
    private bool _isFirstGetAvailableSymbols = true;

    [ObservableProperty] private bool _isLoggingStopped = true;

    #endregion

    private Dictionary<uint, SymbolInfo> _symbolsDict = [];

    public ISukiDialogManager DialogManager { get; } = DialogManageService.DialogManager;

    public DataLogViewModel(IAdsComService adsComService, ILogDataService logDataService, ILogPlotService logPlotService)
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
        if (_adsComService.GetAdsState() == AdsState.Invalid)
        {
            Debug.WriteLine("Ads server is not connected.");
            return;
        }

        AvailableSymbols = _adsComService.GetAvailableSymbols();
        AvailableSymbols.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        Debug.WriteLine("Available symbols: {0}", AvailableSymbols.Count);

        if (_isFirstGetAvailableSymbols)
        {
            InitLogAndPlotSymbolsWithConfig(_logConfig);
            UpdateSearchResultSymbols();
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
            .OrderByDescending(s => GetSimilarityScore(SearchText, s))
            .ToList();
        // Debug.WriteLine("Search results: {0}", searchResults.Count());

        return searchResults;
    }

    public int GetSimilarityScore(string searchText, SymbolInfo symbolInfo)
    {
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
            Debug.WriteLine($"Add device notification for symbol: {symbol.Name}", "DataLogViewModel");
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

    // todo: fix extract type error, such as REAL(length = 4) => BinaryPrimitives.ReadHalfLittleEndian(e.Data.Span);
    private async void AdsNotificationHandler(object? sender, AdsNotificationEventArgs e)
    {
        if (!_symbolsDict.TryGetValue(e.Handle, out var symbol))
        {
            Debug.WriteLine("Symbol not found for handle: {0}", e.Handle);
            return;
        }

        var dataType = (symbol.Symbol.DataType as DataType)?.ManagedType;
        if (dataType == null)
        {
            Debug.WriteLine("ManagedType is null for symbol: {0}", symbol.Name);
            return;
        }

        object data;
        try
        {
            data = SpanConverter.ConvertTo(e.Data.Span, dataType);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error converting data for symbol: {0}, Exception: {1}", symbol.Name, ex);
            return;
        }

        double result = data switch
        {
            bool b => b ? 1.0 : 0.0,
            byte b => b,
            sbyte sb => sb,
            short s => s,
            ushort us => us,
            int i => i,
            uint ui => ui,
            long l => l,
            ulong ul => ul,
            float f => f,
            double d => d,
            _ => throw new InvalidCastException($"Unsupported data type: {dataType}")
        };

        await _logDataService.AddDataAsync(symbol.Name, result);
        _logPlotService.AddData(symbol.Name, result);
    }
}