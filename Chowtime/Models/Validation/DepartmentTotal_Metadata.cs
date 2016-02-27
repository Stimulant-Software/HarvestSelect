using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class DepartmentTotal_Metadata
    {
        [Required]
        [Key]
        public int DepartmentTotalID { get; set; }
        public int DepartmentID { get; set; }
        public System.DateTime DTDate { get; set; }
        public System.DateTime FinishTime { get; set; }
        public int DownTime { get; set; }
        public decimal ShiftWeight { get; set; }
        public int Absences { get; set; }



    }
}