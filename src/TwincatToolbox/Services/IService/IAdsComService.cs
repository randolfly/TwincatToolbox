using System;
using System.Collections.Generic;
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
    public List<SymbolInfo> GetAvailableSymbols();

    public void AddNotificationHandler(EventHandler<AdsNotificationEventArgs> handler);
    public void RemoveNotificationHandler(EventHandler<AdsNotificationEventArgs> handler);
    public uint AddDeviceNotification(string path, int byteSize, NotificationSettings settings);

    public void RemoveDeviceNotification(uint notificationHandle);
}

public class AdsRouteInfo {
    public string Name { get; set; }
    public string Address { get; set; }
    public string NetId { get; set; }
}