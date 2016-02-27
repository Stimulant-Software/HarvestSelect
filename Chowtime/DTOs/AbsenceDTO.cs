using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class AbsenceDTO : IKey
    {
        public string Key { get; set; }
        public string AbsenceID { get; set; }
        public string AbsenceDate { get; set; }
        public string Start_AbsenceDate { get; set; }
        public string End_AbsenceDate { get; set; }
        public string DepartmentID { get; set; }
        public string RegEmpLate { get; set; }
        public string RegEmpOut { get; set; }
        public string RegEmpLeftEarly { get; set; }
        public string TempEmpOut { get; set; }
        public string TempEmpLate { get; set; }
        public string TempEmpLeftEarly { get; set; }
        public string InmateLeftEarly { get; set; }
        public string InmateOut { get; set; }
        public string EmployeesOnVacation { get; set; }
        public string StartDateMonth { get; set; }
        public string StartDateYear { get; set; }
    }
}