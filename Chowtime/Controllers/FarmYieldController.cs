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
    public class FarmYieldController : BaseApiController
    {




        [HttpPut]
        public HttpResponseMessage FarmYieldAddOrEdit([FromBody] FarmYieldDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var FarmYieldId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref FarmYieldId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0 && aur.IsInRole(userId, "Data Entry"))
            {
                var FarmYield = new FarmYield();
                var errors = ValidateDtoData(uDto, FarmYield);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.YieldID, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewFarmYieldRecord(Request, uDto, key, FarmYieldId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingFarmYieldRecord(Request, uDto, NEUserId, key, FarmYieldId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewFarmYieldRecord(HttpRequestMessage request, FarmYieldDTO uDto, string key, int FarmYieldId, int userId)
        {
            var ur = new FarmYieldRepository();
            var user = new FarmYield();


            var validationErrors = GetValidationErrors(ur, user, uDto, FarmYieldId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            user = ur.Save(user);
            uDto.Key = key;
            uDto.YieldID = user.YieldID.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.YieldID
            }));
            return response;
        }

        private HttpResponseMessage ProcessExistingFarmYieldRecord(HttpRequestMessage request, FarmYieldDTO cqDto, int contactId, string key, int FarmYieldId, int userId)
        {
            var ur = new FarmYieldRepository();
            var user = new FarmYield();
            user = ur.GetById(contactId);

            
            var validationErrors = GetValidationErrors(ur, user, cqDto, FarmYieldId, userId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            if (cqDto.Remove != null && int.Parse(cqDto.Remove) == 1)
            {
                ur.Delete(user);
            }
            else
            {
                ur.Save(user);
            }
            
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(FarmYieldRepository pr, FarmYield contact, FarmYieldDTO cqDto, int YieldID, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        internal HttpResponseMessage FarmYields(HttpRequestMessage request, FarmYieldDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new FarmYieldRepository();
                var u = new FarmYield();
                if (cqDTO.YieldDate != null)
                {
                    cqDTO.Start_YieldDate = DateTime.Parse(cqDTO.YieldDate).ToString();
                    cqDTO.End_YieldDate = DateTime.Parse(cqDTO.YieldDate).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_YieldDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_YieldDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_YieldDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_YieldDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_YieldDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_YieldDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.YieldDate).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();

                    dic.Add("YieldId", item.YieldID.ToString());
                    dic.Add("PondID", item.PondID.ToString());
                    dic.Add("PondName", item.Pond.PondName);
                    dic.Add("FarmID", item.Pond.FarmId.ToString());
                    dic.Add("YieldDate", item.YieldDate.ToShortDateString());
                    dic.Add("PoundsYielded", item.PoundsYielded.ToString());
                    dic.Add("PoundsPlant", item.PoundsPlant.ToString());
                    dic.Add("PoundsHeaded", item.PoundsHeaded.ToString());
                    dic.Add("PercentYield", item.PercentYield.ToString());
                    dic.Add("PercentYield2", item.PercentYield.ToString());
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
        public HttpResponseMessage FarmYields([FromBody] FarmYieldDTO cqDTO)
        {
            return FarmYields(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage FarmYieldList([FromBody] FarmYieldDTO cqDTO)
        {
            return FarmYields(Request, cqDTO);
        }

        [HttpPost]
        public HttpResponseMessage FarmYieldDates([FromBody] FarmYieldDTO cqDTO)
        {
            return FarmYieldDates(Request, cqDTO);
        }
        internal HttpResponseMessage FarmYieldDates(HttpRequestMessage request, FarmYieldDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new FarmYieldRepository();
                var u = new FarmYield();
                if (cqDTO.YieldDate != null)
                {
                    cqDTO.Start_YieldDate = DateTime.Parse(cqDTO.YieldDate).ToString();
                    cqDTO.End_YieldDate = DateTime.Parse(cqDTO.YieldDate).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_YieldDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_YieldDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_YieldDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_YieldDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_YieldDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_YieldDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.GroupBy(x => x.YieldDate).Select(x => x.First()).OrderBy(x => x.YieldDate).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();


                    dic.Add("YieldDate", item.YieldDate.ToShortDateString());

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