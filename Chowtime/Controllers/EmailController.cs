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
    public class EmailController : BaseApiController
    {




        [HttpPut]
        public HttpResponseMessage EmailAddOrEdit([FromBody] EmailDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var EmailId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref EmailId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0 && aur.IsInRole(userId, "Admin"))
            {
                var Email = new Email();
                var errors = ValidateDtoData(uDto, Email);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.EmailID, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewEmailRecord(Request, uDto, key, EmailId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingEmailRecord(Request, uDto, NEUserId, key, EmailId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }


        [HttpPut]
        public HttpResponseMessage DeleteEmail([FromBody] EmailDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var EmailId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref EmailId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0 && aur.IsInRole(userId, "Admin"))
            {
                var Email = new Email();
                var errors = ValidateDtoData(uDto, Email);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.EmailID, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewEmailRecord(Request, uDto, key, EmailId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return DeleteEmailRecord(Request, uDto, NEUserId, key, EmailId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }
        private HttpResponseMessage ProcessNewEmailRecord(HttpRequestMessage request, EmailDTO uDto, string key, int EmailId, int userId)
        {
            var ur = new EmailRepository();
            var user = new Email();


            var validationErrors = GetValidationErrors(ur, user, uDto, EmailId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            user = ur.Save(user);
            uDto.Key = key;
            uDto.EmailID = user.EmailID.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.EmailID
            }));
            return response;
        }

        private HttpResponseMessage ProcessExistingEmailRecord(HttpRequestMessage request, EmailDTO cqDto, int contactId, string key, int EmailId, int userId)
        {
            var ur = new EmailRepository();
            var user = new Email();
            user = ur.GetById(contactId);


            var validationErrors = GetValidationErrors(ur, user, cqDto, EmailId, userId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            ur.Save(user);


            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }

        private HttpResponseMessage DeleteEmailRecord(HttpRequestMessage request, EmailDTO cqDto, int contactId, string key, int EmailId, int userId)
        {
            var ur = new EmailRepository();
            var user = new Email();
            user = ur.GetById(contactId);


            var validationErrors = GetValidationErrors(ur, user, cqDto, EmailId, userId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            ur.Delete(user);


            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(EmailRepository pr, Email contact, EmailDTO cqDto, int YieldID, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        internal HttpResponseMessage Emails(HttpRequestMessage request, EmailDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new EmailRepository();
                var u = new Email();

                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.EmailAddress).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();


                    dic.Add("EmailID", item.EmailID.ToString());
                    dic.Add("EmailAddress", item.EmailAddress);
                    dic.Add("ReceiveDailyReport", item.ReceiveDailyReport.ToString());
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
        public HttpResponseMessage Emails([FromBody] EmailDTO cqDTO)
        {
            return Emails(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage EmailList([FromBody] EmailDTO cqDTO)
        {
            return Emails(Request, cqDTO);
        }
    }

}