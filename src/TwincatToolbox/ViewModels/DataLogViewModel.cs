using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Material.Icons;

using TwincatToolbox.Services;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.ViewModels;

public partial class DataLogViewModel(IAdsComService adsComService) : ViewModelBase("DataLog", MaterialIconKind.Blog)
{
    private readonly IAdsComService _adsComService = adsComService;
    [ObservableProperty] private ObservableCollection<string> _availableSymbols = [];
    [ObservableProperty] private ObservableCollection<string> _recordSymbols = [];
    [ObservableProperty] private ObservableCollection<string> _plotSymbols = [];

    [RelayCommand]
    private void OnGetAvailableSymbols()
    {
        var symbols = _adsComService.GetAvailableSymbols();
        AvailableSymbols.Clear();
        foreach (var symbol in symbols)
        {
            AvailableSymbols.Add(symbol.InstanceName);
        }
        // 更新记录符号


        // 更新绘图符号
    }
}