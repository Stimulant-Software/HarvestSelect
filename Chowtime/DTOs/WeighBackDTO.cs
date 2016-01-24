using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class WeighBackDTO : IKey
    {
        public string Key { get; set; }
        public string WeightBackID { get; set; }
        public string PondID { get; set; }
        public string WBDateTime{ get; set; }
        public string Start_WBDateTime { get; set; }
        public string End_WBDateTime { get; set; }
        public string Turtle { get; set; }
        public string Trash { get; set; }
        public string Shad { get; set; }
        public string Carp { get; set; }
        public string Bream { get; set; }
        public string LiveDisease { get; set; }
        public string DressedDisease { get; set; }
        public string DressedDiseasePct { get; set; }
        public string Backs { get; set; }
        public string RedFillet { get; set; }
        public string RedFilletPct { get; set; }
        public string BigFish { get; set; }
        public string DOAs { get; set; }
        public string StartDateMonth { get; set; }
        public string StartDateYear { get; set; }

    }
}