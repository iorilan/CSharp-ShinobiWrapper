using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.ShinobiVideo.Lib.Models
{
	public class ShinobiResponseVideoFileVm
	{
		public bool isUTC { get; set; }
		public int total { get; set; }
		public int limit { get; set; }
		public int skip { get; set; }
		public IList<ShinobiResponseVideoVm> videos { get; set; }
	}
}
