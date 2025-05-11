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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetWindowsCpuUsage();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return GetLinuxCpuUsage();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return GetMacCpuUsage();

            return 0;
        }

        private double GetWindowsCpuUsage()
        {
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            Thread.Sleep(1000);
            return Math.Round(cpuCounter.NextValue(), 2);
        }

        private double GetLinuxCpuUsage()
        {
            try
            {
                var lines = File.ReadLines("/proc/stat");
                var cpuLine = lines.FirstOrDefault(line => line.StartsWith("cpu "));
                if (cpuLine == null) return 0;

                var parts = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var user = double.Parse(parts[1]);
                var nice = double.Parse(parts[2]);
                var system = double.Parse(parts[3]);
                var idle = double.Parse(parts[4]);

                var total = user + nice + system + idle;
                var used = user + nice + system;

                return Math.Round(used / total * 100, 2);
            }
            catch
            {
                return 0;
            }
        }

        private double GetMacCpuUsage()
        {
            try
            {
                var psi = new ProcessStartInfo("sh", "-c \"ps -A -o %cpu | awk '{s+=$1} END {print s}'\"")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                using var process = Process.Start(psi);
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return double.TryParse(output.Trim(), out var cpu) ? Math.Round(cpu, 2) : 0;
            }
            catch
            {
                return 0;
            }
        }

        private (double used, double total) GetRamUsage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return ReadLinuxMemoryInfo();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return ReadWindowsMemoryInfo();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return ReadMacMemoryInfo();

            return (0, 0);
        }

        private (double used, double total) ReadWindowsMemoryInfo()
        {
            try
            {
                using var session = CimSession.Create(null);
                var results = session.QueryInstances("root\\cimv2", "WQL",
                    "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");

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

        private (double used, double total) ReadMacMemoryInfo()
        {
            try
            {
                var psi = new ProcessStartInfo("vm_stat")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                using var process = Process.Start(psi);
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                double pageSize = 4096;
                var lines = output.Split('\n');

                double pagesFree = GetVmStatValue(lines, "Pages free");
                double pagesActive = GetVmStatValue(lines, "Pages active");
                double pagesInactive = GetVmStatValue(lines, "Pages inactive");
                double pagesWired = GetVmStatValue(lines, "Pages wired down");

                double used = (pagesActive + pagesInactive + pagesWired) * pageSize / (1024 * 1024);
                double total = (pagesFree + pagesActive + pagesInactive + pagesWired) * pageSize / (1024 * 1024);

                return (Math.Round(used, 2), Math.Round(total, 2));
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

        private double GetVmStatValue(string[] lines, string key)
        {
            var line = lines.FirstOrDefault(l => l.Contains(key));
            if (line == null) return 0;
            var parts = line.Split(':');
            var valuePart = parts[1].Trim().Split('.')[0];
            return double.TryParse(valuePart, out var value) ? value : 0;
        }

        private (double used, double total) GetDiskUsage()
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && (d.Name == "/" || d.Name == "C:\\"));
            if (drive == null) return (0, 0);

            double total = drive.TotalSize / (1024.0 * 1024.0);
            double free = drive.TotalFreeSpace / (1024.0 * 1024.0);
            return (Math.Round(total - free, 2), Math.Round(total, 2));
        }
    }
}
