using System;
using SGApp.Models.Common;

namespace SGApp.DTOs {
	public class BinLoadDto : IKey{
		public string Key { get; set; }
		public int BinTicketID { get; set; }
		public int BinID { get; set; }
		public int TicketNumber { get; set; }
		public DateTime DateLoaded { get; set; }
		public int PoundsLoaded { get; set; }
		public string Vendor { get; set; }
		public string Note { get; set; }		
	}
}