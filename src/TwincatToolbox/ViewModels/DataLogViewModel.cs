using System.Collections.Generic;
using System.Runtime.InteropServices;

using CommunityToolkit.Mvvm.ComponentModel;

using Material.Icons;

using TwincatToolbox.Services;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.ViewModels;

public partial class DataLogViewModel(IAdsComService adsComService) : ViewModelBase("DataLog", MaterialIconKind.Blog)
{
    private readonly IAdsComService _adsComService = adsComService;
    [ObservableProperty] private HashSet<string> _availableSymbol = ["aaa", "ccc", "ddd.eee"];
    [ObservableProperty] private HashSet<string> _recordSymbol = ["aaa", "ccc", "ddd.eee", "asasas", "爱看书的进阿里"];

}