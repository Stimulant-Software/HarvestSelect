using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class FilletScaleReading_Metadata
    {
        [Required]
        [Key]
        public int FilletScaleReadingID { get; set; }
        public System.DateTime FSRDateTime { get; set; }
        public decimal FilletScaleReading1 { get; set; }



    }
}