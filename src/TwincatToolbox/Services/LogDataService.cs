using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathNet.Numerics.Data.Matlab;
using MathNet.Numerics.LinearAlgebra;

using TwinCAT.Ads;

using TwincatToolbox.Services.IService;

namespace TwincatToolbox.Services;
public class LogDataService : ILogDataService
{
    public int BufferCapacity { get; } = 1000;
    private readonly Dictionary<string, LogDataChannel> _logDict = [];
    public Dictionary<string, LogDataChannel> LogDict => _logDict;

    public void AddChannel(string channelName) {
        _logDict.Add(channelName, new LogDataChannel(BufferCapacity, channelName));
    }

    public void RemoveAllChannels() {
        _logDict.Clear();
    }
    
    public async Task AddDataAsync(string channelName, double data) {
        if (_logDict.ContainsKey(channelName))
        {
            await _logDict[channelName].AddAsync(data);
        }
    }

    public async Task<List<double>> LoadDataAsync(string channelName) {
        return await _logDict[channelName].LoadFromFileAsync();
    }

    public async Task<Dictionary<string, List<double>>> LoadAllChannelsAsync() {
        var resultDict = new Dictionary<string, List<double>>();
        foreach (var channel in _logDict)
        {
            resultDict.Add(channel.Key, await channel.Value.LoadFromFileAsync());
        }
        var minCount = resultDict.Min(x => x.Value.Count);
        foreach (var channel in resultDict)
        {
            channel.Value.RemoveRange(minCount, channel.Value.Count - minCount);
        }
        return resultDict;
    }

    /// <summary>
    /// export data to file
    /// </summary>
    /// <param name="dataSrc"></param>
    /// <param name="fileName">export file full name, doesn't contains suffix, such as "c:/FOLDER/aaa"</param>
    /// <param name="exportTypes"></param>
    /// <returns></returns>
    public async Task ExportDataAsync(Dictionary<string, List<double>>dataSrc, string fileName, List<string> exportTypes) {
        if (exportTypes.Contains("csv"))
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Join(',', dataSrc.Keys));
            var rowCount = dataSrc.First().Value.Count;
            for (var i = 0; i < rowCount; i++)
            {
                var row = new List<string>();
                foreach (var channel in dataSrc)
                {
                    row.Add(channel.Value[i].ToString(CultureInfo.InvariantCulture));
                }
                stringBuilder.AppendLine(string.Join(',', row));
            }
            await File.WriteAllTextAsync(fileName + ".csv", stringBuilder.ToString());
        }
        if (exportTypes.Contains("mat"))
        {
            var exportMatDict = new Dictionary<string, Matrix<double>>();
            foreach (var keyValuePair in dataSrc)
            {
                exportMatDict.Add(keyValuePair.Key.Replace(".", "_"),
                    Matrix<double>.Build.Dense(keyValuePair.Value.Count, 1, keyValuePair.Value.ToArray()));
            }
            await Task.Run(() => MatlabWriter.Write(fileName + ".mat", exportMatDict));
        }
    }

    public void DeleteTmpFiles() {
        _logDict.Values.ToList().ForEach(channel => channel.DeleteTmpFile());
    }
}
