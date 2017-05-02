using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Security.Cryptography;
using System.Text;
using SGApp.Utility;
using SGApp.Models.EF;
using SGApp.DTOs;

namespace SGApp.Repository.Application
{
    public class AD_WeekDataRepository : RepositoryBase<AD_WeekData>
    {
        public override System.Linq.IQueryable<AD_WeekData> EntityCollection
        {
            get
            {
                return DbContext.AD_WeekData.AsQueryable();
            }
        }

        protected override AD_WeekData DeleteRecord(AD_WeekData entity)
        {
            throw new System.NotImplementedException();
        }

        protected override AD_WeekData InsertRecord(AD_WeekData entity)
        {
            DbContext.AD_WeekData.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override AD_WeekData UpdateRecord(AD_WeekData entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<AD_WeekData> GetByPredicate(string predicate)
        {
            var iq = DbContext.AD_WeekData.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<AD_WeekData> GetAD_WeekDatas()
        {

            return DbContext.AD_WeekData
            .OrderBy(x => x.AD_WeekEnd).ToList();
        }


        public override AD_WeekData GetById(int id)
        {
            return DbContext.AD_WeekData.Where(x => x.AD_WeekDataID == id).SingleOrDefault();
        }

        public List<AD_WeekData> GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(1);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.AD_WeekData.Include("AD_Products").Where(x => x.AD_WeekEnd > reportDate && x.AD_WeekEnd < endDate).ToList();
        }

        public List<AD_WeekData> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            
            startDate = startDate.AddSeconds(-1);
            endDate = endDate.AddDays(1);
            return DbContext.AD_WeekData.Where(x => x.AD_WeekEnd > startDate && x.AD_WeekEnd < endDate).GroupBy(x => x.AD_WeekEnd).Select(x => x.FirstOrDefault()).ToList();
        }
        public List<AD_WeekData> GetByWeek(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(6);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.AD_WeekData.Where(x => x.AD_WeekEnd > reportDate && x.AD_WeekEnd < endDate).ToList();
        }

        public List<int> GetAllProducts()
        {
            
            return DbContext.AD_Products.Select( x => x.AD_ProductID).ToList();
        }
        public ChartDTO GetSalesStats()
        {
            var bLbs = DbContext.ADAGIOSalesStatitics.Where(x => x.Date_2.Value.Year > 2000).GroupBy(x => x.Date_2.Value.Year)
            .Select(lg => new ChartDTO.Series.dataitem
            {
                name = lg.Key.ToString(),
                y = lg.Sum(w => w.GrossSalesAmt != null ? (int) w.GrossSalesAmt.Value : 0),
                drilldown = "GYear" + lg.Key.ToString()
            }).OrderBy(o => o.name).ToArray();
            var bDollars = DbContext.ADAGIOSalesStatitics.Where(x => x.Date_2.Value.Year > 2000).GroupBy(x => x.Date_2.Value.Year)
                .Select(lg => new ChartDTO.Series.dataitem
                {
                    name = lg.Key.ToString(),
                    y = (int)lg.Sum(w => w.NetSalesAmt != null ? (int) w.NetSalesAmt.Value : 0),
                    drilldown = "NYear" + lg.Key.ToString()
                }).OrderBy(o => o.name).ToArray();
            //var aLbs = DbContext.ADAGIOSalesStatitics.Where(x => x.Date_2.Value.Year > 2000).GroupBy(x => x.Date_2.Value.Year)
            //    .Select(lg => new ChartDTO.Series.dataitem
            //    {
            //        name = lg.Key.ToString(),
            //        y = lg.Sum(w => w.NetInvoiceAmt != null ? (int) w.NetInvoiceAmt.Value : 0),
            //        drilldown = "Invoice" + lg.Key.ToString()
            //    }).OrderBy(o => o.name).ToArray();
            var aDollars = DbContext.ADAGIOSalesStatitics.Where(x => x.Date_2.Value.Year > 2000).GroupBy(x => x.Date_2.Value.Year)
                .Select(lg => new ChartDTO.Series.dataitem
                {
                    name = lg.Key.ToString(),
                    y = (int)lg.Sum(w => w.NetQtySold != null ? (int) w.NetQtySold.Value : 0),
                    drilldown = "QYear" + lg.Key.ToString()
                }).OrderBy(o => o.name).ToArray();

            var seriesData1 = new ChartDTO.Series
            {
                data = bLbs.ToArray(),
                color = "rgba(165,170,217,1)",
                name = "Gross Sales",
                pointPadding = 0.26M,
                pointPlacement = -0.2M,
                yAxis = 1


            };
            var seriesData2 = new ChartDTO.Series
            {
                data = bDollars.ToArray(),
                color = "rgba(126,86,134,.9)",
                name = "Net Sales",
                pointPadding = 0.4M,
                pointPlacement = -0.2M,
                yAxis = 1
            };
            var seriesData3 = new ChartDTO.Series
            {
                data = aDollars.ToArray(),
                color = "rgba(248,161,63,1)",
                name = "Net Quantity",
                pointPadding = 0.26M,
                pointPlacement = 0.1M,
                yAxis = 1
            };

            ChartDTO.Series[] seriesarray = new ChartDTO.Series[3];
            seriesarray[0] = seriesData1;
            seriesarray[1] = seriesData2;
            seriesarray[2] = seriesData3;
            //seriesarray[3] = seriesData4;

            var cht = new ChartDTO();
            cht.ChartSeries = seriesarray;

            List<ChartDTO.Series> drillDownSeries = new List<ChartDTO.Series>();
            foreach (int year in bLbs.Select(x => int.Parse(x.name))){
                var ddGross = DbContext.ADAGIOSalesStatitics.Where(x => x.Date_2.Value.Year == year).GroupBy(x => x.Date_2.Value.Month)
            .Select(lg => new ChartDTO.Series.dataitem
            {
                name = lg.Key.ToString(),
                y = lg.Sum(w => w.GrossSalesAmt != null ? (int)w.GrossSalesAmt.Value : 0),
                drilldown = "Month" + lg.Key.ToString()
            }).OrderBy(o => o.name).ToArray();
                var ddNet = DbContext.ADAGIOSalesStatitics.Where(x => x.Date_2.Value.Year == year).GroupBy(x => x.Date_2.Value.Month)
                    .Select(lg => new ChartDTO.Series.dataitem
                    {
                        name = lg.Key.ToString(),
                        y = (int)lg.Sum(w => w.NetSalesAmt != null ? (int)w.NetSalesAmt.Value : 0),
                        drilldown = "Month" + lg.Key.ToString()
                    }).OrderBy(o => o.name).ToArray();
                var ddQty = DbContext.ADAGIOSalesStatitics.Where(x => x.Date_2.Value.Year == year).GroupBy(x => x.Date_2.Value.Month)
                    .Select(lg => new ChartDTO.Series.dataitem
                    {
                        name = lg.Key.ToString(),
                        y = (int)lg.Sum(w => w.NetQtySold != null ? (int)w.NetQtySold.Value : 0),
                        drilldown = "Month" + lg.Key.ToString()
                    }).OrderBy(o => o.name).ToArray();

                var ddseriesData1 = new ChartDTO.Series
                {
                    data = ddGross.ToArray(),
                    color = "rgba(165,170,217,1)",
                    name = "Gross Sales",
                    pointPadding = 0.26M,
                    pointPlacement = -0.2M,
                    yAxis = 1,
                    id = "GYear" + year.ToString()


                };
                var ddseriesData2 = new ChartDTO.Series
                {
                    data = ddNet.ToArray(),
                    color = "rgba(126,86,134,.9)",
                    name = "Net Sales",
                    pointPadding = 0.4M,
                    pointPlacement = -0.2M,
                    yAxis = 1,
                    id = "NYear" + year.ToString()
                };
                var ddseriesData3 = new ChartDTO.Series
                {
                    data = ddQty.ToArray(),
                    color = "rgba(248,161,63,1)",
                    name = "Net Quantity",
                    pointPadding = 0.26M,
                    pointPlacement = 0.1M,
                    yAxis = 1,
                    id = "QYear" + year.ToString()
                };

                drillDownSeries.Add(ddseriesData1);
                drillDownSeries.Add(ddseriesData2);
                drillDownSeries.Add(ddseriesData3);
            }

            ChartDTO.DrillDown ddData = new ChartDTO.DrillDown();
            ddData.DrillDownSeries = drillDownSeries.ToArray();
            cht.DrillDownData = ddData;
            return cht;
        }


        public ChartDTO GetYTDGroup()
        {

            var bLbs = DbContext.AD_WeekData.GroupBy(x => x.AD_WeekEnd)
            .Select(lg => new ChartDTO.Series.dataitem
            {
                name = lg.Key.ToString().Replace(" 12:00AM", ""),
                y = lg.Sum(w => w.AD_BudgetLbs != null ? w.AD_BudgetLbs.Value : 0),
                drilldown = "BudgetLbs" + lg.Key.ToString().Replace(" 12:00AM", "")
            }).ToArray();
            var bDollars = DbContext.AD_WeekData.GroupBy(x => x.AD_WeekEnd)
                .Select(lg => new ChartDTO.Series.dataitem
            {
                name = lg.Key.ToString().Replace(" 12:00AM", ""),
                y = (int)lg.Sum(w => w.AD_BudgetDollars != null ? w.AD_BudgetDollars.Value : 0),
                drilldown = "BudgetDollars" + lg.Key.ToString().Replace(" 12:00AM", "")
            }).ToArray();
            var aLbs = DbContext.AD_WeekData.GroupBy(x => x.AD_WeekEnd)
                .Select(lg => new ChartDTO.Series.dataitem
            {
                name = lg.Key.ToString().Replace(" 12:00AM", ""),
                y = lg.Sum(w => w.AD_ActualLbs != null ? w.AD_ActualLbs.Value : 0),
                drilldown = "ActualLbs" + lg.Key.ToString().Replace(" 12:00AM", "")
            }).ToArray();
            var aDollars = DbContext.AD_WeekData.GroupBy(x => x.AD_WeekEnd)
                .Select(lg => new ChartDTO.Series.dataitem
            {
                name = lg.Key.ToString().Replace(" 12:00AM", ""),
                y = (int)lg.Sum(w => w.AD_ActualDollars != null ? w.AD_ActualDollars.Value : 0),
                drilldown = "ActualDollars" + lg.Key.ToString().Replace(" 12:00AM", "")
            }).ToArray();

            var seriesData1 = new ChartDTO.Series
            {
                data = bLbs.ToArray(),
                color = "rgba(165,170,217,1)",
                name = "Budget Lbs",
                pointPadding = 0.26M,
                pointPlacement = -0.2M,
                yAxis = 1
                

            };
            var seriesData2 = new ChartDTO.Series
            {
                data = bDollars.ToArray(),
                color = "rgba(126,86,134,.9)",
                name = "Actual Lbs",
                pointPadding = 0.4M,
                pointPlacement = -0.2M,
                yAxis = 1
            };
            var seriesData3 = new ChartDTO.Series
            {
                data = aLbs.ToArray(),
                color = "rgba(248,161,63,1)",
                name = "Budget Dollars",
                pointPadding = 0.26M,
                pointPlacement = 0.1M,
                yAxis = 1
            };
            var seriesData4 = new ChartDTO.Series
            {
                data = aDollars.ToArray(),
                color = "rgba(186,60,61,.9)",
                name = "Actual Dollars",
                pointPadding = 0.4M,
                pointPlacement = 0.1M,
                yAxis = 1
            };

            ChartDTO.Series[] seriesarray = new ChartDTO.Series[4];
            seriesarray[0] = seriesData1;
            seriesarray[1] = seriesData2;
            seriesarray[2] = seriesData3;
            seriesarray[3] = seriesData4;

            var cht = new ChartDTO();
            cht.ChartSeries = seriesarray;



            //var allDates = DbContext.AD_WeekData.GroupBy(x => x.AD_WeekEnd)
            //.Select(lg => lg.FirstOrDefault().AD_WeekEnd);
            //ChartDTO.DrillDown thisdd = new ChartDTO.DrillDown();
            
            //foreach (var date in allDates)
            //{
            //    ChartDTO.DrillDown.SeriesItem thisseriesitem = new ChartDTO.DrillDown.SeriesItem();
            //    thisseriesitem.id = "BudgetLbs" + date.ToString();
            //    var prods = DbContext.AD_WeekData.Where(x => x.AD_WeekEnd == date).ToList();


            //}
          
            return cht;
            
               
        }
        

    }


}
