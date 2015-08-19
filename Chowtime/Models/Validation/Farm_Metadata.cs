using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class Farm_Metadata
    {
        [Required]
        [Key]
        public int FarmId
        {
            get;
            set;
        }
        [Required]
        public int CompanyId
        {
            get;
            set;
        }

        public int StatusId
        {
            get;
            set;
        }

        [StringLength(50, ErrorMessage = Constants.Max50)]
        public int FarmName
        {
            get;
            set;
        }




    }
}