using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class Pond_Metadata
    {
        [Required]
        [Key]
        public int PondId
        {
            get;
            set;
        }
        [Required]
        public int FarmId
        {
            get;
            set;
        }

        public int StatusId
        {
            get;
            set;
        }

        public int HealthStatus
        {
            get;
            set;
        }

        public decimal Size
        {
            get;
            set;
        }

        public bool NoFeed
        {
            get;
            set;
        }

        public string  InnovaName
        {
            get;
            set;
        }

        public string InnovaCode
        {
            get;
            set;
        }

        [StringLength(50, ErrorMessage = Constants.Max50)]
        public int PondName
        {
            get;
            set;
        }




    }
}