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
using System.Net.Mail;



namespace SGApp.Controllers
{
    public class UtilitiesController: BaseApiController {
    

        [HttpPost]
        public HttpResponseMessage FarmsDDLByUserId([FromBody] UtilityDTO utilityDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var userId = ur.ValidateUser(utilityDto.Key, out key, ref companyId);
            if (userId > 0)
            {
                var sr = new FarmRepository();
                var data = sr.GetUserFarms(userId);
                var retVal = new GenericDTO();
                retVal.Key = key;
                var col = new Collection<Dictionary<string, string>>();
                foreach (var d in data.Select(farm =>
                                                new Dictionary<string, string> {
                                                    {"FarmId", farm.FarmId.ToString()},
                                                    {"FarmName", farm.FarmName.ToString()
                                                    }
                                                }
                                            )
                        )
                {
                    col.Add(d);
                }
                retVal.ReturnData = col;
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        [HttpPut]
        public HttpResponseMessage CompanyAddOrEdit([FromBody] CompanyDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0 && aur.IsInRole(userId, "SGAdmin"))
            {
                var company = new Company();
                var errors = ValidateDtoData(uDto, company);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.CompanyId, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewCompanyRecord(Request, uDto, key, companyId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingCompanyRecord(Request, uDto, NEUserId, key, companyId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewCompanyRecord(HttpRequestMessage request, CompanyDTO uDto, string key, int companyId, int userId)
        {
            var ur = new CompanyRepository();
            var user = new Company();

            
            var validationErrors = GetValidationErrors(ur, user, uDto, companyId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            user = ur.Save(user);
            uDto.Key = key;
            uDto.CompanyId = user.CompanyId.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.CompanyId
            }));
            return response;
        }

        private HttpResponseMessage ProcessExistingCompanyRecord(HttpRequestMessage request, CompanyDTO cqDto, int contactId, string key, int companyId, int userId)
        {
            var ur = new CompanyRepository();
            var user = new Company();
            user = ur.GetById(contactId);


            var validationErrors = GetValidationErrors(ur, user, cqDto, companyId, userId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            ur.Save(user);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(CompanyRepository pr, Company contact, CompanyDTO cqDto, int companyId, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        [HttpPost]
        public HttpResponseMessage EmailDailyReport()
        {
            var reportdate = DateTime.Now;
            var ptr = new ProductionTotalRepository();
            var dtr = new DepartmentTotalRepository();
            var wbr = new WeighBackRepository();
            var ar = new AbsenceRepository();
            var dr = new DownTimeRepository();
            var fsrr = new FilletScaleReadingRepository();

            var pts = ptr.GetByDate(reportdate);
            var dts = dtr.GetByDate(reportdate);
            var wbs = dtr.GetByDate(reportdate);
            var abs = ar.GetByDate(reportdate);
            var ds = dr.GetByDate(reportdate);
            var fsrs = fsrr.GetByDate(reportdate);

            string subject = "";
            string body = "";
            subject = "Harvest Select Daily Production Report";
            body = "Report Date:  " + reportdate.ToShortDateString() + "<br /><br />";
            body += "Fillet Scale Reading: " + fsrs.FilletScaleReading1.ToString() + "<br /><br />";
            body = body + "Name:<br />";
            body = body + "Comnpany Name:<br />";
            body = body + "Email:<br />";
            body = body + "Phone:<br />";
            body = body + "Message:<br />";

            SendMail("harper@stimulantgroup.com", subject, body);

                
            return Request.CreateResponse(HttpStatusCode.OK);

        }



        private bool SendMail(string emailto, string subject, string body)
        {
            try
            {
                MailMessage mm = new MailMessage();
                mm.From = new MailAddress("harper@stimulantgroup.com");
                string[] emails = emailto.Split(',');
                foreach (string addy in emails)
                {
                    mm.To.Add(new MailAddress(addy));
                }
                //(2) Assign the MailMessage's properties
                //MailAddress copy = new MailAddress("support@rcginc.net");
                //mm.CC.Add(copy);
                mm.Subject = subject;
                mm.Body = body;
                mm.IsBodyHtml = true;

                //(3) Create the SmtpClient object
                SmtpClient smtp = new SmtpClient();


                //(4) Send the MailMessage (will use the Web.config settings)
                smtp.Send(mm);
                return true;
                //lblContactSent.Visible = true;
            }
            catch (Exception ex)
            {
                return false;
                //tbComments.Text = ex.Message;
            }
        }
    }

}