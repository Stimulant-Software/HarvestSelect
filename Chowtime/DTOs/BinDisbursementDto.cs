using System;
using SGApp.Models.Common;

namespace SGApp.DTOs{ 
	public class BinDisbursementDto : IKey {
		public int BinDisbursementID { get; set; }
		public int BinID { get; set; }
		public int TicketNumber { get; set; }
		public int Pounds { get; set; }
		public string Note { get; set; }
		public int? DisbursementType { get; set; }
		public DateTime DisbursementDate { get; set; }
		public int? FeedID { get; set; }
		public string Key { get; set; }
		public DateTime CreatedDate { get; set; }
		public int UserID { get; set; }
	}
}