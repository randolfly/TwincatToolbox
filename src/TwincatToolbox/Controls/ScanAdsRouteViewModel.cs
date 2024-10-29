using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ScottPlot;

using TwincatToolbox.Controls;
using TwincatToolbox.Services;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.ViewModels;
public partial class ScanAdsRouteViewModel : ControlModelBase
{
    public ObservableCollection<AdsRouteInfo> AdsRoutes { get; }

    [ObservableProperty]
    private AdsRouteInfo? _selectedAdsRoute = null;

    public ScanAdsRouteViewModel() {
        var routes = AdsComService.ScanAdsRoutes();
        AdsRoutes = new ObservableCollection<AdsRouteInfo>(routes);
    }
}