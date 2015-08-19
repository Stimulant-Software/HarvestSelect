using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class Company_Metadata
    {
        [Required]
        [Key]
        public int CompanyId
        {
            get;
            set;
        }


        [StringLength(50, ErrorMessage = Constants.Max50)]
        public int CompanyName
        {
            get;
            set;
        }




    }
}