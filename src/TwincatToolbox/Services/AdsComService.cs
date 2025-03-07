using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using TwinCAT.ValueAccess;

using TwincatToolbox.Extensions;
using TwincatToolbox.Models;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.Services;

public class AdsComService : IAdsComService
{
    public bool IsAdsConnected => adsClient.IsConnected;

    private readonly AdsClient adsClient = new();

    public AdsState GetAdsState() {
        var adsState = AdsState.Invalid;
        try
        {
            var stateInfo = adsClient.ReadState();
            adsState = stateInfo.AdsState;
            Debug.WriteLine("ADS State: {0}, Device State: {1}", stateInfo.AdsState, stateInfo.DeviceState);
        }
        catch (AdsErrorException ex)
        {
            Debug.WriteLine("ADS Error: {0}", ex.Message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("General Exception: {0}", ex.Message);
        }
        return adsState;
    }

    public void ConnectAdsServer(AdsConfig adsConfig) {
        var amsAddress = new AmsAddress(adsConfig.NetId, adsConfig.PortId);
        //todo: add async method
        adsClient.Connect(amsAddress);
        Debug.WriteLine("Ads server connected: {0}:{1}", adsConfig.NetId, adsConfig.PortId);
        Debug.WriteLine("Ads server state: {0}", GetAdsState());
    }

    public void DisconnectAdsServer() {
        if (IsAdsConnected) adsClient.Disconnect();
        Debug.WriteLine("Ads server state: {0}", GetAdsState());
    }

    /// <summary>
    /// 获取所有可行的Symbols
    /// 1. 通过SymbolLoader加载所有Symbols
    /// 2. 递归遍历所有Symbols(仅获取MAIN和GVL下的Symbols)
    /// 3. 将Symbol转化为SymbolTree，同时执行重复项剔除
    /// 4. 将SymbolTree转化为List<SymbolInfo>，返回
    /// </summary>
    /// <returns></returns>
    public List<SymbolInfo> GetAvailableSymbols() {
        var settings = new SymbolLoaderSettings(SymbolsLoadMode.VirtualTree,
           ValueAccessMode.SymbolicByHandle);
        var symbolLoader = SymbolLoaderFactory.Create(adsClient, settings);
        var symbols = symbolLoader.Symbols;

        var symbolList = new List<SymbolInfo>();

        foreach (var symbol in symbols)
        {
            if (symbol.InstanceName is ("MAIN" or "GVL"))
            {
                symbolList.AddRange(LoadSymbolTreeBFS(symbol));
            }
        }

        return symbolList;

        List<SymbolInfo> LoadSymbolTreeBFS(ISymbol root) {
            var symbolList = new List<SymbolInfo>();
            var transverseOrder = new Queue<ISymbol>();

            var symbolLoadQueue = new Queue<ISymbol>();
            symbolLoadQueue.Enqueue(root);
            

            while (symbolLoadQueue.Count > 0)
            {
                var currentSymbol = symbolLoadQueue.Dequeue();
                transverseOrder.Enqueue(currentSymbol);
                foreach (var subSymbol in currentSymbol.SubSymbols)
                {
                    if (!subSymbol.IsReference)
                    {
                        symbolLoadQueue.Enqueue(subSymbol);
                    }
                }
            }

            while (transverseOrder.Count > 0)
            {
                var symbol = transverseOrder.Dequeue();
                if(symbol.SubSymbols.Count == 0)
                {
                    symbolList.Add(new SymbolInfo(symbol));
                }
            }
            
            return symbolList;
        }

    }

    public void Dispose() {
        adsClient.Dispose();
    }

    public void AddNotificationHandler(EventHandler<AdsNotificationEventArgs> handler) {
        adsClient.AdsNotification += handler;
    }

    public void RemoveNotificationHandler(EventHandler<AdsNotificationEventArgs> handler) {
        adsClient.AdsNotification -= handler;
    }

    public uint AddDeviceNotification(string path, int byteSize, NotificationSettings settings)
    {
        adsClient.TryAddDeviceNotification(path, byteSize, settings, null, out var notificationHandle);
        return notificationHandle;
    }

    public void RemoveDeviceNotification(uint notificationHandle) {
        adsClient.TryDeleteDeviceNotification(notificationHandle);
    }

    public static List<AdsRouteInfo> ScanAdsRoutes() {
        var xmlPath = "C:\\TwinCAT\\3.1\\Target\\StaticRoutes.xml";
        var xml = XDocument.Load(xmlPath);
        var routeList = xml?.Root?.Element("RemoteConnections")?.Elements()
            .Select(route => new AdsRouteInfo
            {
                Name = route.Element("Name")?.Value ?? string.Empty,
                Address = route.Element("Address")?.Value ?? string.Empty,
                NetId = route.Element("NetId")?.Value ?? string.Empty
            }).ToList()!;

        routeList.Add(new AdsRouteInfo
        {
            Name = "Local",
            Address = AmsNetId.Local.ToString(),
            NetId = AmsNetId.Local.ToString()
        });
        return routeList;
    }
}
