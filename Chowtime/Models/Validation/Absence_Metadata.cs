using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class Absence_Metadata
    {
        [Required]
        [Key]
        public int AbsenceID { get; set; }
        public System.DateTime AbsenceDate { get; set; }
        public int DepartmentID { get; set; }
        public int RegEmpLate { get; set; }
        public int RegEmpOut { get; set; }
        public int RegEmpLeftEarly { get; set; }
        public int TempEmpOut { get; set; }
        public int TempEmpLate { get; set; }
        public int TempEmpLeftEarly { get; set; }
        public int InmateLeftEarly { get; set; }
        public int InmateOut { get; set; }
        public int EmployeesOnVacation { get; set; }



    }
}