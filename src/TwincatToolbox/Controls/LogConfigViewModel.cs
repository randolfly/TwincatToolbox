using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Material.Icons;

using SukiUI.Controls;

using TwincatToolbox.Controls;
using TwincatToolbox.Models;
using TwincatToolbox.Services;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using TwincatToolbox.Constants;

namespace TwincatToolbox.ViewModels;
public partial class LogConfigViewModel : ControlModelBase
{
    [ObservableProperty] private string _logFolder = string.Empty;
    [ObservableProperty] private string _logFileName = string.Empty;
    [ObservableProperty] private int _logPeriod = 2;

    public List<string> AvailableLogFileTypes { get; } = AppConstants.SupportedLogFileTypes;
    public ObservableCollection<string> LogFileTypes { get; } = new();

    private LogConfig _logConfig;
    public LogConfigViewModel() 
    {
        _logConfig = AppConfigService.AppConfig.LogConfig;
        LoadConfig();
    }

    [RelayCommand]
    private async Task SelectFolder() {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel((App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow);

        // Start async operation to open the dialog.
        var files = await topLevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        if (files != null && files.Count > 0)
        {
            foreach (var file in files)
            {
                Debug.WriteLine(file.Path);
            }
            LogFolder = files[0]?.Path.AbsolutePath ?? string.Empty;
        }
    }

    [RelayCommand]
    private void CloseDialog() {
        SukiHost.CloseDialog();
    }
    [RelayCommand]
    private void LoadConfig() {
        LogPeriod = _logConfig.Period;
        LogFolder = _logConfig.FolderName;
        LogFileName = _logConfig.FileName;
        LogFileTypes.Clear();
        foreach (var fileType in _logConfig.FileType)
        {
            LogFileTypes.Add(fileType.ToString());
        }
    }
    [RelayCommand]
    private void SaveConfig() {
        _logConfig.Period = LogPeriod;
        _logConfig.FolderName = LogFolder;
        _logConfig.FileName = LogFileName;
        _logConfig.FileType = LogFileTypes.ToList();
        AppConfigService.SaveConfig(AppConfig.ConfigFileFullName);
    }
}