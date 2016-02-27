using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class DepartmentTotalDTO : IKey
    {
        public string Key { get; set; }
        public string DepartmentTotalID { get; set; }
        public string DTDate { get; set; }
        public string Start_DTDate { get; set; }
        public string End_DTDate { get; set; }
        public string FinishTime { get; set; }
        public string DownTime { get; set; }
        public string ShiftWeight { get; set; }
        public string Absences { get; set; }
        public string StartDateMonth { get; set; }
        public string StartDateYear { get; set; }
        public string FilletScaleReading { get; set; }
    }
}