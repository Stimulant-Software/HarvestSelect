using SGApp.Models.Common;
using System;

namespace SGApp.DTOs
{
    public class O2ReadingDTO : IKey
    {

        public string Key { get; set; }





        public string ReadingId
        {
            get;
            set;
        }
        public string PondId
        {
            get;
            set;
        }



        public string ReadingDate
        {
            get;
            set;
        }

        public string O2Level
        {
            get;
            set;
        }
        public string PortableCount { get; set; }
        public string StaticCount { get; set; }

        public string UsersFarmId
        {
            get;
            set;
        }

        public string Note
        {
            get;
            set;
        }
        public string DayPeriod
        {
            get;
            set;
        }





    }
}