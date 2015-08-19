using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation {
    class User_Metadata {
        [Required]
        [Key]
        public int UserId {
            get;
            set;
        }
        [Required]
        public int CompanyId {
            get;
            set;
        }

        public int StatusId
        {
            get;
            set;
        }

        [StringLength(50, ErrorMessage = Constants.Max50)]
        public int FirstName {
            get;
            set;
        }
        [StringLength(50, ErrorMessage = Constants.Max50)]
        public int LastName {
            get;
            set;
        }
        [StringLength(100, ErrorMessage = Constants.Max100)]
        public string EmailAddress {
            get;
            set;
        }

        [StringLength(20, ErrorMessage = Constants.Max20)]
        public string Phone {
            get;
            set;
        }



    }
}
