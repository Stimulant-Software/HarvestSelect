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
    public class DepartmentTotalController : BaseApiController
    {

        [HttpPost]
        public HttpResponseMessage DepartmentDates([FromBody] DepartmentTotalDTO cqDTO)
        {
            return DepartmentDates(Request, cqDTO);
        }
        internal HttpResponseMessage DepartmentDates(HttpRequestMessage request, DepartmentTotalDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new DepartmentTotalRepository();
                var u = new DepartmentTotal();
                if (cqDTO.DTDate != null)
                {
                    cqDTO.Start_DTDate = DateTime.Parse(cqDTO.DTDate).ToString();
                    cqDTO.End_DTDate = DateTime.Parse(cqDTO.DTDate).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_DTDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_DTDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_DTDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_DTDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_DTDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_DTDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();
                    dic.Add("DTDate", item.DTDate.ToShortDateString());
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
            return request.CreateResponse(HttpStatusCode.NotFound, message);

        }

        [HttpPost]
        public HttpResponseMessage DepartmentTotals([FromBody] DepartmentTotalDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new DepartmentTotalRepository();
                var u = new DepartmentTotal();
                if (cqDTO.DTDate != null)
                {
                    cqDTO.Start_DTDate = DateTime.Parse(cqDTO.DTDate).ToString();
                    cqDTO.End_DTDate = DateTime.Parse(cqDTO.DTDate).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_DTDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_DTDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_DTDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_DTDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_DTDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_DTDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                SGApp.DTOs.GenericDTO dto = new GenericDTO();
                dto.StartDate = DateTime.Parse(cqDTO.Start_DTDate);
                dto.EndDate = DateTime.Parse(cqDTO.End_DTDate);
                
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.DTDate).ToList();

                var depr = new DepartmentRepository();
                var deps = depr.GetDepartments();

                foreach (Department dep in deps)
                {

                    DepartmentTotal dt = data.Where(x => x.DepartmentID == dep.DepartmentID).FirstOrDefault();
                    var dic = new Dictionary<string, string>();
                    if (dt != null)
                    {
                        dic.Add("DepartmentTotalId", dt.DepartmentTotalID.ToString());
                        dic.Add("DepartmentID", dt.DepartmentID.ToString());
                        dic.Add("DepartmentName", dt.Department.DepartmentName);
                        dic.Add("DTDate", dt.DTDate.ToShortDateString());
                        dic.Add("FinishTime", dt.FinishTime != null ? dt.FinishTime.ToString() : "---");
                        dic.Add("DownTime", dt.DownTime != null ? dt.DownTime.ToString() : "---");
                        dic.Add("ShiftWeight", dt.ShiftWeight != null ? dt.ShiftWeight.ToString() : "---");
                        dic.Add("Absences", dt.Absences != null ? dt.Absences.ToString() : "---");
                    }
                    else
                    {
                        dic.Add("DepartmentTotalId", "-1");
                        dic.Add("DepartmentID", dep.DepartmentID.ToString());
                        dic.Add("DepartmentName", dep.DepartmentName);
                        dic.Add("DTDate", dto.StartDate.ToShortDateString());
                        dic.Add("FinishTime", "---");
                        dic.Add("DownTime", "---");
                        dic.Add("ShiftWeight", "---");
                        dic.Add("Absences", "---");

                    }

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
    }

}