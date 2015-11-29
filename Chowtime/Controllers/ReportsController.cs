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

    }

}