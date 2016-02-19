using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class ProductionTotalDTO : IKey
    {
        public string Key { get; set; }
        public string ProductionTotalID { get; set; }
        public string ProductionDate { get; set; }
        public string PondId { get; set; }
        public string PlantWeight { get; set; }
        public string PondWeight { get; set; }
        public string WeighBacks { get; set; }
        public string AverageYield { get; set; }
        public string Start_ProductionDate { get; set; }
        public string End_ProductionDate { get; set; }
        public string StartDateMonth { get; set; }
        public string StartDateYear { get; set; }

    }
}