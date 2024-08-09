using System;
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

    public static void LoadConfig(string configFileFullName)
    {
        if (!File.Exists(configFileFullName)) return;
        using var fs = new FileStream(configFileFullName, FileMode.Open);
        AppConfig = JsonSerializer.Deserialize<AppConfig>(fs, jsonSerializerOptions) ?? new();
    }

    public static void SaveConfig(string configFileFullName)
    {
        if (!Directory.Exists(Path.GetDirectoryName(configFileFullName)))
            Directory.CreateDirectory(Path.GetDirectoryName(configFileFullName)!);
        using var fs = new FileStream(configFileFullName, FileMode.Create);
        JsonSerializer.Serialize(fs, AppConfig, jsonSerializerOptions);
    }

}
