using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class FarmYield_Metadata
    {
        [Required]
        [Key]
        public int YieldID { get; set; }
        public System.DateTime YieldDate { get; set; }
        public int PondID { get; set; }
        public decimal PoundsYielded { get; set; }
        public decimal PoundsPlant { get; set; }
        public decimal PoundsHeaded { get; set; }
        public decimal PercentYield { get; set; }
        public decimal PercentYield2 { get; set; }
        public System.DateTime Start_YieldDate { get; set; }
        public System.DateTime End_YieldDate { get; set; }



    }
}
