using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwincatToolbox.Models;
using TwincatToolbox.Services.IService;

namespace TwincatToolbox.Services;

public class AdsComService : IAdsComService
{
    public bool IsAdsConnected => adsClient.IsConnected;

    private readonly AdsClient adsClient = new();

    public AdsState GetAdsState() => adsClient.ReadState().AdsState;

    public void ConnectAdsServer(AdsConfig adsConfig)
    {
        adsClient.Connect(adsConfig.NetId, adsConfig.PortId);
    }

    public void DisconnectAdsServer()
    {
        if (IsAdsConnected) adsClient.Disconnect();
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
