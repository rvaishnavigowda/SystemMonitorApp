using Model;
using SystemMonitorApp.Domain.Contracts;

public class SystemMonitorService
{
    private readonly ISystemMonitor _systemMonitor;
    private readonly IEnumerable<IMonitorPlugin> _plugins;

    public SystemMonitorService(ISystemMonitor systemMonitor, IEnumerable<IMonitorPlugin> plugins)
    {
        _systemMonitor = systemMonitor;
        _plugins = plugins;
    }

    public SystemStats CollectStats()
    {
        var stats = _systemMonitor.GetSystemStats();

        foreach (var plugin in _plugins)
        {
            plugin.OnStatsCollected(stats);
        }

        return stats;
    }
}
