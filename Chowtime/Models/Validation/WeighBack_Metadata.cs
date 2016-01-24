using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class WeighBack_Metadata
    {
        [Required]
        [Key]
        public int WeightBackID { get; set; }
        public int PondID { get; set; }
        public System.DateTime WBDateTime { get; set; }
        public decimal Turtle { get; set; }
        public decimal Trash { get; set; }
        public decimal Shad { get; set; }
        public decimal Carp { get; set; }
        public decimal Bream { get; set; }
        public decimal LiveDisease { get; set; }
        public decimal DressedDisease { get; set; }
        public decimal DressedDiseasePct { get; set; }
        public decimal Backs { get; set; }
        public decimal RedFillet { get; set; }
        public decimal RedFilletPct { get; set; }
        public decimal BigFish { get; set; }
        public decimal DOAs { get; set; }


    }
}
