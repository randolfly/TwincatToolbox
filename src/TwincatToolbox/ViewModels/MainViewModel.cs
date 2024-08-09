using System.Collections.Generic;
using System.Linq;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

namespace TwincatToolbox.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public IAvaloniaReadOnlyList<ViewModelBase> NavViews { get; }
    [ObservableProperty] private ViewModelBase? _activeView;

    public MainViewModel(IEnumerable<ViewModelBase> viewBases)
    {
        NavViews = new AvaloniaList<ViewModelBase>(viewBases.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
    }
}
