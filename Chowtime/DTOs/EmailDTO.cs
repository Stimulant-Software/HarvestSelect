using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class EmailDTO : IKey
    {
        public string Key { get; set; }
        public string EmailID { get; set; }
        public string EmailAddress { get; set; }
        public string ReceiveDailyReport { get; set; }

    }
}