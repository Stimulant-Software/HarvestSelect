using SGApp.Models.Common;
using System;

namespace SGApp.DTOs
{
    public class FeedingDTO : IKey
    {

        public string Key { get; set; }





        public string FeedingId
        {
            get;
            set;
        }
        public string PondId
        {
            get;
            set;
        }



        public string FeedDate
        {
            get;
            set;
        }

        public string PoundsFed
        {
            get;
            set;
        }

        public string UsersFarmId
        {
            get;
            set;
        }







    }
}