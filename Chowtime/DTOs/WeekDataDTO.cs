using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class WeekDataDTO : IKey
    {
        public string Key { get; set; }
        public string AD_WeekDataID { get; set; }
        public string AD_ProductID { get; set; }
        public string AD_BudgetLbs { get; set; }
        public string AD_ActualLbs { get; set; }
        public string AD_BudgetDollars { get; set; }
        public string AD_ActualDollars { get; set; }
        public string AD_WeekEnd { get; set; }
        public string Start_WeekDataDate { get; set; }
        public string End_WeekDataDate { get; set; }
        public string StartDateMonth { get; set; }
        public string StartDateYear { get; set; }
    }
}

