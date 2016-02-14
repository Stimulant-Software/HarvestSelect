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
    public class ShiftEndController : BaseApiController
    {




        [HttpPut]
        public HttpResponseMessage ShiftEndAddOrEdit([FromBody] ShiftEndDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var ShiftEndId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref ShiftEndId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0 && aur.IsInRole(userId, "Data Entry"))
            {
                var ShiftEnd = new ShiftEnd();
                uDto.DayFinishedFreezing = uDto.DayFinishedFreezing == "" ? null : uDto.DayFinishedFreezing;
                uDto.RegEmpLate = uDto.RegEmpLate == "" ? null : uDto.RegEmpLate;
                uDto.RegEmpOut = uDto.RegEmpOut == "" ? null : uDto.RegEmpOut;
                uDto.RegEmplLeftEarly = uDto.RegEmplLeftEarly == "" ? null : uDto.RegEmplLeftEarly;
                uDto.TempEmpOut = uDto.TempEmpOut == "" ? null : uDto.TempEmpOut;
                uDto.InmateLeftEarly = uDto.InmateLeftEarly == "" ? null : uDto.InmateLeftEarly;
                uDto.FinishedKill = uDto.FinishedKill == "" ? null : uDto.FinishedKill;
                uDto.FinishedFillet = uDto.FinishedFillet == "" ? null : uDto.FinishedFillet;
                uDto.FinishedSkinning = uDto.FinishedSkinning == "" ? null : uDto.FinishedSkinning;
                uDto.DayFinishedFreezing = uDto.DayFinishedFreezing == "" ? null : uDto.DayFinishedFreezing;
                uDto.NightFinishedFreezing = uDto.NightFinishedFreezing == "" ? null : uDto.NightFinishedFreezing;
                uDto.DayShiftFroze = uDto.DayShiftFroze == "" ? null : uDto.DayShiftFroze;
                uDto.NightShiftFroze = uDto.NightShiftFroze == "" ? null : uDto.NightShiftFroze;
                uDto.FilletScaleReading = uDto.FilletScaleReading == "" ? null : uDto.FilletScaleReading;
                uDto.DowntimeMinutes = uDto.DowntimeMinutes == "" ? null : uDto.DowntimeMinutes;
                uDto.InLateOut = uDto.InLateOut == "" ? null : uDto.InLateOut;
                uDto.EmployeesOnVacation = uDto.EmployeesOnVacation == "" ? null : uDto.EmployeesOnVacation;
                var errors = ValidateDtoData(uDto, ShiftEnd);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.ShiftEndID, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewShiftEndRecord(Request, uDto, key, ShiftEndId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingShiftEndRecord(Request, uDto, NEUserId, key, ShiftEndId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewShiftEndRecord(HttpRequestMessage request, ShiftEndDTO uDto, string key, int ShiftEndId, int userId)
        {
            var ur = new ShiftEndRepository();
            var user = new ShiftEnd();


            var validationErrors = GetValidationErrors(ur, user, uDto, ShiftEndId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            user = ur.Save(user);
            uDto.Key = key;
            uDto.ShiftEndID = user.ShiftEndID.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.ShiftEndID
            }));
            return response;
        }

        private HttpResponseMessage ProcessExistingShiftEndRecord(HttpRequestMessage request, ShiftEndDTO cqDto, int contactId, string key, int ShiftEndId, int userId)
        {
            var ur = new ShiftEndRepository();
            var user = new ShiftEnd();
            user = ur.GetById(contactId);


            var validationErrors = GetValidationErrors(ur, user, cqDto, ShiftEndId, userId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            ur.Save(user);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(ShiftEndRepository pr, ShiftEnd contact, ShiftEndDTO cqDto, int ShiftEndID, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        internal HttpResponseMessage ShiftEnds(HttpRequestMessage request, ShiftEndDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new ShiftEndRepository();
                var u = new ShiftEnd();
                if (cqDTO.ShiftDate != null)
                {
                    cqDTO.Start_ShiftDate = DateTime.Parse(cqDTO.ShiftDate).ToString();
                    cqDTO.End_ShiftDate = DateTime.Parse(cqDTO.ShiftDate).AddDays(1).ToString();
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                data = data.OrderBy(x => x.ShiftDate).ToList();
                var col = new Collection<Dictionary<string, string>>();

                foreach (var item in data)
                {
                    
                    var dic = new Dictionary<string, string>();

                    dic.Add("ShiftEndID", item.ShiftEndID.ToString());
                    dic.Add("DayFinishedFreezing", item.DayFinishedFreezing == null ? "" : DateTime.Parse(item.DayFinishedFreezing.ToString()).ToString("HH:mm"));
                    dic.Add("DayShiftFroze", item.DayShiftFroze == null ? "" : item.DayShiftFroze.ToString());
                    dic.Add("DowntimeMinutes", item.DowntimeMinutes == null ? "" : item.DowntimeMinutes.ToString());
                    dic.Add("EmployeesOnVacation", item.EmployeesOnVacation == null ? "" : item.EmployeesOnVacation.ToString());
                    dic.Add("FilletScaleReading", item.FilletScaleReading == null ? "" : item.FilletScaleReading.ToString());
                    dic.Add("FinishedFillet", item.FinishedFillet == null ? "" : DateTime.Parse(item.FinishedFillet.ToString()).ToString("HH:mm"));
                    dic.Add("FinishedKill", item.FinishedKill == null ? "" : DateTime.Parse(item.FinishedKill.ToString()).ToString("HH:mm"));
                    dic.Add("FinishedSkinning", item.FinishedSkinning == null ? "" : DateTime.Parse(item.FinishedSkinning.ToString()).ToString("HH:mm"));
                    dic.Add("InmateLeftEarly", item.InmateLeftEarly == null ? "" : item.InmateLeftEarly.ToString());
                    dic.Add("InLateOut", item.InLateOut == null ? "" : item.InLateOut.ToString());
                    dic.Add("NightFinishedFreezing", item.NightFinishedFreezing == null ? "" : DateTime.Parse(item.NightFinishedFreezing.ToString()).ToString("HH:mm"));
                    dic.Add("NightShiftFroze", item.NightShiftFroze == null ? "" : item.NightShiftFroze.ToString());
                    dic.Add("RegEmpLate", item.RegEmpLate == null ? "" : item.RegEmpLate.ToString());
                    dic.Add("RegEmpOut", item.RegEmpOut == null ? "" : item.RegEmpOut.ToString());
                    dic.Add("RegEmplLeftEarly", item.RegEmplLeftEarly == null ? "" : item.RegEmplLeftEarly.ToString());
                    dic.Add("ShiftDate", item.ShiftDate == null ? "" : item.ShiftDate.ToString());
                    dic.Add("TempEmpOut", item.TempEmpOut == null ? "" : item.TempEmpOut.ToString());
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
        public HttpResponseMessage ShiftEnds([FromBody] ShiftEndDTO cqDTO)
        {
            return ShiftEnds(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage ShiftEndList([FromBody] ShiftEndDTO cqDTO)
        {
            return ShiftEnds(Request, cqDTO);
        }
    }

}