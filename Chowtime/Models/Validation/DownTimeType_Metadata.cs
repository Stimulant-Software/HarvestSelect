using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class DownTimeType_Metadata
    {
        [Required]
        [Key]
        public int DownTimeTypeID { get; set; }
        public string DownTimeName { get; set; }
        public int DepartmentID { get; set; }



    }
}