namespace TwincatToolbox.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    public string Greeting => "Welcome to Avalonia!";

    public string Name { get; set; } = "Avalonia";
}
