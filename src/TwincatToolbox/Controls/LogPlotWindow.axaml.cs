using System.Linq;
using System.Timers;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using ScottPlot;
using ScottPlot.Plottables;

namespace TwincatToolbox;

public partial class LogPlotWindow : Window
{
    private readonly DataStreamer DataStreamer;
    private readonly Timer UpdatePlotTimer = new() { Interval = 50, Enabled = true, AutoReset = true };

    public LogPlotWindow(string title, int logNum) {
        InitializeComponent();

        Title = title;
        // LogPlot.Plot.Title(Title);
        LogPlot.Plot.Axes.ContinuouslyAutoscale = true;
        LogPlot.Plot.ScaleFactor = 1.5;

        DataStreamer = LogPlot.Plot.Add.DataStreamer(logNum);
        DataStreamer.ViewScrollLeft();

        // setup a timer to request a render periodically
        UpdatePlotTimer.Elapsed += (s, e) =>
            {
                if (DataStreamer.HasNewData)
                {
                    LogPlot.Refresh();
                }
            };
    }

    public void UpdatePlot(double newData) {
        DataStreamer.Add(newData);
        // slide marker to the left
        LogPlot.Plot.GetPlottables<Marker>()
            .ToList()
    .ForEach(m => m.X -= 1);

        // remove off-screen marks
        LogPlot.Plot.GetPlottables<Marker>()
              .Where(m => m.X < 0)
              .ToList()
              .ForEach(m => LogPlot.Plot.Remove(m));
    }

}
