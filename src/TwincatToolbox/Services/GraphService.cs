using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwincatToolbox.Services.IService;

namespace TwincatToolbox.Services;
public class GraphService
{
    public int PlotBufferCapacity { get; } = 5000;

    private readonly Dictionary<string, LogPlotWindow> _plotDict = [];
    public Dictionary<string, LogPlotWindow> PlotDict => _plotDict;

    public void AddChannel(string channelName) {
        var logPlotWindow = new LogPlotWindow(channelName, PlotBufferCapacity);
        _plotDict.Add(channelName, logPlotWindow);
        logPlotWindow.Show();
    }

    public void RemoveAllChannel() {

    }

    public void AddData(string channelName, double data) {
        if (_plotDict.ContainsKey(channelName))
        {
            _plotDict[channelName].Add(data);
        }
    }


}
