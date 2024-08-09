using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Avalonia.Logging;

using TwinCAT.Ads;
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

    public void Dispose()
    {
        adsClient.Dispose();
    }

    public IEnumerable<AmsNetId> ScanAdsNetwork()
    {
        // todo: 没有现成的API可以扫描本地网络上的Ads服务器(PowerShell可以实现)
        var adsServers = new List<AmsNetId>();

        return adsServers;
    }


}
