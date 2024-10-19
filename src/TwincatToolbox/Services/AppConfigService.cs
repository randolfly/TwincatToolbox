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
    /// ��json�ļ��������ã�ֻӦ���ڳ�ʼ��Serviceʱ����һ�Σ���������ط���AppConfig��ȡ�������µ����ã������߸�AppConfig�µ���config DeepClone��
    /// </summary>
    /// <param name="configFileFullName">�����ļ���ַ</param>
    public static void LoadConfig(string configFileFullName)
    {
        Debug.WriteLine($"LoadConfig: {configFileFullName}");
        if (!File.Exists(configFileFullName)) return;
        using var fs = new FileStream(configFileFullName, FileMode.Open);
        AppConfig = JsonSerializer.Deserialize<AppConfig>(fs, jsonSerializerOptions) ?? new();
    }

    /// <summary>
    /// �洢���õ�json�ļ�
    /// </summary>
    /// <param name="configFileFullName">�����ļ���ַ</param>
    public static void SaveConfig(string configFileFullName)
    {
        Debug.WriteLine($"SaveConfig: {configFileFullName}");
        if (!Directory.Exists(Path.GetDirectoryName(configFileFullName)))
            Directory.CreateDirectory(Path.GetDirectoryName(configFileFullName)!);
        using var fs = new FileStream(configFileFullName, FileMode.Create);
        JsonSerializer.Serialize(fs, AppConfig, jsonSerializerOptions);
    }

}
