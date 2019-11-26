using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGApp.DTOs
{
    public class dtoAtAGlance
    {
        public string[] PondsforWeights { get; set; }
        public decimal?[] Weights { get; set; }
        public List<dtoNameAndDoubleArray> PondSamplings { get; set; }
        public string[] PondsForSamplings { get; set; }
        public double? WTDSaleLbs { get; set; }
        public double? WTDSaleDollars { get; set; }
        public double? WTDASP { get; set; }
        public double? WTDOrderCount { get; set; }
        public decimal? TodayProductionLbs { get; set; }
        public decimal? WTDProductionLbs { get; set; }
        public decimal? TodayFreshLbs { get; set; }
        public decimal? WTDFreshLbs { get; set; }
        public decimal? TodayIQFLbs { get; set; }
        public decimal? WTDIQFLbs { get; set; }

        public string[] WeightCategories { get; set; }
        public double?[] TodaySamplingTotals { get; set; }
        public double?[] WTDSamplingTotals { get; set; }
        public dtoNameAndDoubleArray Samplings { get; set; }

    }

    public class dtoNameAndDoubleArray
    {
        public string name { get; set; }
        public double?[] data { get; set; }
    }
}