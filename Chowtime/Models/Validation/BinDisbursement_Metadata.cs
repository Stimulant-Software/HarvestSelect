using System;
using System.ComponentModel.DataAnnotations;

namespace SGApp.Models.Validation {
	class BinDisbursement_Metadata {
		[Required]
		[Key]
		public int BinDisbursementID { get; set; }
		[Required]
		public int BinID { get; set; }
		[Required]
		public int TicketNumber { get; set; }
		[Required]
		public int Pounds { get; set; }
		public string Note { get; set; }
		public int? DisbursementType { get; set; }
		[Required]
		public DateTime DisbursementDate { get; set; }
		public int? FeedID { get; set; }
		[Required]
		public DateTime CreatedDate { get; set; }
		[Required]
		public int UserID { get; set; }
	}
}