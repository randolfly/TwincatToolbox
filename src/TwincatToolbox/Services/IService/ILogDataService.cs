using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

using HarfBuzzSharp;

using TwincatToolbox.Models;
using TwincatToolbox.Utils;

namespace TwincatToolbox.Services.IService;

public interface ILogDataService
{
    
    public Dictionary<string, LogDataChannel> LogDict { get; }

    public void AddChannel(string channelName);
    public void AddData(string channelName, double data);
    public void RemoveAllChannel();

}

public class LogDataChannel(int bufferCapacity)
{

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BufferCapacity { get; set; } = bufferCapacity;

    public static string LogDataTempFolder
    {
        get
        {
            var path = Path.Combine(AppConfig.FolderName, "tmp/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
    public string FilePath {
        get
        {
            var file = Path.Combine(LogDataTempFolder, "_" + Name + ".csv");
            if (!File.Exists(file))
            {
                File.Create(file);
            }
            return file;
        }
     }

    // storage tmp data for logging(default data type is double)
    private readonly CircularBuffer<double> _buffer = new(bufferCapacity);

    public void Add(double data) {
        _buffer.Add(data);
        if ((_buffer.Size * 2) >= _buffer.Capacity)
        {
            var dataSrc = _buffer.RemoveRange(_buffer.Size);
            Task.Run(() => SaveToFile(dataSrc, FilePath));
        }
    }

    public static void SaveToFile(ArraySegment<double> array, string filePath) {
        using var writer = new StreamWriter(filePath, true);
        foreach (double data in array)
        {
            writer.WriteLine(data.ToString());
        }
    }
}
