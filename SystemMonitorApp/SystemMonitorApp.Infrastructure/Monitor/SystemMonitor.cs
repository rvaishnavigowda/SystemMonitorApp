using Microsoft.Management.Infrastructure;
using Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SystemMonitorApp.Domain.Contracts;

namespace SystemMonitorApp.Infrastructure.Monitor
{
    public class SystemMonitor : ISystemMonitor
    {
        public SystemStats GetSystemStats()
        {
            var cpu = GetCpuUsage();
            var ram = GetRamUsage();
            var disk = GetDiskUsage();

            return new SystemStats
            {
                CpuUsagePercent = cpu,
                RamUsedMB = ram.used,
                RamTotalMB = ram.total,
                DiskUsedMB = disk.used,
                DiskTotalMB = disk.total
            };
        }

        private double GetCpuUsage()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return 0; // Skip CPU tracking on non-Windows for now

            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); // Warm-up
            Thread.Sleep(1000);
            return Math.Round(cpuCounter.NextValue(), 2);
        }

        private (double used, double total) GetRamUsage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return ReadLinuxMemoryInfo();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ReadWindowsMemoryInfo();
            }

            return (0, 0); // Default fallback
        }

        private (double used, double total) ReadWindowsMemoryInfo()
        {
            try
            {
                using var session = CimSession.Create(null);
                var results = session.QueryInstances(
                    "root\\cimv2",
                    "WQL",
                    "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"
                );

                var info = results.FirstOrDefault();
                if (info == null) return (0, 0);

                double total = Convert.ToDouble(info.CimInstanceProperties["TotalVisibleMemorySize"].Value) / 1024;
                double free = Convert.ToDouble(info.CimInstanceProperties["FreePhysicalMemory"].Value) / 1024;

                return (Math.Round(total - free, 2), Math.Round(total, 2));
            }
            catch
            {
                return (0, 0);
            }
        }

        private (double used, double total) ReadLinuxMemoryInfo()
        {
            try
            {
                var lines = File.ReadAllLines("/proc/meminfo");
                double totalKb = GetKbValue(lines, "MemTotal");
                double availableKb = GetKbValue(lines, "MemAvailable");

                double usedMb = (totalKb - availableKb) / 1024.0;
                double totalMb = totalKb / 1024.0;

                return (Math.Round(usedMb, 2), Math.Round(totalMb, 2));
            }
            catch
            {
                return (0, 0);
            }
        }

        private double GetKbValue(string[] lines, string key)
        {
            var line = lines.FirstOrDefault(l => l.StartsWith(key));
            if (line == null) return 0;
            var parts = line.Split(':');
            var valuePart = parts[1].Trim().Split(' ')[0];
            return double.TryParse(valuePart, out var value) ? value : 0;
        }

        private (double used, double total) GetDiskUsage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name == "C:\\");
                if (drive == null) return (0, 0);

                double total = drive.TotalSize / (1024.0 * 1024.0);
                double free = drive.TotalFreeSpace / (1024.0 * 1024.0);
                return (Math.Round(total - free, 2), Math.Round(total, 2));
            }
            else
            {
                var rootDrive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name == "/");
                if (rootDrive == null) return (0, 0);

                double total = rootDrive.TotalSize / (1024.0 * 1024.0);
                double free = rootDrive.TotalFreeSpace / (1024.0 * 1024.0);
                return (Math.Round(total - free, 2), Math.Round(total, 2));
            }
        }

    }
}
