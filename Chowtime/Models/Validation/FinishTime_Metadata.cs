using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class FinishTime_Metadata
    {
        [Required]
        [Key]
        public int FinishTimeID { get; set; }
        public System.DateTime FinishDateTime { get; set; }
        public int DepartmentID { get; set; }



    }
}