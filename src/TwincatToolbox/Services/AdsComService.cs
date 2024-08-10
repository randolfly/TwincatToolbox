using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using TwinCAT.ValueAccess;

using TwincatToolbox.Models;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.Services;

public class AdsComService : IAdsComService
{
    public bool IsAdsConnected => adsClient.IsConnected;

    private readonly AdsClient adsClient = new();

    public AdsState GetAdsState()
    {
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

    public void ConnectAdsServer(AdsConfig adsConfig)
    {
        var amsAddress = new AmsAddress(adsConfig.NetId, adsConfig.PortId);
        //todo: add async method
        adsClient.Connect(amsAddress);
        Debug.WriteLine("Ads server connected: {0}:{1}", adsConfig.NetId, adsConfig.PortId);
        Debug.WriteLine("Ads server state: {0}", GetAdsState());
    }

    public void DisconnectAdsServer()
    {
        if (IsAdsConnected) adsClient.Disconnect();
        Debug.WriteLine("Ads server state: {0}", GetAdsState());
    }

    /// <summary>
    /// 获取所有可行的Symbols
    /// </summary>
    /// <returns>符号列表</returns>
    public List<SymbolInfo> GetAvailableSymbols()
    {
        var settings = new SymbolLoaderSettings(SymbolsLoadMode.VirtualTree,
            ValueAccessMode.IndexGroupOffset);
        var symbolLoader = SymbolLoaderFactory.Create(adsClient, settings);
        var symbols = symbolLoader.Symbols;

        var symbolList = new List<SymbolInfo>(2);
        foreach (var symbol in symbols)
        {
            if (symbol.InstanceName is not ("MAIN" or "GVL")) continue;
            LoadSymbolNode(new SymbolInfo(symbol), ref symbolList);
        }
        return symbolList;

        static void LoadSymbolNode(SymbolInfo symbol, ref List<SymbolInfo> symbolList)
        {
            if (symbol.Symbol.SubSymbols.Count > 0)
            {
                foreach (var subSymbol in symbol.Symbol.SubSymbols)
                {
                    if (subSymbol.SubSymbols.Count > 0)
                    {
                        LoadSymbolNode(new SymbolInfo(subSymbol), ref symbolList);
                    }
                    else
                    {
                        symbolList.Add(new SymbolInfo(subSymbol));
                    }
                }
            }
        }
    }
    public void Dispose()
    {
        adsClient.Dispose();
    }

    public List<AmsNetId> ScanAdsNetwork()
    {
        // todo: 没有现成的API可以扫描本地网络上的Ads服务器(PowerShell可以实现)
        var adsServers = new List<AmsNetId>();

        return adsServers;
    }
}
