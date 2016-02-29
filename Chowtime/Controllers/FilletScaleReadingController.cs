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
    public class FilletScaleReadingController : BaseApiController
    {




        [HttpPut]
        public HttpResponseMessage FilletScaleReadingAddOrEdit([FromBody] FilletScaleReadingDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var FilletScaleReadingId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref FilletScaleReadingId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0 && aur.IsInRole(userId, "Data Entry"))
            {
                var FilletScaleReading = new FilletScaleReading();
                var errors = ValidateDtoData(uDto, FilletScaleReading);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;


                if (int.TryParse(uDto.FilletScaleReadingID, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewFilletScaleReadingRecord(Request, uDto, key, FilletScaleReadingId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingFilletScaleReadingRecord(Request, uDto, NEUserId, key, FilletScaleReadingId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewFilletScaleReadingRecord(HttpRequestMessage request, FilletScaleReadingDTO uDto, string key, int FilletScaleReadingId, int userId)
        {
            var ur = new FilletScaleReadingRepository();
            var user = new FilletScaleReading();


            var validationErrors = GetValidationErrors(ur, user, uDto, FilletScaleReadingId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            user.FilletScaleReading1 = decimal.Parse(uDto.FilletScaleReading);

            user = ur.Save(user);

            uDto.Key = key;
            uDto.FilletScaleReadingID = user.FilletScaleReadingID.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.FilletScaleReadingID
            }));
            return response;
        }


        private HttpResponseMessage ProcessExistingFilletScaleReadingRecord(HttpRequestMessage request, FilletScaleReadingDTO cqDto, int contactId, string key, int FilletScaleReadingId, int userId)
        {
            var ur = new FilletScaleReadingRepository();
            var user = new FilletScaleReading();
            user = ur.GetById(contactId);


            var validationErrors = GetValidationErrors(ur, user, cqDto, FilletScaleReadingId, userId);
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
                user.FilletScaleReading1 = decimal.Parse(cqDto.FilletScaleReading);
                ur.Save(user);
            }

            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(FilletScaleReadingRepository pr, FilletScaleReading contact, FilletScaleReadingDTO cqDto, int YieldID, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        internal HttpResponseMessage FilletScaleReadings(HttpRequestMessage request, FilletScaleReadingDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new FilletScaleReadingRepository();
                var u = new FilletScaleReading();
                if (cqDTO.FSRDateTime != null)
                {
                    cqDTO.Start_FSRDateTime = DateTime.Parse(cqDTO.FSRDateTime).ToString();
                    cqDTO.End_FSRDateTime = DateTime.Parse(cqDTO.FSRDateTime).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_FSRDateTime = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_FSRDateTime = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_FSRDateTime = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_FSRDateTime = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_FSRDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_FSRDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }

                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.FSRDateTime).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();

                    dic.Add("FilletScaleReadingID", item.FilletScaleReadingID.ToString());
                    dic.Add("FSRDateTime", item.FSRDateTime.ToShortDateString());
                    dic.Add("FilletScaleReading", item.FilletScaleReading1.ToString());

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
        public HttpResponseMessage FilletScaleReadings([FromBody] FilletScaleReadingDTO cqDTO)
        {
            return FilletScaleReadings(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage FilletScaleReadingList([FromBody] FilletScaleReadingDTO cqDTO)
        {
            return FilletScaleReadings(Request, cqDTO);
        }

        [HttpPost]
        public HttpResponseMessage FSRDateTimes([FromBody] FilletScaleReadingDTO cqDTO)
        {
            return FSRDateTimes(Request, cqDTO);
        }
        internal HttpResponseMessage FSRDateTimes(HttpRequestMessage request, FilletScaleReadingDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new FilletScaleReadingRepository();
                var u = new FilletScaleReading();
                if (cqDTO.FSRDateTime != null)
                {
                    cqDTO.Start_FSRDateTime = DateTime.Parse(cqDTO.FSRDateTime).ToString();
                    cqDTO.End_FSRDateTime = DateTime.Parse(cqDTO.FSRDateTime).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_FSRDateTime = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_FSRDateTime = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_FSRDateTime = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_FSRDateTime = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_FSRDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_FSRDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.GroupBy(x => x.FSRDateTime).Select(x => x.First()).OrderBy(x => x.FSRDateTime).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();


                    dic.Add("FSRDateTime", item.FSRDateTime.ToShortDateString());

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