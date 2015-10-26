using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class FarmYieldHeaderDTO : IKey
    {
        public string Key { get; set; }
        public string FarmYieldHeaderID { get; set; }
        public string YieldDate { get; set; }
        public string Start_YieldDate { get; set; }
        public string End_YieldDate { get; set; }
        public string PlantWeight { get; set; }
        public string WeighBacks { get; set; }

    }
}