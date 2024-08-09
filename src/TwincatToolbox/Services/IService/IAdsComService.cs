using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwincatToolbox.Models;

namespace TwincatToolbox.Services.IService;

public interface IAdsComService : IDisposable
{

    /// <summary>
    /// Ads连接状态
    /// </summary>
    /// <value>true: 连接上; false: 未连接</value>
    public bool IsAdsConnected { get; }
    
    public AdsState GetAdsState();
    public void ConnectAdsServer(AdsConfig adsConfig);
    public void DisconnectAdsServer();

    /// <summary>
    /// 扫描本地网络上的Ads服务器
    /// </summary>
    /// <returns>服务器地址(key: NetId, value: PortId)</returns>
    public IEnumerable<AmsNetId> ScanAdsNetwork();
}