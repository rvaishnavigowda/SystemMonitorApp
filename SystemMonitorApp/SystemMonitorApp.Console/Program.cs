using System.Text.Json;
using SystemMonitorApp.Domain.Contracts;
using SystemMonitorApp.Domain.Model;
using SystemMonitorApp.Infrastructure.Monitor;
using SystemMonitorApp.Infrastructure.Plugin;

namespace SystemMonitorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "config.json");
            if (!File.Exists(configFilePath))
            {
                Console.WriteLine("Configuration file not found.");
                return;
            }
            var config = JsonSerializer.Deserialize<MonitorConfig>(File.ReadAllText(configFilePath));

            if (config == null)
            {
                Console.WriteLine("Failed to load configuration.");
                return;
            }

            ISystemMonitor monitor = new SystemMonitor();

            List<IMonitorPlugin> plugins = new()
            {
                new ConsolePlugin(),
                new FileLoggerPlugin()
            };

            if (!string.IsNullOrEmpty(config.ApiUrl))
            {
                plugins.Add(new APIPlugin(config.ApiUrl));
            }

            var service = new SystemMonitorService(monitor, plugins);

            Console.WriteLine("System Monitor Started. Press Ctrl+C to stop.");
            while (true)
            {
                service.CollectStats();
                Thread.Sleep(config.IntervalMs);
            }
        }
    }
}
