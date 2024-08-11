using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using TwinCAT.Ads;

using TwincatToolbox.Constants;

namespace TwincatToolbox.Models;

public class AppConfig
{
    public AdsConfig AdsConfig { get; set; } = new();
    public LogConfig LogConfig { get; set; } = new();

    #region 配置文件存储路径
    public static readonly string FolderName;
    public static readonly string FileName;

    static AppConfig()
    {
        var appName = Assembly.GetCallingAssembly().FullName!.Split(',')[0];
        FolderName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName);
        FileName = appName;
    }

    [JsonIgnore]
    public static string ConfigFileFullName => Path.Combine(FolderName, FileName + ".json");
    #endregion
}

public class AdsConfig
{
    public string NetId { get; set; } = AmsNetId.Local.ToString();
    public int PortId { get; set; } = 851;
}

public class LogConfig
{
    // 记录日志的周期，单位：毫秒，默认2ms
    public int Period { get; set; } = 2;
    public List<string> FileType { get; set; } = AppConstants.SupportedLogFileTypes;
    public List<string> LogSymbols { get; set; } = new();
    public List<string> PlotSymbols { get; set; } = new();

    public string FolderName { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    public string FileName { get; set; } = "log";

    [JsonIgnore]
    public string TempFileFullName => Path.Combine(FolderName, FileName);

    [JsonIgnore]
    public string FileFullName
    {
        get
        {
            var datetime = DateTime.Now;
            var fileName = FileName + "_" + datetime.ToString("yyyyMMddHHmmss");
            return Path.Combine(FolderName, fileName);
        }
    }
}