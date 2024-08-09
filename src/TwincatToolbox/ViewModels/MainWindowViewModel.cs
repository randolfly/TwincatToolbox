using CommunityToolkit.Mvvm.ComponentModel;

namespace TwincatToolbox.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private object? _content = new SettingViewModel();

    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
}
