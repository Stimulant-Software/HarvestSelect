using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGApp.DTOs
{
    public class BOLs
    {
        public List<BOLs> bols { get; set; }

    }
    public class BOL
    {
        //public string code {get; set;}
        public string Station { get; set; }
        public string Nominal { get; set; }
        public string Weight { get; set; }
        public string OrderCode { get; set; }
                        public string CustNumber { get; set; }
                        public string CustShort { get; set; }
                        public string CustLong { get; set; }
                        public string OrderDate { get; set; }
                        public string MaterialID { get; set; }
                        public string PM_Name43 { get; set; }
                        public string OrderedAmt { get; set; }
                        public string UnitCount { get; set; }
                        public string NewShippedAmt { get; set; }
                        public string ExtendedTotalWt { get; set; }
                        public string HowPacked { get; set; }
                        public string WeightLabel { get; set; }
    }
}