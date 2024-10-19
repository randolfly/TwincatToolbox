using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwincatToolbox.Services.IService;

namespace TwincatToolbox.Services;
public class LogPlotService : ILogPlotService
{

    private readonly Dictionary<string, LogPlotWindow> _plotDict = [];
    public Dictionary<string, LogPlotWindow> PlotDict => _plotDict;

    public void AddChannel(string channelName, int plotBufferCapacity) {
        var logPlotWindow = new LogPlotWindow(channelName, plotBufferCapacity);
        _plotDict.Add(channelName, logPlotWindow);
        logPlotWindow.Show();
    }

    public void RemoveAllChannels() {
        foreach (var plot in _plotDict.Values)
        {
            plot.Close();
        }
        _plotDict.Clear();
    }

    public void AddData(string channelName, double data) {
        if (_plotDict.ContainsKey(channelName))
        {
            _plotDict[channelName].UpdatePlot(data);
        }
    }

    public void ShowAllChannelsWithNewData(Dictionary<string, List<double>> dataSrcDict, int sampleTime = 1) {
        foreach (var (channelName, data) in dataSrcDict)
        {
            if (_plotDict.ContainsKey(channelName))
            {
                _plotDict[channelName].ShowAllData(data.ToArray(), sampleTime);
            }
        }
    }
}
