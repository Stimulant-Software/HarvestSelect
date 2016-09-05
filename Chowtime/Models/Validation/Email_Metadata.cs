using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class Email_Metadata
    {
        [Required]
        [Key]
        public int EmailID
        {
            get;
            set;
        }


        [StringLength(100, ErrorMessage = Constants.Max100)]
        public int EmailAddress
        {
            get;
            set;
        }
        public bool ReceiveDailyReport { get; set; }



    }
}