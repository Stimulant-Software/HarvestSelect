using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class FarmYieldHeader_Metadata
    {
        [Required]
        [Key]
        public int FarmYieldHeaderID { get; set; }
        public System.DateTime YieldDate { get; set; }
        public decimal PlantWeight { get; set; }
        public decimal WeighBacks { get; set; }



    }
}
