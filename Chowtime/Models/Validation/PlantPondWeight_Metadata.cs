using System.ComponentModel.DataAnnotations;
using SGApp.Utility;

namespace SGApp.Models.Validation
{
    class PlantPondWeight_Metadata
    {
        [Required]
        [Key]
        public int PlantPondWeightID { get; set; }
        public int PondID { get; set; }
        public System.DateTime PPWDateTime { get; set; }
        public decimal PlantWeight { get; set; }
        public decimal PondWeight { get; set; }


    }
}
