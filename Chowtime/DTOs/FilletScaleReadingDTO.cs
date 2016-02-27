using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class FilletScaleReadingDTO : IKey
    {
        public string Key { get; set; }
        public string FilletScaleReadingID { get; set; }
        public string FSRDateTime { get; set; }
        public string Start_FSRDateTime { get; set; }
        public string End_FSRDateTime { get; set; }
        public string FilletScaleReading { get; set; }
        public string StartDateMonth { get; set; }
        public string StartDateYear { get; set; }

    }
}