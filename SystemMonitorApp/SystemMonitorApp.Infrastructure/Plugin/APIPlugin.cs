using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Model;
using SystemMonitorApp.Domain.Contracts;

namespace SystemMonitorApp.Infrastructure.Plugin
{
    public class APIPlugin : IMonitorPlugin
    {
        private readonly string _url;

        public APIPlugin(string url)
        {
            _url = url;
        }

        public async void OnStatsCollected(SystemStats stats)
        {
            using var client = new HttpClient();

            var payload = new
            {
                cpu = stats.CpuUsagePercent,
                ram_used = stats.RamUsedMB,
                disk_used = stats.DiskUsedMB
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                await client.PostAsync(_url, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API Plugin] Failed to post: {ex.Message}");
            }
        }
    }
}
