using System.ComponentModel.DataAnnotations;
using SGApp.Utility;
using System;

namespace SGApp.Models.Validation
{
    class O2Reading_Metadata
    {
        [Required]
        [Key]
        public int ReadingId
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
        public decimal O2Level
        {
            get;
            set;
        }
        [Required]
        public int UsersFarmId
        {
            get;
            set;
        }
        [Required]
        public DateTime ReadingDate
        {
            get;
            set;
        }

        [Required]
        public int PortableCount
        {
            get;
            set;
        }

        [Required]
        public int StaticCount
        {
            get;
            set;
        }

        [Required]
        public DateTime DayPeriod
        {
            get;
            set;
        }




    }
}