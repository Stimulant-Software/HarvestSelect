using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class ShiftEndDTO : IKey
    {
        public string Key { get; set; }
        public string ShiftEndID { get; set; }
        public string ShiftDate { get; set; }
        public string Start_ShiftDate { get; set; }
        public string End_ShiftDate { get; set; }
        public string RegEmpLate { get; set; }
        public string RegEmpOut { get; set; }
        public string RegEmplLeftEarly { get; set; }
        public string TempEmpOut { get; set; }
        public string InmateLeftEarly { get; set; }
        public string FinishedKill { get; set; }
        public string FinishedFillet { get; set; }
        public string FinishedSkinning { get; set; }
        public string DayFinishedFreezing { get; set; }
        public string NightFinishedFreezing { get; set; }
        public string DayShiftFroze { get; set; }
        public string NightShiftFroze { get; set; }
        public string FilletScaleReading { get; set; }
        public string DowntimeMinutes { get; set; }
        public string InLateOut { get; set; }
        public string EmployeesOnVacation { get; set; }
    }
}