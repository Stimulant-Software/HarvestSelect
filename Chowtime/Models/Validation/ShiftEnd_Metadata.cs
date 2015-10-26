using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class ShiftEnd_Metadata
    {
        [Required]
        [Key]
        public int ShiftEndID { get; set; }
        public System.DateTime ShiftDate { get; set; }
        public int RegEmpLate { get; set; }
        public int RegEmpOut { get; set; }
        public int RegEmplLeftEarly { get; set; }
        public int TempEmpOut { get; set; }
        public int InmateLeftEarly { get; set; }
        public System.DateTime FinishedKill { get; set; }
        public System.DateTime FinishedFillet { get; set; }
        public System.DateTime FinishedSkinning { get; set; }
        public System.DateTime DayFinishedFreezing { get; set; }
        public System.DateTime NightFinishedFreezing { get; set; }
        public decimal DayShiftFroze { get; set; }
        public decimal NightShiftFroze { get; set; }
        public decimal FilletScaleReading { get; set; }
        public int DowntimeMinutes { get; set; }



    }
}
