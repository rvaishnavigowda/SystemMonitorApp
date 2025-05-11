using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemMonitorApp.Domain.Contracts;

namespace SystemMonitorApp.Infrastructure.Plugin
{
    public class ConsolePlugin : IMonitorPlugin
{
    public void OnStatsCollected(SystemStats stats)
    {
        Console.WriteLine($"[Plugin] CPU: {stats.CpuUsagePercent}% | RAM: {stats.RamUsedMB}/{stats.RamTotalMB} MB | Disk: {stats.DiskUsedMB}/{stats.DiskTotalMB} MB");
    }
}
}
