using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class DownTime_Metadata
    {
        [Required]
        [Key]
        public int DownTimeID { get; set; }
        public int DownTimeTypeID { get; set; }
        public int Minutes { get; set; }
        public string DownTimeNote { get; set; }
        public System.DateTime DownTimeDate { get; set; }



    }
}