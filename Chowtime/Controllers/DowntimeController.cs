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
    public class DownTimeController : BaseApiController
    {




        [HttpPut]
        public HttpResponseMessage DownTimeAddOrEdit([FromBody] DownTimeDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var DownTimeId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref DownTimeId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0 && aur.IsInRole(userId, "Data Entry"))
            {
                var DownTime = new DownTime();
                var errors = ValidateDtoData(uDto, DownTime);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                uDto.DownTimeNote = uDto.DownTimeNote == "" ? null : uDto.DownTimeNote;
                uDto.Minutes = uDto.Minutes == "" ? null : uDto.Minutes;

                if (int.TryParse(uDto.DownTimeID, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewDownTimeRecord(Request, uDto, key, DownTimeId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingDownTimeRecord(Request, uDto, NEUserId, key, DownTimeId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewDownTimeRecord(HttpRequestMessage request, DownTimeDTO uDto, string key, int DownTimeId, int userId)
        {
            var ur = new DownTimeRepository();
            var user = new DownTime();


            var validationErrors = GetValidationErrors(ur, user, uDto, DownTimeId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            user = ur.Save(user);

            UpdateDepartmentTotalDownTime(uDto);
            uDto.Key = key;
            uDto.DownTimeID = user.DownTimeID.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.DownTimeID
            }));
            return response;
        }
        private void UpdateDepartmentTotalDownTime(DownTimeDTO uDto)
        {
            var pr = new DepartmentTotalRepository();
            var prod = new DepartmentTotal();
            var prodexists = pr.GetByDateAndDepartment(DateTime.Parse(uDto.DownTimeDate), int.Parse(uDto.DepartmentID));
            int wb = 0;
            wb = uDto.Minutes == null ? 0 : int.Parse(uDto.Minutes);

            if (prodexists == null)
            {
                prod.DepartmentID = int.Parse(uDto.DepartmentID);
                prod.DTDate = DateTime.Parse(uDto.DownTimeDate);
                prod.DownTime = wb;
                pr.Save(prod);
            }
            else
            {
                prod = prodexists;
                var wbr = new DownTimeRepository();
                List<DownTime> wbl = wbr.GetByDateAndDepartment(DateTime.Parse(uDto.DownTimeDate), int.Parse(uDto.DepartmentID));
                wb = wbl.Where(x => x.Minutes != null).Sum(x => x.Minutes);


                if (wb != 0) { prod.DownTime = wb; }
                pr.Save(prod);
            }
        }

        private HttpResponseMessage ProcessExistingDownTimeRecord(HttpRequestMessage request, DownTimeDTO cqDto, int contactId, string key, int DownTimeId, int userId)
        {
            var ur = new DownTimeRepository();
            var user = new DownTime();
            user = ur.GetById(contactId);


            var validationErrors = GetValidationErrors(ur, user, cqDto, DownTimeId, userId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            //if (cqDto.Remove != null && int.Parse(cqDto.Remove) == 1)
            //{
            //    ur.Delete(user);
            //}
            else
            {
                ur.Save(user);
            }

            UpdateDepartmentTotalDownTime(cqDto);

            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(DownTimeRepository pr, DownTime contact, DownTimeDTO cqDto, int YieldID, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        internal HttpResponseMessage DownTimes(HttpRequestMessage request, DownTimeDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new DownTimeRepository();
                var u = new DownTime();
                if (cqDTO.DownTimeDate != null)
                {
                    cqDTO.Start_DownTimeDate = DateTime.Parse(cqDTO.DownTimeDate).ToString();
                    cqDTO.End_DownTimeDate = DateTime.Parse(cqDTO.DownTimeDate).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_DownTimeDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_DownTimeDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_DownTimeDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_DownTimeDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_DownTimeDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_DownTimeDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }

                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.DownTimeDate).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();

                    dic.Add("DownTimeID", item.DownTimeID.ToString());
                    dic.Add("DepartmentID", item.DownTimeType.DepartmentID.ToString());
                    dic.Add("DepartmentName", item.DownTimeType.Department.DepartmentName);
                    dic.Add("DownTimeDate", item.DownTimeDate.ToShortDateString());
                    dic.Add("DownTimeTypeID", item.DownTimeTypeID.ToString());
                    dic.Add("DownTimeType", item.DownTimeType.DownTimeName.ToString());
                    dic.Add("Minutes", item.Minutes.ToString());
                    dic.Add("Note", item.DownTimeNote.ToString());

                    col.Add(dic);
                    var ufdic = new Dictionary<string, string>();


                }
                var dttr = new DownTimeTypeRepository();
                var dtts = dttr.GetDownTimeTypesByDept(int.Parse(cqDTO.DepartmentID));
                var col2 = new Collection<Dictionary<string, string>>();
                foreach (var dtt in dtts)
                {
                    var dic = new Dictionary<string, string>();
                    dic.Add("DownTimeTypeID", dtt.DownTimeTypeID.ToString());
                    dic.Add("DownTypeName", dtt.DownTimeName);
                    col2.Add(dic);
                }

                var retVal = new GenericDTO
                {
                    Key = key,
                    ReturnData = col,
                    ReturnData1 = col2
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return request.CreateResponse(HttpStatusCode.NotFound, message);

        }




        [HttpPost]
        public HttpResponseMessage DownTimes([FromBody] DownTimeDTO cqDTO)
        {
            return DownTimes(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage DownTimeList([FromBody] DownTimeDTO cqDTO)
        {
            return DownTimes(Request, cqDTO);
        }

        [HttpPost]
        public HttpResponseMessage DownTimeDates([FromBody] DownTimeDTO cqDTO)
        {
            return DownTimeDates(Request, cqDTO);
        }
        internal HttpResponseMessage DownTimeDates(HttpRequestMessage request, DownTimeDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new DownTimeRepository();
                var u = new DownTime();
                if (cqDTO.DownTimeDate != null)
                {
                    cqDTO.Start_DownTimeDate = DateTime.Parse(cqDTO.DownTimeDate).ToString();
                    cqDTO.End_DownTimeDate = DateTime.Parse(cqDTO.DownTimeDate).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_DownTimeDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_DownTimeDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_DownTimeDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_DownTimeDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_DownTimeDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_DownTimeDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.GroupBy(x => x.DownTimeDate).Select(x => x.First()).OrderBy(x => x.DownTimeDate).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();


                    dic.Add("DownTimeDate", item.DownTimeDate.ToShortDateString());

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
    }

}