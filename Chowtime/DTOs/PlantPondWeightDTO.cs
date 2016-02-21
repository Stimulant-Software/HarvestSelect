using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class PlantPondWeightDTO : IKey
    {
        public string Key { get; set; }
        public string PlantPondWeightID { get; set; }
        public string PondID { get; set; }
        public string PPWDateTime { get; set; }
        public string PlantWeight { get; set; }
        public string PondWeight { get; set; }
        public string TicketNumber { get; set; }
        public string Start_PPWDateTime { get; set; }
        public string End_PPWDateTime { get; set; }
        public string StartDateMonth { get; set; }
        public string StartDateYear { get; set; }

    }
}