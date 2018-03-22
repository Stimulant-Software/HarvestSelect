using System.ComponentModel.DataAnnotations;
using SGApp.Utility;
using System;

namespace SGApp.Models.Validation
{
    class Bin_Metadata
    {
        [Required]
        [Key]
        public int BinID { get; set; }
        public string BinName { get; set; }
        public int FarmID { get; set; }
        public int CurrentTicket { get; set; }
        public int CurrentPounds { get; set; }
        public DateTime LastDispersement { get; set; }
        public DateTime LastLoaded { get; set; }



    }
}