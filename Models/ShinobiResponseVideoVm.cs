using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.ShinobiVideo.Lib.Models
{
    public class ShinobiResponseVideoVm
    {
        /// <summary>
        /// monitor id
        /// </summary>
        public string mid { get; set; }

        /// <summary>
        /// group key
        /// </summary>
        public string ke { get; set; }

        public string ext { get; set; }

        public string time { get; set; }

        public string duration { get; set; }

        public long size { get; set; }

        //public string frames { get; set; }

        public string end { get; set; }

        public int status { get; set; }

        //public string details { get; set; }

        public string href { get; set; }

        public ShinobiResponseVideoActionLinks links { get; set; }
    
    }
    public class ShinobiResponseVideoActionLinks{
        public string deleteVideo { get; set; }
        public string changeToUnread { get; set; }
        public string changeToRead { get; set; }
    }
}
