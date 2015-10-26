using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class FarmYieldDTO : IKey
    {
        public string Key { get; set; }
        public string YieldID { get; set; }
        public string YieldDate { get; set; }
        public string Start_YieldDate { get; set; }
        public string End_YieldDate { get; set; }
        public string PondID { get; set; }
        public string PoundsYielded { get; set; }
        public string Remove { get; set; }
        public string PoundsPlant { get; set; }
        public string PoundsHeaded { get; set; }
        public string PercentYield { get; set; }
        public string StartDateMonth { get; set; }
        public string StartDateYear { get; set; }

    }
}