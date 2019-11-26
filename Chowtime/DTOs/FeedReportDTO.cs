using SGApp.Models.Common;
using System.Collections.Generic;


namespace SGApp.DTOs
{
    public class FeedReportDTO : IKey
    {
        public string Key { get; set; }
        public int FarmId { get; set; }
        public int Year { get; set; }
        

        public List<MonthPondTotalDTO> MonthPondTotals { get; set; }
    }

    public class MonthPondTotalDTO
    {
        public int? PoundsFed { get; set; }
        public int? Month { get; set; }
        public string Pond { get; set; }
        public int? Year { get; set; }
        public string MonthName { get; set; }
    }
}