using System;
using System.ComponentModel.DataAnnotations;

namespace SGApp.Models.Validation {
	class BinLoad_Metadata {
		[Required]
		[Key]
		public int BinTicketID { get; set; }
		[Required]
		public int BinID { get; set; }
		[Required]
		public int TicketNumber { get; set; }
		[Required]
		public DateTime DateLoaded { get; set; }
		[Required]
		public int PoundsLoaded { get; set; }
		public string Vendor { get; set; }
		public string Note { get; set; }
		[Required]
		public DateTime CreatedDate { get; set; }
		[Required]
		public int UserID { get; set; }
	}
}