using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
using TwincatToolbox.Models;

namespace TwincatToolbox.Services;

public static class AppConfigService
{
    public static AppConfig AppConfig { get; set; } = new();

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// 从json文件加载配置，只应该在初始化Service时调用一次（否则其余地方的AppConfig读取不到最新的配置）【或者给AppConfig下的子config DeepClone】
    /// </summary>
    /// <param name="configFileFullName">配置文件地址</param>
    public static void LoadConfig(string configFileFullName)
    {
        Debug.WriteLine($"LoadConfig: {configFileFullName}");
        if (!File.Exists(configFileFullName)) return;
        using var fs = new FileStream(configFileFullName, FileMode.Open);
        AppConfig = JsonSerializer.Deserialize<AppConfig>(fs, jsonSerializerOptions) ?? new();
    }

    /// <summary>
    /// 存储配置到json文件
    /// </summary>
    /// <param name="configFileFullName">配置文件地址</param>
    public static void SaveConfig(string configFileFullName)
    {
        Debug.WriteLine($"SaveConfig: {configFileFullName}");
        if (!Directory.Exists(Path.GetDirectoryName(configFileFullName)))
            Directory.CreateDirectory(Path.GetDirectoryName(configFileFullName)!);
        using var fs = new FileStream(configFileFullName, FileMode.Create);
        JsonSerializer.Serialize(fs, AppConfig, jsonSerializerOptions);
    }

}
