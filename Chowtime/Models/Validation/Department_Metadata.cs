using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class Department_Metadata
    {
        [Required]
        [Key]
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }



    }
}