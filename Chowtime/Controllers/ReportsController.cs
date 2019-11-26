using System.Data.Metadata.Edm;
using SGApp.BusinessLogic.Application;
using SGApp.Controllers;
using SGApp.Repository.Application;
using SGApp.Utility;
using SGApp.DTOs;
using SGApp.Models.EF;
using SGApp.Models.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace SGApp.Controllers
{
    public class ReportsController : BaseApiController
    {
        internal HttpResponseMessage DailyReport(HttpRequestMessage request, DailyReportDTO cqDTO)
            {
                string key;
                var aur = new AppUserRepository();
                var companyId = 0;
                var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
                if (userId > 0)
                {
                    var fyr = new FarmYieldRepository();
                    var ser = new ShiftEndRepository();
                    var fyhr = new FarmYieldHeaderRepository();
                    var fyh = new FarmYieldHeader();
                    //var lfsr = new LiveFishSamplingRepository();
                    //var lfs = new LiveFishSampling();


                    var reportDate = new DateTime();
                    var se = new ShiftEnd();
                    decimal? pondWeight = 0;
                    if (cqDTO.ReportDate != null)
                    {
                        reportDate = DateTime.Parse(cqDTO.ReportDate).AddDays(-1);
                    }
                    fyh = fyhr.GetByDate(reportDate);
                    pondWeight = fyr.GetPondWeightByDate(reportDate);
                    se = ser.GetByDate(reportDate);
                    string downtime = "";
                    //lfs = lfsr.GetByDate(reportDate);
                    if (se != null && se.DowntimeMinutes != null)
                    {
                        downtime = String.Format((se.DowntimeMinutes / 60).ToString(), "{0, D2}") + ":" + String.Format((se.DowntimeMinutes % 60).ToString(), "{0, D2}");
                    }
                    else
                    {
                        downtime = "";
                    }
                    var col = new Collection<Dictionary<string, string>>();
                    var dic = new Dictionary<string, string>();
                    if (fyh != null)
                    {
                        dic.Add("PlantWeight", string.IsNullOrEmpty(fyh.PlantWeight.ToString()) ? "" : (fyh.PlantWeight.ToString()));
                        dic.Add("WeighBacks", string.IsNullOrEmpty(fyh.WeighBacks.ToString()) ? "" : (fyh.WeighBacks.ToString()));
                        dic.Add("TotalPounds", string.IsNullOrEmpty(fyh.WeighBacks.ToString()) || string.IsNullOrEmpty(fyh.PlantWeight.ToString()) ? "" : (fyh.PlantWeight - fyh.WeighBacks).ToString());
                        dic.Add("PondWeight", string.IsNullOrEmpty(pondWeight.ToString()) ? "" : pondWeight.ToString());
                        dic.Add("Variance", string.IsNullOrEmpty(pondWeight.ToString()) || string.IsNullOrEmpty(fyh.PlantWeight.ToString()) ? "" : (pondWeight - fyh.PlantWeight).ToString());
                        dic.Add("DownTime", downtime);
                    }
                    col.Add(dic);
                    var data = fyr.GetByDate(reportDate);
                    var col1 = new Collection<Dictionary<string, string>>();
                    data = data.OrderBy(x => x.YieldDate).ToList();

                    foreach (var item in data)
                    {

                        var dic1 = new Dictionary<string, string>();

                        dic1.Add("YieldId", string.IsNullOrEmpty(item.YieldID.ToString()) ? "" : (item.YieldID.ToString()));

                        dic1.Add("PondID", string.IsNullOrEmpty(item.PondID.ToString()) ? "" : (item.PondID.ToString()));
                        dic1.Add("PondName", string.IsNullOrEmpty(item.Pond.PondName) ? "" : (item.Pond.PondName));
                        dic1.Add("FarmID", string.IsNullOrEmpty(item.Pond.Farm.FarmName) ? "" : (item.Pond.Farm.FarmName));
                        dic1.Add("YieldDate", string.IsNullOrEmpty(item.YieldDate.ToString()) ? "" : (item.YieldDate.ToString()));
                        dic1.Add("PoundsYielded", string.IsNullOrEmpty(item.PoundsYielded.ToString()) ? "" : (item.PoundsYielded.ToString()));
                        dic1.Add("PoundsPlant", string.IsNullOrEmpty(item.PoundsPlant.ToString()) ? "" : (item.PoundsPlant.ToString()));
                        dic1.Add("PoundsHeaded", string.IsNullOrEmpty(item.PoundsHeaded.ToString()) ? "" : (item.PoundsHeaded.ToString()));
                        dic1.Add("PercentYield", string.IsNullOrEmpty(item.PercentYield.ToString()) ? "" : (item.PercentYield.ToString())); ;
                        dic1.Add("PercentYield2", string.IsNullOrEmpty(item.PercentYield2.ToString()) ? "" : (item.PercentYield2.ToString()));
                        col1.Add(dic1);



                    }

                    var col2 = new Collection<Dictionary<string, string>>();


                    var dic2 = new Dictionary<string, string>();
                    var col3 = new Collection<Dictionary<string, string>>();


                    var dic3 = new Dictionary<string, string>();
                    var col4 = new Collection<Dictionary<string, string>>();


                    var dic4 = new Dictionary<string, string>();

                    if (se != null)
                    {
                        dic4.Add("DayFinishedFreezing", string.IsNullOrEmpty(se.DayFinishedFreezing.ToString()) ? "" : DateTime.Parse(se.DayFinishedFreezing.ToString()).ToString("HH:mm"));
                        dic2.Add("DayShiftFroze", string.IsNullOrEmpty(se.DayShiftFroze.ToString()) ? "" : (se.DayShiftFroze.ToString()));
                        dic3.Add("EmployeesOnVacation", string.IsNullOrEmpty(se.EmployeesOnVacation.ToString()) ? "" : (se.EmployeesOnVacation.ToString()));
                        dic2.Add("FilletScaleReading", string.IsNullOrEmpty(se.FilletScaleReading.ToString()) ? "" : (se.FilletScaleReading.ToString()));
                        dic4.Add("FinishedFillet", string.IsNullOrEmpty(se.FinishedFillet.ToString()) ? "" : DateTime.Parse(se.FinishedFillet.ToString()).ToString("HH:mm"));
                        dic4.Add("FinishedKill", string.IsNullOrEmpty(se.FinishedKill.ToString()) ? "" : DateTime.Parse(se.FinishedKill.ToString()).ToString("HH:mm"));
                        dic4.Add("FinishedSkinning", string.IsNullOrEmpty(se.FinishedSkinning.ToString()) ? "" : DateTime.Parse(se.FinishedSkinning.ToString()).ToString("HH:mm"));
                        dic3.Add("InmateLeftEarly", string.IsNullOrEmpty(se.InmateLeftEarly.ToString()) ? "" : (se.InmateLeftEarly.ToString()));
                        dic3.Add("InLateOut", string.IsNullOrEmpty(se.InLateOut.ToString()) ? "" : (se.InLateOut.ToString()));
                        dic4.Add("NightFinishedFreezing", string.IsNullOrEmpty(se.NightFinishedFreezing.ToString()) ? "" : DateTime.Parse(se.NightFinishedFreezing.ToString()).ToString("HH:mm"));
                        dic2.Add("NightShiftFroze", string.IsNullOrEmpty(se.NightShiftFroze.ToString()) ? "" : (se.NightShiftFroze.ToString()));
                        dic3.Add("RegEmpLate", string.IsNullOrEmpty(se.RegEmpLate.ToString()) ? "" : (se.RegEmpLate.ToString()));
                        dic3.Add("RegEmpOut", string.IsNullOrEmpty(se.RegEmpOut.ToString()) ? "" : (se.RegEmpOut.ToString()));
                        dic3.Add("RegEmplLeftEarly", string.IsNullOrEmpty(se.RegEmplLeftEarly.ToString()) ? "" : (se.RegEmplLeftEarly.ToString()));
                        dic3.Add("TempEmpOut", string.IsNullOrEmpty(se.TempEmpOut.ToString()) ? "" : (se.TempEmpOut.ToString()));
                    }
                    col2.Add(dic2);
                    col3.Add(dic3);
                    col4.Add(dic4);


                    //var col5 = new Collection<Dictionary<string, string>>();


                    //var dic5 = new Dictionary<string, string>();
                    //if (lfs != null)
                    //{
                    //    dic5.Add("Pct0_125", string.IsNullOrEmpty(lfs.Pct0_125.ToString()) ? "" : (lfs.Pct0_125.ToString()));
                    //    dic5.Add("Avg0_125", string.IsNullOrEmpty(lfs.Avg0_125.ToString()) ? "" : (lfs.Avg0_125.ToString()));
                    //    dic5.Add("Pct125_225", string.IsNullOrEmpty(lfs.Pct125_225.ToString()) ? "" : (lfs.Pct125_225.ToString()));
                    //    dic5.Add("Avg125_225", string.IsNullOrEmpty(lfs.Avg125_225.ToString()) ? "" : (lfs.Avg125_225.ToString()));
                    //    dic5.Add("Pct225_3", string.IsNullOrEmpty(lfs.Pct225_3.ToString()) ? "" : (lfs.Pct225_3.ToString()));
                    //    dic5.Add("Avg225_3", string.IsNullOrEmpty(lfs.Avg225_3.ToString()) ? "" : (lfs.Avg225_3.ToString()));
                    //    dic5.Add("Pct3_5", string.IsNullOrEmpty(lfs.Pct3_5.ToString()) ? "" : (lfs.Pct3_5.ToString()));
                    //    dic5.Add("Avg3_5", string.IsNullOrEmpty(lfs.Avg3_5.ToString()) ? "" : (lfs.Avg3_5.ToString()));
                    //    dic5.Add("Pct5_Up", string.IsNullOrEmpty(lfs.Pct5_Up.ToString()) ? "" : (lfs.Pct5_Up.ToString()));
                    //    dic5.Add("Avg5_Up", string.IsNullOrEmpty(lfs.Avg5_Up.ToString()) ? "" : (lfs.Avg5_Up.ToString()));
                    //}
                    //col5.Add(dic5);



                    var retVal = new DailyReportDTO
                    {
                        Key = key,
                        Header = col,
                        Ponds = col1,
                        Employees = col3,
                        Finish = col4,
                        Freezing = col2,
                        //Samplings = col5
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, retVal);
                }
                var message = "validation failed";
                return request.CreateResponse(HttpStatusCode.NotFound, message);

            }
        [HttpPost]
        public HttpResponseMessage DailyReport([FromBody] DailyReportDTO cqDTO)
        {
            return DailyReport(Request, cqDTO);
        }

        internal HttpResponseMessage GetShippingReportFrozen(HttpRequestMessage request, DailyReportDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var uc = new UtilitiesController();
                //uc.UpdateCurrentShipping();

                var db = new AppEntities();
                var shipTotals = db.CurrentShippings.Select(x => new
                {
                    CustomerName = x.CustomerName,
                    ItemCode = x.ItemCode,
                    ItemDescription = x.ItemDescription,
                    OrderAmount = x.OrderAmount,
                    OrderDate = x.OrderDate,
                    QuantityOnHand = x.QuantityOnHand,
                    ShippedAmount = x.ShippedAmount,
                    TodayUnits = x.TodayUnits

                }).ToList().Select(x => new CurrentShippingDTO
                {
                    CustomerName = x.CustomerName,
                    ItemCode = x.ItemCode,
                    ItemDescription = x.ItemDescription,
                    OrderAmount = x.OrderAmount,
                    OrderDate = x.OrderDate.Value.ToShortDateString(),
                    QuantityOnHand = x.QuantityOnHand,
                    ShippedAmount = x.ShippedAmount,
                    TodayUnits = x.TodayUnits

                }).ToList();

                var resultList = new List<Dictionary<string, string>>();
                var itemlisting = shipTotals.Select(x => x.ItemCode).Distinct();
                foreach (var itemrow in itemlisting)
                {
                    var headerRow = new Dictionary<string, string>();
                    var itemOrders = shipTotals.Where(x => x.ItemCode == itemrow);
                    

                }

                var items = shipTotals.GroupBy(g => new { g.ItemCode, g.ItemDescription })
                    .Select(x => new
                    {
                        ItemCode = x.Key.ItemCode,
                        ItemDescription = x.Key.ItemDescription
                    }).ToList();

                foreach(var item in items)
                {
                    var cOrders = shipTotals.Where(x => x.ItemCode == item.ItemCode).GroupBy(g => new { g.CustomerName, g.OrderDate })
                        .Select(x => new
                        {
                            Customer = x.Key.CustomerName,
                            OrderDate = x.Key.OrderDate,
                            CustomerDayTotals = x.Sum(y => y.OrderAmount).Value
                        });
                }



                var retVal = new ShippingReportDTO
                {
                    Key = key,
                    ShippingTotals = shipTotals
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return request.CreateResponse(HttpStatusCode.NotFound, message);

        }
        [HttpPost]
        public HttpResponseMessage GetShippingReportFrozen([FromBody] DailyReportDTO cqDTO)
        {
            return GetShippingReportFrozen(Request, cqDTO);
        }

        internal HttpResponseMessage GetAtAGlance(HttpRequestMessage request, DailyReportDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var uc = new UtilitiesController();
                //uc.UpdateCurrentShipping();  //Update all the adagio and innova records if stale

                DayOfWeek day = DateTime.Now.DayOfWeek;
                int days = day - DayOfWeek.Monday;
                DateTime start = DateTime.Now.AddDays(-days).Date;
                DateTime end = start.AddDays(6);

                var db = new AppEntities();
                //var wtdHeaders = db.AdagioOrderHeaders.Where(x => x.ExpectedShipDate >= start && x.ExpectedShipDate <= end);
                //var wtdDollars = wtdHeaders.Sum(x => x.TotalOrderValue);
                //var wtdOrderCount = db.AdagioOrderHeaders.Where(x => x.ExpectedShipDate >= start && x.ExpectedShipDate <= end).Count();
                //var todayHeaders = db.AdagioTodayOrders;
                //var todayDollars = todayHeaders.Sum(x => x.TotalOrderValue);
                //var todayOrderCount = db.AdagioTodayOrders.Count();

                //var ddocs = wtdHeaders.Select(x => x.Doc).Distinct().ToArray();

                //var wtdDetails = db.AdagioOrderDetails.Where(x => ddocs.Contains(x.DDoc));
                //var wtdLbs = wtdDetails.Sum(x => x.ExtWeight);
                //var wtdTotalPrice = wtdDetails.Sum(x => x.ExtPrice);

                //var todayddocs = todayHeaders.Select(x => x.OrderKey).Distinct().ToArray();
                var todayDetails = db.AdagioOrderDetailsForTodays.Where(x => !(x.Description.Contains("hush")) && !(x.Description.Contains("head")));
                var todayLbs = todayDetails.Sum(x => x.ExtWeight);
                var todayTotalPrice = todayDetails.Sum(x => x.ExtPrice);
                var todayASP = todayTotalPrice / todayLbs;
                //var wtdASP = (wtdTotalPrice + todayTotalPrice) / (wtdLbs + todayLbs);
                //var realwtdTotalPrice = (wtdTotalPrice + todayTotalPrice);
                //var wtdordercounttotal = wtdOrderCount + todayOrderCount;


                //var todayProductionTotals = db.TodaysProductionTotals.Where(x => x.Station != null).Sum(x => x.Weight);
                int[] prodstations = new int []{ 8, 2, 4, 3, 866, 9, 10, 1024, 1025, 7 };
                var stationList = prodstations.ToList();
                int[] freshstations = new int[] { 8, 2, 4, 3, 866};
                var frershstationList = freshstations.ToList();
                int[] iqfstations = new int[] {9, 10, 1024, 1025, 7 };
                var iqfstationList = iqfstations.ToList();
                var todayProductionTotals = db.TodaysProductionTotals.Where(x => x.Station.HasValue && prodstations.Contains((Int32)x.Station));
                var wtdprodrecords = db.DepartmentTotals.Where(x => x.DTDate >= start && x.DTDate <= end && (x.DepartmentID == 4 || x.DepartmentID == 5 || x.DepartmentID == 6));
                var wtdprodlbs = todayProductionTotals.Sum(x => x.Weight) + wtdprodrecords.Sum(x => x.ShiftWeight);
                var todayprodlbs = todayProductionTotals.Sum(x => x.Weight);
                var todayiqf = todayProductionTotals.Where(x => x.Station.HasValue && iqfstations.Contains((Int32)x.Station)).Sum(x => x.Weight);
                var wtdiqf = wtdprodrecords.Where(x => (x.DepartmentID == 5 || x.DepartmentID == 6)).Sum(x => x.ShiftWeight) + todayProductionTotals.Where(x => x.Station.HasValue && iqfstations.Contains((Int32)x.Station)).Sum(x => x.Weight);
                var todayfresh = todayProductionTotals.Where(x => x.Station.HasValue && freshstations.Contains((Int32)x.Station)).Sum(x => x.Weight);
                var wtdfresh = wtdprodrecords.Where(x => x.DepartmentID == 4).Sum(x => x.ShiftWeight) + todayProductionTotals.Where(x => x.Station.HasValue && freshstations.Contains((Int32)x.Station)).Sum(x => x.Weight);

                var thisday = DateTime.Now.Date;
                var prodponds = db.PlantPondWeights.Include("Pond").Where(x => x.PPWDateTime >= thisday && x.PPWDateTime <= end).
                    GroupBy(g => g.Pond.InnovaName).Select(x => new
                    {
                        PondName = x.Key,
                        Weight = x.Sum(y => y.PondWeight)
                    }).OrderBy(x => x.PondName);
                List<string> prodpondlist = new List<string>();
                List<decimal?> pondweightlist = new List<decimal?>();
                foreach (var ppw in prodponds)
                {
                    prodpondlist.Add(ppw.PondName);
                    pondweightlist.Add(ppw.Weight);
                }

                var sampponds = db.TodaySamplings.
                GroupBy(g => new { g.pondname, g.code2 }).Select(x => new
                {
                    PondName = x.Key.pondname,
                    Code = x.Key.code2,
                    Weight = x.Sum(y => y.weight)
                }).OrderBy(x => x.PondName).ThenBy(x => x.Code);
                var wtdsamps = db.Samplings.Where(x => x.regtime >= start && x.regtime <= end).GroupBy(x => x.code2).Select(x => new
                {
                    Code = x.Key,
                    Weight = x.Sum(y => y.weight)
                });
                List<string> sampondlist = sampponds.Select(x => x.PondName).Distinct().ToList();
                List<dtoNameAndDoubleArray> pondsamplings = new List<dtoNameAndDoubleArray>();
                List<double?> todaysamplingpcts = new List<double?>();
                List<double?> wtdsamplingpcts = new List<double?>();
                double? totalSamplingWeightToday = sampponds.Sum(x => x.Weight);
                double? totalSamplingWeightWTD = wtdsamps.Sum(x => x.Weight);
                foreach (var code in sampponds.Select(x => x.Code).Distinct())
                {
                    List<double?> datarray = new List<double?>();
                    var seriesitem = new dtoNameAndDoubleArray()
                    {
                        name = code
                    };
                    foreach (var sp in sampondlist)
                    {
                        double? totalpondweight = sampponds.Where(x => x.PondName == sp).Any() ? sampponds.Where(x => x.PondName == sp).Sum(x => x.Weight) : 1;
                        double ? dataitem = sampponds.Where(x => x.PondName == sp && x.Code == code).Any() ? sampponds.Where(x => x.PondName == sp && x.Code == code).FirstOrDefault().Weight/totalpondweight * 100 : 0;
                        datarray.Add(dataitem);
                    }
                    seriesitem.data = datarray.ToArray();
                    pondsamplings.Add(seriesitem);

                    todaysamplingpcts.Add(sampponds.Where(x => x.Code == code).Any() ? sampponds.Where(x => x.Code == code).Sum(x => x.Weight) / totalSamplingWeightToday * 100 : 0);
                    wtdsamplingpcts.Add(wtdsamps.Where(x => x.Code == code).Any() ? wtdsamps.Where(x => x.Code == code).FirstOrDefault().Weight / totalSamplingWeightWTD * 100 : 0);


                }

                var wtdSamplingCodes = sampponds.Select(x => x.Code).Distinct().ToArray();

                var todaySamplingAll = db.TodaySamplings;






                var retVal = new dtoAtAGlance
                {
                    //WTDSaleDollars = wtdDollars == null ? 0 : wtdDollars,
                    //WTDASP = wtdASP == null ? 0 : wtdASP,
                    //WTDSaleLbs = wtdLbs == null ? 0 : wtdLbs,
                    WTDSaleDollars = todayTotalPrice == null ? 0 : todayTotalPrice,
                    WTDASP = todayASP == null ? 0 : todayASP,
                    WTDSaleLbs = todayLbs == null ? 0 : todayLbs,
                    TodayProductionLbs = todayprodlbs == null ? 0 : todayprodlbs,
                    WTDProductionLbs = wtdprodlbs == null ? 0 : wtdprodlbs,
                    TodayIQFLbs = todayiqf == null ? 0 : todayiqf,
                    WTDIQFLbs = wtdiqf == null ? 0 : wtdiqf,
                    TodayFreshLbs = todayfresh == null ? 0 : todayfresh,
                    WTDFreshLbs = wtdfresh == null ? 0 : wtdfresh,
                    //WTDOrderCount = wtdordercounttotal,
                    PondsforWeights = prodpondlist.ToArray(),
                    Weights = pondweightlist.ToArray(),
                    PondsForSamplings = sampponds.Select(x => x.PondName).Distinct().ToArray(),
                    PondSamplings = pondsamplings,
                    WeightCategories = wtdSamplingCodes,
                    TodaySamplingTotals = todaysamplingpcts.ToArray(),
                    WTDSamplingTotals = wtdsamplingpcts.ToArray()




                };








                //var retVal = new ShippingReportDTO
                //{
                //    Key = key,
                //};
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return request.CreateResponse(HttpStatusCode.NotFound, message);

        }
        [HttpPost]
        public HttpResponseMessage GetAtAGlance([FromBody] DailyReportDTO cqDTO)
        {
            return GetAtAGlance(Request, cqDTO);
        }


        internal HttpResponseMessage GetAtAGlanceEmail(HttpRequestMessage request, DailyReportDTO cqDTO)
        {

                var uc = new UtilitiesController();
                //uc.UpdateCurrentShipping();  //Update all the adagio and innova records if stale

                DayOfWeek day = DateTime.Now.DayOfWeek;
                int days = day - DayOfWeek.Monday;
                DateTime start = DateTime.Now.AddDays(-days).Date;
                DateTime end = start.AddDays(6);
            DateTime today = DateTime.Now.Date;

                var db = new AppEntities();
            //var wtdHeaders = db.AdagioOrderHeaders.Where(x => x.ExpectedShipDate >= start && x.ExpectedShipDate <= end);
            //var wtdDollars = wtdHeaders.Sum(x => x.TotalOrderValue);
            //var wtdOrderCount = db.AdagioOrderHeaders.Where(x => x.ExpectedShipDate >= start && x.ExpectedShipDate <= end).Count();
            //var todayHeaders = db.AdagioTodayOrders;
            //var todayDollars = todayHeaders.Sum(x => x.TotalOrderValue);
            //var todayOrderCount = db.AdagioTodayOrders.Count();

            //var ddocs = wtdHeaders.Select(x => x.Doc).Distinct().ToArray();

            //var wtdDetails = db.AdagioOrderDetails.Where(x => ddocs.Contains(x.DDoc));
            //var wtdLbs = wtdDetails.Sum(x => x.ExtWeight);
            //var wtdTotalPrice = wtdDetails.Sum(x => x.ExtPrice);

            //var todayddocs = todayHeaders.Select(x => x.OrderKey).Distinct().ToArray();
            var todayDetails = db.AdagioOrderDetailsForTodays.Where(x => !(x.Description.Contains("hush")) && !(x.Description.Contains("head")));
            var todayLbs = todayDetails.Sum(x => x.ExtWeight);
            var todayTotalPrice = todayDetails.Sum(x => x.ExtPrice);
            var todayASP = todayTotalPrice / todayLbs;
            //var wtdASP = (wtdTotalPrice + todayTotalPrice) / (wtdLbs + todayLbs);
            //var realwtdTotalPrice = (wtdTotalPrice + todayTotalPrice);
            //var wtdordercounttotal = wtdOrderCount + todayOrderCount;


            //var todayProductionTotals = db.TodaysProductionTotals.Where(x => x.Station != null).Sum(x => x.Weight);
            int[] prodstations = new int[] { 8, 2, 4, 3, 866, 9, 10, 1024, 1025, 7 };
                var stationList = prodstations.ToList();
                int[] freshstations = new int[] { 8, 2, 4, 3, 866 };
                var frershstationList = freshstations.ToList();
                int[] iqfstations = new int[] { 9, 10, 1024, 1025, 7 };
                var iqfstationList = iqfstations.ToList();
                var todayProductionTotals = db.TodaysProductionTotals.Where(x => x.Station.HasValue && prodstations.Contains((Int32)x.Station));
                var wtdprodrecords = db.DepartmentTotals.Where(x => x.DTDate >= start && x.DTDate <= end && (x.DepartmentID == 4 || x.DepartmentID == 5 || x.DepartmentID == 6));
                var wtdprodlbs = todayProductionTotals.Sum(x => x.Weight) + (wtdprodrecords.Sum(x => x.ShiftWeight) != null ? wtdprodrecords.Sum(x => x.ShiftWeight) : 0);
                var todayprodlbs = todayProductionTotals.Sum(x => x.Weight);
                var todayiqf = todayProductionTotals.Where(x => x.Station.HasValue && iqfstations.Contains((Int32)x.Station)).Sum(x => x.Weight);
                var wtdiqf = (wtdprodrecords.Where(x => (x.DepartmentID == 5 || x.DepartmentID == 6)).Sum(x => x.ShiftWeight) != null
                ? wtdprodrecords.Where(x => (x.DepartmentID == 5 || x.DepartmentID == 6)).Sum(x => x.ShiftWeight) : 0) 
                + todayProductionTotals.Where(x => x.Station.HasValue && iqfstations.Contains((Int32)x.Station)).Sum(x => x.Weight);
                var todayfresh = todayProductionTotals.Where(x => x.Station.HasValue && freshstations.Contains((Int32)x.Station)).Sum(x => x.Weight);
                var wtdfresh = (wtdprodrecords.Where(x => x.DepartmentID == 4).Sum(x => x.ShiftWeight) != null
                ? wtdprodrecords.Where(x => x.DepartmentID == 4).Sum(x => x.ShiftWeight) : 0)
                + todayProductionTotals.Where(x => x.Station.HasValue && freshstations.Contains((Int32)x.Station)).Sum(x => x.Weight);

                var thisday = DateTime.Now.Date;
                var prodponds = db.PlantPondWeights.Include("Pond").Where(x => x.PPWDateTime >= thisday && x.PPWDateTime <= end).
                    GroupBy(g => g.Pond.InnovaName).Select(x => new
                    {
                        PondName = x.Key,
                        Weight = x.Sum(y => y.PondWeight)
                    }).OrderBy(x => x.PondName);
                List<string> prodpondlist = new List<string>();
                List<decimal?> pondweightlist = new List<decimal?>();
                foreach (var ppw in prodponds)
                {
                    prodpondlist.Add(ppw.PondName);
                    pondweightlist.Add(ppw.Weight);
                }

                var sampponds = db.TodaySamplings.
                GroupBy(g => new { g.pondname, g.code2 }).Select(x => new
                {
                    PondName = x.Key.pondname,
                    Code = x.Key.code2,
                    Weight = x.Sum(y => y.weight)
                }).OrderBy(x => x.PondName).ThenBy(x => x.Code);
                var wtdsamps = db.Samplings.Where(x => x.regtime >= start && x.regtime <= end).GroupBy(x => x.code2).Select(x => new
                {
                    Code = x.Key,
                    Weight = x.Sum(y => y.weight) != null ? x.Sum(y => y.weight) : 0
                });
                List<string> sampondlist = sampponds.Select(x => x.PondName).Distinct().ToList();
                List<dtoNameAndDoubleArray> pondsamplings = new List<dtoNameAndDoubleArray>();
                List<double?> todaysamplingpcts = new List<double?>();
                List<double?> wtdsamplingpcts = new List<double?>();
                double? totalSamplingWeightToday = sampponds.Sum(x => x.Weight);
                double? totalSamplingWeightWTD = wtdsamps.Sum(x => x.Weight) != null ? wtdsamps.Sum(x => x.Weight) : 1;
                foreach (var code in sampponds.Select(x => x.Code).Distinct())
                {
                    List<double?> datarray = new List<double?>();
                    var seriesitem = new dtoNameAndDoubleArray()
                    {
                        name = code
                    };
                    foreach (var sp in sampondlist)
                    {
                        double? totalpondweight = sampponds.Where(x => x.PondName == sp).Any() ? sampponds.Where(x => x.PondName == sp).Sum(x => x.Weight) : 1;
                        double? dataitem = sampponds.Where(x => x.PondName == sp && x.Code == code).Any() ? sampponds.Where(x => x.PondName == sp && x.Code == code).FirstOrDefault().Weight / totalpondweight * 100 : 0;
                        datarray.Add(dataitem);
                    }
                    seriesitem.data = datarray.ToArray();
                    pondsamplings.Add(seriesitem);
                var todayweightbycode = sampponds.Where(x => x.Code == code).Any() ? sampponds.Where(x => x.Code == code).Sum(x => x.Weight)  : 0;
                    todaysamplingpcts.Add(todayweightbycode / totalSamplingWeightToday * 100);
                    wtdsamplingpcts.Add(wtdsamps.Where(x => x.Code == code).Any() ? 
                        (wtdsamps.Where(x => x.Code == code).FirstOrDefault().Weight != null ? wtdsamps.Where(x => x.Code == code).FirstOrDefault().Weight + todayweightbycode : todayweightbycode)
                        / (totalSamplingWeightWTD + totalSamplingWeightToday) * 100 
                        : todayweightbycode / totalSamplingWeightToday * 100);


                }

                var wtdSamplingCodes = sampponds.Select(x => x.Code).Distinct().ToArray();

                var todaySamplingAll = db.TodaySamplings;






                var retVal = new dtoAtAGlance
                {
                    //WTDSaleDollars = wtdDollars == null ? 0 : wtdDollars,
                    //WTDASP = wtdASP == null ? 0 : wtdASP,
                    //WTDSaleLbs = wtdLbs == null ? 0 : wtdLbs,
                    WTDSaleDollars = todayTotalPrice == null ? 0 : todayTotalPrice,
                    WTDASP = todayASP == null ? 0 : todayASP,
                    WTDSaleLbs = todayLbs == null ? 0 : todayLbs,
                    TodayProductionLbs = todayprodlbs == null ? 0 : todayprodlbs,
                    WTDProductionLbs = wtdprodlbs == null ? 0 : wtdprodlbs,
                    TodayIQFLbs = todayiqf == null ? 0 : todayiqf,
                    WTDIQFLbs = wtdiqf == null ? 0 : wtdiqf,
                    TodayFreshLbs = todayfresh == null ? 0 : todayfresh,
                    WTDFreshLbs = wtdfresh == null ? 0 : wtdfresh,
                    //WTDOrderCount = wtdordercounttotal,
                    PondsforWeights = prodpondlist.ToArray(),
                    Weights = pondweightlist.ToArray(),
                    PondsForSamplings = sampponds.Select(x => x.PondName).Distinct().ToArray(),
                    PondSamplings = pondsamplings,
                    WeightCategories = wtdSamplingCodes,
                    TodaySamplingTotals = todaysamplingpcts.ToArray(),
                    WTDSamplingTotals = wtdsamplingpcts.ToArray()




                };








                //var retVal = new ShippingReportDTO
                //{
                //    Key = key,
                //};
                return Request.CreateResponse(HttpStatusCode.OK, retVal);


        }
        [HttpPost]
        public HttpResponseMessage GetAtAGlanceEmail([FromBody] DailyReportDTO cqDTO)
        {
            return GetAtAGlanceEmail(Request, cqDTO);
        }


        internal HttpResponseMessage SendAtAGlanceEmail(HttpRequestMessage request, EmailContentDTO cqDTO)
        {

            var uc = new UtilitiesController();
            uc.SendMailLocalHostTest("harper@bhammountainradio.com", "Harvest Select - At A Glance - " + DateTime.Now.ToShortDateString(), cqDTO.Content);
       

            return Request.CreateResponse(HttpStatusCode.OK, "Success");


        }
        [HttpPost]
        public HttpResponseMessage SendAtAGlanceEmail([FromBody] EmailContentDTO cqDTO)
        {
            return SendAtAGlanceEmail(Request, cqDTO);
        }

    }

}