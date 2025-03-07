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
    public static string AppName => Assembly.GetCallingAssembly().FullName!.Split(',')[0];

    public static string FolderName => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName
    );

    public static string FileName => AppName + ".json";
    public static string ConfigFileFullName => Path.Combine(FolderName, FileName);
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
            var fileName = FileName + "_" + Period + "ms"+ "_" + datetime.ToString("yyyyMMddHHmmss") ;
            return Path.Combine(FolderName, fileName);
        }
    }
}