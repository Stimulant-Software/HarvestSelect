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

        [HttpGet]
        public HttpResponseMessage EmailDailyReport()
        {
            var reportdate = DateTime.Now;
            reportdate = DateTime.Parse("2016-04-14");
            var ptr = new ProductionTotalRepository();
            var dtr = new DepartmentTotalRepository();
            var wbr = new WeighBackRepository();
            var ar = new AbsenceRepository();
            var dr = new DownTimeRepository();
            var fsrr = new FilletScaleReadingRepository();

            var pts = ptr.GetByDate(reportdate);
            var dts = dtr.GetByDate(reportdate);
            var wbs = wbr.GetByDate(reportdate);
            var abs = ar.GetByDate(reportdate);
            var dsl = dr.GetByDate(reportdate);
            var fsrs = fsrr.GetByDate(reportdate);

            decimal headedweighttotal = 0;
            decimal plweight = 0;
            decimal wbweight = 0;
            decimal avgyield = 100;
            foreach (ProductionTotal pt in pts)
            {
                if (pt.PlantWeight != null)
                {plweight = pt.PlantWeight.Value;}
                if (pt.WeighBacks != null)
                {wbweight = pt.WeighBacks.Value;}
                if (pt.AverageYield != null)
                {avgyield = pt.AverageYield.Value;}
                headedweighttotal += (plweight - wbweight) * avgyield / 100;
                plweight = 0;
                wbweight = 0;
                avgyield = 100;
            }
            decimal avgTotal = headedweighttotal * 100 / (pts.Sum(x => x.PlantWeight).Value - pts.Sum(x => x.WeighBacks).Value);
            string filletscale = fsrs == null ? "0" : fsrs.FilletScaleReading1.ToString();
            string subject = "";
            string body = "";
            body += "<html><head><style>table, td, th {border: 1px solid #ddd; text-align: left;}table {border-collapse: collapse; width: 100%;} th, td {padding: 5px;} tr:nth-child(2) {background-color: #f8f8f8;} th {background-color: #ddd;}</style></head><body>";
            subject = "Harvest Select Daily Production Report";
            body += "Report Date:  " + reportdate.ToShortDateString() + "<br /><br />";
            body += "Fillet Scale Reading: " + filletscale + "<br /><br />";
            body += "<b>Production Weights</b><br />";
            body += "<table><tr><th></th><th>Pond Weight</th><th>Plant Weight</th><th>WeighBacks</th><th>Yield %</th><th>Headed Yield</th></tr>";
            body += "<tr><td>TOTAL</td><td>" + pts.Sum(x => x.PlantWeight).Value.ToString() + "</td>";
            body += "<td>" + pts.Sum(x => x.PondWeight).Value.ToString() + "</td>";
            body += "<td>" + pts.Sum(x => x.WeighBacks).Value.ToString() + "</td>";
            body += "<td>" + avgTotal.ToString() + "</td>";
            body += "<td>" + headedweighttotal.ToString() + "</td></tr>";
            foreach (ProductionTotal pt in pts)
            {
                body += "<tr><td>" + pt.Pond.Farm.FarmName + " - " + pt.Pond.PondName + "</td><td>" + pt.PlantWeight.ToString() + "</td>";
                body += "<td>" + pt.PondWeight.ToString() + "</td>";
                body += "<td>" + pt.WeighBacks.ToString() + "</td>";
                body += "<td>" + pt.AverageYield.ToString() + "</td>"; 
                body += "<td>" + ((pt.PlantWeight.Value - pt.WeighBacks.Value) * pt.AverageYield / 100).ToString() + "</td></tr>";
            }
            body += "</table><br /><br />";

            body += "<b>Department Metrics</b><br />";
            body += "<table><tr><th></th><th>Absences</th><th>Finish Time</th><th>Shift Weight</th><th>Downtime</th></tr>";
            body += "<tr><td>TOTAL</td><td>" + dts.Sum(x => x.Absences).Value.ToString() + "</td>";
            body += "<td>---</td>";
            body += "<td>" + dts.Sum(x => x.ShiftWeight).Value.ToString() + "</td>";
            body += "<td>" + dts.Sum(x => x.DownTime).Value.ToString() + "</td></tr>";
            foreach (DepartmentTotal dt in dts)
            {
                body += "<tr><td>" + dt.Department.DepartmentName + "</td><td>" + dt.Absences.Value.ToString() + "</td>";
                body += "<td>" + dt.FinishTime.Value.ToShortTimeString() + "</td>";
                body += "<td>" + dt.ShiftWeight.Value.ToString() + "</td>";
                body += "<td>" + dt.DownTime.Value.ToString() + "</td></tr>";
            }
            body += "</table><br /><br />";

            body += "<b>WeighBack Details</b><br />";
            body += "<table><tr><th></th><th>Turtle/Trash</th><th>Shad/Carp/Bream</th><th>Live Disease</th><th>Dressed Disease</th><th>~~Backs</th><th>Red Fillet</th><th>Big Fish</th><th>DOAs</th></tr>";
            body += "<tr><td>TOTAL</td><td>" + wbs.Sum(x => x.Turtle).Value.ToString() + "</td>";
            body += "<td>" + wbs.Sum(x => x.Shad).Value.ToString() +  "</td>";
            body += "<td>" + wbs.Sum(x => x.LiveDisease).Value.ToString() + "</td>";
            body += "<td>" + wbs.Sum(x => x.DressedDisease).Value.ToString() + "</td>";
            body += "<td>" + wbs.Sum(x => x.Backs).Value.ToString() + "</td>";
            body += "<td>" + wbs.Sum(x => x.RedFillet).Value.ToString() + "</td>";
            body += "<td>" + wbs.Sum(x => x.BigFish).Value.ToString() + "</td>";
            body += "<td>" + wbs.Sum(x => x.DOAs).Value.ToString() + "</td></tr>";
            List<int> ponds = new List<int>();
            foreach (WeighBack wb in wbs)
            {
                if (!ponds.Contains(wb.PondID)){

                    body += "<tr><td>" + wb.Pond.Farm.FarmName + " - " + wb.Pond.PondName + "</td><td>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Turtle).Value.ToString() + "</td>";
                    body += "<td>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Shad).Value.ToString() + "</td>";
                    body += "<td>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.LiveDisease).Value.ToString() + "</td>";
                    body += "<td>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.DressedDisease).Value.ToString() + "</td>";
                    body += "<td>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Backs).Value.ToString() + "</td>";
                    body += "<td>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.RedFillet).Value.ToString() + "</td>";
                    body += "<td>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.BigFish).Value.ToString() + "</td>";
                    body += "<td>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.DOAs).Value.ToString() + "</td></tr>";

                }
                ponds.Add(wb.PondID);
            }
            body += "</table><br /><br />";


            body += "<b>Employee Absence Details</b><br />";
            body += "<table><tr><th></th><th>Reg Out</th><th>Reg Late</th><th>Reg Left Early</th><th>Temp Out</th><th>Temp Late</th><th>Temp Left Early</th><th>Inmate Out</th><th>Inmate Left Early</th><th>Vacation</th></tr>";
            body += "<tr><td>TOTAL</td><td>" + abs.Sum(x => x.RegEmpOut).Value.ToString() + "</td>";
            body += "<td>" + abs.Sum(x => x.RegEmpLate).Value.ToString() + "</td>";
            body += "<td>" + abs.Sum(x => x.RegEmpLeftEarly).Value.ToString() + "</td>";
            body += "<td>" + abs.Sum(x => x.TempEmpOut).Value.ToString() + "</td>";
            body += "<td>" + abs.Sum(x => x.TempEmpLate).Value.ToString() + "</td>";
            body += "<td>" + abs.Sum(x => x.TempEmpLeftEarly).Value.ToString() + "</td>";
            body += "<td>" + abs.Sum(x => x.InmateOut).Value.ToString() + "</td>";
            body += "<td>" + abs.Sum(x => x.InmateLeftEarly).Value.ToString() + "</td>";
            body += "<td>" + abs.Sum(x => x.EmployeesOnVacation).Value.ToString() + "</td></tr>";
            foreach (Absence ab in abs)
            {

                body += "<tr><td>" + ab.Department.DepartmentName + "</td><td>" + ab.RegEmpOut.Value.ToString() + "</td>";
                body += "<td>" + ab.RegEmpLate.Value.ToString() + "</td>";
                body += "<td>" + ab.RegEmpLeftEarly.Value.ToString() + "</td>";
                body += "<td>" + ab.TempEmpOut.Value.ToString() + "</td>";
                body += "<td>" + ab.TempEmpLate.Value.ToString() + "</td>";
                body += "<td>" + ab.TempEmpLeftEarly.Value.ToString() + "</td>";
                body += "<td>" + ab.InmateOut.Value.ToString() + "</td>";
                body += "<td>" + ab.InmateLeftEarly.Value.ToString() + "</td>";
                body += "<td>" + ab.EmployeesOnVacation.Value.ToString() + "</td></tr>";

            }
            body += "</table><br /><br />";

            body += "<b>Downtime Details</b><br />";
            body += "<table><tr><th></th><th>Type</th><th>Minutes</th><th>Note</th></tr>";
            body += "<tr><td>TOTAL</td><td>---</td>";
            body += "<td>" + dsl.Sum(x => x.Minutes).ToString() + "</td>";
            body += "<td>---</td></tr>";
            foreach (DownTime dt in dsl)
            {

                body += "<tr><td>" + dt.DownTimeType.Department.DepartmentName + "</td><td>" + dt.DownTimeType.DownTimeName + "</td>";
                body += "<td>" + dt.Minutes.ToString() + "</td>";
                body += "<td>" + dt.DownTimeNote + "</td></tr>";


            }
            body += "</table><br /><br />";

            body += "</table></body></html>";

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