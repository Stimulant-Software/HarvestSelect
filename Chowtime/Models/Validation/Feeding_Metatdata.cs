using System.ComponentModel.DataAnnotations;
using SGApp.Utility;
using System;

namespace SGApp.Models.Validation
{
    class Feeding_Metadata
    {
        [Required]
        [Key]
        public int FeedingId
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
        public int PoundsFed
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
        public DateTime FeedDate
        {
            get;
            set;
        }




    }
}