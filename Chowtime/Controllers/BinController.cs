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
    public class BinController : BaseApiController
    {
        internal HttpResponseMessage Bins(HttpRequestMessage request, BinDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var UserId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            AppUserRoleRepository aur1 = new AppUserRoleRepository();


            if (UserId > 0 && aur1.IsInRole(UserId, "User"))
            {
                var ur = new BinRepository();
                var u = new Bin();
                cqDTO.CompanyId = companyId.ToString();
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();

                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();

                    dic.Add("BinId", item.BinID.ToString());
                    dic.Add("BinName", item.BinName);
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
        public HttpResponseMessage Bins([FromBody] BinDTO cqDTO)
        {
            return Bins(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage BinList([FromBody] BinDTO cqDTO)
        {
            return Bins(Request, cqDTO);
        }

        [HttpPost]
        public HttpResponseMessage BinDetail([FromBody] BinDTO cqDTO)
        {
            return Bins(Request, cqDTO);
        }
        [HttpPut]
        public HttpResponseMessage BinAddOrEdit([FromBody] BinDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Admin"))
            {
                var Bin = new Bin();
                var errors = ValidateDtoData(uDto, Bin);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEBinId = 0;
                if (int.TryParse(uDto.BinID, out NEBinId))
                {
                    if (NEBinId == -1)
                    {
                        //  creating new Bin record   
                        return ProcessNewBinRecord(Request, uDto, key, companyId, UserId);
                    }
                    else
                    {
                        //  editing existing Bin record  
                        return ProcessExistingBinRecord(Request, uDto, NEBinId, key, companyId, UserId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }
        private HttpResponseMessage ProcessNewBinRecord(HttpRequestMessage request, BinDTO uDto, string key, int companyId, int UserId)
        {
            var ur = new BinRepository();
            var Bin = new Bin();
            
            var validationErrors = GetValidationErrors(ur, Bin, uDto, companyId, UserId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors... 
            //Bin.CompanyId = companyId;

            Bin = ur.Save(Bin);

            uDto.Key = key;
            uDto.BinID= Bin.BinID.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = Bin.BinID
            }));
            return response;
        }

        private HttpResponseMessage ProcessExistingBinRecord(HttpRequestMessage request, BinDTO cqDto, int contactId, string key, int companyId, int UserId)
        {
            var ur = new BinRepository();
            var Bin = new Bin();
            Bin = ur.GetById(contactId);
            //  is the Bin eligible to update the prospect?

            var validationErrors = GetValidationErrors(ur, Bin, cqDto, companyId, UserId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors...              
            ur.Save(Bin);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }

        private List<DbValidationError> GetValidationErrors(BinRepository pr, Bin contact, BinDTO cqDto, int companyId, int BinId)
        {
            contact.ProcessRecord(cqDto);

            return pr.Validate(contact);
        }
    }
}