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
    public class FarmYieldHeaderController : BaseApiController
    {




        [HttpPut]
        public HttpResponseMessage FarmYieldHeaderAddOrEdit([FromBody] FarmYieldHeaderDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var FarmYieldHeaderId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref FarmYieldHeaderId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0 && aur.IsInRole(userId, "Data Entry"))
            {
                var FarmYieldHeader = new FarmYieldHeader();
                var errors = ValidateDtoData(uDto, FarmYieldHeader);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.FarmYieldHeaderID, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewFarmYieldHeaderRecord(Request, uDto, key, FarmYieldHeaderId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingFarmYieldHeaderRecord(Request, uDto, NEUserId, key, FarmYieldHeaderId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewFarmYieldHeaderRecord(HttpRequestMessage request, FarmYieldHeaderDTO uDto, string key, int FarmYieldHeaderId, int userId)
        {
            var ur = new FarmYieldHeaderRepository();
            var user = new FarmYieldHeader();


            var validationErrors = GetValidationErrors(ur, user, uDto, FarmYieldHeaderId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            user = ur.Save(user);
            uDto.Key = key;
            uDto.FarmYieldHeaderID = user.FarmYieldHeaderID.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.FarmYieldHeaderID
            }));
            return response;
        }

        private HttpResponseMessage ProcessExistingFarmYieldHeaderRecord(HttpRequestMessage request, FarmYieldHeaderDTO cqDto, int contactId, string key, int FarmYieldHeaderId, int userId)
        {
            var ur = new FarmYieldHeaderRepository();
            var user = new FarmYieldHeader();
            user = ur.GetById(contactId);


            var validationErrors = GetValidationErrors(ur, user, cqDto, FarmYieldHeaderId, userId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            ur.Save(user);


            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(FarmYieldHeaderRepository pr, FarmYieldHeader contact, FarmYieldHeaderDTO cqDto, int YieldID, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        internal HttpResponseMessage FarmYieldHeaders(HttpRequestMessage request, FarmYieldHeaderDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new FarmYieldHeaderRepository();
                var u = new FarmYieldHeader();
                if (cqDTO.YieldDate != null)
                {
                    cqDTO.Start_YieldDate = DateTime.Parse(cqDTO.YieldDate).AddHours(-1).ToString();
                    cqDTO.End_YieldDate = DateTime.Parse(cqDTO.YieldDate).AddHours(1).ToString();
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.YieldDate).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();


                    dic.Add("FarmYieldHeaderID", item.FarmYieldHeaderID.ToString());
                    dic.Add("YieldDate", item.YieldDate.ToShortDateString());
                    dic.Add("PlantWeight", item.PlantWeight.ToString());
                    dic.Add("WeighBacks", item.WeighBacks.ToString());
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
        public HttpResponseMessage FarmYieldHeaders([FromBody] FarmYieldHeaderDTO cqDTO)
        {
            return FarmYieldHeaders(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage FarmYieldHeaderList([FromBody] FarmYieldHeaderDTO cqDTO)
        {
            return FarmYieldHeaders(Request, cqDTO);
        }
    }

}