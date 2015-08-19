using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class Status_Metadata
    {
        [Required]
        [Key]
        public int StatusId
        {
            get;
            set;
        }


        [StringLength(50, ErrorMessage = Constants.Max50)]
        public int StatusName
        {
            get;
            set;
        }




    }
}