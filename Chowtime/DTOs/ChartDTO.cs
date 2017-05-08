using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class ChartDTO
    {
        public class Series
        {
            public dataitem[] data { get; set; }
            public string name { get; set; }
            public string color { get; set; }
            public class dataitem
            {
                public int y { get; set; }
                public string name { get; set; }
                public string drilldown { get; set; }
            }
            public decimal pointPadding { get; set; }
            public decimal pointPlacement { get; set; }
            public decimal? yAxis { get; set; }
            public string id { get; set; }
            public string type { get; set; }

        }

       
        
        public Series[] ChartSeries { get; set; }
        


        public class DrillDown {

            public class SeriesItem
            {
                public string id  { get; set; }

                public dataitem[] data { get; set; }
                public class dataitem
                {
                    public ICollection<Dictionary<string, decimal>> ddDataItems { get; set; }

                }
            }
            public List<Series[]> ddCollection { get; set; }
            public ICollection<Dictionary<string, List<ChartDTO.Series>>> DrillDownCollection { get; set; }
           
            public SeriesItem[] Series { get; set; }
            
            public Series[] DrillDownSeries { get; set; }
            

        }

        public DrillDown DrillDownData { get; set; }

        public class SeriesDec
        {
            public dataitem[] data { get; set; }
            public string name { get; set; }
            public string color { get; set; }
            public class dataitem
            {
                public decimal y { get; set; }
                public string name { get; set; }
                public string drilldown { get; set; }
            }
            public decimal pointPadding { get; set; }
            public decimal pointPlacement { get; set; }
            public decimal? yAxis { get; set; }
            public string id { get; set; }
            public string type { get; set; }

        }
        public SeriesDec[] ChartSeriesDec { get; set; }

        public class SeriesPie
        {
            public dataitem[] data { get; set; }
            public string name { get; set; }
            public string color { get; set; }
            public string innerSize { get; set; }
            public class dataitem
            {
                public decimal y { get; set; }
                public string name { get; set; }
            }
            public decimal pointPadding { get; set; }
            public decimal pointPlacement { get; set; }
            public decimal? yAxis { get; set; }
            public string id { get; set; }
            public string type { get; set; }

        }



        public SeriesPie[] ChartSeriesPie { get; set; }

    }
}