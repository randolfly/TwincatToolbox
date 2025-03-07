using System;
using System.Linq;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MathNet.Numerics.Integration;
using ScottPlot;
using ScottPlot.Plottables;

namespace TwincatToolbox;

public partial class LogPlotWindow : Window, IDisposable
{
    public string LogName { get; set; } = "Log";
    private readonly DataStreamer DataStreamer;
    private readonly Timer UpdatePlotTimer = new() { Interval = 50, Enabled = true, AutoReset = true };

    private SignalXY FullDataSignal;
    private Crosshair FullDataCrosshair;

    public LogPlotWindow(string title, int logNum)
    {
        InitializeComponent();
        LogName = title;
        Title = LogName;
        // will cause const data flow not render!
        //LogPlot.Plot.Axes.ContinuouslyAutoscale = true;
        LogPlot.Plot.ScaleFactor = 1.0;

        DataStreamer = LogPlot.Plot.Add.DataStreamer(logNum);
        DataStreamer.ViewScrollLeft();

        // setup a timer to request a render periodically
        UpdatePlotTimer.Elapsed += (s, e) =>
        {
            if (DataStreamer.HasNewData)
            {
                LogPlot.Refresh();
            }

            LogPlot.Plot.Axes.AutoScale();
        };
    }

    public void UpdatePlot(double newData)
    {
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

    /// <summary>
    /// manage plot window position by plot id
    /// </summary>
    /// <param name="windowId">the id of plot window in the plot dict</param>
    public void SetPlotViewWindowPosById(int windowId)
    {
        // todo: figure out the windows place rule
        // the window height is wrong, actual height:1440, return height: 1600=>about 0.19 ratio
        var screen = Screens?.Primary;
        var widthScaling = (screen?.Scaling ?? 1.0);
        var heightScaling = widthScaling + 0.2;
        var workingArea = screen?.WorkingArea ?? new Avalonia.PixelRect(0, 0, (int)Width, (int)Height);
        var windowRowSize = (int)((workingArea.Height) / Height / heightScaling);
        var left = (workingArea.Right) - (windowId / windowRowSize + 1) * ((int)(Width * widthScaling));
        var top = (workingArea.Bottom) - (windowId % windowRowSize + 1) * ((int)(Height * heightScaling));

        Position = new PixelPoint(left, top);
    }

    /// <summary>
    /// clear current plot and show new data with SignalConst Type for better performance  
    /// </summary>
    /// <param name="ys"></param>
    /// <param name="sampleTime">sample time, unit ms</param>
    public void ShowAllData(double[] ys, int sampleTime = 1)
    {
        UpdatePlotTimer.Stop();
        LogPlot.Plot.Clear();
        //LogPlot.Plot.Axes.ContinuouslyAutoscale = false;
        var xs = Enumerable.Range(0, ys.Length)
            .Select(x => x * sampleTime).ToArray();
        LogPlot.Plot.Add.Palette = new ScottPlot.Palettes.Nord();
        FullDataSignal = LogPlot.Plot.Add.SignalXY(xs, ys);

        FullDataCrosshair = LogPlot.Plot.Add.Crosshair(0, 0);
        FullDataCrosshair.IsVisible = false;
        FullDataCrosshair.MarkerShape = MarkerShape.OpenCircle;
        FullDataCrosshair.MarkerSize = 5;

        //LogPlot.Plot.XLabel("Time(ms)");
        //CustomPlotInteraction();
        LogPlot.Plot.Axes.AutoScale();
        LogPlot.Refresh();

        LogPlot.PointerMoved += (s, e) =>
        {
            var currentPosition = e.GetCurrentPoint(LogPlot).Position;
            // determine where the mouse is and get the nearest point
            Pixel mousePixel = new(currentPosition.X, currentPosition.Y);
            Coordinates mouseLocation = LogPlot.Plot.GetCoordinates(mousePixel);

            DataPoint nearest = FullDataSignal.Data.GetNearest(mouseLocation,
                LogPlot.Plot.LastRender);

            // place the crosshair over the highlighted point
            if (nearest.IsReal)
            {
                FullDataCrosshair.IsVisible = true;
                FullDataCrosshair.Position = nearest.Coordinates;
                LogPlot.Refresh();
                Title = $"{LogName}: X={nearest.X:0.##}, Y={nearest.Y:0.##}";
            }

            // hide the crosshair when no point is selected
            if (!nearest.IsReal && FullDataCrosshair.IsVisible)
            {
                FullDataCrosshair.IsVisible = false;
                LogPlot.Refresh();
                Title = $"{LogName}";
            }
        };
    }

    private void CustomPlotInteraction()
    {
        LogPlot.UserInputProcessor.IsEnabled = true;

        // remove all existing responses so we can create and add our own
        LogPlot.UserInputProcessor.UserActionResponses.Clear();

        // middle-click-drag pan
        var panButton = ScottPlot.Interactivity.StandardMouseButtons.Middle;
        var panResponse = new ScottPlot.Interactivity.UserActionResponses.MouseDragPan(panButton);
        LogPlot.UserInputProcessor.UserActionResponses.Add(panResponse);

        // right-click-drag zoom rectangle
        var zoomRectangleButton = ScottPlot.Interactivity.StandardMouseButtons.Right;
        var zoomRectangleResponse =
            new ScottPlot.Interactivity.UserActionResponses.MouseDragZoomRectangle(zoomRectangleButton);
        LogPlot.UserInputProcessor.UserActionResponses.Add(zoomRectangleResponse);

        // right-click autoscale
        var autoscaleButton = ScottPlot.Interactivity.StandardMouseButtons.Right;
        var autoscaleResponse = new ScottPlot.Interactivity.UserActionResponses.SingleClickAutoscale(autoscaleButton);
        LogPlot.UserInputProcessor.UserActionResponses.Add(autoscaleResponse);

        // left-click menu
        var menuButton = ScottPlot.Interactivity.StandardMouseButtons.Left;
        var menuResponse = new ScottPlot.Interactivity.UserActionResponses.SingleClickContextMenu(menuButton);
        LogPlot.UserInputProcessor.UserActionResponses.Add(menuResponse);

        // Q key autoscale too
        var autoscaleKey = new ScottPlot.Interactivity.Key("Q");
        Action<ScottPlot.Plot, ScottPlot.Pixel> autoscaleAction = (plot, pixel) => plot.Axes.AutoScale();
        var autoscaleKeyResponse =
            new ScottPlot.Interactivity.UserActionResponses.KeyPressResponse(autoscaleKey, autoscaleAction);
        LogPlot.UserInputProcessor.UserActionResponses.Add(autoscaleKeyResponse);

        // WASD keys pan
        var keyPanResponse = new ScottPlot.Interactivity.UserActionResponses.KeyboardPanAndZoom()
        {
            PanUpKey = new ScottPlot.Interactivity.Key("W"),
            PanLeftKey = new ScottPlot.Interactivity.Key("A"),
            PanDownKey = new ScottPlot.Interactivity.Key("S"),
            PanRightKey = new ScottPlot.Interactivity.Key("D"),
        };
        LogPlot.UserInputProcessor.UserActionResponses.Add(keyPanResponse);
    }

    public void Dispose()
    {
        UpdatePlotTimer.Dispose();
    }
}