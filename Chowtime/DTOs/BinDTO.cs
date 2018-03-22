using SGApp.Models.Common;
using System;

namespace SGApp.DTOs
{
    public class BinDTO : IKey
    {
        public string Key { get; set; }
        public string BinID { get; set; }
        public string BinName { get; set; }
        public string FarmID { get; set; }
        public string CurrentTicket { get; set; }
        public string CurrentPounds { get; set; }
        public string LastDispersement { get; set; }
        public string LastLoaded { get; set; }
        public string CompanyId { get; set; }







    }
}