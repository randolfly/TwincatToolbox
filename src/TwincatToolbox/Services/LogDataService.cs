using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwinCAT.Ads;

using TwincatToolbox.Services.IService;

namespace TwincatToolbox.Services;
public class LogDataService : ILogDataService {
    public int BufferCapacity { get; } = 1000;
    private readonly Dictionary<string, LogDataChannel> _logDict = [];
    public Dictionary<string, LogDataChannel> LogDict => _logDict;

    public void AddChannel(string channelName) {
        _logDict.Add(channelName, new LogDataChannel(BufferCapacity));
    }

    public void RemoveAllChannel() {

    }
    
    public void AddData(string channelName, double data) {
        if (_logDict.ContainsKey(channelName))
        {
            _logDict[channelName].Add(data);
        }
    }

    
}
