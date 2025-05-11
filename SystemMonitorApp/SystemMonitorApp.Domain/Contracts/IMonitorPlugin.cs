using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace SystemMonitorApp.Domain.Contracts
{
    public interface IMonitorPlugin
    {
        public void OnStatsCollected(SystemStats stats);

    }
}
