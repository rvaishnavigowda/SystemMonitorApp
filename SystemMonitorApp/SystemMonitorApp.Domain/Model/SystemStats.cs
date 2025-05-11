using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class SystemStats
    {
        public double CpuUsagePercent { get; set; }
        public double RamUsedMB { get; set; }
        public double RamTotalMB { get; set; }
        public double DiskUsedMB { get; set; }
        public double DiskTotalMB { get; set; }
    }
}
