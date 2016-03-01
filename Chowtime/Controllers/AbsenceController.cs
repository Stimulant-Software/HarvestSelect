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
    public class AbsenceController : BaseApiController
    {




        [HttpPut]
        public HttpResponseMessage AbsenceAddOrEdit([FromBody] AbsenceDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var AbsenceId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref AbsenceId);

            AppUserRoleRepository aur = new AppUserRoleRepository();
            uDto.RegEmpLate = uDto.RegEmpLate == "" ? null : uDto.RegEmpLate;
            uDto.RegEmpLeftEarly = uDto.RegEmpLeftEarly == "" ? null : uDto.RegEmpLeftEarly;
            uDto.RegEmpOut = uDto.RegEmpOut == "" ? null : uDto.RegEmpOut;
            uDto.TempEmpLate = uDto.TempEmpLate == "" ? null : uDto.TempEmpLate;
            uDto.TempEmpLeftEarly = uDto.TempEmpLeftEarly == "" ? null : uDto.TempEmpLeftEarly;
            uDto.TempEmpOut = uDto.TempEmpOut == "" ? null : uDto.TempEmpOut;
            uDto.InmateLeftEarly = uDto.InmateLeftEarly == "" ? null : uDto.InmateLeftEarly;
            uDto.InmateOut = uDto.InmateOut == "" ? null : uDto.InmateOut;
            uDto.EmployeesOnVacation = uDto.EmployeesOnVacation == "" ? null : uDto.EmployeesOnVacation;

            if (userId > 0 && aur.IsInRole(userId, "Data Entry"))
            {
                var Absence = new Absence();
                var errors = ValidateDtoData(uDto, Absence);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                

                if (int.TryParse(uDto.AbsenceID, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewAbsenceRecord(Request, uDto, key, AbsenceId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingAbsenceRecord(Request, uDto, NEUserId, key, AbsenceId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewAbsenceRecord(HttpRequestMessage request, AbsenceDTO uDto, string key, int AbsenceId, int userId)
        {
            var ur = new AbsenceRepository();
            var user = new Absence();


            var validationErrors = GetValidationErrors(ur, user, uDto, AbsenceId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            user = ur.Save(user);

            UpdateDepartmentTotalAbsence(uDto);
            uDto.Key = key;
            uDto.AbsenceID = user.AbsenceID.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.AbsenceID
            }));
            return response;
        }
        private void UpdateDepartmentTotalAbsence(AbsenceDTO uDto)
        {
            var pr = new DepartmentTotalRepository();
            var prod = new DepartmentTotal();
            var prodexists = pr.GetByDateAndDepartment(DateTime.Parse(uDto.AbsenceDate), int.Parse(uDto.DepartmentID));
            int wb = 0;
            wb = uDto.RegEmpLate == null ? 0 : int.Parse(uDto.RegEmpLate);
            wb += uDto.RegEmpLeftEarly == null ? 0 : int.Parse(uDto.RegEmpLeftEarly);
            wb += uDto.RegEmpOut == null ? 0 : int.Parse(uDto.RegEmpOut);
            wb += uDto.TempEmpLate == null ? 0 : int.Parse(uDto.TempEmpLate);
            wb += uDto.TempEmpLeftEarly == null ? 0 : int.Parse(uDto.TempEmpLeftEarly);
            wb += uDto.TempEmpOut == null ? 0 : int.Parse(uDto.TempEmpOut);
            wb += uDto.InmateLeftEarly == null ? 0 : int.Parse(uDto.InmateLeftEarly);
            wb += uDto.InmateOut == null ? 0 : int.Parse(uDto.InmateOut);
            wb += uDto.EmployeesOnVacation == null ? 0 : int.Parse(uDto.EmployeesOnVacation);
            if (prodexists == null)
            {
                prod.DepartmentID = int.Parse(uDto.DepartmentID);
                prod.DTDate = DateTime.Parse(uDto.AbsenceDate);
                prod.Absences = wb;
                pr.Save(prod);
            }
            else
            {
                prod = prodexists;
                var wbr = new AbsenceRepository();
                List<Absence> wbl = wbr.GetByDateAndDepartment(DateTime.Parse(uDto.AbsenceDate), int.Parse(uDto.DepartmentID));
                wb = wbl.Where(x => x.RegEmpLate != null).Sum(x => x.RegEmpLate).Value;
                wb += wbl.Where(x => x.RegEmpLeftEarly != null).Sum(x => x.RegEmpLeftEarly).Value;
                wb += wbl.Where(x => x.RegEmpOut != null).Sum(x => x.RegEmpOut).Value;
                wb += wbl.Where(x => x.TempEmpLate != null).Sum(x => x.TempEmpLate).Value;
                wb += wbl.Where(x => x.TempEmpLeftEarly != null).Sum(x => x.TempEmpLeftEarly).Value;
                wb += wbl.Where(x => x.TempEmpOut != null).Sum(x => x.TempEmpOut).Value;
                wb += wbl.Where(x => x.InmateLeftEarly != null).Sum(x => x.InmateLeftEarly).Value;
                wb += wbl.Where(x => x.InmateOut != null).Sum(x => x.InmateOut).Value;
                wb += wbl.Where(x => x.EmployeesOnVacation != null).Sum(x => x.EmployeesOnVacation).Value;

                if (wb != 0) { prod.Absences = wb; }
                pr.Save(prod);
            }
        }

        private HttpResponseMessage ProcessExistingAbsenceRecord(HttpRequestMessage request, AbsenceDTO cqDto, int contactId, string key, int AbsenceId, int userId)
        {
            var ur = new AbsenceRepository();
            var user = new Absence();
            user = ur.GetById(contactId);


            var validationErrors = GetValidationErrors(ur, user, cqDto, AbsenceId, userId);
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

            UpdateDepartmentTotalAbsence(cqDto);

            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(AbsenceRepository pr, Absence contact, AbsenceDTO cqDto, int YieldID, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        internal HttpResponseMessage Absences(HttpRequestMessage request, AbsenceDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new AbsenceRepository();
                var u = new Absence();
                if (cqDTO.AbsenceDate != null)
                {
                    cqDTO.Start_AbsenceDate = DateTime.Parse(cqDTO.AbsenceDate).ToString();
                    cqDTO.End_AbsenceDate = DateTime.Parse(cqDTO.AbsenceDate).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_AbsenceDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_AbsenceDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_AbsenceDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_AbsenceDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_AbsenceDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_AbsenceDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }

                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.AbsenceDate).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();

                    dic.Add("AbsenceID", item.AbsenceID.ToString());
                    dic.Add("DepartmentID", item.DepartmentID.ToString());
                    dic.Add("DepartmentName", item.Department.DepartmentName);
                    dic.Add("AbsenceDate", item.AbsenceDate.ToShortDateString());
                    dic.Add("RegEmpLate", item.RegEmpLate.ToString());
                    dic.Add("RegEmpLeftEarly", item.RegEmpLeftEarly.ToString());
                    dic.Add("RegEmpOut", item.RegEmpOut.ToString());
                    dic.Add("TempEmpLate", item.TempEmpLate.ToString());
                    dic.Add("TempEmpLeftEarly", item.TempEmpLeftEarly.ToString());
                    dic.Add("TempEmpOut", item.TempEmpOut.ToString());
                    dic.Add("InmateLeftEarly", item.InmateLeftEarly.ToString());
                    dic.Add("InmateOut", item.InmateOut.ToString());
                    dic.Add("EmployeesOnVacation", item.EmployeesOnVacation.ToString());
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
        public HttpResponseMessage Absences([FromBody] AbsenceDTO cqDTO)
        {
            return Absences(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage AbsenceList([FromBody] AbsenceDTO cqDTO)
        {
            return Absences(Request, cqDTO);
        }

        [HttpPost]
        public HttpResponseMessage AbsenceDates([FromBody] AbsenceDTO cqDTO)
        {
            return AbsenceDates(Request, cqDTO);
        }
        internal HttpResponseMessage AbsenceDates(HttpRequestMessage request, AbsenceDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new AbsenceRepository();
                var u = new Absence();
                if (cqDTO.AbsenceDate != null)
                {
                    cqDTO.Start_AbsenceDate = DateTime.Parse(cqDTO.AbsenceDate).ToString();
                    cqDTO.End_AbsenceDate = DateTime.Parse(cqDTO.AbsenceDate).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_AbsenceDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_AbsenceDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_AbsenceDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_AbsenceDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_AbsenceDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_AbsenceDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.GroupBy(x => x.AbsenceDate).Select(x => x.First()).OrderBy(x => x.AbsenceDate).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();


                    dic.Add("AbsenceDate", item.AbsenceDate.ToShortDateString());

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