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
    public class ProductionController : BaseApiController
    {     

        [HttpPost]
        public HttpResponseMessage ProductionDates([FromBody] ProductionTotalDTO cqDTO)
        {
            return ProductionDates(Request, cqDTO);
        }
        internal HttpResponseMessage ProductionDates(HttpRequestMessage request, ProductionTotalDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new ProductionTotalRepository();
                var u = new ProductionTotal();
                if (cqDTO.ProductionDate != null)
                {
                    cqDTO.Start_ProductionDate = DateTime.Parse(cqDTO.ProductionDate).ToString();
                    cqDTO.End_ProductionDate = DateTime.Parse(cqDTO.ProductionDate).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_ProductionDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_ProductionDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_ProductionDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_ProductionDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_ProductionDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_ProductionDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                foreach (var item in data)
                {
                    
                    var dic = new Dictionary<string, string>();
                    dic.Add("ProductionDate", item.ProductionDate.ToShortDateString());
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
        public HttpResponseMessage ProductionTotals([FromBody] ProductionTotalDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new ProductionTotalRepository();
                var u = new ProductionTotal();
                if (cqDTO.ProductionDate != null)
                {
                    cqDTO.Start_ProductionDate = DateTime.Parse(cqDTO.ProductionDate).ToString();
                    cqDTO.End_ProductionDate = DateTime.Parse(cqDTO.ProductionDate).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_ProductionDate = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_ProductionDate = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_ProductionDate = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_ProductionDate = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_ProductionDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_ProductionDate = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                SGApp.DTOs.GenericDTO dto = new GenericDTO();
                dto.StartDate = DateTime.Parse(cqDTO.Start_ProductionDate);
                dto.EndDate = DateTime.Parse(cqDTO.End_ProductionDate);
                List<Sampling> samplingResults = new List<Sampling>();
                PondRepository pr = new PondRepository();
                var client = new HttpClient
                {
                    //BaseAddress = new Uri("http://323-booth-svr2:3030/")
                    BaseAddress = new Uri("http://64.139.95.243:7846/")
                    //BaseAddress = new Uri(baseAddress)                
                };
                try
                {
                    //var response = client.PostAsJsonAsync("api/Remote/GetKeithsData", dto).Result;
                    //response.EnsureSuccessStatusCode();
                    JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                    //Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(response.Content.ReadAsStringAsync().Result); // new List<Sampling>();
                    //Sampling[] samplingResultsArray = response.Content.ReadAsAsync<Sampling[]>().Result;
                    //samplingResults = samplingResultsArray.ToList();
                    //JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                    Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(Constants.testdata);
                    //Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(response.Content.ReadAsStringAsync().Result);
                    samplingResults = samplingResultsArray.ToList();
                    samplingResults = samplingResults.GroupBy(x => x.farmPond).Select(group => group.First()).ToList();
                    //var result = response.Content.ReadAsStringAsync().Result;

                    //return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                catch (Exception e)
                {
                    throw new HttpException("Error occurred: " + e.Message);
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.ProductionDate).ToList();

                foreach (Sampling sam in samplingResults)
                {

                    ProductionTotal fy = data.Where(x => x.Pond.InnovaName == sam.farmPond).FirstOrDefault();
                    Pond pd = pr.GetPondFromInnovaName(sam.farmPond);
                    var dic = new Dictionary<string, string>();
                    if (fy != null)
                    {
                        var wb = fy.WeighBacks != null ? fy.WeighBacks : 0;
                        dic.Add("ProductionTotalId", fy.ProductionTotalID.ToString());
                        dic.Add("PondID", fy.PondId.ToString());
                        dic.Add("PondName", sam.farmPond);
                        dic.Add("FarmID", fy.Pond.FarmId.ToString());
                        dic.Add("ProductionDate", fy.ProductionDate.ToShortDateString());
                        dic.Add("PlantWeight", fy.PlantWeight != null ? fy.PlantWeight.ToString() : "---");
                        dic.Add("PondWeight", fy.PondWeight != null ? fy.PondWeight.ToString() : "---");
                        dic.Add("WeighBacks", fy.WeighBacks != null ? fy.WeighBacks.ToString() : "---");
                        dic.Add("AverageYield", fy.AverageYield != null ? fy.AverageYield.ToString() : "---");
                        dic.Add("HeadedWeight", fy.AverageYield != null && fy.PlantWeight != null ? String.Format("{0:0.00}", ((fy.AverageYield / 100) * (fy.PlantWeight - wb))) : "---");
                    }
                    else
                    {
                        dic.Add("ProductionTotalId", "-1");
                        dic.Add("PondID", pd.PondId.ToString() != null ? pd.PondId.ToString() : "");
                        dic.Add("PondName", sam.farmPond != null ? sam.farmPond : "");
                        dic.Add("FarmID", pd.FarmId.ToString() != null ? pd.FarmId.ToString() : "");
                        dic.Add("ProductionDate", cqDTO.ProductionDate);
                        dic.Add("PlantWeight", "---");
                        dic.Add("PondWeight", "---");
                        dic.Add("WeighBacks", "---");
                        dic.Add("AverageYield", "---");
                        dic.Add("HeadedWeight", "---");

                    }

                    col.Add(dic);

                }
                //foreach (FarmYield fy in data)
                //{

                //    Sampling samp = samplingResults.Where(x => x.farmPond == fy.Pond.InnovaName).FirstOrDefault();
                //    var dic = new Dictionary<string, string>();
                //    if (samp == null)
                //    {
                //        dic.Add("YieldId", fy.YieldID.ToString());
                //        dic.Add("PondID", fy.PondID.ToString());
                //        dic.Add("PondName", fy.Pond.InnovaName != null ? fy.Pond.InnovaName : fy.Pond.PondName);
                //        dic.Add("FarmID", fy.Pond.FarmId.ToString());
                //        dic.Add("YieldDate", fy.YieldDate.ToShortDateString());
                //        dic.Add("PoundsYielded", fy.PoundsYielded.ToString());
                //        dic.Add("PoundsPlant", fy.PoundsPlant.ToString());
                //        dic.Add("PoundsHeaded", fy.PoundsHeaded.ToString());
                //        dic.Add("PercentYield", fy.PercentYield.ToString());
                //        dic.Add("PercentYield2", fy.PercentYield2.ToString());
                //        col.Add(dic);
                //    }


                //}

                var retVal = new GenericDTO
                {
                    Key = key,
                    ReturnData = col
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);

        }
    }

}