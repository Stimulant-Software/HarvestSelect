﻿using SGApp.BusinessLogic.Application;
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
            body += "<style>table, td, th {border: 1px solid #ddd; text-align: left;}table {border-collapse: collapse; width: 100%;} th, td {padding: 5px;} tr:nth-child(2) {background-color: #f8f8f8;} th {background-color: #ddd;}</style>";
            subject = "Harvest Select Daily Production Report";
            body += "Report Date:  " + reportdate.ToShortDateString() + "<br /><br />";
            body += "Fillet Scale Reading: " + filletscale + "<br /><br />";
            body += "<b>Production Weights</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Pond Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Plant Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>WeighBacks</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Yield %</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Headed Yield</th></tr>";
            body += "<tr style='background-color: #f8f8f8;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.PlantWeight).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.PondWeight).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.WeighBacks).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + avgTotal.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + headedweighttotal.ToString() + "</td></tr>";
            foreach (ProductionTotal pt in pts)
            {
                decimal plantweight = pt.PlantWeight.HasValue ? pt.PlantWeight.Value : 0;
                decimal pondweight = pt.PondWeight.HasValue ? pt.PondWeight.Value : 0;
                decimal weighbacks = pt.WeighBacks.HasValue ? pt.WeighBacks.Value : 0;
                decimal averageyield = pt.AverageYield.HasValue ? pt.AverageYield.Value : 0;
                body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pt.Pond.Farm.FarmName + " - " + pt.Pond.PondName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + plantweight.ToString() + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pondweight.ToString() + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + weighbacks.ToString() + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + averageyield.ToString() + "</td>"; 
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + ((plantweight - weighbacks) * averageyield / 100).ToString() + "</td></tr>";
            }
            body += "</table><br /><br />";

            body += "<b>Department Metrics</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Absences</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Finish Time</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Shift Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Downtime</th></tr>";
            body += "<tr style='background-color: #f8f8f8;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dts.Sum(x => x.Absences).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>---</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dts.Sum(x => x.ShiftWeight).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dts.Sum(x => x.DownTime).Value.ToString() + "</td></tr>";
            
            foreach (DepartmentTotal dt in dts)
            {
                string finishtime = dt.FinishTime.HasValue ? dt.FinishTime.Value.ToShortTimeString() : "---";
                string shiftweight = dt.ShiftWeight.HasValue ? dt.ShiftWeight.Value.ToString() : "---";
                string downtime = dt.DownTime.HasValue ? dt.DownTime.Value.ToString() : "---";
                string absences = dt.Absences.HasValue ? dt.Absences.Value.ToString() : "---";
                body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.Department.DepartmentName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + absences + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + finishtime + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + shiftweight + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + downtime + "</td></tr>";
            }
            body += "</table><br /><br />";

            body += "<b>WeighBack Details</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Turtle/Trash</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Shad/Carp/Bream</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Live Disease</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Dressed Disease</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>~~Backs</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Red Fillet</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Big Fish</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>DOAs</th></tr>";
            body += "<tr style='background-color: #f8f8f8;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.Turtle).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.Shad).Value.ToString() +  "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.LiveDisease).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.DressedDisease).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.Backs).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.RedFillet).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.BigFish).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.DOAs).Value.ToString() + "</td></tr>";
            List<int> ponds = new List<int>();
            foreach (WeighBack wb in wbs)
            {
                if (!ponds.Contains(wb.PondID)){

                    body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wb.Pond.Farm.FarmName + " - " + wb.Pond.PondName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Turtle).Value.ToString() + "</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Shad).Value.ToString() + "</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.LiveDisease).Value.ToString() + "</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.DressedDisease).Value.ToString() + "</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Backs).Value.ToString() + "</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.RedFillet).Value.ToString() + "</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.BigFish).Value.ToString() + "</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.DOAs).Value.ToString() + "</td></tr>";

                }
                ponds.Add(wb.PondID);
            }
            body += "</table><br /><br />";


            body += "<b>Employee Absence Details</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Late</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Late</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Inmate Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Inmate Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Vacation</th></tr>";
            body += "<tr style='background-color: #f8f8f8;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.RegEmpOut).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.RegEmpLate).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.RegEmpLeftEarly).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.TempEmpOut).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.TempEmpLate).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.TempEmpLeftEarly).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.InmateOut).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.InmateLeftEarly).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.EmployeesOnVacation).Value.ToString() + "</td></tr>";
            foreach (Absence ab in abs)
            {
                string RegEmpOut = ab.RegEmpOut.HasValue ? ab.RegEmpOut.Value.ToString() : "---";
                string RegEmpLate = ab.RegEmpLate.HasValue ? ab.RegEmpLate.Value.ToString() : "---";
                string RegEmpLeftEarly = ab.RegEmpLeftEarly.HasValue ? ab.RegEmpLeftEarly.Value.ToString() : "---";
                string TempEmpOut = ab.TempEmpOut.HasValue ? ab.TempEmpOut.Value.ToString() : "---";
                string TempEmpLate = ab.TempEmpLate.HasValue ? ab.TempEmpLate.Value.ToString() : "---";
                string TempEmpLeftEarly = ab.TempEmpLeftEarly.HasValue ? ab.TempEmpLeftEarly.Value.ToString() : "---";
                string InmateOut = ab.InmateOut.HasValue ? ab.InmateOut.Value.ToString() : "---";
                string InmateLeftEarly = ab.InmateLeftEarly.HasValue ? ab.InmateLeftEarly.Value.ToString() : "---";
                string EmployeesOnVacation = ab.EmployeesOnVacation.HasValue ? ab.EmployeesOnVacation.Value.ToString() : "---";
                body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + ab.Department.DepartmentName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + RegEmpOut + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + RegEmpLate + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + RegEmpLeftEarly + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + TempEmpOut + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + TempEmpLate + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + TempEmpLeftEarly + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + InmateOut + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + InmateLeftEarly + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + EmployeesOnVacation + "</td></tr>";

            }
            body += "</table><br /><br />";

            body += "<b>Downtime Details</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Type</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Minutes</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Note</th></tr>";
            body += "<tr style='background-color: #f8f8f8;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>---</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dsl.Sum(x => x.Minutes).ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>---</td></tr>";
            foreach (DownTime dt in dsl)
            {

                string DownTimeNote = dt.DownTimeNote != null ? dt.DownTimeNote : "---";
                body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.DownTimeType.Department.DepartmentName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.DownTimeType.DownTimeName + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.Minutes.ToString() + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.DownTimeNote + "</td></tr>";


            }
            body += "</table><br /><br />";

            body += "</table>";

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