using System.ComponentModel.DataAnnotations;
using SGApp.Utility;
using System;

namespace SGApp.Models.Validation
{
    class Harvest_Metadata
    {
        [Required]
        [Key]
        public int HarvestId
        {
            get;
            set;
        }
        [Required]
        public int PondId
        {
            get;
            set;
        }

        [Required]
        public DateTime HarvestDate
        {
            get;
            set;
        }




    }
}