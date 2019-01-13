using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.ShinobiVideo.Lib.Models
{
    public class ShinobiMonitoringVm
    {
        public string MonitorId { get; set; }
        public DateTime LastPingTime { get; set; }
        public string CameraIp { get; set; }
    }
}
