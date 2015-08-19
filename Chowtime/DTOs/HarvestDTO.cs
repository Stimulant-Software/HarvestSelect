using SGApp.Models.Common;
using System;

namespace SGApp.DTOs
{
    public class HarvestDTO : IKey
    {

        public string Key { get; set; }



        public string PondId
        {
            get;
            set;
        }



        public string HarvestDate
        {
            get;
            set;
        }


        public string HarvestId
        {
            get;
            set;
        }







    }
}