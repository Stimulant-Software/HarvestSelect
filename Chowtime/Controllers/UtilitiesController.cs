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
using System.Web;
using Newtonsoft.Json;
using System.Web.Script.Serialization;



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
        public HttpResponseMessage GetBOLReport()
        {

            //Update Shift Weights
            List<BOL> BOLResults = new List<BOL>();
            //SGApp.DTOs.GenericDTO dto = new GenericDTO();
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var dic = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            var sDate = DateTime.ParseExact(dic.First().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            var eDate = DateTime.ParseExact(dic.Last().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            dto.StartDate = sDate;
            dto.EndDate = eDate;
            //dto.StartDate = DateTime.Now.AddDays(-1).Date;
            //dto.EndDate = DateTime.Now.Date;
            //dto.StartDate = DateTime.Now.AddDays(1).Date;
            //dto.EndDate = DateTime.Now.AddDays(2).Date;
            var client = new HttpClient
            {
                //BaseAddress = new Uri("http://323-booth-svr2:3030/")
                //BaseAddress = new Uri("http://64.139.95.243:7846/")
                BaseAddress = new Uri("http://64.139.95.243:7846/")
                //BaseAddress = new Uri(baseAddress)                
            };
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetBOLData", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                BOL[] BOLResultsArray = json_serializer.Deserialize<BOL[]>(response.Content.ReadAsStringAsync().Result);
                BOLResults = BOLResultsArray.ToList();
                
            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            
            //var IQFweight = shiftResults.Where(x => iqfstations.Contains(x.Station)).Sum(x => decimal.Parse(x.Nominal)).ToString();

            string subject = "";
            string body = "";
            body += "<style>table, td, th {border: 1px solid #ddd; text-align: left;}table {border-collapse: collapse; width: 100%;} th, td {padding: 5px;} tr:nth-child(2) {background-color: #f8f8f8;} th {background-color: #ddd;}</style>";
            subject = "Harvest Select Daily Production Report";
            body += "Report Date:  " + sDate.ToShortDateString() + "<br /><br />";
            body += "Fillet Scale Reading: " + "Stuff Here" + "<br /><br />";
            body += "<b>Live Fish Receiving</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Pond Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Plant Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Difference</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>WeighBacks</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Net Live Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Yield %</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Headed Yield</th></tr>";
            body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td>";
            //<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.PondWeight).Value.ToString("#") + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.PlantWeight).Value.ToString("#") + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (pts.Sum(x => x.PondWeight).Value - pts.Sum(x => x.PlantWeight).Value).ToString("#") + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.WeighBacks).Value.ToString("#") + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (pts.Sum(x => x.PlantWeight).Value - pts.Sum(x => x.WeighBacks).Value).ToString("#") + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + avgTotal.ToString("#.####") + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + headedweighttotal.ToString("#") + "</td></tr>";
            //foreach (ProductionTotal pt in pts)
            //{
            //    decimal plantweight = pt.PlantWeight.HasValue ? pt.PlantWeight.Value : 0;
            //    decimal pondweight = pt.PondWeight.HasValue ? pt.PondWeight.Value : 0;
            //    decimal weighbacks = pt.WeighBacks.HasValue ? pt.WeighBacks.Value : 0;
            //    decimal averageyield = pt.AverageYield.HasValue ? pt.AverageYield.Value : 0;
            //    body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pt.Pond.Farm.InnovaName + " - " + pt.Pond.PondName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pondweight.ToString("#") + "</td>";
            //    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + plantweight.ToString("#") + "</td>";
            //    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (pondweight - plantweight).ToString("#") + "</td>";
            //    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + weighbacks.ToString("#") + "</td>";
            //    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (plantweight - weighbacks).ToString("#") + "</td>";
            //    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + averageyield.ToString("#.####") + "</td>";
            //    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + ((plantweight - weighbacks) * averageyield / 100).ToString("#") + "</td></tr>";
            //}
            //body += "</table><br /><br />";

            

            body += "</table>";
            ////, danielw@harvestselect.com
            //string elist = "";
            //EmailRepository er = new EmailRepository();
            //List<Email> emails = er.GetEmails();
            //foreach (Email em in emails)
            //{
            //    elist += em.EmailAddress + ", ";
            //}
            //elist = elist.Substring(0, elist.Length - 2);
            ////SendMail("harper@stimulantgroup.com", subject, body);// danielw@harvestselect.com, RobertL@HarvestSelect.com, Alice@HarvestSelect.com, Betsya@HarvestSelect.com, Bobby@HarvestSelect.com, Brenda@harvestselect.com, ChrisH@HarvestSelect.com, cory@harvestselect.com, daniel@harvestselect.com, Debi@HarvestSelect.com, jimbo@harvestselect.com, johnny@harvestselect.com, leec@harvestselect.com, lee@harvestselect.com, Mark@HarvestSelect.com, Michael@harvestselect.com, Mike@HarvestSelect.com, Randy@HarvestSelect.com, reed@harvestselect.com, rhonda@harvestselect.com, Ryan@HarvestSelect.com, Shirley@HarvestSelect.com, tammy@harvestselect.com, tom@harvestselect.com, trey@harvestselect.com, sam@harvestselect.com", subject, body);
            //SendMail(elist, subject, body);

            return Request.CreateResponse(HttpStatusCode.OK, BOLResults);

        }
        [HttpGet]
        public HttpResponseMessage EmailDailyReport()
        {

            //Update Shift Weights
            List<ShiftWeight> shiftResults = new List<ShiftWeight>();
            //SGApp.DTOs.GenericDTO dto = new GenericDTO();
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var dic = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            var sDate = DateTime.ParseExact(dic.First().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            var eDate = DateTime.ParseExact(dic.Last().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            dto.StartDate = sDate;
            dto.EndDate = eDate;
            //dto.StartDate = DateTime.Now.AddDays(-1).Date;
            //dto.EndDate = DateTime.Now.Date;
            //dto.StartDate = DateTime.Now.AddDays(1).Date;
            //dto.EndDate = DateTime.Now.AddDays(2).Date;
            var client = new HttpClient
            {
                //BaseAddress = new Uri("http://323-booth-svr2:3030/")
                //BaseAddress = new Uri("http://64.139.95.243:7846/")
                BaseAddress = new Uri("http://64.139.95.243:7846/")
                //BaseAddress = new Uri(baseAddress)                
            };
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetDailyProductionTotal", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                //Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(response.Content.ReadAsStringAsync().Result); // new List<Sampling>();
                //Sampling[] samplingResultsArray = response.Content.ReadAsAsync<Sampling[]>().Result;
                //samplingResults = samplingResultsArray.ToList();
                //JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                //Object[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(Constants.testdata);
                //string teststuff = "[{\"station\":10,\"nominal\":34038.25,\"weight\":35469.6},{\"station\":12,\"nominal\":7950.0,\"weight\":8062.02},{\"station\":13,\"nominal\":3165.0,\"weight\":3213.56},{\"station\":14,\"nominal\":3920.0,\"weight\":3990.14},{\"station\":15,\"nominal\":8342.0,\"weight\":8987.8},{\"station\":16,\"nominal\":10580.0,\"weight\":10862.35}]";
                //ShiftWeight[] samplingResultsArray = json_serializer.Deserialize<ShiftWeight[]>(Constants.testprod);
                ShiftWeight[] samplingResultsArray = json_serializer.Deserialize<ShiftWeight[]>(response.Content.ReadAsStringAsync().Result);
                shiftResults = samplingResultsArray.ToList();
                //shiftResults = shiftResults.GroupBy(x => x.farmPond).Select(group => group.First()).ToList();
                //var result = response.Content.ReadAsStringAsync().Result;

                //return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            List<string> iqfstations = new List<string>();
            iqfstations.Add("9");
            iqfstations.Add("10");
            var IQFweight = shiftResults.Where(x => iqfstations.Contains(x.Station)).Sum(x => decimal.Parse(x.Nominal)).ToString();
            var BaggerWeightRecord = shiftResults.Where(x => x.Station == "7").FirstOrDefault();
            var BaggerWeight = BaggerWeightRecord != null ? BaggerWeightRecord.Nominal : "0";
            List<string> stations = new List<string>();
            stations.Add("8");
            stations.Add("2");
            stations.Add("4");
            stations.Add("3");
            stations.Add("866");
            var FreshWeight = shiftResults.Where(x => stations.Contains(x.Station)).Sum(x => decimal.Parse(x.Nominal)).ToString();
            

            var reportdate = DateTime.Now;
            //reportdate = DateTime.Parse(DateTime.Now.AddDays(-1).ToShortDateString());
            reportdate = dto.StartDate;
            //dto.StartDate = DateTime.Now.AddDays(-1).Date;
            //reportdate = DateTime.Parse(DateTime.Now.AddDays(11).ToShortDateString());
            var ptr = new ProductionTotalRepository();
            var dtr = new DepartmentTotalRepository();
            var wbr = new WeighBackRepository();
            var ar = new AbsenceRepository();
            var dr = new DownTimeRepository();
            var fsrr = new FilletScaleReadingRepository();


            var iqfw = dtr.GetByDateAndDepartment(reportdate, 5);
            if (iqfw != null && IQFweight != null)
            {
                iqfw.ShiftWeight = decimal.Parse(IQFweight);
                dtr.Save(iqfw);
            }
            else
            {
                iqfw = new DepartmentTotal();
                if (IQFweight != null)
                {
                    iqfw.ShiftWeight = decimal.Parse(IQFweight);
                }
                iqfw.DepartmentID = 5;
                iqfw.DTDate = reportdate;
                dtr.Save(iqfw);

            }
            var freshw = dtr.GetByDateAndDepartment(reportdate, 4);
            if (freshw != null && FreshWeight != null)
            {
                freshw.ShiftWeight = decimal.Parse(FreshWeight);
                dtr.Save(freshw);
            }
            else
            {
                freshw = new DepartmentTotal();
                freshw.DepartmentID = 4;
                freshw.DTDate = reportdate;
                if (FreshWeight != null)
                {
                    freshw.ShiftWeight = decimal.Parse(FreshWeight);
                }

                dtr.Save(freshw);
            }
            var bagw = dtr.GetByDateAndDepartment(reportdate, 6);
            if (bagw != null && BaggerWeight != null)
            {
                bagw.ShiftWeight = decimal.Parse(BaggerWeight);
                dtr.Save(bagw);
            }
            else
            {
                bagw = new DepartmentTotal();
                bagw.DepartmentID = 6;
                bagw.DTDate = reportdate;
                if (BaggerWeight != null)
                {
                    bagw.ShiftWeight = decimal.Parse(BaggerWeight);
                }

                dtr.Save(bagw);
            }

            dtr.SaveRepoChanges();
            
            List<Sampling> samplingResults = new List<Sampling>();

            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetKeithsData", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                //Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(response.Content.ReadAsStringAsync().Result); // new List<Sampling>();
                //Sampling[] samplingResultsArray = response.Content.ReadAsAsync<Sampling[]>().Result;
                //samplingResults = samplingResultsArray.ToList();
                //JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                //Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(Constants.testdata);
                Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(response.Content.ReadAsStringAsync().Result);
                samplingResults = samplingResultsArray.ToList();
                var samplingResultsData = samplingResults.GroupBy(x => new { x.farm, x.pond, x.farmPond, x.rangeName })
                    .Select(group => new { Key = group.Key, Weight = group.Sum(s => decimal.Parse(s.weight)), Count = group.Count() }).ToList();
                //var result = response.Content.ReadAsStringAsync().Result;

                //return Request.CreateResponse(HttpStatusCode.OK, result);
                List<Sampling> samplingReport = new List<Sampling>(samplingResultsData.Capacity);
                foreach (var rec in samplingResultsData)
                {
                    Sampling fee2 = new Sampling();
                    fee2.farm = rec.Key.farm;
                    fee2.pond = rec.Key.pond;
                    fee2.farmPond = rec.Key.farmPond;
                    fee2.rangeName = rec.Key.rangeName;
                    fee2.weight = rec.Weight.ToString();
                    fee2.count = rec.Count.ToString();

                    samplingReport.Add(fee2);
                }
                samplingResults = samplingReport;
            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }

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
            decimal notzero = pts.Sum(x => x.PlantWeight).Value - pts.Sum(x => x.WeighBacks).Value;
            decimal avgTotal = 1;
            if (notzero == 0)
            {
                avgTotal = 0;
            }
            else
            {
                avgTotal = headedweighttotal * 100 / (pts.Sum(x => x.PlantWeight).Value - pts.Sum(x => x.WeighBacks).Value);
            }
            //decimal avgTotal = headedweighttotal * 100 / (pts.Sum(x => x.PlantWeight).Value - pts.Sum(x => x.WeighBacks).Value);
            string filletscale = fsrs == null ? "0" : fsrs.FilletScaleReading1.ToString();
            string subject = "";
            string body = "";
            body += "<style>table, td, th {border: 1px solid #ddd; text-align: left;}table {border-collapse: collapse; width: 100%;} th, td {padding: 5px;} tr:nth-child(2) {background-color: #f8f8f8;} th {background-color: #ddd;}</style>";
            subject = "Harvest Select Daily Production Report";
            body += "Report Date:  " + reportdate.ToShortDateString() + "<br /><br />";
            body += "Fillet Scale Reading: " + filletscale + "<br /><br />";
            body += "<b>Live Fish Receiving</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Pond Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Plant Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Difference</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>WeighBacks</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Net Live Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Yield %</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Headed Yield</th></tr>";
            body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.PondWeight).Value.ToString("#") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.PlantWeight).Value.ToString("#") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (pts.Sum(x => x.PondWeight).Value - pts.Sum(x => x.PlantWeight).Value).ToString("#") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.WeighBacks).Value.ToString("#") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (pts.Sum(x => x.PlantWeight).Value - pts.Sum(x => x.WeighBacks).Value).ToString("#") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + avgTotal.ToString("#.####") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + headedweighttotal.ToString("#") + "</td></tr>";
            foreach (ProductionTotal pt in pts)
            {
                decimal plantweight = pt.PlantWeight.HasValue ? pt.PlantWeight.Value : 0;
                decimal pondweight = pt.PondWeight.HasValue ? pt.PondWeight.Value : 0;
                decimal weighbacks = pt.WeighBacks.HasValue ? pt.WeighBacks.Value : 0;
                decimal averageyield = pt.AverageYield.HasValue ? pt.AverageYield.Value : 0;
                body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pt.Pond.Farm.InnovaName + " - " + pt.Pond.PondName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pondweight.ToString("#") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + plantweight.ToString("#") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (pondweight - plantweight).ToString("#") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + weighbacks.ToString("#") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (plantweight - weighbacks).ToString("#") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + averageyield.ToString("#.####") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + ((plantweight - weighbacks) * averageyield / 100).ToString("#") + "</td></tr>";
            }
            body += "</table><br /><br />";

            body += "<b>Samplings</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Farm</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Pond</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Fish Size</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Count</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>% of Count</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Weight (lbs)</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>% of Weight</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Avg Weight (lbs)</th>";
            body += "</tr>";
            List<Sampling> sresultsRanges = new List<Sampling>();
            List<Sampling> sresultsPonds = new List<Sampling>();
            List<Sampling> sresultsFarms = new List<Sampling>();
            sresultsRanges = samplingResults.GroupBy(x => x.rangeName).Select(group => group.First()).ToList();            
            sresultsFarms = samplingResults.GroupBy(x => x.farm).Select(group => group.First()).ToList();
            sresultsPonds = samplingResults.GroupBy(x => x.pond).Select(group => group.First()).ToList();
            var totalScount = samplingResults.Sum(x => decimal.Parse(x.count));
            var totalSweight = samplingResults.Sum(x => decimal.Parse(x.weight));
            var totalSaverage = totalScount == 0 ? 0 : (totalSweight / totalScount);
            body += "<tr style='background-color: #A1D490;'>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>All Farms</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalScount) + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalSweight) + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalSaverage) + "</td>";
            body += "</tr>";
            foreach (Sampling sam3 in sresultsRanges)
            {
                body += "<tr>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + sam3.rangeName + "</td>";
                var thispondScount = samplingResults.Where(x => x.rangeName == sam3.rangeName).Sum(x => decimal.Parse(x.count));
                var thispondScountpercent = totalScount == 0 ? 0 : (thispondScount / totalScount) * 100;
                var thispondSweight = samplingResults.Where(x => x.rangeName == sam3.rangeName).Sum(x => decimal.Parse(x.weight));
                var thispondSweightpercent = totalSweight == 0 ? 0 : (thispondSweight / totalSweight) * 100;
                var thisSaverage = thispondScount == 0 ? 0 : thispondSweight / thispondScount;
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thispondScount) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}%", thispondScountpercent) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thispondSweight) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}%", thispondSweightpercent) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thisSaverage) + "</td>";
                body += "</tr>";
            }

            foreach (Sampling sam in sresultsFarms)
            {
                var totalfarmcount = samplingResults.Where(x => x.farm == sam.farm).Sum(x => decimal.Parse(x.count));
                var totalfarmweight = samplingResults.Where(x => x.farm == sam.farm).Sum(x => decimal.Parse(x.weight));
                var totalfarmaverage = totalfarmcount == 0 ? 0 : (totalfarmweight / totalfarmcount);
                body += "<tr style='background-color: #A1D490;'>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + sam.farm + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>Farm Total</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalfarmcount) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalfarmweight) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalfarmaverage) + "</td>";
                body += "</tr>";
                
                foreach (Sampling sam1 in sresultsPonds.Where(x => x.farm == sam.farm))
                {
                    bool pNameLabel = true;
                    var totalpondcount = samplingResults.Where(x => x.pond == sam1.pond && x.farm == sam.farm).Sum(x => decimal.Parse(x.count));
                    var totalpondweight = samplingResults.Where(x => x.pond == sam1.pond && x.farm == sam.farm).Sum(x => decimal.Parse(x.weight));
                    var totalaverage = totalpondcount == 0 ? 0 : (totalpondweight / totalpondcount);
                    body += "<tr style='background-color: #CED490;'>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + sam1.pond + "</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>Pond Total</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalpondcount) + "</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalpondweight) + "</td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalaverage) + "</td>";
                    body += "</tr>";
                    foreach (Sampling sam2 in sresultsRanges)
                    {
                        body += "<tr>";
                        
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                                            
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                        
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + sam2.rangeName + "</td>";

                        var thispondcount = samplingResults.Where(x => x.pond == sam1.pond && x.rangeName == sam2.rangeName && x.farm == sam.farm).Sum(x => decimal.Parse(x.count));
                        var thispondcountpercent = totalpondcount == 0 ? 0 : (thispondcount / totalpondcount) * 100;
                        var thispondweight = samplingResults.Where(x => x.pond == sam1.pond && x.rangeName == sam2.rangeName && x.farm == sam.farm).Sum(x => decimal.Parse(x.weight));
                        var thispondweightpercent = totalpondweight == 0 ? 0 : (thispondweight / totalpondweight) * 100;
                        var thisaverage = thispondcount == 0 ? 0 : thispondweight / thispondcount;
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thispondcount) + "</td>";
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}%", thispondcountpercent) + "</td>";
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thispondweight) + "</td>";
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}%", thispondweightpercent) + "</td>";
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thisaverage) + "</td>";
                        body += "</tr>";
                    }
                    
                }

            }
            
            //body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
            //foreach (Sampling sam in sresultsRanges)
            //{
            //    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>Weight</td>";
            //    body += "<td style='border: 1px solid #ddd; border-right: 2px solid #222; text-align:left; padding: 5px;'>Count</td>";
            //}
            //body += "</tr>";
            //body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL (Weight | Count)</td>";
            //foreach (Sampling sam in sresultsRanges)
            //{
            //    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + samplingResults.Where(x => x.rangeName == sam.rangeName).Sum(x => decimal.Parse(x.weight)).ToString() + "</td>";
            //    body += "<td style='border: 1px solid #ddd; border-right: 2px solid #222; text-align:left; padding: 5px;'>" + samplingResults.Where(x => x.rangeName == sam.rangeName).Sum(x => decimal.Parse(x.count)).ToString() + "</td>";
            //}
            //body += "</tr>";
            
            //foreach (Sampling sam1 in sresultsPonds)
            //{
            //    body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + sam1.farmPond + "</td>";
            //    foreach (Sampling sam in sresultsRanges)
            //    {
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + samplingResults.Where(x => x.rangeName == sam.rangeName && x.farmPond == sam1.farmPond).Select(x => x.weight).FirstOrDefault() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; border-right: 2px solid #222; text-align:left; padding: 5px;'>" + samplingResults.Where(x => x.rangeName == sam.rangeName && x.farmPond == sam1.farmPond).Select(x => x.count).FirstOrDefault() + "</td>";
            //    }
            //    body += "</tr>";
            //}
            body += "</table><br /><br />";

            body += "<b>Production By Department</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Absences</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Finish Time</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Production Total</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Downtime</th></tr>";
            body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dts.Sum(x => x.Absences).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>---</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dts.Sum(x => x.ShiftWeight).Value.ToString() + " lbs</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dts.Sum(x => x.DownTime).Value.ToString() + "</td></tr>";
            
            foreach (DepartmentTotal dt in dts)
            {
                string finishtime = dt.FinishTime.HasValue ? dt.FinishTime.Value.ToShortTimeString() : "---";
                string shiftweight = dt.ShiftWeight.HasValue ? dt.ShiftWeight.Value.ToString() : "---";
                if (dt.DepartmentID == 3)
                {
                    shiftweight = filletscale;
                }
                string downtime = dt.DownTime.HasValue ? dt.DownTime.Value.ToString() : "---";
                string absences = dt.Absences.HasValue ? dt.Absences.Value.ToString() : "---";
                body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.Department.DepartmentName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + absences + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + finishtime + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + shiftweight + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + downtime + "</td></tr>";
            }
            body += "</table><br /><br />";

            //body += "<b>WeighBack Details</b><br />";
            //body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Turtle/Trash</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Shad/Carp/Bream</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Live Disease</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Dressed Disease</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>~~Backs</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Red Fillet</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Big Fish</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>DOAs</th></tr>";
            //body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.Turtle).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.Shad).Value.ToString() +  "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.LiveDisease).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.DressedDisease).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.Backs).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.RedFillet).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.BigFish).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.DOAs).Value.ToString() + "</td></tr>";
            //List<int> ponds = new List<int>();
            //foreach (WeighBack wb in wbs)
            //{
            //    if (!ponds.Contains(wb.PondID)){

            //        body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wb.Pond.Farm.FarmName + " - " + wb.Pond.PondName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Turtle).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Shad).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.LiveDisease).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.DressedDisease).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Backs).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.RedFillet).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.BigFish).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.DOAs).Value.ToString() + "</td></tr>";

            //    }
            //    ponds.Add(wb.PondID);
            //}
            //body += "</table><br /><br />";


            body += "<b>Employee Absence Details</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Late</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Late</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Inmate Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Inmate Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Vacation</th></tr>";
            body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.RegEmpOut).Value.ToString() + "</td>";
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
            body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>---</td>";
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
            //, danielw@harvestselect.com
            string elist = "";
            EmailRepository er = new EmailRepository();
            List<Email> emails = er.GetEmails();
            foreach (Email em in emails)
            {
                elist += em.EmailAddress + ", ";
            }
            elist = elist.Substring(0, elist.Length - 2);
            //SendMail("harper@stimulantgroup.com", subject, body);// danielw@harvestselect.com, RobertL@HarvestSelect.com, Alice@HarvestSelect.com, Betsya@HarvestSelect.com, Bobby@HarvestSelect.com, Brenda@harvestselect.com, ChrisH@HarvestSelect.com, cory@harvestselect.com, daniel@harvestselect.com, Debi@HarvestSelect.com, jimbo@harvestselect.com, johnny@harvestselect.com, leec@harvestselect.com, lee@harvestselect.com, Mark@HarvestSelect.com, Michael@harvestselect.com, Mike@HarvestSelect.com, Randy@HarvestSelect.com, reed@harvestselect.com, rhonda@harvestselect.com, Ryan@HarvestSelect.com, Shirley@HarvestSelect.com, tammy@harvestselect.com, tom@harvestselect.com, trey@harvestselect.com, sam@harvestselect.com", subject, body);
            SendMail(elist, subject, body);
                
            return Request.CreateResponse(HttpStatusCode.OK);

        }

        [HttpGet]
        public HttpResponseMessage EmailWeeklyReport()
        {

            //Update Shift Weights
            List<ShiftWeight> shiftResults = new List<ShiftWeight>();
            //SGApp.DTOs.GenericDTO dto = new GenericDTO();
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var dic = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            var sDate = DateTime.ParseExact(dic.First().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            var eDate = DateTime.ParseExact(dic.Last().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            dto.StartDate = sDate;
            dto.EndDate = eDate;
            //dto.StartDate = DateTime.Now.AddDays(-1).Date;
            //dto.EndDate = DateTime.Now.Date;
            //dto.StartDate = DateTime.Now.AddDays(1).Date;
            //dto.EndDate = DateTime.Now.AddDays(2).Date;
            var client = new HttpClient
            {
                //BaseAddress = new Uri("http://323-booth-svr2:3030/")
                //BaseAddress = new Uri("http://64.139.95.243:7846/")
                BaseAddress = new Uri("http://64.139.95.243:7846/")
                //BaseAddress = new Uri(baseAddress)                
            };
            //try
            //{
            //    var response = client.PostAsJsonAsync("api/Remote/GetDailyProductionTotal", dto).Result;
            //    response.EnsureSuccessStatusCode();
            //    JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            //    //Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(response.Content.ReadAsStringAsync().Result); // new List<Sampling>();
            //    //Sampling[] samplingResultsArray = response.Content.ReadAsAsync<Sampling[]>().Result;
            //    //samplingResults = samplingResultsArray.ToList();
            //    //JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            //    //Object[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(Constants.testdata);
            //    //string teststuff = "[{\"station\":10,\"nominal\":34038.25,\"weight\":35469.6},{\"station\":12,\"nominal\":7950.0,\"weight\":8062.02},{\"station\":13,\"nominal\":3165.0,\"weight\":3213.56},{\"station\":14,\"nominal\":3920.0,\"weight\":3990.14},{\"station\":15,\"nominal\":8342.0,\"weight\":8987.8},{\"station\":16,\"nominal\":10580.0,\"weight\":10862.35}]";
            //    //ShiftWeight[] samplingResultsArray = json_serializer.Deserialize<ShiftWeight[]>(Constants.testprod);
            //    ShiftWeight[] samplingResultsArray = json_serializer.Deserialize<ShiftWeight[]>(response.Content.ReadAsStringAsync().Result);
            //    shiftResults = samplingResultsArray.ToList();
            //    //shiftResults = shiftResults.GroupBy(x => x.farmPond).Select(group => group.First()).ToList();
            //    //var result = response.Content.ReadAsStringAsync().Result;

            //    //return Request.CreateResponse(HttpStatusCode.OK, result);
            //}
            //catch (Exception e)
            //{
            //    throw new HttpException("Error occurred: " + e.Message);
            //}
            //List<string> iqfstations = new List<string>();
            //iqfstations.Add("9");
            //iqfstations.Add("10");
            //var IQFweight = shiftResults.Where(x => iqfstations.Contains(x.Station)).Sum(x => decimal.Parse(x.Nominal)).ToString();
            //var BaggerWeightRecord = shiftResults.Where(x => x.Station == "7").FirstOrDefault();
            //var BaggerWeight = BaggerWeightRecord != null ? BaggerWeightRecord.Nominal : "0";
            //List<string> stations = new List<string>();
            //stations.Add("8");
            //stations.Add("2");
            //stations.Add("4");
            //stations.Add("3");
            //stations.Add("866");
            //var FreshWeight = shiftResults.Where(x => stations.Contains(x.Station)).Sum(x => decimal.Parse(x.Nominal)).ToString();


            var reportdate = DateTime.Now;
            //reportdate = DateTime.Parse(DateTime.Now.AddDays(-1).ToShortDateString());
            reportdate = dto.StartDate;
            //dto.StartDate = DateTime.Now.AddDays(-1).Date;
            //reportdate = DateTime.Parse(DateTime.Now.AddDays(11).ToShortDateString());
            var ptr = new ProductionTotalRepository();
            var dtr = new DepartmentTotalRepository();
            var wbr = new WeighBackRepository();
            var ar = new AbsenceRepository();
            var dr = new DownTimeRepository();
            var fsrr = new FilletScaleReadingRepository();


            //var iqfw = dtr.GetByDateAndDepartment(reportdate, 5);
            //if (iqfw != null && IQFweight != null)
            //{
            //    iqfw.ShiftWeight = decimal.Parse(IQFweight);
            //    dtr.Save(iqfw);
            //}
            //else
            //{
            //    iqfw = new DepartmentTotal();
            //    if (IQFweight != null)
            //    {
            //        iqfw.ShiftWeight = decimal.Parse(IQFweight);
            //    }
            //    iqfw.DepartmentID = 5;
            //    iqfw.DTDate = reportdate;
            //    dtr.Save(iqfw);

            //}
            //var freshw = dtr.GetByDateAndDepartment(reportdate, 4);
            //if (freshw != null && FreshWeight != null)
            //{
            //    freshw.ShiftWeight = decimal.Parse(FreshWeight);
            //    dtr.Save(freshw);
            //}
            //else
            //{
            //    freshw = new DepartmentTotal();
            //    freshw.DepartmentID = 4;
            //    freshw.DTDate = reportdate;
            //    if (FreshWeight != null)
            //    {
            //        freshw.ShiftWeight = decimal.Parse(FreshWeight);
            //    }

            //    dtr.Save(freshw);
            //}
            //var bagw = dtr.GetByDateAndDepartment(reportdate, 6);
            //if (bagw != null && BaggerWeight != null)
            //{
            //    bagw.ShiftWeight = decimal.Parse(BaggerWeight);
            //    dtr.Save(bagw);
            //}
            //else
            //{
            //    bagw = new DepartmentTotal();
            //    bagw.DepartmentID = 6;
            //    bagw.DTDate = reportdate;
            //    if (BaggerWeight != null)
            //    {
            //        bagw.ShiftWeight = decimal.Parse(BaggerWeight);
            //    }

            //    dtr.Save(bagw);
            //}
            //dtr.SaveRepoChanges();
            List<Sampling> samplingResults = new List<Sampling>();

            try
            {
                dto.EndDate = dto.EndDate.AddDays(1);
                var response = client.PostAsJsonAsync("api/Remote/GetKeithsData", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                //Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(response.Content.ReadAsStringAsync().Result); // new List<Sampling>();
                //Sampling[] samplingResultsArray = response.Content.ReadAsAsync<Sampling[]>().Result;
                //samplingResults = samplingResultsArray.ToList();
                //JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                //Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(Constants.testdata);
                Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(response.Content.ReadAsStringAsync().Result);
                samplingResults = samplingResultsArray.ToList();
                var samplingResultsData = samplingResults.GroupBy(x => new { x.farm, x.pond, x.farmPond, x.rangeName })
                    .Select(group => new { Key = group.Key, Weight = group.Sum(s => decimal.Parse(s.weight)), Count = group.Count() }).ToList();
                //var result = response.Content.ReadAsStringAsync().Result;

                //return Request.CreateResponse(HttpStatusCode.OK, result);
                List<Sampling> samplingReport = new List<Sampling>(samplingResultsData.Capacity);
                foreach (var rec in samplingResultsData)
                {
                    Sampling fee2 = new Sampling();
                    fee2.farm = rec.Key.farm;
                    fee2.pond = rec.Key.pond;
                    fee2.farmPond = rec.Key.farmPond;
                    fee2.rangeName = rec.Key.rangeName;
                    fee2.weight = rec.Weight.ToString();
                    fee2.count = rec.Count.ToString();

                    samplingReport.Add(fee2);
                }
                samplingResults = samplingReport;
            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }

            var pts = ptr.GetByWeek(reportdate);
            var dts = dtr.GetByWeek(reportdate);
            var wbs = wbr.GetByWeek(reportdate);
            var abs = ar.GetByWeek(reportdate);
            var dsl = dr.GetByWeek(reportdate);
            var fsrs = fsrr.GetByWeek(reportdate);

            decimal headedweighttotal = 0;
            decimal plweight = 0;
            decimal wbweight = 0;
            decimal avgyield = 100;
            foreach (ProductionTotal pt in pts)
            {
                if (pt.PlantWeight != null)
                { plweight = pt.PlantWeight.Value; }
                if (pt.WeighBacks != null)
                { wbweight = pt.WeighBacks.Value; }
                if (pt.AverageYield != null)
                { avgyield = pt.AverageYield.Value; }
                headedweighttotal += (plweight - wbweight) * avgyield / 100;
                plweight = 0;
                wbweight = 0;
                avgyield = 100;
            }
            decimal notzero = pts.Sum(x => x.PlantWeight).Value - pts.Sum(x => x.WeighBacks).Value;
            decimal avgTotal = 1;
            if (notzero == 0)
            {
                avgTotal = 0;
            }
            else
            {
                avgTotal = headedweighttotal * 100 / (pts.Sum(x => x.PlantWeight).Value - pts.Sum(x => x.WeighBacks).Value);
            }
            //decimal avgTotal = headedweighttotal * 100 / (pts.Sum(x => x.PlantWeight).Value - pts.Sum(x => x.WeighBacks).Value);
            string filletscale = fsrs == null ? "0" : fsrs.ToString();
            string subject = "";
            string body = "";
            body += "<style>table, td, th {border: 1px solid #ddd; text-align: left;}table {border-collapse: collapse; width: 100%;} th, td {padding: 5px;} tr:nth-child(2) {background-color: #f8f8f8;} th {background-color: #ddd;}</style>";
            subject = "Harvest Select Daily Production Report";
            body += "Report Date:  " + reportdate.ToShortDateString() + "<br /><br />";
            body += "Fillet Scale Reading: " + filletscale + "<br /><br />";
            body += "<b>Live Fish Receiving</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Pond Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Plant Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Difference</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>WeighBacks</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Net Live Weight</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Yield %</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Headed Yield</th></tr>";
            body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.PondWeight).Value.ToString("#") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.PlantWeight).Value.ToString("#") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (pts.Sum(x => x.PondWeight).Value - pts.Sum(x => x.PlantWeight).Value).ToString("#") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pts.Sum(x => x.WeighBacks).Value.ToString("#") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (pts.Sum(x => x.PlantWeight).Value - pts.Sum(x => x.WeighBacks).Value).ToString("#") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + avgTotal.ToString("#.####") + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + headedweighttotal.ToString("#") + "</td></tr>";
            var farmpts = pts.GroupBy(s=> new {s.Pond.Farm.InnovaName})
                .Select(g => new { Farm = g.Key.InnovaName, 
                    PlantWeight = g.Sum(x => x.PlantWeight), 
                    PondWeight = g.Sum(x => x.PondWeight),
                    WeighBacks = g.Sum(x => x.WeighBacks),
                                   AverageYield = g.Sum(x => x.AverageYield) / g.Count()
                });
            foreach (var pt in farmpts)
            {
                //COME BACK TO THIS TO DO CORRECT AVERAGE YIELD
                decimal plantweight = pt.PlantWeight.HasValue ? pt.PlantWeight.Value : 0;
                decimal pondweight = pt.PondWeight.HasValue ? pt.PondWeight.Value : 0;
                decimal weighbacks = pt.WeighBacks.HasValue ? pt.WeighBacks.Value : 0;
                decimal averageyield = pt.AverageYield.HasValue ? pt.AverageYield.Value : 0;
                body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pt.Farm + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + pondweight.ToString("#") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + plantweight.ToString("#") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (pondweight - plantweight).ToString("#") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + weighbacks.ToString("#") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + (plantweight - weighbacks).ToString("#") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + averageyield.ToString("#.####") + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + ((plantweight - weighbacks) * averageyield / 100).ToString("#") + "</td></tr>";
            }
            body += "</table><br /><br />";

            body += "<b>Samplings</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Farm</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Pond</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Fish Size</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Count</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>% of Count</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Weight (lbs)</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>% of Weight</th>";
            body += "<th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Avg Weight (lbs)</th>";
            body += "</tr>";
            List<Sampling> sresultsRanges = new List<Sampling>();
            List<Sampling> sresultsPonds = new List<Sampling>();
            List<Sampling> sresultsFarms = new List<Sampling>();
            sresultsRanges = samplingResults.GroupBy(x => x.rangeName).Select(group => group.First()).ToList();
            sresultsFarms = samplingResults.GroupBy(x => x.farm).Select(group => group.First()).ToList();
            sresultsPonds = samplingResults.GroupBy(x => x.pond).Select(group => group.First()).ToList();
            var totalScount = samplingResults.Sum(x => decimal.Parse(x.count));
            var totalSweight = samplingResults.Sum(x => decimal.Parse(x.weight));
            var totalSaverage = totalScount == 0 ? 0 : (totalSweight / totalScount);
            body += "<tr style='background-color: #A1D490;'>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>All Farms</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalScount) + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalSweight) + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalSaverage) + "</td>";
            body += "</tr>";
            foreach (Sampling sam3 in sresultsRanges)
            {
                body += "<tr>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + sam3.rangeName + "</td>";
                var thispondScount = samplingResults.Where(x => x.rangeName == sam3.rangeName).Sum(x => decimal.Parse(x.count));
                var thispondScountpercent = totalScount == 0 ? 0 : (thispondScount / totalScount) * 100;
                var thispondSweight = samplingResults.Where(x => x.rangeName == sam3.rangeName).Sum(x => decimal.Parse(x.weight));
                var thispondSweightpercent = totalSweight == 0 ? 0 : (thispondSweight / totalSweight) * 100;
                var thisSaverage = thispondScount == 0 ? 0 : thispondSweight / thispondScount;
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thispondScount) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}%", thispondScountpercent) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thispondSweight) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}%", thispondSweightpercent) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thisSaverage) + "</td>";
                body += "</tr>";
            }

            foreach (Sampling sam in sresultsFarms)
            {
                var totalfarmcount = samplingResults.Where(x => x.farm == sam.farm).Sum(x => decimal.Parse(x.count));
                var totalfarmweight = samplingResults.Where(x => x.farm == sam.farm).Sum(x => decimal.Parse(x.weight));
                var totalfarmaverage = totalfarmcount == 0 ? 0 : (totalfarmweight / totalfarmcount);
                body += "<tr style='background-color: #A1D490;'>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + sam.farm + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>Farm Total</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalfarmcount) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalfarmweight) + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalfarmaverage) + "</td>";
                body += "</tr>";

                //foreach (Sampling sam1 in sresultsPonds.Where(x => x.farm == sam.farm))
                //{
                    //bool pNameLabel = true;
                    var totalpondcount = samplingResults.Where(x => x.farm == sam.farm).Sum(x => decimal.Parse(x.count));
                    var totalpondweight = samplingResults.Where(x => x.farm == sam.farm).Sum(x => decimal.Parse(x.weight));
                    var totalaverage = totalpondcount == 0 ? 0 : (totalpondweight / totalpondcount);
                    //body += "<tr style='background-color: #CED490;'>";
                    //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                    //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + sam1.pond + "</td>";
                    //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>Pond Total</td>";
                    //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalpondcount) + "</td>";
                    //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                    //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalpondweight) + "</td>";
                    //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
                    //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", totalaverage) + "</td>";
                    //body += "</tr>";
                    foreach (Sampling sam2 in sresultsRanges)
                    {
                        body += "<tr>";

                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";

                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";

                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + sam2.rangeName + "</td>";

                        var thispondcount = samplingResults.Where(x => x.rangeName == sam2.rangeName && x.farm == sam.farm).Sum(x => decimal.Parse(x.count));
                        var thispondcountpercent = totalpondcount == 0 ? 0 : (thispondcount / totalpondcount) * 100;
                        var thispondweight = samplingResults.Where(x => x.rangeName == sam2.rangeName && x.farm == sam.farm).Sum(x => decimal.Parse(x.weight));
                        var thispondweightpercent = totalpondweight == 0 ? 0 : (thispondweight / totalpondweight) * 100;
                        var thisaverage = thispondcount == 0 ? 0 : thispondweight / thispondcount;
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thispondcount) + "</td>";
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}%", thispondcountpercent) + "</td>";
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thispondweight) + "</td>";
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}%", thispondweightpercent) + "</td>";
                        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + string.Format("{0:N2}", thisaverage) + "</td>";
                        body += "</tr>";
                    }

                //}

            }

            //body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'></td>";
            //foreach (Sampling sam in sresultsRanges)
            //{
            //    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>Weight</td>";
            //    body += "<td style='border: 1px solid #ddd; border-right: 2px solid #222; text-align:left; padding: 5px;'>Count</td>";
            //}
            //body += "</tr>";
            //body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL (Weight | Count)</td>";
            //foreach (Sampling sam in sresultsRanges)
            //{
            //    body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + samplingResults.Where(x => x.rangeName == sam.rangeName).Sum(x => decimal.Parse(x.weight)).ToString() + "</td>";
            //    body += "<td style='border: 1px solid #ddd; border-right: 2px solid #222; text-align:left; padding: 5px;'>" + samplingResults.Where(x => x.rangeName == sam.rangeName).Sum(x => decimal.Parse(x.count)).ToString() + "</td>";
            //}
            //body += "</tr>";

            //foreach (Sampling sam1 in sresultsPonds)
            //{
            //    body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + sam1.farmPond + "</td>";
            //    foreach (Sampling sam in sresultsRanges)
            //    {
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + samplingResults.Where(x => x.rangeName == sam.rangeName && x.farmPond == sam1.farmPond).Select(x => x.weight).FirstOrDefault() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; border-right: 2px solid #222; text-align:left; padding: 5px;'>" + samplingResults.Where(x => x.rangeName == sam.rangeName && x.farmPond == sam1.farmPond).Select(x => x.count).FirstOrDefault() + "</td>";
            //    }
            //    body += "</tr>";
            //}
            body += "</table><br /><br />";

            body += "<b>Production By Department</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Absences</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Production Total</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Downtime</th></tr>";
            body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td>";//<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dts.Sum(x => x.Absences).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>---</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dts.Sum(x => x.ShiftWeight).Value.ToString() + " lbs</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dts.Sum(x => x.DownTime).Value.ToString() + "</td></tr>";
            var farmdts = dts.GroupBy(s => new { s.Department })
                .Select(g => new
                {
                    Department = g.Key.Department,
                    ShiftWeight = g.Sum(x => x.ShiftWeight),
                    DownTime = g.Sum(x => x.DownTime),
                    Absences = g.Sum(x => x.Absences)
                });
            foreach (var dt in farmdts)
            {
                //string finishtime = dt.FinishTime.HasValue ? dt.FinishTime.Value.ToShortTimeString() : "---";
                string shiftweight = dt.ShiftWeight.HasValue ? dt.ShiftWeight.Value.ToString() : "---";
                if (dt.Department.DepartmentID == 3)
                {
                    shiftweight = filletscale;
                }
                string downtime = dt.DownTime.HasValue ? dt.DownTime.Value.ToString() : "---";
                string absences = dt.Absences.HasValue ? dt.Absences.Value.ToString() : "---";
                body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.Department.DepartmentName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + absences + "</td>";
                //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + finishtime + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + shiftweight + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + downtime + "</td></tr>";
            }
            body += "</table><br /><br />";

            //body += "<b>WeighBack Details</b><br />";
            //body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Turtle/Trash</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Shad/Carp/Bream</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Live Disease</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Dressed Disease</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>~~Backs</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Red Fillet</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Big Fish</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>DOAs</th></tr>";
            //body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.Turtle).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.Shad).Value.ToString() +  "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.LiveDisease).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.DressedDisease).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.Backs).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.RedFillet).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.BigFish).Value.ToString() + "</td>";
            //body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Sum(x => x.DOAs).Value.ToString() + "</td></tr>";
            //List<int> ponds = new List<int>();
            //foreach (WeighBack wb in wbs)
            //{
            //    if (!ponds.Contains(wb.PondID)){

            //        body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wb.Pond.Farm.FarmName + " - " + wb.Pond.PondName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Turtle).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Shad).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.LiveDisease).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.DressedDisease).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.Backs).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.RedFillet).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.BigFish).Value.ToString() + "</td>";
            //        body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + wbs.Where(x => x.PondID == wb.PondID).Sum(x => x.DOAs).Value.ToString() + "</td></tr>";

            //    }
            //    ponds.Add(wb.PondID);
            //}
            //body += "</table><br /><br />";


            body += "<b>Employee Absence Details By Deptartment</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Late</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Late</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Inmate Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Inmate Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Vacation</th></tr>";
            body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.RegEmpOut).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.RegEmpLate).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.RegEmpLeftEarly).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.TempEmpOut).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.TempEmpLate).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.TempEmpLeftEarly).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.InmateOut).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.InmateLeftEarly).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.EmployeesOnVacation).Value.ToString() + "</td></tr>";
            var farmabs = abs.GroupBy(s => new { s.Department })
                .Select(g => new
                {
                    Department = g.Key.Department,
                    RegEmpOut = g.Sum(x => x.RegEmpOut),
                    RegEmpLate = g.Sum(x => x.RegEmpLate),
                    RegEmpLeftEarly = g.Sum(x => x.RegEmpLeftEarly),
                    TempEmpOut = g.Sum(x => x.TempEmpOut),
                    TempEmpLate = g.Sum(x => x.TempEmpLate),
                    TempEmpLeftEarly = g.Sum(x => x.TempEmpLeftEarly),
                    InmateOut = g.Sum(x => x.InmateOut),
                    InmateLeftEarly = g.Sum(x => x.InmateLeftEarly),
                    EmployeesOnVacation = g.Sum(x => x.EmployeesOnVacation),
                });
            foreach (var ab in farmabs)
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

            body += "<b>Employee Absence Details By Day of Week</b><br />";
            body += "<table style='border: 1px solid #ddd; text-align:left; border-collapse: collapse; width: 100%;'><tr><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'></th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Late</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Reg Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Late</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Temp Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Inmate Out</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Inmate Left Early</th><th style='border: 1px solid #ddd; text-align:left; padding: 5px; background-color: #ddd;'>Vacation</th></tr>";
            body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.RegEmpOut).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.RegEmpLate).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.RegEmpLeftEarly).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.TempEmpOut).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.TempEmpLate).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.TempEmpLeftEarly).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.InmateOut).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.InmateLeftEarly).Value.ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + abs.Sum(x => x.EmployeesOnVacation).Value.ToString() + "</td></tr>";
            var farmabs2 = abs.GroupBy(s => new { s.AbsenceDate.DayOfWeek })
                .Select(g => new
                {
                    WeekDay = g.Key.DayOfWeek.ToString(),
                    RegEmpOut = g.Sum(x => x.RegEmpOut),
                    RegEmpLate = g.Sum(x => x.RegEmpLate),
                    RegEmpLeftEarly = g.Sum(x => x.RegEmpLeftEarly),
                    TempEmpOut = g.Sum(x => x.TempEmpOut),
                    TempEmpLate = g.Sum(x => x.TempEmpLate),
                    TempEmpLeftEarly = g.Sum(x => x.TempEmpLeftEarly),
                    InmateOut = g.Sum(x => x.InmateOut),
                    InmateLeftEarly = g.Sum(x => x.InmateLeftEarly),
                    EmployeesOnVacation = g.Sum(x => x.EmployeesOnVacation),
                });
            foreach (var ab in farmabs2)
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
                body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + ab.WeekDay + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + RegEmpOut + "</td>";
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
            body += "<tr style='background-color: #A1D490; font-weight: bold;'><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>TOTAL</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>---</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dsl.Sum(x => x.Minutes).ToString() + "</td>";
            body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>---</td></tr>";
            foreach (DownTime dt in dsl.OrderBy(x=> x.Department.DepartmentName))
            {

                string DownTimeNote = dt.DownTimeNote != null ? dt.DownTimeNote : "---";
                body += "<tr><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.DownTimeType.Department.DepartmentName + "</td><td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.DownTimeType.DownTimeName + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.Minutes.ToString() + "</td>";
                body += "<td style='border: 1px solid #ddd; text-align:left; padding: 5px;'>" + dt.DownTimeNote + "</td></tr>";


            }
            body += "</table><br /><br />";

            body += "</table>";
            //, danielw@harvestselect.com
            string elist = "";
            EmailRepository er = new EmailRepository();
            List<Email> emails = er.GetEmails();
            foreach (Email em in emails)
            {
                elist += em.EmailAddress + ", ";
            }
            elist = elist.Substring(0, elist.Length - 2);
            //SendMail("harper@stimulantgroup.com, danielw@harvestselect.com, RobertL@HarvestSelect.com, Alice@HarvestSelect.com, Betsya@HarvestSelect.com, Bobby@HarvestSelect.com, Brenda@harvestselect.com, ChrisH@HarvestSelect.com, cory@harvestselect.com, daniel@harvestselect.com, Debi@HarvestSelect.com, jimbo@harvestselect.com, johnny@harvestselect.com, leec@harvestselect.com, lee@harvestselect.com, Mark@HarvestSelect.com, Michael@harvestselect.com, Mike@HarvestSelect.com, Randy@HarvestSelect.com, reed@harvestselect.com, rhonda@harvestselect.com, Ryan@HarvestSelect.com, Shirley@HarvestSelect.com, tammy@harvestselect.com, tom@harvestselect.com, trey@harvestselect.com, sam@harvestselect.com", subject, body);
            //SendMail(elist, subject, body);

            return Request.CreateResponse(HttpStatusCode.OK, body);

        }



        private bool SendMail(string emailto, string subject, string body)
        {
            try
            {
                MailMessage mm = new MailMessage();
                mm.From = new MailAddress("reports@harvestselect.com");
                //mm.From = new MailAddress("harper@stimulantgroup.com");
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