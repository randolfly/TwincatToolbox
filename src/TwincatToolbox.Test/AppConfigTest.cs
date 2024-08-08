using System.Security.Cryptography;
using TwincatToolbox.Services;

namespace TwincatToolbox.Test;

public class AppConfigTest
{
    [Fact]
    public async Task SaveConfigAsync()
    {
        var tmpPath = Path.GetTempPath();
        var tmpFile = Path.Combine(tmpPath, "appconfig.json");
        if (File.Exists(tmpFile)) File.Delete(tmpFile);
        Assert.False(File.Exists(tmpFile));
        await AppConfigService.SaveConfigAsync(tmpFile);
        Assert.True(File.Exists(tmpFile));
    }

    [Fact]
    public async Task LoadConfigAsyncTest()
    {
        var tmpPath = Path.GetTempPath();
        var tmpFile = Path.Combine(tmpPath, "appconfig.json");
        if (File.Exists(tmpFile)) File.Delete(tmpFile);
        AppConfigService.AppConfig.LogConfig.Period = 10;
        Assert.NotEqual(2, AppConfigService.AppConfig.LogConfig.Period);
        await AppConfigService.SaveConfigAsync(tmpFile);
        await AppConfigService.LoadConfigAsync(tmpFile);
        Assert.Equal(10, AppConfigService.AppConfig.LogConfig.Period);
    }
}