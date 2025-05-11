using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemMonitorApp.Domain.Model
{
    public class MonitorConfig
    {
        public string? ApiUrl { get; set; }
        public int IntervalMs { get; set; } = 5000;
    }
}
