using System;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace TwincatToolbox.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private object? _content = new SettingViewModel();

    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    [RelayCommand]
    private void OnNavigate(string? page)
    {
        Content = page switch
        {
            "Setting" => new SettingViewModel(),
            "Dashboard" => new DashboardViewModel(),
            "DataLog" => new DataLogViewModel(),
            _ => throw new ArgumentException("Invalid page", nameof(page))
        };
    }
}
