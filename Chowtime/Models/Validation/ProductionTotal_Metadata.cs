using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class ProductionTotal_Metadata
    {
        [Required]
        [Key]
        public int ProductionTotalID { get; set; }
        public System.DateTime ProductionDate { get; set; }
        public int PondId { get; set; }
        public decimal PlantWeight { get; set; }
        public decimal PondWeight { get; set; }
        public decimal WeighBacks { get; set; }
        public decimal AverageYield { get; set; }



    }
}
