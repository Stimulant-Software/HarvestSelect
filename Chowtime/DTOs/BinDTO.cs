using System;
using SGApp.Models.Common;

namespace SGApp.DTOs {
	public class BinDto :IKey {
		public int BinID { get; set; }
		public string BinName { get; set; }
		public int? FarmID { get; set; }
		public int? CurrentTicket { get; set; }
		public int? CurrentPounds { get; set; }
		public DateTime? LastDisbursement { get; set; }
		public DateTime? LastLoaded { get; set; }
		public string Key { get; set; }
		public int? Reconciliation { get; set; }
        public int[] BinFarms { get; set; }
    }
}