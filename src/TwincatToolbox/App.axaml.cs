using System;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

using HotAvalonia;

using Microsoft.Extensions.DependencyInjection;

using TwincatToolbox.Models;
using TwincatToolbox.Services;
using TwincatToolbox.Services.IService;
using TwincatToolbox.ViewModels;

namespace TwincatToolbox;
public partial class App : Application
{
    private IServiceProvider? _provider;

    public override void Initialize()
    {
        this.EnableHotReload();
        AvaloniaXamlLoader.Load(this);
        _provider = ConfigureServices();
        ConfigureApp();
    }

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            var viewLocator = _provider?.GetRequiredService<IDataTemplate>();
            var mainViewModel = _provider?.GetRequiredService<MainViewModel>();

            desktop.MainWindow = viewLocator?.Build(mainViewModel) as Window;
        }

        base.OnFrameworkInitializationCompleted();
    }
    private static ServiceProvider ConfigureServices()
    {
        var viewLocator = Current?.DataTemplates.First(x => x is ViewLocator);
        var services = new ServiceCollection();

        // services
        services.AddSingleton<IAdsComService, AdsComService>();
        services.AddSingleton<ILogDataService, LogDataService>();
        services.AddSingleton<ILogPlotService, LogPlotService>();

        // viewmodels
        if (viewLocator != null)
            services.AddSingleton(viewLocator);
        services.AddTransient<MainViewModel>();
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => !p.IsAbstract && typeof(ViewModelBase).IsAssignableFrom(p));
        foreach (var type in types)
            services.AddSingleton(typeof(ViewModelBase), type);

        return services.BuildServiceProvider();
    }

    private static void ConfigureApp()
    {
        AppConfigService.LoadConfig(AppConfig.ConfigFileFullName);
    }

}