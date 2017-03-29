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
using System.Web;
using Newtonsoft.Json;
using System.Web.Script.Serialization;


namespace SGApp.Controllers
{
    public class AdagioDataController : BaseApiController
    {




        [HttpPost]
        public HttpResponseMessage WeekDataAddOrEdit([FromBody] WeekDataDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var AbsenceId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref AbsenceId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0)
            {
                var wer = new AD_WeekDataRepository();
                var WeekEndDate = DateTime.Parse(uDto.AD_WeekEnd);

                var data = wer.GetByDate(WeekEndDate);
                if (data.Count == 0)
                {
                    var prodData = wer.GetAllProducts();
                    foreach (var prod in prodData)
                    {
                        var wkData = new AD_WeekData();
                        wkData.AD_ProductID = prod;
                        wkData.AD_WeekEnd = WeekEndDate;
                        wer.Save(wkData);
                    }
                    data = wer.GetByDate(WeekEndDate);
                }
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.AD_Products.AD_GroupID).ToList();
                
                foreach (var wd in data)
                {
                    var dic = new Dictionary<string, string>();
                    dic.Add("AD_ProductID", wd.AD_ProductID.ToString());
                    dic.Add("ProductName", wd.AD_Products.AD_ProductName);
                    dic.Add("BudgetLbs", wd.AD_BudgetLbs != null ? wd.AD_BudgetLbs.ToString() : "0");
                    dic.Add("BudgetDollars", wd.AD_BudgetDollars != null ? wd.AD_BudgetDollars.ToString() : "0");
                    dic.Add("ActualLbs", wd.AD_ActualLbs != null ? wd.AD_ActualLbs.ToString() : "0");
                    dic.Add("ActualDollars", wd.AD_ActualDollars != null ? wd.AD_ActualDollars.ToString() : "0");
                    dic.Add("AD_WeekDataID", wd.AD_WeekDataID.ToString());
                    col.Add(dic);

                }
                var retVal = new GenericDTO
                {
                    Key = key,
                    ReturnData = col
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);

            }

            

            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        




        [HttpPost]
        public HttpResponseMessage WeekDataDates([FromBody] WeekDataDTO cqDTO)
        {
            return WeekDataDates(Request, cqDTO);
        }
        internal HttpResponseMessage WeekDataDates(HttpRequestMessage request, WeekDataDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new AD_WeekDataRepository();
                
                if (cqDTO.AD_WeekEnd != null)
                {
                    cqDTO.Start_WeekDataDate = DateTime.Parse(cqDTO.AD_WeekEnd).ToString();
                    cqDTO.End_WeekDataDate = DateTime.Parse(cqDTO.AD_WeekEnd).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_WeekDataDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_WeekDataDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_WeekDataDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_WeekDataDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_WeekDataDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_WeekDataDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                
                var data = ur.GetByDateRange(DateTime.Parse(cqDTO.Start_WeekDataDate), DateTime.Parse(cqDTO.End_WeekDataDate));
                var col = new Collection<Dictionary<string, string>>();
                //data = data.GroupBy(x => x.AbsenceDate).Select(x => x.First()).OrderBy(x => x.AbsenceDate).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();


                    dic.Add("WeekDataDate", item.AD_WeekEnd.Value.ToShortDateString());

                    col.Add(dic);
                    var ufdic = new Dictionary<string, string>();


                }

                var retVal = new GenericDTO
                {
                    Key = key,
                    ReturnData = col
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return request.CreateResponse(HttpStatusCode.NotFound, message);

        }

        [HttpPost]
        public HttpResponseMessage ChangeWeekDataProperty(HttpRequestMessage request, WeekDataDTO cqDTO)
        {
            string key;
            var ur = new AppUserRepository();
            var CompanyId = 0;
            var userId = ur.ValidateUser(cqDTO.Key, out key, ref CompanyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0)
            {
                var sor = new AD_WeekDataRepository();

                var data = sor.GetById(int.Parse(cqDTO.AD_WeekDataID));
                if (cqDTO.AD_BudgetLbs != null)
                {
                    data.AD_BudgetLbs = int.Parse(cqDTO.AD_BudgetLbs);
                }
                if (cqDTO.AD_BudgetDollars != null)
                {
                    data.AD_BudgetDollars = decimal.Parse(cqDTO.AD_BudgetDollars);
                }
                if (cqDTO.AD_ActualLbs != null)
                {
                    data.AD_ActualLbs = int.Parse(cqDTO.AD_ActualLbs);
                }

                if (cqDTO.AD_ActualDollars != null)
                {
                    data.AD_ActualDollars = decimal.Parse(cqDTO.AD_ActualDollars);
                }
                sor.Save(data);

                return Request.CreateResponse(HttpStatusCode.OK, "Success");
            }//}
            var message = "validation failed";
            return request.CreateResponse(HttpStatusCode.NotFound, message);

        }
    }

}