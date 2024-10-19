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

public interface ILogPlotService
{
    
    public Dictionary<string, LogPlotWindow> PlotDict { get; }

    public void AddChannel(string channelName, int plotBufferCapacity);
    public void AddData(string channelName, double data);
    public void RemoveAllChannels();
    public void ShowAllChannelsWithNewData(Dictionary<string, List<double>> dataSrcDict, int sampleTime);
}