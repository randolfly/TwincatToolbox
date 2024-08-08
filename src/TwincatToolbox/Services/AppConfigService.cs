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

    public static async Task LoadConfigAsync(string configFileFullName)
    {
        if (!File.Exists(configFileFullName)) return;
        await using var fs = new FileStream(configFileFullName, FileMode.Open);
        AppConfig = await JsonSerializer.DeserializeAsync<AppConfig>(fs, jsonSerializerOptions) ?? new();
    }

    public static async Task SaveConfigAsync(string configFileFullName)
    {
        if (!Directory.Exists(Path.GetDirectoryName(configFileFullName)))
            Directory.CreateDirectory(Path.GetDirectoryName(configFileFullName)!);
        await using var fs = new FileStream(configFileFullName, FileMode.Create);
        await JsonSerializer.SerializeAsync(fs, AppConfig, jsonSerializerOptions);
    }

}
