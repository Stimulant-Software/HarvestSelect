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
    public class FinishTimeController : BaseApiController
    {




        [HttpPut]
        public HttpResponseMessage FinishTimeAddOrEdit([FromBody] FinishTimeDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var FinishTimeId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref FinishTimeId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0 && aur.IsInRole(userId, "Data Entry"))
            {
                var FinishTime = new FinishTime();
                var errors = ValidateDtoData(uDto, FinishTime);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;

                uDto.FinishDateTime = uDto.FinishDateTime + " " + uDto.FinishTime;
                if (int.TryParse(uDto.FinishTimeID, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewFinishTimeRecord(Request, uDto, key, FinishTimeId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingFinishTimeRecord(Request, uDto, NEUserId, key, FinishTimeId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewFinishTimeRecord(HttpRequestMessage request, FinishTimeDTO uDto, string key, int FinishTimeId, int userId)
        {
            var ur = new FinishTimeRepository();
            var user = new FinishTime();


            var validationErrors = GetValidationErrors(ur, user, uDto, FinishTimeId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            user = ur.Save(user);

            UpdateDepartmentTotalFinishTime(uDto);
            uDto.Key = key;
            uDto.FinishTimeID = user.FinishTimeID.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.FinishTimeID
            }));
            return response;
        }
        private void UpdateDepartmentTotalFinishTime(FinishTimeDTO uDto)
        {
            var pr = new DepartmentTotalRepository();
            var prod = new DepartmentTotal();
            var prodexists = pr.GetByDateAndDepartment(DateTime.Parse(uDto.FinishDateTime), int.Parse(uDto.DepartmentID));
            DateTime fdt = DateTime.Parse(uDto.FinishDateTime);
            if (prodexists == null)
            {
                prod.DepartmentID = int.Parse(uDto.DepartmentID);
                prod.DTDate = DateTime.Parse(uDto.FinishDateTime);
                prod.FinishTime = fdt;
                pr.Save(prod);
            }
            else
            {
                prod = prodexists;
                var wbr = new FinishTimeRepository();
                List<FinishTime> wbl = wbr.GetByDateAndDepartment(DateTime.Parse(uDto.FinishDateTime), int.Parse(uDto.DepartmentID));
                fdt = wbl.Select(x => x.FinishDateTime).FirstOrDefault();
                prod.FinishTime = fdt;
                pr.Save(prod);
            }
        }

        private HttpResponseMessage ProcessExistingFinishTimeRecord(HttpRequestMessage request, FinishTimeDTO cqDto, int contactId, string key, int FinishTimeId, int userId)
        {
            var ur = new FinishTimeRepository();
            var user = new FinishTime();
            user = ur.GetById(contactId);


            var validationErrors = GetValidationErrors(ur, user, cqDto, FinishTimeId, userId);
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

            UpdateDepartmentTotalFinishTime(cqDto);

            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(FinishTimeRepository pr, FinishTime contact, FinishTimeDTO cqDto, int YieldID, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        internal HttpResponseMessage FinishTimes(HttpRequestMessage request, FinishTimeDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new FinishTimeRepository();
                var u = new FinishTime();
                if (cqDTO.FinishDateTime != null)
                {
                    cqDTO.Start_FinishDateTime = DateTime.Parse(cqDTO.FinishDateTime).ToString();
                    cqDTO.End_FinishDateTime = DateTime.Parse(cqDTO.FinishDateTime).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_FinishDateTime = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_FinishDateTime = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_FinishDateTime = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_FinishDateTime = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_FinishDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_FinishDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }

                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.FinishDateTime).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();

                    dic.Add("FinishTimeID", item.FinishTimeID.ToString());
                    dic.Add("DepartmentID", item.DepartmentID.ToString());
                    dic.Add("DepartmentName", item.Department.DepartmentName);
                    dic.Add("FinishDateTime", item.FinishDateTime.ToShortTimeString());

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
        public HttpResponseMessage FinishTimes([FromBody] FinishTimeDTO cqDTO)
        {
            return FinishTimes(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage FinishTimeList([FromBody] FinishTimeDTO cqDTO)
        {
            return FinishTimes(Request, cqDTO);
        }

        [HttpPost]
        public HttpResponseMessage FinishDateTimes([FromBody] FinishTimeDTO cqDTO)
        {
            return FinishDateTimes(Request, cqDTO);
        }
        internal HttpResponseMessage FinishDateTimes(HttpRequestMessage request, FinishTimeDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new FinishTimeRepository();
                var u = new FinishTime();
                if (cqDTO.FinishDateTime != null)
                {
                    cqDTO.Start_FinishDateTime = DateTime.Parse(cqDTO.FinishDateTime).ToString();
                    cqDTO.End_FinishDateTime = DateTime.Parse(cqDTO.FinishDateTime).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_FinishDateTime = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_FinishDateTime = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_FinishDateTime = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_FinishDateTime = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_FinishDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_FinishDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.GroupBy(x => x.FinishDateTime).Select(x => x.First()).OrderBy(x => x.FinishDateTime).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();


                    dic.Add("FinishDateTime", item.FinishDateTime.ToShortDateString());

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