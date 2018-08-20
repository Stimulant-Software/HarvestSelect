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
using System.Net.Http.Headers;
using System.Web.Http;
using System.Net.Mail;
using System.Web;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Text;
using System.Data;
using System.Xml;
using System.IO;
using System.Web.Http.Cors;



namespace SGApp.Controllers
{
    [EnableCors(origins: "http://reports.harvestselect.com", headers: "*", methods: "*")]
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
        public HttpResponseMessage GetBOLReport([FromBody] UtilityDTO utilityDto)
        {

            //Update Shift Weights
            List<BOL> BOLResults = new List<BOL>();
            //SGApp.DTOs.GenericDTO dto = new GenericDTO();
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            //var dic = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            //var sDate = DateTime.ParseExact(dic.First().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            //var eDate = DateTime.ParseExact(dic.Last().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            //var sDate = utilityDto.OrderDate;
            //var eDate = utilityDto.OrderDate;
            //dto.StartDate = DateTime.Parse(sDate);
            //dto.EndDate = DateTime.Parse(sDate);
            //dto.CustomerNumber = utilityDto.CustomerNumber;
            //dto.OrderNumber = utilityDto.OrderNumber;
            dto.OrderCode = utilityDto.OrderCode;
            //dto.StartDate = DateTime.Now.AddDays(-1).Date;
            //dto.EndDate = DateTime.Now.Date;
            //dto.StartDate = DateTime.Now.AddDays(1).Date;
            //dto.EndDate = DateTime.Now.AddDays(2).Date;
            var client = new HttpClient
            {
                //BaseAddress = new Uri("http://323-booth-svr2:3030/")
                //BaseAddress = new Uri("http://64.139.95.243:7846/")
                BaseAddress = new Uri("http://64.139.95.243:7846/")
                //BaseAddress = new Uri("http://localhost:51888/")
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
            string body = "<html><head><style>thead, tfoot { display: table-row-group } .contentTable tr, td {page-break-inside: avoid !important;}</style>";
            body += "<script>function subst(){var vars={};var x=document.location.search.substring(1).split('&'); for(var i in x) {var z=x[i].split('=',2);vars[z[0]] = unescape(z[1]);}" + "var x=['frompage','topage','page','webpage','section','subsection','subsubsection'];for(var i in x) {var y = document.getElementsByClassName(x[i]);" + "for(var j=0; j<y.length; ++j) y[j].textContent = vars[x[i]];}}</script></head>";
            //each order goes here
            BOL[] resultsByOrder = BOLResults.GroupBy(x => x.OrderCode).Select(group => group.First()).ToArray();
            body += "<body onload=\"subst()\">";
            foreach (BOL order in resultsByOrder)
            {
                body += "<div><div><table style='width: 100%'><tbody><tr><td><table style='width: 100%'><tr><td><h1>Bill of Lading</h1></td></tr>";
                body += "<tr><td><img src='http://dashboards.harvestselect.com/Images/logo.png' height='150px'><br />Harvest Select Catfish<br>Highway 80 East<br>P.O. Box 769<br>Uniontown, AL 36786<br>TEL: (334) 628-3474 Fax: (334) 628-6122</td></tr></table></td><td valign='top'><table style='width: 100%'>";
                body += "<tr><td style='border: 1px solid #ddd'>Order No.</td><td style='border: 1px solid #ddd'>" + order.OrderCode + "</td></tr><tr><td style='border: 1px solid #ddd'>Order Date</td><td style='border: 1px solid #ddd'>" + DateTime.Parse(order.OrderDate).ToShortDateString() + "</td></tr>";
                body += "<tr><td style='border: 1px solid #ddd'>Ship Date</td><td style='border: 1px solid #ddd'>" + DateTime.Parse(order.DispDate).ToShortDateString() + "</td></tr></table></td></tr><tr><td colspan='2'><br /><table style='width: 100%'>";
                body += "<thead><tr><th style='border: 1px solid #ddd; background-color: #ddd;'>Sold To:</th>";
                body += "<th style='border: 1px solid #ddd; background-color: #ddd;'>Ship To:</th></tr></thead>";
                body += "<tbody><tr><td style='border: 1px solid #ddd'>" + order.CustLong + "<br>" + order.CustomerAddress + "<br>" + order.CustomerAddress2 + "<br>" + order.CustomerCity + "  " + order.CustomerZip + "<br>" + order.CustomerPhone + "</td>";
                body += "<td style='border: 1px solid #ddd'>" + order.ShipToName + "<br>" + order.ShipToAddress + "</td></tr><tr></tr></tbody></table>";
                body += "</td></tr><tr><td colspan='2'><br /><table style='width: 100%'><thead><tr><th style='border: 1px solid #ddd; background-color: #ddd;'>Cust No.</th><th style='border: 1px solid #ddd; background-color: #ddd; text-align: center;'>Company PO</th><th style='border: 1px solid #ddd; background-color: #ddd; text-align: center;'>Corporate PO</th><th style='border: 1px solid #ddd; background-color: #ddd; text-align: center;'>Freight PO</th><th style='border: 1px solid #ddd; background-color: #ddd;'>Terms</th></tr></thead>";
                body += "<tbody><tr><td style='border: 1px solid #ddd'>" + order.CustNumber + "</td><td style='border: 1px solid #ddd; text-align: center;'>" + order.PO1 + "</td><td style='border: 1px solid #ddd; text-align: center;'>" + order.PO2 + "</td><td style='border: 1px solid #ddd;  text-align: center;'>" + order.PO3 + "</td><td style='border: 1px solid #ddd'>" + order.OrderTerms + "</td></tr><tr></tr></tbody></table></td></tr><tr><td colspan='2'>";
                var ncw = BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "NON CATCH WEIGHT ITEM");
                var tmp = BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "CATCH WEIGHT ITEM").GroupBy(x => x.MaterialID).Select(group => group.First()).ToArray();
                if (ncw.Count() > 0)
                {
                    body += " <br />Non Catch Weight Items<br /><table style='width: 100%' class='contentTable'><thead><tr><th style='border: 1px solid #ddd; background-color: #ddd;'>Item</th><th style='border: 1px solid #ddd; background-color: #ddd;'>Product Description</th><th style='border: 1px solid #ddd; background-color: #ddd;'>UM</th><th style='border: 1px solid #ddd; background-color: #ddd;'>Ordered</th><th style='border: 1px solid #ddd; background-color: #ddd;'>Shipped</th><th style='border: 1px solid #ddd; background-color: #ddd;'>Unit Net Wt.</th><th style='border: 1px solid #ddd; background-color: #ddd;'>Total Net Wt.</th></tr></thead><tbody>";
                }
                foreach (BOL orderdetail in ncw)
                {
                    body += "<tr><td style='border: 1px solid #ddd; text-align: right;'>" + orderdetail.ProdCode + "</td><td style='border: 1px solid #ddd; text-align: right;'>" + orderdetail.ProdName + "</td><td style='border: 1px solid #ddd; text-align: right;'>ca</td><td style='border: 1px solid #ddd; text-align: right;'>" + String.Format("{0:0}", Decimal.Parse(orderdetail.OrderedAmt)) + "</td><td style='border: 1px solid #ddd; text-align: right;'>" + orderdetail.ShippedQty + "</td><td style='border: 1px solid #ddd; text-align: right;'>" + String.Format("{0:.##}", Decimal.Parse(orderdetail.ApproxUnitWeight)) + "</td><td style='border: 1px solid #ddd; text-align: right;'>" + String.Format("{0:.##}", Decimal.Parse(orderdetail.ShippedWeight)) + "</td></tr>";
                }
                if (ncw.Count() > 0)
                {
                    body += "<tr><td colspan='3' style='border: 1px solid #ddd'><strong>TOTALS</strong></td><td style='border: 1px solid #ddd; text-align: right;'><strong>" + String.Format("{0:0}", BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "NON CATCH WEIGHT ITEM").Sum(x => decimal.Parse(x.OrderedAmt))) + "</strong></td><td style='border: 1px solid #ddd; text-align: right;'><strong>" + BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "NON CATCH WEIGHT ITEM").Sum(x => decimal.Parse(x.ShippedQty)).ToString() + "</strong></td><td style='border: 1px solid #ddd'></td><td style='border: 1px solid #ddd; text-align: right;'><strong>" + String.Format("{0:.##}", BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "NON CATCH WEIGHT ITEM").Sum(x => decimal.Parse(x.ShippedWeight))) + "</strong></td></tr></tbody></table>";
                }
                if (tmp.Count() > 0)
                {

                    body += " <br />Catch Weight Items<br /><table style='width: 100%'  class='contentTable'><tbody>";
                }  
                foreach (BOL odet in tmp)
                {
                    var cw = BOLResults.Where(x => x.MaterialID == odet.MaterialID && x.OrderCode == order.OrderCode && x.CWItem == "CATCH WEIGHT ITEM");
                    body += "<tr><td style='border: 1px solid #ddd; background-color: #ddd; text-align:center;'><strong>Item</strong></td><td style='border: 1px solid #ddd; background-color: #ddd; text-align:center;'><strong>Product Description</strong></td><td style='border: 1px solid #ddd; background-color: #ddd; text-align:center;'><strong>UM</strong></td><td style='border: 1px solid #ddd; background-color: #ddd;'></td><td style='border: 1px solid #ddd; background-color: #ddd;'></td><!--<td style='border: 1px solid #ddd; background-color: #ddd;'><strong>Approx Unit Weight</strong></td>--><td style='border: 1px solid #ddd; background-color: #ddd; text-align: right;'><strong>Case Weight</strong></td></tr>";
                    foreach (BOL orderdetail in cw)
                    {
                        //removed " + orderdetail.OrderedAmt + "
                        //removed " + orderdetail.ShippedQty + "
                        //removed " + orderdetail.ApproxUnitWeight + "

                        body += "<tr><td style='border: 1px solid #ddd; text-align: right;'>" + orderdetail.ProdCode + "</td><td style='border: 1px solid #ddd; text-align: right;'>" + orderdetail.ProdName + "</td><td style='border: 1px solid #ddd; text-align: right;'>ca</td><td style='border: 1px solid #ddd; text-align: right;'></td><td style='border: 1px solid #ddd; text-align: right;'></td><td style='border: 1px solid #ddd; text-align: right;'>" + String.Format("{0:0.00}", decimal.Parse(orderdetail.ShippedWeight)) + "</td></tr>";
                    }
                    if (cw.Count() > 0)
                    {
                        //<td style='border: 1px solid #ddd; text-align: right;''><strong>" + String.Format("{0:0.00}", BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "CATCH WEIGHT ITEM" && x.MaterialID == odet.MaterialID).Average(x => decimal.Parse(x.ShippedWeight))) + "</strong></td> -------REMOVED PER DANIEL
                        body += "<tr><td colspan='3' style='border: 1px solid #ddd; background-color: #ddd;'></td><td style='border: 1px solid #ddd; background-color: #ddd;; text-align: right;'><strong>Ordered</strong></td><td style='border: 1px solid #ddd; background-color: #ddd;; text-align: right;'><strong>Shipped</strong></td><td style='border: 1px solid #ddd; background-color: #ddd;; text-align: right;'><strong>Total Case Weight</strong></td></tr>";
                        body += "<tr><td colspan='3' style='border: 1px solid #ddd'><strong>ITEM TOTALS</strong></td><td style='border: 1px solid #ddd; text-align: right;'><strong>" + BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "CATCH WEIGHT ITEM" && x.MaterialID == odet.MaterialID).Count().ToString() + "</strong></td><td style='border: 1px solid #ddd; text-align: right;'><strong>" + String.Format("{0:0}", BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "CATCH WEIGHT ITEM" && x.MaterialID == odet.MaterialID).Sum(x => decimal.Parse(x.ShippedQty))) + "</strong></td><td style='border: 1px solid #ddd; text-align: right;'><strong>" + String.Format("{0:.##}", BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "CATCH WEIGHT ITEM" && x.MaterialID == odet.MaterialID).Sum(x => decimal.Parse(x.ShippedWeight))) + "</strong></td></tr>";
                        body += "<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>";
                    }
                     }
                if (tmp.Count() > 0)
                {
                    //REMOVED PER DANIEL
                    //<td style='border: 1px solid #ddd; text-align: right;'><strong>" + String.Format("{0:0.00}", BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "CATCH WEIGHT ITEM").Average(x => decimal.Parse(x.ShippedWeight))) + "</strong></td>
                    body += "<tr><td colspan='3' style='border: 1px solid #ddd; background-color: #ddd;'></td><td style='border: 1px solid #ddd; background-color: #ddd;; text-align: right;'><strong>Ordered</strong></td><td style='border: 1px solid #ddd; background-color: #ddd;; text-align: right;'><strong>Shipped</strong></td><td style='border: 1px solid #ddd; background-color: #ddd;; text-align: right;'><strong>Total Case Weight</strong></td></tr>";
                    body += "<tr><td colspan='3' style='border: 1px solid #ddd'><strong>CATCHWEIGHT TOTALS</strong></td><td style='border: 1px solid #ddd; text-align: right;'><strong>" + BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "CATCH WEIGHT ITEM").Count().ToString() + "</strong></td><td style='border: 1px solid #ddd; text-align: right;'><strong>" + String.Format("{0:0}", BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "CATCH WEIGHT ITEM").Sum(x => decimal.Parse(x.ShippedQty))) + "</strong></td><td style='border: 1px solid #ddd; text-align: right;'><strong>" + String.Format("{0:.##}", BOLResults.Where(x => x.OrderCode == order.OrderCode && x.CWItem == "CATCH WEIGHT ITEM").Sum(x => decimal.Parse(x.ShippedWeight))) + "</strong></td></tr></tbody></table>";
                }
                body += "</td></tr></tbody></table></div></div>";
            }
          
            
            
            body += "</body></html>";
            var pdfGen = new NReco.PdfGenerator.HtmlToPdfConverter();
            var pdfheader = "<div>Page <span class=\"page\"></span> of <span class=\"topage\"></span><span>&nbsp;&nbsp;&nbsp;Order:&nbsp;" + BOLResults[0].OrderCode + "&nbsp;&nbsp;&nbsp;Ship Date:&nbsp;" + DateTime.Parse(BOLResults[0].DispDate).ToShortDateString() + "</span></div>";
            pdfGen.PageFooterHtml = pdfheader;
            var pdfMargins = new NReco.PdfGenerator.PageMargins();
            pdfMargins.Top = 10;
            pdfMargins.Bottom = 10;
            pdfGen.Margins = pdfMargins;
            var pdfBytes = pdfGen.GeneratePdf(body);
            //return Request.CreateResponse(HttpStatusCode.OK, body);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            //result.Content = new ByteArrayContent(pdfBytes);
            
            result.Content = new StringContent(System.Convert.ToBase64String(pdfBytes));
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            result.Content.Headers.Add("Content-Disposition", "inline; filename=BOL.pdf");
            return result;


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
            iqfstations.Add("1024");
            iqfstations.Add("1025");
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
            
            List<DTOs.Sampling> samplingResults = new List<DTOs.Sampling>();

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
                DTOs.Sampling[] samplingResultsArray = json_serializer.Deserialize<DTOs.Sampling[]>(response.Content.ReadAsStringAsync().Result);
                samplingResults = samplingResultsArray.ToList();
                var samplingResultsData = samplingResults.GroupBy(x => new { x.farm, x.pond, x.farmPond, x.rangeName })
                    .Select(group => new { Key = group.Key, Weight = group.Sum(s => decimal.Parse(s.weight)), Count = group.Count() }).ToList();
                //var result = response.Content.ReadAsStringAsync().Result;

                //return Request.CreateResponse(HttpStatusCode.OK, result);
                List<DTOs.Sampling> samplingReport = new List<DTOs.Sampling>(samplingResultsData.Capacity);
                foreach (var rec in samplingResultsData)
                {
                    DTOs.Sampling fee2 = new DTOs.Sampling();
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
            List<DTOs.Sampling> sresultsRanges = new List<DTOs.Sampling>();
            List<DTOs.Sampling> sresultsPonds = new List<DTOs.Sampling>();
            List<DTOs.Sampling> sresultsFarms = new List<DTOs.Sampling>();
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
            foreach (DTOs.Sampling sam3 in sresultsRanges)
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

            foreach (DTOs.Sampling sam in sresultsFarms)
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
                
                foreach (DTOs.Sampling sam1 in sresultsPonds.Where(x => x.farm == sam.farm))
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
                    foreach (DTOs.Sampling sam2 in sresultsRanges)
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
            List<DTOs.Sampling> samplingResults = new List<DTOs.Sampling>();

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
                DTOs.Sampling[] samplingResultsArray = json_serializer.Deserialize<DTOs.Sampling[]>(response.Content.ReadAsStringAsync().Result);
                samplingResults = samplingResultsArray.ToList();
                var samplingResultsData = samplingResults.GroupBy(x => new { x.farm, x.pond, x.farmPond, x.rangeName })
                    .Select(group => new { Key = group.Key, Weight = group.Sum(s => decimal.Parse(s.weight)), Count = group.Count() }).ToList();
                //var result = response.Content.ReadAsStringAsync().Result;

                //return Request.CreateResponse(HttpStatusCode.OK, result);
                List<DTOs.Sampling> samplingReport = new List<DTOs.Sampling>(samplingResultsData.Capacity);
                foreach (var rec in samplingResultsData)
                {
                    DTOs.Sampling fee2 = new DTOs.Sampling();
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
            List<DTOs.Sampling> sresultsRanges = new List<DTOs.Sampling>();
            List<DTOs.Sampling> sresultsPonds = new List<DTOs.Sampling>();
            List<DTOs.Sampling> sresultsFarms = new List<DTOs.Sampling>();
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
            foreach (DTOs.Sampling sam3 in sresultsRanges)
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

            foreach (DTOs.Sampling sam in sresultsFarms)
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
                    foreach (DTOs.Sampling sam2 in sresultsRanges)
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
        [HttpGet]
        public HttpResponseMessage TestDailyReport()
        {

            //Update Shift Weights
            List<ShiftWeight> shiftResults = new List<ShiftWeight>();
            //SGApp.DTOs.GenericDTO dto = new GenericDTO();
            SGApp.DTOs.GenericDTO dto = new GenericDTO();

            dto.StartDate = DateTime.Now.AddYears(102);
            dto.EndDate = DateTime.Now.AddYears(102);
            //dto.StartDate = DateTime.Now.AddDays(-1).Date;
            //dto.EndDate = DateTime.Now.Date;
            //dto.StartDate = DateTime.Now.AddDays(1).Date;
            //dto.EndDate = DateTime.Now.AddDays(2).Date;

            List<string> iqfstations = new List<string>();
            iqfstations.Add("9");
            iqfstations.Add("10");
            iqfstations.Add("1024");
            iqfstations.Add("1025");
            var IQFweight = 99999.ToString();
            var BaggerWeightRecord = shiftResults.Where(x => x.Station == "7").FirstOrDefault();
            var BaggerWeight = "88888";
            List<string> stations = new List<string>();
            stations.Add("8");
            stations.Add("2");
            stations.Add("4");
            stations.Add("3");
            stations.Add("866");
            var FreshWeight = 77777.ToString();


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

            List<DTOs.Sampling> samplingResults = new List<DTOs.Sampling>();



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
           

            return Request.CreateResponse(HttpStatusCode.OK);

        }
        [HttpGet]
        public HttpResponseMessage UpdateSalesData()
        {


            List<BOL> BOLResults = new List<BOL>();
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            DataSet myData = new DataSet();
            var client = new HttpClient
            {

                BaseAddress = new Uri("http://64.139.95.243:7846/")

            };
            try
            {
                var response = client.GetAsync("api/Remote/UpdateSalesData").Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                //BOL[] BOLResultsArray = json_serializer.Deserialize<BOL[]>(response.Content.ReadAsStringAsync().Result);
                //DataSet SalesArray = json_serializer.Deserialize<DataSet>(response.Content.ReadAsStringAsync().Result);
                //myData = SalesArray;
                var xd = new XmlDocument();

                //// Note:Json convertor needs a json with one node as root
                //string jsonString = "{ \"rootNode\": {" + Constants.testsales.Trim().TrimStart('{').TrimEnd('}') + @"} }";
                //// Now it is secure that we have always a Json with one node as root 
                xd = JsonConvert.DeserializeXmlNode(response.Content.ReadAsStringAsync().Result, "table");
                //xd = JsonConvert.DeserializeXmlNode(jsonString);


                //// DataSet is able to read from XML and return a proper DataSet
                var result = new DataSet();
                result.ReadXml(new XmlNodeReader(xd));
                myData = result;


            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            StringBuilder sb = new StringBuilder();

            //IEnumerable<string> columnNames = myData.Tables[0].Columns.Cast<DataColumn>().
            //                                  Select(column => column.ColumnName);
            //string columnames = string.Join(",", columnNames);

            foreach (DataRow row in myData.Tables[0].Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString().Replace(",", " "));
                sb.AppendLine(string.Join(",", fields));
            }
            File.AppendAllText("C:\\Users\\Administrator\\Documents\\AS90ATRN.csv", sb.ToString());

            return Request.CreateResponse(HttpStatusCode.OK, BOLResults);


        }

        [HttpGet]
        public HttpResponseMessage UpdateAdagioFile()
        {
            var dic = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            var fileToUpdate = dic.First().Value;
            DataSet myData = new DataSet();
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };
            try
            {
                var response = client.GetAsync("api/Remote/UpdateAdagioFile?ftu=" + fileToUpdate).Result;
                response.EnsureSuccessStatusCode();
                var xd = new XmlDocument();
                xd = JsonConvert.DeserializeXmlNode(response.Content.ReadAsStringAsync().Result, "table");
                var result = new DataSet();
                result.ReadXml(new XmlNodeReader(xd));
                myData = result;
            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            StringBuilder sb = new StringBuilder();
            string columnLine = "";
            foreach (DataColumn dc in myData.Tables[0].Columns)
            {
                columnLine += dc.ColumnName + ",";
            }
            columnLine = columnLine.TrimEnd(',');
            sb.AppendLine(columnLine);
            foreach (DataRow row in myData.Tables[0].Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString().Replace(",", " "));
                sb.AppendLine(string.Join(",", fields));
            }
            File.WriteAllText("C:\\Users\\Administrator\\Documents\\" + fileToUpdate + ".csv", sb.ToString());
            return Request.CreateResponse(HttpStatusCode.OK, "Success");
        }

        [HttpGet]
        public HttpResponseMessage UpdateSamplingData()
        {
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var dic = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            var sDate = DateTime.ParseExact(dic.First().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            var eDate = DateTime.ParseExact(dic.Last().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            dto.StartDate = sDate;
            dto.EndDate = eDate;
            var db = new AppEntities();
            var dontUpdate = db.Samplings.Where(x => x.regtime >= sDate).Any();
            if (dontUpdate)
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Table already updated");
            }
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };

            List<DTOs.Sampling> samplingResults = new List<DTOs.Sampling>();
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetKeithsData", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                DTOs.Sampling[] samplingResultsArray = json_serializer.Deserialize<DTOs.Sampling[]>(response.Content.ReadAsStringAsync().Result);
                samplingResults = samplingResultsArray.ToList();
                var dbSamples = samplingResults.Select(x => new Models.EF.Sampling
                {
                    code = x.rangeValue,
                    code2 = x.rangeValue,
                    codename = x.rangeName,
                    farmname = x.farm,
                    pondname = x.farmPond,
                    shname = x.pond,
                    regtime = DateTime.Parse(x.date),
                    weight = System.Convert.ToDouble(decimal.Parse(x.weight))
                });
                db.Samplings.AddRange(dbSamples);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Update Successful");
        }

        [HttpGet]
        public HttpResponseMessage UpdateTodaySamplingData()
        {
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var dic = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            var sDate = DateTime.ParseExact(dic.First().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            var eDate = DateTime.ParseExact(dic.Last().Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
            dto.StartDate = sDate;
            dto.EndDate = eDate;
            var db = new AppEntities();
            //var dontUpdate = db.Samplings.Where(x => x.regtime >= sDate).Any();
            //if (dontUpdate)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, "Table already updated");
            //}
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };

            List<DTOs.Sampling> samplingResults = new List<DTOs.Sampling>();
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetKeithsData", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                DTOs.Sampling[] samplingResultsArray = json_serializer.Deserialize<DTOs.Sampling[]>(response.Content.ReadAsStringAsync().Result);
                samplingResults = samplingResultsArray.ToList();
                var dbSamples = samplingResults.Select(x => new Models.EF.TodaySampling
                {
                    code = x.rangeValue,
                    code2 = x.rangeValue,
                    codename = x.rangeName,
                    farmname = x.farm,
                    pondname = x.farmPond,
                    shname = x.pond,
                    regtime = DateTime.Parse(x.date),
                    weight = System.Convert.ToDouble(decimal.Parse(x.weight))
                });
                db.Database.ExecuteSqlCommand("DELETE FROM TodaySamplings");
                db.TodaySamplings.AddRange(dbSamples);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Update Successful");
        }

        [HttpGet]
        public HttpResponseMessage UpdateCurrentShipping()
        {
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var db = new AppEntities();


            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };

            List<DTOs.CurrentShippingDTO> shippingResults = new List<DTOs.CurrentShippingDTO>();
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetCurrentShipping", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                DTOs.CurrentShippingDTO[] shippingResultsArray = json_serializer.Deserialize<DTOs.CurrentShippingDTO[]>(response.Content.ReadAsStringAsync().Result);
                shippingResults = shippingResultsArray.ToList();
                if (shippingResults.Count() > 0)
                {
                    db.Database.ExecuteSqlCommand("DELETE FROM CurrentShipping");
                    var dbShippings = shippingResults.Select(x => new Models.EF.CurrentShipping
                    {
                        CustomerName = x.CustomerName,
                        ItemCode = x.ItemCode,
                        ItemDescription = x.ItemDescription,
                        QuantityOnHand = x.QuantityOnHand,
                        OrderAmount = x.OrderAmount,
                        OrderDate = DateTime.Parse(x.OrderDate),
                        ShippedAmount = x.ShippedAmount,
                        TodayUnits = x.TodayUnits
                    });
                    db.CurrentShippings.AddRange(dbShippings);
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Update Successful");
        }

        [HttpGet]
        public HttpResponseMessage GetDailySalesUpdate()
        {
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var db = new AppEntities();
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };
            List<dtoAdagioSalesTransaction> results = new List<dtoAdagioSalesTransaction>();
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetAdagioSalesTransactions", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                json_serializer.MaxJsonLength = Int32.MaxValue;
                dtoAdagioSalesTransaction[] resultsArray = json_serializer.Deserialize<dtoAdagioSalesTransaction[]>(response.Content.ReadAsStringAsync().Result);
                results = resultsArray.ToList();
                if (results.Count() > 0)
                {
                    List<AdagioSalesTransaction> castedResults = results.Select(x => new AdagioSalesTransaction
                    {
                        Cust = double.Parse(x.Cust),
                        Shipto = x.Shipto,
                        JulianDateKey = x.JulianDateKey,
                        HexUniquifer = x.HexUniquifer,
                        TransactionUnquifier = x.TransactionUnquifier,
                        Item = double.Parse(x.Item),
                        ItemSegment1CATEGORY = double.Parse(x.ItemSegment1CATEGORY),
                        ItemSegment2ITEM = double.Parse(x.ItemSegment2ITEM),
                        ItemSegment3CTNWGT = double.Parse(x.ItemSegment3CTNWGT),
                        QIItem = double.Parse(x.QIItem),
                        PrefixDocumentNumber = x.PrefixDocumentNumber,
                        Prefix = x.Prefix,
                        DocumentNumber = x.DocumentNumber,
                        //Uniquifier = x.Uniquifier,
                        Type_T = x.Type_T,
                        Date_2 = DateTime.Parse(x.Date_2),
                        Salesperson = double.Parse(x.Salesperson),
                        Qty = double.Parse(x.Qty),
                        Amt = double.Parse(x.Amt),
                        Cost = double.Parse(x.Cost),
                        BasePrice = double.Parse(x.BasePrice),
                        CustomerTaxStatus = double.Parse(x.CustomerTaxStatus),
                        ItemTaxStatus = double.Parse(x.ItemTaxStatus),
                        Location = double.Parse(x.Location),
                        Category = x.Category,
                        Order_2 = x.Order_2,
                        TaxGroup = x.TaxGroup,
                        RecordSource_T = x.RecordSource_T,
                        ItemSource_T = x.ItemSource_T,
                        PriceList = x.PriceList,
                        DiscountDate = x.DiscountDate,
                        DueDate = DateTime.Parse(x.DueDate),
                        DatePaid = x.DatePaid,
                        InvoicePaid_T = x.InvoicePaid_T,
                        Reference = x.Reference,
                        PONumber = x.PONumber,
                        OrginalInvoice = x.OrginalInvoice,
                        Line = double.Parse(x.Line),
                        Territory = x.Territory,
                        Itemreportgroup = x.Itemreportgroup,
                        Customerreportgroup = x.Customerreportgroup,
                        ManualStyleCode = x.ManualStyleCode,
                        AutomaticStyleCode = x.AutomaticStyleCode,
                        //SourceDecimals = x.SourceDecimals,
                        HomeCurr = x.HomeCurr,
                        RateType = x.RateType,
                        SourceCurr = x.SourceCurr,
                        RateDate = DateTime.Parse(x.RateDate),
                        Rate = double.Parse(x.Rate),
                        RateRep = x.RateRep,
                        DateMatching = x.DateMatching,
                        SourceAmt = double.Parse(x.SourceAmt),
                        SourceCost = double.Parse(x.SourceCost),
                        SourceBasePrice = double.Parse(x.SourceBasePrice),
                        LineItemDescription = x.LineItemDescription,
                        RNumAS90ATRN = x.RNumAS90ATRN,

                    }).ToList();
                    db.AdagioSalesTransactions.AddRange(castedResults);
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Update Successful");
        }


        [HttpGet]
        public HttpResponseMessage GetAdagioItems()
        {
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var db = new AppEntities();
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };
            List<dtoAdagioItem> results = new List<dtoAdagioItem>();
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetAdagioItems", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                json_serializer.MaxJsonLength = Int32.MaxValue;
                dtoAdagioItem[] resultsArray = json_serializer.Deserialize<dtoAdagioItem[]>(response.Content.ReadAsStringAsync().Result);
                results = resultsArray.ToList();
                if (results.Count() > 0)
                {
                    db.Database.ExecuteSqlCommand("DELETE FROM AdagioItems");
                    List<AdagioItem> castedResults = results.Select(x => new AdagioItem
                    {
                        Item = double.Parse(x.Item),
                        SetKey = double.Parse(x.SetKey),
                        Item2 = double.Parse(x.Item2),
                        Description = x.Description,
                        Category = x.Category,
                        //ReportGroup = x.ReportGroup,
                        //StockItem = double.Parse(x.StockItem),
                        //ItemsUsedinBOMCount = double.Parse(x.ItemsUsedinBOMCount),
                        //SerialCount = double.Parse(x.SerialCount),
                        //LayawayCounter = double.Parse(x.LayawayCounter),
                        Unit = x.Unit,
                        //AltUnit1 = x.AltUnit1,
                        //AltUnit2 = x.AltUnit2,
                        //AltUnit3 = x.AltUnit3,
                        //AltUnit4 = x.AltUnit4,
                        PurchaseUnit = x.PurchaseUnit,
                        PriceUnit = x.PriceUnit,
                        //AltFactor1 = double.Parse(x.AltFactor1),
                        //AltFactor2 = double.Parse(x.AltFactor2),
                        //AltFactor3 = double.Parse(x.AltFactor3),
                        //AltFactor4 = double.Parse(x.AltFactor4),
                        BasePrice = double.Parse(x.BasePrice),
                        PickingSeq = x.PickingSeq,
                        StdCost = double.Parse(x.StdCost),
                        UnitWeight = double.Parse(x.UnitWeight),
                        SaleStartDate = x.SaleStartDate,
                        SaleEndDate = x.SaleEndDate,
                        SalePrice = double.Parse(x.SalePrice),
                        //CtrlAcctSet = double.Parse(x.CtrlAcctSet),
                        //TaxStatus = double.Parse(x.TaxStatus),
                        //UserDefinedCost1 = double.Parse(x.UserDefinedCost1),
                        //UserDefinedCost2 = double.Parse(x.UserDefinedCost2),
                        //DiscType_T = x.DiscType_T,
                        //DiscFormat_T = x.DiscFormat_T,
                        //DiscBase_T = x.DiscBase_T,
                        //DiscMarkupQty1 = double.Parse(x.DiscMarkupQty1),
                        //DiscMarkupQty2 = double.Parse(x.DiscMarkupQty2),
                        //DiscMarkupQty3 = double.Parse(x.DiscMarkupQty3),
                        //DiscMarkupQty4 = double.Parse(x.DiscMarkupQty4),
                        //DiscMarkupQty5 = double.Parse(x.DiscMarkupQty5),
                        QtyonPO = double.Parse(x.QtyonPO),
                        QtyonSO = double.Parse(x.QtyonSO),
                        QtyTempCommitted = double.Parse(x.QtyTempCommitted),
                        QtyShippednotCost = double.Parse(x.QtyShippednotCost),
                        LastShipmentDate = DateTime.Parse(x.LastShipmentDate),
                        LastRcptDate = DateTime.Parse(x.LastRcptDate),
                        MostRecentCost = double.Parse(x.MostRecentCost),
                        MarkupFactor = double.Parse(x.MarkupFactor),
                        PrevPriceChangeDate = DateTime.Parse(x.PrevPriceChangeDate),
                        PrevPrice = double.Parse(x.PrevPrice),
                        PrevStdCostDate = DateTime.Parse(x.PrevStdCostDate),
                        PrevStdCost = double.Parse(x.PrevStdCost),
                        PrevRecentCostDate = DateTime.Parse(x.PrevRecentCostDate),
                        PrevRecentCost = double.Parse(x.PrevRecentCost),
                        //Rcpt1 = x.Rcpt1,
                        //Rcpt2 = x.Rcpt2,
                        //Rcpt3 = x.Rcpt3,
                        //Rcpt4 = x.Rcpt4,
                        //Rcpt5 = x.Rcpt5,
                        //RcptDate1 = x.RcptDate1,
                        //RcptDate2 = x.RcptDate2,
                        //RcptDate3 = x.RcptDate3,
                        //RcptDate4 = x.RcptDate4,
                        //RcptDate5 = x.RcptDate5,
                        QtyonHand1 = double.Parse(x.QtyonHand1),
                        QtyonHand2 = double.Parse(x.QtyonHand2),
                        QtyonHand3 = double.Parse(x.QtyonHand3),
                        QtyonHand4 = double.Parse(x.QtyonHand4),
                        QtyonHand5 = double.Parse(x.QtyonHand5),
                        TotalCost1 = double.Parse(x.TotalCost1),
                        TotalCost2 = double.Parse(x.TotalCost2),
                        TotalCost3 = double.Parse(x.TotalCost3),
                        TotalCost4 = double.Parse(x.TotalCost4),
                        TotalCost5 = double.Parse(x.TotalCost5),
                        AvgDaysBetweenShipments = double.Parse(x.AvgDaysBetweenShipments),
                        AvgUnitsShipped = double.Parse(x.AvgUnitsShipped),
                        ShipmentCount = double.Parse(x.ShipmentCount),
                        UnitsSoldPeriod1 = double.Parse(x.UnitsSoldPeriod1),
                        UnitsSoldPeriod2 = double.Parse(x.UnitsSoldPeriod2),
                        UnitsSoldPeriod3 = double.Parse(x.UnitsSoldPeriod3),
                        UnitsSoldPeriod4 = double.Parse(x.UnitsSoldPeriod4),
                        UnitsSoldPeriod5 = double.Parse(x.UnitsSoldPeriod5),
                        UnitsSoldPeriod6 = double.Parse(x.UnitsSoldPeriod6),
                        UnitsSoldPeriod7 = double.Parse(x.UnitsSoldPeriod7),
                        UnitsSoldPeriod8 = double.Parse(x.UnitsSoldPeriod8),
                        UnitsSoldPeriod9 = double.Parse(x.UnitsSoldPeriod9),
                        UnitsSoldPeriod10 = double.Parse(x.UnitsSoldPeriod10),
                        UnitsSoldPeriod11 = double.Parse(x.UnitsSoldPeriod11),
                        UnitsSoldPeriod12 = double.Parse(x.UnitsSoldPeriod12),
                        UnitsSoldPeriod13 = double.Parse(x.UnitsSoldPeriod13),
                        UnitsSoldYTD = double.Parse(x.UnitsSoldYTD),
                        UnitsSoldLY = double.Parse(x.UnitsSoldLY),
                        AmtSoldPeriod1 = double.Parse(x.AmtSoldPeriod1),
                        AmtSoldPeriod2 = double.Parse(x.AmtSoldPeriod2),
                        AmtSoldPeriod3 = double.Parse(x.AmtSoldPeriod3),
                        AmtSoldPeriod4 = double.Parse(x.AmtSoldPeriod4),
                        AmtSoldPeriod5 = double.Parse(x.AmtSoldPeriod5),
                        AmtSoldPeriod6 = double.Parse(x.AmtSoldPeriod6),
                        AmtSoldPeriod7 = double.Parse(x.AmtSoldPeriod7),
                        AmtSoldPeriod8 = double.Parse(x.AmtSoldPeriod8),
                        AmtSoldPeriod9 = double.Parse(x.AmtSoldPeriod9),
                        AmtSoldPeriod10 = double.Parse(x.AmtSoldPeriod10),
                        AmtSoldPeriod11 = double.Parse(x.AmtSoldPeriod11),
                        AmtSoldPeriod12 = double.Parse(x.AmtSoldPeriod12),
                        AmtSoldPeriod13 = double.Parse(x.AmtSoldPeriod13),
                        AmtSoldYTD = double.Parse(x.AmtSoldYTD),
                        AmtSoldLY = double.Parse(x.AmtSoldLY),
                        TotalCostsPeriod1 = double.Parse(x.TotalCostsPeriod1),
                        TotalCostsPeriod2 = double.Parse(x.TotalCostsPeriod2),
                        TotalCostsPeriod3 = double.Parse(x.TotalCostsPeriod3),
                        TotalCostsPeriod4 = double.Parse(x.TotalCostsPeriod4),
                        TotalCostsPeriod5 = double.Parse(x.TotalCostsPeriod5),
                        TotalCostsPeriod6 = double.Parse(x.TotalCostsPeriod6),
                        TotalCostsPeriod7 = double.Parse(x.TotalCostsPeriod7),
                        TotalCostsPeriod8 = double.Parse(x.TotalCostsPeriod8),
                        TotalCostsPeriod9 = double.Parse(x.TotalCostsPeriod9),
                        TotalCostsPeriod10 = double.Parse(x.TotalCostsPeriod10),
                        TotalCostsPeriod11 = double.Parse(x.TotalCostsPeriod11),
                        TotalCostsPeriod12 = double.Parse(x.TotalCostsPeriod12),
                        TotalCostsPeriod13 = double.Parse(x.TotalCostsPeriod13),
                        TotalCostsYTD = double.Parse(x.TotalCostsYTD),
                        TotalCostsLY = double.Parse(x.TotalCostsLY),
                        UnitsLostYTD = double.Parse(x.UnitsLostYTD),
                        UnitsLostLY = double.Parse(x.UnitsLostLY),
                        DiscMarkupAmt1 = double.Parse(x.DiscMarkupAmt1),
                        DiscMarkupAmt2 = double.Parse(x.DiscMarkupAmt2),
                        DiscMarkupAmt3 = double.Parse(x.DiscMarkupAmt3),
                        DiscMarkupAmt4 = double.Parse(x.DiscMarkupAmt4),
                        DiscMarkupAmt5 = double.Parse(x.DiscMarkupAmt5),
                        QtyOnHand = double.Parse(x.QtyOnHand),
                        Selected = double.Parse(x.Selected),
                        ActiveItem = x.ActiveItem,
                        RNumAN81CITM = x.RNumAN81CITM

                    }).ToList();
                    db.AdagioItems.AddRange(castedResults);
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Update Successful");
        }

        [HttpGet]
        public HttpResponseMessage GetTodaysProductionTotal()
        {
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var db = new AppEntities();
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };
            List<TodaysProductionTotal> results = new List<TodaysProductionTotal>();
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetTodaysProductionTotal", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                json_serializer.MaxJsonLength = Int32.MaxValue;
                TodaysProductionTotal[] resultsArray = json_serializer.Deserialize<TodaysProductionTotal[]>(response.Content.ReadAsStringAsync().Result);
                results = resultsArray.ToList();
                if (results.Count() > 0)
                {
                    db.Database.ExecuteSqlCommand("DELETE FROM TodaysProductionTotals");
                    List<TodaysProductionTotal> castedResults = results.Select(x => new TodaysProductionTotal
                    {
                       Weight = x.Weight,
                       Nominal = x.Nominal,
                       Station = x.Station

                    }).ToList();
                    db.TodaysProductionTotals.AddRange(castedResults);
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Update Successful");
        }

        [HttpGet]
        public HttpResponseMessage GetAdagioCustomers()
        {
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var db = new AppEntities();
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };
            List<dtoAdagioCustomer> results = new List<dtoAdagioCustomer>();
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetAdagioCustomers", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                json_serializer.MaxJsonLength = Int32.MaxValue;
                dtoAdagioCustomer[] resultsArray = json_serializer.Deserialize<dtoAdagioCustomer[]>(response.Content.ReadAsStringAsync().Result);
                results = resultsArray.ToList();
                if (results.Count() > 0)
                {
                    db.Database.ExecuteSqlCommand("DELETE FROM AdagioCustomers");
                    double value = 0;
                    DateTime dValue = DateTime.Now;
                    List<AdagioCustomer> castedResults = results.Select(x => new AdagioCustomer
                    {
                        Activestatus = x.Activestatus,
                        Cust = x.Cust,
                        CtrlAcctSet = x.CtrlAcctSet,
                        BillingCycle = x.BillingCycle,
                        Name = x.Name,
                        Name2 = x.Name2,
                        //CustDecimals = double.Parse(x.CustDecimals),
                        //CreditLimit = double.Parse(x.CreditLimit),
                        OnHold = double.TryParse(x.OnHold, out value) ? double.Parse(x.OnHold) : 0,
                        Alert = double.TryParse(x.Alert, out value) ? double.Parse(x.Alert) : 0,
                        Terms = x.Terms,
                        CustCurrency = x.CustCurrency,
                        Accounttype_T = x.Accounttype_T,
                        BalanceOutstanding = double.TryParse(x.BalanceOutstanding, out value) ? double.Parse(x.BalanceOutstanding) : 0,
                        ShortName = x.ShortName,
                        Address1 = x.Address1,
                        Address2 = x.Address2,
                        Address3 = x.Address3,
                        Address4 = x.Address4,
                        ZipPostal = x.ZipPostal,
                        //Formataddress = x.Formataddress,
                        City = x.City,
                        StateProvince = x.StateProvince,
                        Country = x.Country,
                        Telephone = x.Telephone,
                        Fax = x.Fax,
                        Comments1 = x.Comments1,
                        Comments2 = x.Comments2,
                        Contact = x.Contact,
                        Salesperson = double.TryParse(x.Salesperson, out value) ? double.Parse(x.Salesperson) : 0,
                        //PrintStmts = x.PrintStmts,
                        //ChargeInt = x.ChargeInt,
                        TaxExemptID1 = x.TaxExemptID1,
                        TaxExemptID2 = x.TaxExemptID2,
                        //TaxStatus = x.TaxStatus,
                        TaxGroup = x.TaxGroup,
                        CustType = x.CustType,
                        ReportGroup = x.ReportGroup,
                        DefaultDistributionSet = x.DefaultDistributionSet,
                        DefaultDistributionCode = x.DefaultDistributionCode,
                        Territory = x.Territory,
                        ShipVia = x.ShipVia,
                        StartDate = DateTime.TryParse(x.StartDate, out dValue) ? DateTime.Parse(x.StartDate) : DateTime.Now,
                        CustID = x.CustID,
                        BalanceOutstandingHome = double.TryParse(x.BalanceOutstandingHome, out value) ? double.Parse(x.BalanceOutstandingHome) : 0,
                        BalanceasofLastStmt = double.TryParse(x.BalanceasofLastStmt, out value) ? double.Parse(x.BalanceasofLastStmt) : 0,
                        OpenInvsCount = double.TryParse(x.OpenInvsCount, out value) ? double.Parse(x.OpenInvsCount) : 0,
                        RetainageTrxsCount = double.TryParse(x.RetainageTrxsCount, out value) ? double.Parse(x.RetainageTrxsCount) : 0,
                        RetainageOutstanding = double.TryParse(x.RetainageOutstanding, out value) ? double.Parse(x.RetainageOutstanding) : 0,
                        RetainageOutstandingHome = double.TryParse(x.RetainageOutstandingHome, out value) ? double.Parse(x.RetainageOutstandingHome) : 0,
                        InvTotalPTD = double.TryParse(x.InvTotalPTD, out value) ? double.Parse(x.InvTotalPTD) : 0,
                        InvCountPTD = double.TryParse(x.InvCountPTD, out value) ? double.Parse(x.InvCountPTD) : 0,
                        PaymentTotalPTD = double.TryParse(x.PaymentTotalPTD, out value) ? double.Parse(x.PaymentTotalPTD) : 0,
                        PaymentCountPTD = double.TryParse(x.PaymentCountPTD, out value) ? double.Parse(x.PaymentCountPTD) : 0,
                        CNTotalPTD = double.TryParse(x.CNTotalPTD, out value) ? double.Parse(x.CNTotalPTD) : 0,
                        CNCountPTD = double.TryParse(x.CNCountPTD, out value) ? double.Parse(x.CNCountPTD) : 0,
                        DNTotalPTD = double.TryParse(x.DNTotalPTD, out value) ? double.Parse(x.DNTotalPTD) : 0,
                        DNCountPTD = double.TryParse(x.DNCountPTD, out value) ? double.Parse(x.DNCountPTD) : 0,
                        DiscTotalPTD = double.TryParse(x.DiscTotalPTD, out value) ? double.Parse(x.DiscTotalPTD) : 0,
                        DiscCountPTD = double.TryParse(x.DiscCountPTD, out value) ? double.Parse(x.DiscCountPTD) : 0,
                        IntTotalPTD = double.TryParse(x.IntTotalPTD, out value) ? double.Parse(x.IntTotalPTD) : 0,
                        IntCountPTD = double.TryParse(x.IntCountPTD, out value) ? double.Parse(x.IntCountPTD) : 0,
                        InvTotalYTD = double.TryParse(x.InvTotalYTD, out value) ? double.Parse(x.InvTotalYTD) : 0,
                        InvCountYTD = double.TryParse(x.InvCountYTD, out value) ? double.Parse(x.InvCountYTD) : 0,
                        PaymentTotalYTD = double.TryParse(x.PaymentTotalYTD, out value) ? double.Parse(x.PaymentTotalYTD) : 0,
                        PaymentCountYTD = double.TryParse(x.PaymentCountYTD, out value) ? double.Parse(x.PaymentCountYTD) : 0,
                        CNTotalYTD = double.TryParse(x.CNTotalYTD, out value) ? double.Parse(x.CNTotalYTD) : 0,
                        CNCountYTD = double.TryParse(x.CNCountYTD, out value) ? double.Parse(x.CNCountYTD) : 0,
                        DNTotalYTD = double.TryParse(x.DNTotalYTD, out value) ? double.Parse(x.DNTotalYTD) : 0,
                        DNCountYTD = double.TryParse(x.DNCountYTD, out value) ? double.Parse(x.DNCountYTD) : 0,
                        DiscTotalYTD = double.TryParse(x.DiscTotalYTD, out value) ? double.Parse(x.DiscTotalYTD) : 0,
                        DiscCountYTD = double.TryParse(x.DiscCountYTD, out value) ? double.Parse(x.DiscCountYTD) : 0,
                        IntTotalYTD = double.TryParse(x.IntTotalYTD, out value) ? double.Parse(x.IntTotalYTD) : 0,
                        IntCountYTD = double.TryParse(x.IntCountYTD, out value) ? double.Parse(x.IntCountYTD) : 0,
                        InvTotalLY = double.TryParse(x.InvTotalLY, out value) ? double.Parse(x.InvTotalLY) : 0,
                        InvCountLY = double.TryParse(x.InvCountLY, out value) ? double.Parse(x.InvCountLY) : 0,
                        PaymentTotalLY = double.TryParse(x.PaymentTotalLY, out value) ? double.Parse(x.PaymentTotalLY) : 0,
                        PaymentCountLY = double.TryParse(x.PaymentCountLY, out value) ? double.Parse(x.PaymentCountLY) : 0,
                        CNTotalLY = double.TryParse(x.CNTotalLY, out value) ? double.Parse(x.CNTotalLY) : 0,
                        CNCountLY = double.TryParse(x.CNCountLY, out value) ? double.Parse(x.CNCountLY) : 0,
                        DNTotalLY = double.TryParse(x.DNTotalLY, out value) ? double.Parse(x.DNTotalLY) : 0,
                        DNCountLY = double.TryParse(x.DNCountLY, out value) ? double.Parse(x.DNCountLY) : 0,
                        DiscTotalLY = double.TryParse(x.DiscTotalLY, out value) ? double.Parse(x.DiscTotalLY) : 0,
                        DiscCountLY = double.TryParse(x.DiscCountLY, out value) ? double.Parse(x.DiscCountLY) : 0,
                        IntTotalLY = double.TryParse(x.IntTotalLY, out value) ? double.Parse(x.IntTotalLY) : 0,
                        IntCountLY = double.TryParse(x.IntCountLY, out value) ? double.Parse(x.IntCountLY) : 0,
                        LastAdjDate = DateTime.TryParse(x.LastAdjDate, out dValue) ? DateTime.Parse(x.LastAdjDate) : DateTime.Now,
                        LastPostedDate = DateTime.TryParse(x.LastPostedDate, out dValue) ? DateTime.Parse(x.LastPostedDate) : DateTime.Now,
                        LastIntDate = x.LastIntDate,
                        LastPurgeDate = DateTime.TryParse(x.LastPurgeDate, out dValue) ? DateTime.Parse(x.LastPurgeDate) : DateTime.Now,
                        LastStmtDate = x.LastStmtDate,
                        LastRevalueDate = x.LastRevalueDate,
                        HighInvAmt = double.TryParse(x.HighInvAmt, out value) ? double.Parse(x.HighInvAmt) : 0,
                        HighInvDate = DateTime.TryParse(x.HighInvDate, out dValue) ? DateTime.Parse(x.HighInvDate) : DateTime.Now,
                        HighBalanceAmt = double.TryParse(x.HighBalanceAmt, out value) ? double.Parse(x.HighBalanceAmt) : 0,
                        HighBalanceDate = DateTime.TryParse(x.HighBalanceDate, out dValue) ? DateTime.Parse(x.HighBalanceDate) : DateTime.Now,
                        LastInvAmt = double.TryParse(x.LastInvAmt, out value) ? double.Parse(x.LastInvAmt) : 0,
                        LastInvDate = DateTime.TryParse(x.LastInvDate, out dValue) ? DateTime.Parse(x.LastInvDate) : DateTime.Now,
                        LastPaymentAmt = double.TryParse(x.LastPaymentAmt, out value) ? double.Parse(x.LastPaymentAmt) : 0,
                        LastPaymentDate = DateTime.TryParse(x.LastPaymentDate, out dValue) ? DateTime.Parse(x.LastPaymentDate) : DateTime.Now,
                        LastCNAmt = double.TryParse(x.LastCNAmt, out value) ? double.Parse(x.LastCNAmt) : 0,
                        LastCNDate = DateTime.TryParse(x.LastCNDate, out dValue) ? DateTime.Parse(x.LastCNDate) : DateTime.Now,
                        HighInvAmtLY = double.TryParse(x.HighInvAmtLY, out value) ? double.Parse(x.HighInvAmtLY) : 0,
                        HighInvDateLY = DateTime.TryParse(x.HighInvDateLY, out dValue) ? DateTime.Parse(x.HighInvDateLY) : DateTime.Now,
                        HighBalanceAmtLY = double.TryParse(x.HighBalanceAmtLY, out value) ? double.Parse(x.HighBalanceAmtLY) : 0,
                        HighBalanceDateLY = DateTime.TryParse(x.HighBalanceDateLY, out dValue) ? DateTime.Parse(x.HighBalanceDateLY) : DateTime.Now,
                        TotalDaystoPay = double.TryParse(x.TotalDaystoPay, out value) ? double.Parse(x.TotalDaystoPay) : 0,
                        TotalInvsPaid = double.TryParse(x.TotalInvsPaid, out value) ? double.Parse(x.TotalInvsPaid) : 0,
                        AvgTimetoPayLY = double.TryParse(x.AvgTimetoPayLY, out value) ? double.Parse(x.AvgTimetoPayLY) : 0,
                        LatestDate = DateTime.TryParse(x.LatestDate, out dValue) ? DateTime.Parse(x.LatestDate) : DateTime.Now,
                        LatestDateDue = DateTime.TryParse(x.LatestDateDue, out dValue) ? DateTime.Parse(x.LatestDateDue) : DateTime.Now,
                        PriceList = x.PriceList,
                        ContactEmail = x.ContactEmail,
                        InvoicesEmail = x.InvoicesEmail,
                        StatementsEmail = x.StatementsEmail,
                        InvoicesContact = x.InvoicesContact,
                        StatementsContact = x.StatementsContact,
                        Website = x.Website,
                        Creditcardnumber1 = x.Creditcardnumber1,
                        ExpiryDate1 = x.ExpiryDate1,
                        NameonCard1 = x.NameonCard1,
                        Creditcardnumber2 = x.Creditcardnumber2,
                        ExpiryDate2 = x.ExpiryDate2,
                        NameonCard2 = x.NameonCard2,
                        //PrintStatementMethod = x.PrintStatementMethod,
                        //FaxStatementMethod = x.FaxStatementMethod,
                        //EmailStatementMethod = x.EmailStatementMethod,
                        //PrintInvoiceMethod = x.PrintInvoiceMethod,
                        //FaxInvoiceMethod = x.FaxInvoiceMethod,
                        //EmailInvoiceMethod = x.EmailInvoiceMethod,
                        StatementPrintSpec = x.StatementPrintSpec,
                        StatementFaxSpec = x.StatementFaxSpec,
                        StatementEmailSpec = x.StatementEmailSpec,
                        StatementEmailCoverCode = x.StatementEmailCoverCode,
                        CustOptionalText1 = x.CustOptionalText1,
                        CustOptionalText2 = x.CustOptionalText2,
                        CustOptionalText3 = x.CustOptionalText3,
                        CustOptionalDate1 = x.CustOptionalDate1,
                        CustOptionalDate2 = x.CustOptionalDate2,
                        //CustOptionalAmount1 = x.CustOptionalAmount1,
                        //CustOptionalAmount2 = x.CustOptionalAmount2,
                        //CustOptionalUnits1 = x.CustOptionalUnits1,
                        //CustOptionalUnits2 = x.CustOptionalUnits2,
                        OEShipFromLocation = x.OEShipFromLocation,
                        OEShiptoCode = x.OEShiptoCode,
                        OEFOBPoint = x.OEFOBPoint,
                        OEOrderConfEmailCover = x.OEOrderConfEmailCover,
                        OEInvoiceEmailCover = x.OEInvoiceEmailCover,
                        OECreditNoteEmailCover = x.OECreditNoteEmailCover,
                        OEOrderConfPrintSpec = x.OEOrderConfPrintSpec,
                        OEInvoicePrintSpec = x.OEInvoicePrintSpec,
                        OECreditNotePrintSpec = x.OECreditNotePrintSpec,
                        OEPickingSlipPrintSpec = x.OEPickingSlipPrintSpec,
                        OEOrderConfFaxSpec = x.OEOrderConfFaxSpec,
                        OEInvoiceFaxSpec = x.OEInvoiceFaxSpec,
                        OECreditNoteFaxSpec = x.OECreditNoteFaxSpec,
                        OEOrderConfEmailSpec = x.OEOrderConfEmailSpec,
                        OEInvoiceEmailSpec = x.OEInvoiceEmailSpec,
                        OECreditNoteEmailSpec = x.OECreditNoteEmailSpec,
                        INTextCode = x.INTextCode,
                        INFOBPoint = x.INFOBPoint,
                        INInvoiceEmailCover = x.INInvoiceEmailCover,
                        INCreditNoteEmailCover = x.INCreditNoteEmailCover,
                        INShipFromLocation = x.INShipFromLocation,
                        INInvoicePrintSpec = x.INInvoicePrintSpec,
                        INCreditNotePrintSpec = x.INCreditNotePrintSpec,
                        INinvoiceFaxSpec = x.INinvoiceFaxSpec,
                        INCreditNoteFaxSpec = x.INCreditNoteFaxSpec,
                        INInvoiceEmailSpec = x.INInvoiceEmailSpec,
                        INCreditNoteEmailSpec = x.INCreditNoteEmailSpec,
                        Agingtype_T = x.Agingtype_T,
                        AgedasofDate = DateTime.TryParse(x.AgedasofDate, out dValue) ? DateTime.Parse(x.AgedasofDate) : DateTime.Now,
                        AgedCurrent = double.TryParse(x.AgedCurrent, out value) ? double.Parse(x.AgedCurrent) : 0,
                        AgedBucket1 = double.TryParse(x.AgedBucket1, out value) ? double.Parse(x.AgedBucket1) : 0,
                        AgedBucket2 = double.TryParse(x.AgedBucket2, out value) ? double.Parse(x.AgedBucket2) : 0,
                        AgedBucket3 = double.TryParse(x.AgedBucket3, out value) ? double.Parse(x.AgedBucket3) : 0,
                        AgedBucket4 = double.TryParse(x.AgedBucket4, out value) ? double.Parse(x.AgedBucket4) : 0,
                        AgedDays1 = double.TryParse(x.AgedDays1, out value) ? double.Parse(x.AgedDays1) : 0,
                        AgedDays2 = double.TryParse(x.AgedDays2, out value) ? double.Parse(x.AgedDays2) : 0,
                        AgedDays3 = double.TryParse(x.AgedDays3, out value) ? double.Parse(x.AgedDays3) : 0,
                        DefaultPaymentCode = x.DefaultPaymentCode,
                        CC1PaymentCode = x.CC1PaymentCode,
                        CC2PaymentCode = x.CC2PaymentCode,
                        //PADEnabled = x.PADEnabled,
                        PADAccountNumber = x.PADAccountNumber,
                        PADbranchnumber = x.PADbranchnumber,
                        PADTransitNumber = x.PADTransitNumber,
                        //LastMaintenanceDate = x.LastMaintenanceDate,
                        //LastMaintenanceTime = x.LastMaintenanceTime,
                        LastMaintenanceUser = x.LastMaintenanceUser,
                        CustomerNameIndexUppercase = x.CustomerNameIndexUppercase,
                        Prospectcode = x.Prospectcode,
                        PayablesVendorcode = x.PayablesVendorcode,
                        DUNS = x.DUNS,
                        //DefaultOEDiscount = x.DefaultOEDiscount,
                        INShiptoCode = x.INShiptoCode,
                        ManualStyleCode = x.ManualStyleCode,
                        AutomaticStyleCode = x.AutomaticStyleCode,
                        //Payablesuseaddress = x.Payablesuseaddress,
                        Payablesusecontactcode = x.Payablesusecontactcode,
                        //Autoattachunpaidinvoices = x.Autoattachunpaidinvoices,
                        Preferredstatementmethod_T = x.Preferredstatementmethod_T,
                        Preferredinvoicemethod_T = x.Preferredinvoicemethod_T,
                        Oldestunpaidinvoicedate = x.Oldestunpaidinvoicedate,
                        RNumAR90ACST = double.TryParse(x.RNumAR90ACST, out value) ? double.Parse(x.RNumAR90ACST) : 0

                    }).ToList();

                    db.AdagioCustomers.AddRange(castedResults);
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Update Successful");
        }


        [HttpGet]
        public HttpResponseMessage GetAdagioOrdersForToday()
        {
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var db = new AppEntities();
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };
            List<Adagio_O_OrderHeaderDTO> results = new List<Adagio_O_OrderHeaderDTO>();
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetAdagioOrdersForToday", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                json_serializer.MaxJsonLength = Int32.MaxValue;
                Adagio_O_OrderHeaderDTO[] resultsArray = json_serializer.Deserialize<Adagio_O_OrderHeaderDTO[]>(response.Content.ReadAsStringAsync().Result);
                results = resultsArray.ToList();
                if (results.Count() > 0)
                {
                    double value = 0;
                    DateTime dValue = DateTime.Now;
                    db.Database.ExecuteSqlCommand("DELETE FROM AdagioTodayOrders");
                    List<AdagioTodayOrder> castedResults = results.Select(x => new AdagioTodayOrder
                    {
                        CustPrefix = x.CustPrefix,
                        Cust = double.TryParse(x.Cust, out value) ? double.Parse(x.Cust) : 0,
                        OrderKey = x.OrderKey,
                        DocType_T = x.DocType_T,
                        Doc = double.TryParse(x.Doc, out value) ? double.Parse(x.Doc) : 0,
                        Order_2 = double.TryParse(x.Order_2, out value) ? double.Parse(x.Order_2) : 0,
                        Inv = double.TryParse(x.Inv, out value) ? double.Parse(x.Inv) : 0,
                        OrderType_T = x.OrderType_T,
                        OrderRelease = double.TryParse(x.OrderRelease, out value) ? double.Parse(x.OrderRelease) : 0,
                        PrintStatus_T = x.PrintStatus_T,
                        CostedStatus = double.TryParse(x.CostedStatus, out value) ? double.Parse(x.CostedStatus) : 0,
                        OnHold = double.TryParse(x.OnHold, out value) ? double.Parse(x.OnHold) : 0,
                        Posting = double.TryParse(x.Posting, out value) ? double.Parse(x.Posting) : 0,
                        InventoryUpdated = double.TryParse(x.InventoryUpdated, out value) ? double.Parse(x.InventoryUpdated) : 0,
                        OrderComplete = double.TryParse(x.OrderComplete, out value) ? double.Parse(x.OrderComplete) : 0,
                        OrderDate = DateTime.TryParse(x.OrderDate, out dValue) ? DateTime.Parse(x.OrderDate) : DateTime.Now,
                        InvDate = x.InvDate,
                        ShipmentDate = x.ShipmentDate,
                        ExpectedShipDate = DateTime.TryParse(x.ExpectedShipDate, out dValue) ? DateTime.Parse(x.ExpectedShipDate) : DateTime.Now,
                        ShipVia = x.ShipVia,
                        Comment1 = x.Comment1,
                        Comment2 = x.Comment2,
                        Loc = double.TryParse(x.Loc, out value) ? double.Parse(x.Loc) : 0,
                        CreditApprovalPerson = x.CreditApprovalPerson,
                        FOBPoint = x.FOBPoint,
                        TaxGroup = x.TaxGroup,
                        TotalOrderValueHome = double.TryParse(x.TotalOrderValueHome, out value) ? double.Parse(x.TotalOrderValueHome) : 0,
                        LinesinOrder = double.TryParse(x.LinesinOrder, out value) ? double.Parse(x.LinesinOrder) : 0,
                        InvDisc = double.TryParse(x.InvDisc, out value) ? double.Parse(x.InvDisc) : 0,
                        Reference = x.Reference,
                        Contact = x.Contact,
                        TaxExempt1 = x.TaxExempt1,
                        TaxExempt2 = x.TaxExempt2,
                        Terms = x.Terms,
                        Salesperson = double.TryParse(x.Salesperson, out value) ? double.Parse(x.Salesperson) : 0,
                        CustTaxStatus = double.TryParse(x.CustTaxStatus, out value) ? double.Parse(x.CustTaxStatus) : 0,
                        PreAR90Territory = x.PreAR90Territory,
                        CustDiscLevel = x.CustDiscLevel,
                        Name = x.Name,
                        BillingAddress1 = x.BillingAddress1,
                        BillingAddress2 = x.BillingAddress2,
                        BillingAddress3 = x.BillingAddress3,
                        BillingAddress4 = x.BillingAddress4,
                        BillingZipPostal = x.BillingZipPostal,
                        ShipName = x.ShipName,
                        ShipAddress1 = x.ShipAddress1,
                        ShipAddress2 = x.ShipAddress2,
                        ShipAddress3 = x.ShipAddress3,
                        ShipAddress4 = x.ShipAddress4,
                        ShipZipPostal = x.ShipZipPostal,
                        LabelsCount = double.TryParse(x.LabelsCount, out value) ? double.Parse(x.LabelsCount) : 0,
                        GLPosting = double.TryParse(x.GLPosting, out value) ? double.Parse(x.GLPosting) : 0,
                        ARPosting = double.TryParse(x.ARPosting, out value) ? double.Parse(x.ARPosting) : 0,
                        TermsCode = x.TermsCode,
                        Bank = x.Bank,
                        CashAcct = double.TryParse(x.CashAcct, out value) ? double.Parse(x.CashAcct) : 0,
                        CashDept = x.CashDept,
                        Cheque = x.Cheque,
                        DateDue = DateTime.TryParse(x.DateDue, out dValue) ? DateTime.Parse(x.DateDue) : DateTime.Now,
                        DateDisc = x.DateDisc,
                        TermsDisc = double.TryParse(x.TermsDisc, out value) ? double.Parse(x.TermsDisc) : 0,
                        SourceDecimals = double.TryParse(x.SourceDecimals, out value) ? double.Parse(x.SourceDecimals) : 0,
                        HomeCurr = x.HomeCurr,
                        RateType = x.RateType,
                        SourceCurr = x.SourceCurr,
                        RateDate = DateTime.TryParse(x.RateDate, out dValue) ? DateTime.Parse(x.RateDate) : DateTime.Now,
                        ExchgRate = double.TryParse(x.ExchgRate, out value) ? double.Parse(x.ExchgRate) : 0,
                        ExchgSpread = double.TryParse(x.ExchgSpread, out value) ? double.Parse(x.ExchgSpread) : 0,
                        RateRep = x.RateRep,
                        DateMatching = x.DateMatching,
                        BillingTelephone = x.BillingTelephone,
                        BillingFax = x.BillingFax,
                        ShipTelephone = x.ShipTelephone,
                        ShipFax = x.ShipFax,
                        PriceList = x.PriceList,
                        CashDisc = double.TryParse(x.CashDisc, out value) ? double.Parse(x.CashDisc) : 0,
                        ForeignHomeCurr = x.ForeignHomeCurr,
                        ForeignRateType = x.ForeignRateType,
                        ForeignSourceCurr = x.ForeignSourceCurr,
                        ForeignRateDate = DateTime.TryParse(x.ForeignRateDate, out dValue) ? DateTime.Parse(x.ForeignRateDate) : DateTime.Now,
                        ForeignRate = double.TryParse(x.ForeignRate, out value) ? double.Parse(x.ForeignRate) : 0,
                        ForeignSpread = double.TryParse(x.ForeignSpread, out value) ? double.Parse(x.ForeignSpread) : 0,
                        ForeignRateRep = x.ForeignRateRep,
                        ForeignDateMatching = x.ForeignDateMatching,
                        RateOverride = double.TryParse(x.RateOverride, out value) ? double.Parse(x.RateOverride) : 0,
                        ForeignRateOverride = double.TryParse(x.ForeignRateOverride, out value) ? double.Parse(x.ForeignRateOverride) : 0,
                        ForeignDecimals = double.TryParse(x.ForeignDecimals, out value) ? double.Parse(x.ForeignDecimals) : 0,
                        DiscAcct = double.TryParse(x.DiscAcct, out value) ? double.Parse(x.DiscAcct) : 0,
                        DiscDept = x.DiscDept,
                        RoundAcct = x.RoundAcct,
                        RoundDept = x.RoundDept,
                        TaxByInv = double.TryParse(x.TaxByInv, out value) ? double.Parse(x.TaxByInv) : 0,
                        TotalOrderValue = double.TryParse(x.TotalOrderValue, out value) ? double.Parse(x.TotalOrderValue) : 0,
                        TotalDollarValue = double.TryParse(x.TotalDollarValue, out value) ? double.Parse(x.TotalDollarValue) : 0,
                        InvDiscAmt = double.TryParse(x.InvDiscAmt, out value) ? double.Parse(x.InvDiscAmt) : 0,
                        Tax1 = double.TryParse(x.Tax1, out value) ? double.Parse(x.Tax1) : 0,
                        Tax2 = double.TryParse(x.Tax2, out value) ? double.Parse(x.Tax2) : 0,
                        Tax3 = double.TryParse(x.Tax3, out value) ? double.Parse(x.Tax3) : 0,
                        Tax4 = double.TryParse(x.Tax4, out value) ? double.Parse(x.Tax4) : 0,
                        Tax5 = double.TryParse(x.Tax5, out value) ? double.Parse(x.Tax5) : 0,
                        TotalCostValue = double.TryParse(x.TotalCostValue, out value) ? double.Parse(x.TotalCostValue) : 0,
                        CashPayment = double.TryParse(x.CashPayment, out value) ? double.Parse(x.CashPayment) : 0,
                        DiscBase = double.TryParse(x.DiscBase, out value) ? double.Parse(x.DiscBase) : 0,
                        ForeignAmt = double.TryParse(x.ForeignAmt, out value) ? double.Parse(x.ForeignAmt) : 0,
                        MovedtoHistory = double.TryParse(x.MovedtoHistory, out value) ? double.Parse(x.MovedtoHistory) : 0,
                        RNumAO80AHED = x.RNumAO80AHED

                    }).ToList();
                    db.AdagioTodayOrders.AddRange(castedResults);
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Update Successful");
        }


        [HttpGet]
        public HttpResponseMessage GetAdagioOrderDetails()
        {
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var db = new AppEntities();
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };
            List<Adagio_O_OrderDetailDTO> results = new List<Adagio_O_OrderDetailDTO>();
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetAdagioOrderDetails", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                json_serializer.MaxJsonLength = Int32.MaxValue;
                Adagio_O_OrderDetailDTO[] resultsArray = json_serializer.Deserialize<Adagio_O_OrderDetailDTO[]>(response.Content.ReadAsStringAsync().Result);
                results = resultsArray.ToList();
                if (results.Count() > 0)
                {
                    double value = 0;
                    DateTime dValue = DateTime.Now;
                    db.Database.ExecuteSqlCommand("DELETE FROM AdagioOrderDetails");
                    List<AdagioOrderDetail> castedResults = results.Select(x => new AdagioOrderDetail
                    {
                        OrderKey = x.OrderKey,
                        Line = double.TryParse(x.Line, out value) ? double.Parse(x.Line) : 0,
                        DDocType_T = x.DDocType_T,
                        DDoc = double.TryParse(x.DDoc, out value) ? double.Parse(x.DDoc) : 0,
                        LineType_T = x.LineType_T,
                        Item = double.TryParse(x.Item, out value) ? double.Parse(x.Item) : 0,
                        Description = x.Description,
                        Unit = x.Unit,
                        PickingSeq = x.PickingSeq,
                        DiscLevel = x.DiscLevel,
                        DCategory = x.DCategory,
                        PriceUnit = x.PriceUnit,
                        PriceOverride = double.TryParse(x.PriceOverride, out value) ? double.Parse(x.PriceOverride) : 0,
                        ExtensionOverride = double.TryParse(x.ExtensionOverride, out value) ? double.Parse(x.ExtensionOverride) : 0,
                        ReturntoInventory = double.TryParse(x.ReturntoInventory, out value) ? double.Parse(x.ReturntoInventory) : 0,
                        SerialCount = double.TryParse(x.SerialCount, out value) ? double.Parse(x.SerialCount) : 0,
                        TaxStatus = double.TryParse(x.TaxStatus, out value) ? double.Parse(x.TaxStatus) : 0,
                        Commissionable = double.TryParse(x.Commissionable, out value) ? double.Parse(x.Commissionable) : 0,
                        UnitFactor = double.TryParse(x.UnitFactor, out value) ? double.Parse(x.UnitFactor) : 0,
                        UnitWeight = double.TryParse(x.UnitWeight, out value) ? double.Parse(x.UnitWeight) : 0,
                        QtyOriginalOrdered = double.TryParse(x.QtyOriginalOrdered, out value) ? double.Parse(x.QtyOriginalOrdered) : 0,
                        QtyOrdered = double.TryParse(x.QtyOrdered, out value) ? double.Parse(x.QtyOrdered) : 0,
                        QtyShippedtoDate = double.TryParse(x.QtyShippedtoDate, out value) ? double.Parse(x.QtyShippedtoDate) : 0,
                        QtyShipped = double.TryParse(x.QtyShipped, out value) ? double.Parse(x.QtyShipped) : 0,
                        QtyBackordered = double.TryParse(x.QtyBackordered, out value) ? double.Parse(x.QtyBackordered) : 0,
                        ExtWeight = double.TryParse(x.ExtWeight, out value) ? double.Parse(x.ExtWeight) : 0,
                        ShipStockingUnits = double.TryParse(x.ShipStockingUnits, out value) ? double.Parse(x.ShipStockingUnits) : 0,
                        Loc = double.TryParse(x.Loc, out value) ? double.Parse(x.Loc) : 0,
                        Complete = double.TryParse(x.Complete, out value) ? double.Parse(x.Complete) : 0,
                        Curr = x.Curr,
                        Decimals = double.TryParse(x.Decimals, out value) ? double.Parse(x.Decimals) : 0,
                        PriceList = x.PriceList,
                        OrderedInPO = double.TryParse(x.OrderedInPO, out value) ? double.Parse(x.OrderedInPO) : 0,
                        UnitDecimals = double.TryParse(x.UnitDecimals, out value) ? double.Parse(x.UnitDecimals) : 0,
                        UnitPrice = double.TryParse(x.UnitPrice, out value) ? double.Parse(x.UnitPrice) : 0,
                        UnitCost = double.TryParse(x.UnitCost, out value) ? double.Parse(x.UnitCost) : 0,
                        ExtPrice = double.TryParse(x.ExtPrice, out value) ? double.Parse(x.ExtPrice) : 0,
                        ExtCost = double.TryParse(x.ExtCost, out value) ? double.Parse(x.ExtCost) : 0,
                        TaxAmt = double.TryParse(x.TaxAmt, out value) ? double.Parse(x.TaxAmt) : 0,
                        DTax1 = double.TryParse(x.DTax1, out value) ? double.Parse(x.DTax1) : 0,
                        DTax2 = double.TryParse(x.DTax2, out value) ? double.Parse(x.DTax2) : 0,
                        DTax3 = double.TryParse(x.DTax3, out value) ? double.Parse(x.DTax3) : 0,
                        DTax4 = double.TryParse(x.DTax4, out value) ? double.Parse(x.DTax4) : 0,
                        DTax5 = double.TryParse(x.DTax5, out value) ? double.Parse(x.DTax5) : 0,
                        DBase1 = double.TryParse(x.DBase1, out value) ? double.Parse(x.DBase1) : 0,
                        DBase2 = double.TryParse(x.DBase2, out value) ? double.Parse(x.DBase2) : 0,
                        DBase3 = double.TryParse(x.DBase3, out value) ? double.Parse(x.DBase3) : 0,
                        DBase4 = double.TryParse(x.DBase4, out value) ? double.Parse(x.DBase4) : 0,
                        DBase5 = double.TryParse(x.DBase5, out value) ? double.Parse(x.DBase5) : 0,
                        DiscAmt = double.TryParse(x.DiscAmt, out value) ? double.Parse(x.DiscAmt) : 0,
                        DiscExtension = double.TryParse(x.DiscExtension, out value) ? double.Parse(x.DiscExtension) : 0,
                        BasePrice = double.TryParse(x.BasePrice, out value) ? double.Parse(x.BasePrice) : 0,
                        ExtOrderPrice = double.TryParse(x.ExtOrderPrice, out value) ? double.Parse(x.ExtOrderPrice) : 0,
                        PriceFactor = double.TryParse(x.PriceFactor, out value) ? double.Parse(x.PriceFactor) : 0,
                        //Serial1 = double.Parse(x.Serial1),
                        //Serial2 = double.Parse(x.Serial2),
                        //Serial3 = double.Parse(x.Serial3),
                        //Serial4 = double.Parse(x.Serial4),
                        //Serial5 = double.Parse(x.Serial5),
                        DExpectedShipDate = DateTime.TryParse(x.DExpectedShipDate, out dValue) ? DateTime.Parse(x.DExpectedShipDate) : DateTime.Now,
                        MiscAmount = double.TryParse(x.MiscAmount, out value) ? double.Parse(x.MiscAmount) : 0,
                        MiscBasePrice = double.TryParse(x.MiscBasePrice, out value) ? double.Parse(x.MiscBasePrice) : 0,
                        MiscShortDescription = x.MiscShortDescription,
                        MiscQuantity = double.TryParse(x.MiscQuantity, out value) ? double.Parse(x.MiscQuantity) : 0,
                        MiscFiller = x.MiscFiller,
                        RNumAO80ALIN = x.RNumAO80ALIN

                    }).ToList();
                    db.AdagioOrderDetails.AddRange(castedResults);
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Update Successful");
        }

        [HttpGet]
        public HttpResponseMessage GetAdagioOrderHeaders()
        {
            SGApp.DTOs.GenericDTO dto = new GenericDTO();
            var db = new AppEntities();
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://64.139.95.243:7846/")
            };
            List<Adagio_O_OrderHeaderDTO> results = new List<Adagio_O_OrderHeaderDTO>();
            try
            {
                var response = client.PostAsJsonAsync("api/Remote/GetAdagioOrderHeaders", dto).Result;
                response.EnsureSuccessStatusCode();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                json_serializer.MaxJsonLength = Int32.MaxValue;
                Adagio_O_OrderHeaderDTO[] resultsArray = json_serializer.Deserialize<Adagio_O_OrderHeaderDTO[]>(response.Content.ReadAsStringAsync().Result);
                results = resultsArray.ToList();
                if (results.Count() > 0)
                {
                    double value = 0;
                    DateTime dValue = DateTime.Now;
                    db.Database.ExecuteSqlCommand("DELETE FROM AdagioOrderHeaders");
                    List<AdagioOrderHeader> castedResults = results.Select(x => new AdagioOrderHeader
                    {
                        CustPrefix = x.CustPrefix,
                        Cust = double.TryParse(x.Cust, out value) ? double.Parse(x.Cust) : 0,
                        OrderKey = x.OrderKey,
                        DocType_T = x.DocType_T,
                        Doc = double.TryParse(x.Doc, out value) ? double.Parse(x.Doc) : 0,
                        Order_2 = double.TryParse(x.Order_2, out value) ? double.Parse(x.Order_2) : 0,
                        Inv = double.TryParse(x.Inv, out value) ? double.Parse(x.Inv) : 0,
                        OrderType_T = x.OrderType_T,
                        OrderRelease = double.TryParse(x.OrderRelease, out value) ? double.Parse(x.OrderRelease) : 0,
                        PrintStatus_T = x.PrintStatus_T,
                        CostedStatus = double.TryParse(x.CostedStatus, out value) ? double.Parse(x.CostedStatus) : 0,
                        OnHold = double.TryParse(x.OnHold, out value) ? double.Parse(x.OnHold) : 0,
                        Posting = double.TryParse(x.Posting, out value) ? double.Parse(x.Posting) : 0,
                        InventoryUpdated = double.TryParse(x.InventoryUpdated, out value) ? double.Parse(x.InventoryUpdated) : 0,
                        OrderComplete = double.TryParse(x.OrderComplete, out value) ? double.Parse(x.OrderComplete) : 0,
                        OrderDate = DateTime.TryParse(x.OrderDate, out dValue) ? DateTime.Parse(x.OrderDate) : DateTime.Now,
                        InvDate = x.InvDate,
                        ShipmentDate = x.ShipmentDate,
                        ExpectedShipDate = DateTime.TryParse(x.ExpectedShipDate, out dValue) ? DateTime.Parse(x.ExpectedShipDate) : DateTime.Now,
                        ShipVia = x.ShipVia,
                        Comment1 = x.Comment1,
                        Comment2 = x.Comment2,
                        Loc = double.TryParse(x.Loc, out value) ? double.Parse(x.Loc) : 0,
                        CreditApprovalPerson = x.CreditApprovalPerson,
                        FOBPoint = x.FOBPoint,
                        TaxGroup = x.TaxGroup,
                        TotalOrderValueHome = double.TryParse(x.TotalOrderValueHome, out value) ? double.Parse(x.TotalOrderValueHome) : 0,
                        LinesinOrder = double.TryParse(x.LinesinOrder, out value) ? double.Parse(x.LinesinOrder) : 0,
                        InvDisc = double.TryParse(x.InvDisc, out value) ? double.Parse(x.InvDisc) : 0,
                        Reference = x.Reference,
                        Contact = x.Contact,
                        TaxExempt1 = x.TaxExempt1,
                        TaxExempt2 = x.TaxExempt2,
                        Terms = x.Terms,
                        Salesperson = double.TryParse(x.Salesperson, out value) ? double.Parse(x.Salesperson) : 0,
                        CustTaxStatus = double.TryParse(x.CustTaxStatus, out value) ? double.Parse(x.CustTaxStatus) : 0,
                        PreAR90Territory = x.PreAR90Territory,
                        CustDiscLevel = x.CustDiscLevel,
                        Name = x.Name,
                        BillingAddress1 = x.BillingAddress1,
                        BillingAddress2 = x.BillingAddress2,
                        BillingAddress3 = x.BillingAddress3,
                        BillingAddress4 = x.BillingAddress4,
                        BillingZipPostal = x.BillingZipPostal,
                        ShipName = x.ShipName,
                        ShipAddress1 = x.ShipAddress1,
                        ShipAddress2 = x.ShipAddress2,
                        ShipAddress3 = x.ShipAddress3,
                        ShipAddress4 = x.ShipAddress4,
                        ShipZipPostal = x.ShipZipPostal,
                        LabelsCount = double.TryParse(x.LabelsCount, out value) ? double.Parse(x.LabelsCount) : 0,
                        GLPosting = double.TryParse(x.GLPosting, out value) ? double.Parse(x.GLPosting) : 0,
                        ARPosting = double.TryParse(x.ARPosting, out value) ? double.Parse(x.ARPosting) : 0,
                        TermsCode = x.TermsCode,
                        Bank = x.Bank,
                        CashAcct = double.TryParse(x.CashAcct, out value) ? double.Parse(x.CashAcct) : 0,
                        CashDept = x.CashDept,
                        Cheque = x.Cheque,
                        DateDue = DateTime.TryParse(x.DateDue, out dValue) ? DateTime.Parse(x.DateDue) : DateTime.Now,
                        DateDisc = x.DateDisc,
                        TermsDisc = double.TryParse(x.TermsDisc, out value) ? double.Parse(x.TermsDisc) : 0,
                        SourceDecimals = double.TryParse(x.SourceDecimals, out value) ? double.Parse(x.SourceDecimals) : 0,
                        HomeCurr = x.HomeCurr,
                        RateType = x.RateType,
                        SourceCurr = x.SourceCurr,
                        RateDate = DateTime.TryParse(x.RateDate, out dValue) ? DateTime.Parse(x.RateDate) : DateTime.Now,
                        ExchgRate = double.TryParse(x.ExchgRate, out value) ? double.Parse(x.ExchgRate) : 0,
                        ExchgSpread = double.TryParse(x.ExchgSpread, out value) ? double.Parse(x.ExchgSpread) : 0,
                        RateRep = x.RateRep,
                        DateMatching = x.DateMatching,
                        BillingTelephone = x.BillingTelephone,
                        BillingFax = x.BillingFax,
                        ShipTelephone = x.ShipTelephone,
                        ShipFax = x.ShipFax,
                        PriceList = x.PriceList,
                        CashDisc = double.TryParse(x.CashDisc, out value) ? double.Parse(x.CashDisc) : 0,
                        ForeignHomeCurr = x.ForeignHomeCurr,
                        ForeignRateType = x.ForeignRateType,
                        ForeignSourceCurr = x.ForeignSourceCurr,
                        ForeignRateDate = DateTime.TryParse(x.ForeignRateDate, out dValue) ? DateTime.Parse(x.ForeignRateDate) : DateTime.Now,
                        ForeignRate = double.TryParse(x.ForeignRate, out value) ? double.Parse(x.ForeignRate) : 0,
                        ForeignSpread = double.TryParse(x.ForeignSpread, out value) ? double.Parse(x.ForeignSpread) : 0,
                        ForeignRateRep = x.ForeignRateRep,
                        ForeignDateMatching = x.ForeignDateMatching,
                        RateOverride = double.TryParse(x.RateOverride, out value) ? double.Parse(x.RateOverride) : 0,
                        ForeignRateOverride = double.TryParse(x.ForeignRateOverride, out value) ? double.Parse(x.ForeignRateOverride) : 0,
                        ForeignDecimals = double.TryParse(x.ForeignDecimals, out value)? double.Parse(x.ForeignDecimals) : 0,
                        DiscAcct = double.TryParse(x.DiscAcct, out value) ? double.Parse(x.DiscAcct) : 0,
                        DiscDept = x.DiscDept,
                        RoundAcct = x.RoundAcct,
                        RoundDept = x.RoundDept,
                        TaxByInv = double.TryParse(x.TaxByInv, out value) ? double.Parse(x.TaxByInv) : 0,
                        TotalOrderValue = double.TryParse(x.TotalOrderValue, out value) ? double.Parse(x.TotalOrderValue) : 0,
                        TotalDollarValue = double.TryParse(x.TotalDollarValue, out value) ? double.Parse(x.TotalDollarValue) : 0,
                        InvDiscAmt = double.TryParse(x.InvDiscAmt, out value) ? double.Parse(x.InvDiscAmt) : 0,
                        Tax1 = double.TryParse(x.Tax1, out value) ? double.Parse(x.Tax1) : 0,
                        Tax2 = double.TryParse(x.Tax2, out value) ? double.Parse(x.Tax2) : 0,
                        Tax3 = double.TryParse(x.Tax3, out value) ? double.Parse(x.Tax3) : 0,
                        Tax4 = double.TryParse(x.Tax4, out value) ? double.Parse(x.Tax4) : 0,
                        Tax5 = double.TryParse(x.Tax5, out value) ? double.Parse(x.Tax5) : 0,
                        TotalCostValue = double.TryParse(x.TotalCostValue, out value) ? double.Parse(x.TotalCostValue) : 0,
                        CashPayment = double.TryParse(x.CashPayment, out value) ? double.Parse(x.CashPayment) : 0,
                        DiscBase = double.TryParse(x.DiscBase, out value) ? double.Parse(x.DiscBase) : 0,
                        ForeignAmt = double.TryParse(x.ForeignAmt, out value) ? double.Parse(x.ForeignAmt) : 0,
                        MovedtoHistory = double.TryParse(x.MovedtoHistory, out value) ? double.Parse(x.MovedtoHistory) : 0,
                        RNumAO80AHED = x.RNumAO80AHED
                    }).ToList();
                    db.AdagioOrderHeaders.AddRange(castedResults);
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw new HttpException("Error occurred: " + e.Message);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Update Successful");
        }

        [HttpPost]
        public HttpResponseMessage TestPDFer([FromBody] UtilityDTO utilityDto)
        {
            string body = utilityDto.OrderCode;
            var pdfGen = new NReco.PdfGenerator.HtmlToPdfConverter();
            var pdfMargins = new NReco.PdfGenerator.PageMargins();
            pdfMargins.Top = 10;
            pdfMargins.Bottom = 10;
            pdfGen.Margins = pdfMargins;
            var pdfBytes = pdfGen.GeneratePdf(body);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(System.Convert.ToBase64String(pdfBytes));
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            result.Content.Headers.Add("Content-Disposition", "inline; filename=Shipping.pdf");
            return result;


        }
    }

}