using System.Security.Cryptography;
using TwincatToolbox.Services;

namespace TwincatToolbox.Test;

public class AppConfigTest
{
    [Fact]
    public void SaveConfigTest()
    {
        var tmpPath = Path.GetTempPath();
        var tmpFile = Path.Combine(tmpPath, "appconfig.json");
        if (File.Exists(tmpFile)) File.Delete(tmpFile);
        Assert.False(File.Exists(tmpFile));
        AppConfigService.SaveConfig(tmpFile);
        Assert.True(File.Exists(tmpFile));
    }

    [Fact]
    public void LoadConfigTest()
    {
        var tmpPath = Path.GetTempPath();
        var tmpFile = Path.Combine(tmpPath, "appconfig.json");
        if (File.Exists(tmpFile)) File.Delete(tmpFile);
        AppConfigService.AppConfig.LogConfig.Period = 10;
        Assert.NotEqual(2, AppConfigService.AppConfig.LogConfig.Period);
        AppConfigService.SaveConfig(tmpFile);
        Assert.Equal(10, AppConfigService.AppConfig.LogConfig.Period);
    }
}