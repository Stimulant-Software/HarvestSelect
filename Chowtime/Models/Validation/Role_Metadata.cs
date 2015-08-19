using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class Role_Metadata
    {
        [Required]
        [Key]
        public int RoleId
        {
            get;
            set;
        }


        [StringLength(50, ErrorMessage = Constants.Max50)]
        public int RoleName
        {
            get;
            set;
        }




    }
}