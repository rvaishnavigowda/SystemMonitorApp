using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using SystemMonitorApp.Domain.Contracts;

namespace SystemMonitorApp.Infrastructure.Plugin
{
    public class FileLoggerPlugin : IMonitorPlugin
    {
        private readonly string _filePath = "monitor_log.txt";

        public void OnStatsCollected(SystemStats stats)
        {
            string log = $"{DateTime.Now:u} | CPU: {stats.CpuUsagePercent}% | RAM: {stats.RamUsedMB}/{stats.RamTotalMB} MB | Disk: {stats.DiskUsedMB}/{stats.DiskTotalMB} MB";
            File.AppendAllText(_filePath, log + Environment.NewLine);
        }
    }
}
