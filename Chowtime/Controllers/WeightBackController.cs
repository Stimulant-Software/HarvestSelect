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
    public class WeighBackController : BaseApiController
    {




        [HttpPut]
        public HttpResponseMessage WeighBackAddOrEdit([FromBody] WeighBackDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var WeighBackId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref WeighBackId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (userId > 0 && aur.IsInRole(userId, "Data Entry"))
            {
                var WeighBack = new WeighBack();
                var errors = ValidateDtoData(uDto, WeighBack);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                uDto.Turtle = uDto.Turtle == "" ? null : uDto.Turtle;
                uDto.Trash = uDto.Trash == "" ? null : uDto.Trash;
                uDto.Shad = uDto.Shad == "" ? null : uDto.Shad;
                uDto.Carp = uDto.Carp == "" ? null : uDto.Carp;
                uDto.Bream = uDto.Bream == "" ? null : uDto.Bream;
                uDto.LiveDisease = uDto.LiveDisease == "" ? null : uDto.LiveDisease;
                uDto.DressedDisease = uDto.DressedDisease == "" ? null : uDto.DressedDisease;
                uDto.Backs = uDto.Backs == "" ? null : uDto.Backs;
                uDto.RedFillet = uDto.RedFillet == "" ? null : uDto.RedFillet;
                uDto.BigFish = uDto.BigFish == "" ? null : uDto.BigFish;
                uDto.DOAs = uDto.DOAs == "" ? null : uDto.DOAs;
                uDto.DressedDiseasePct = ".6";
                uDto.RedFilletPct = ".36";
                if (int.TryParse(uDto.WeightBackID, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewWeighBackRecord(Request, uDto, key, WeighBackId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingWeighBackRecord(Request, uDto, NEUserId, key, WeighBackId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewWeighBackRecord(HttpRequestMessage request, WeighBackDTO uDto, string key, int WeighBackId, int userId)
        {
            var ur = new WeighBackRepository();
            var user = new WeighBack();


            var validationErrors = GetValidationErrors(ur, user, uDto, WeighBackId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            user = ur.Save(user);
            uDto.Key = key;
            uDto.WeightBackID = user.WeightBackID.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.WeightBackID
            }));
            return response;
        }

        private HttpResponseMessage ProcessExistingWeighBackRecord(HttpRequestMessage request, WeighBackDTO cqDto, int contactId, string key, int WeighBackId, int userId)
        {
            var ur = new WeighBackRepository();
            var user = new WeighBack();
            user = ur.GetById(contactId);


            var validationErrors = GetValidationErrors(ur, user, cqDto, WeighBackId, userId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }

            //if (cqDto.Remove != null && int.Parse(cqDto.Remove) == 1)
            //{
            //    ur.Delete(user);
            //}
            else
            {
                ur.Save(user);
            }

            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(WeighBackRepository pr, WeighBack contact, WeighBackDTO cqDto, int YieldID, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        internal HttpResponseMessage WeighBacks(HttpRequestMessage request, WeighBackDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new WeighBackRepository();
                var u = new WeighBack();
                if (cqDTO.WBDateTime != null)
                {
                    cqDTO.Start_WBDateTime = DateTime.Parse(cqDTO.WBDateTime).ToString();
                    cqDTO.End_WBDateTime = DateTime.Parse(cqDTO.WBDateTime).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_WBDateTime = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_WBDateTime = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_WBDateTime = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_WBDateTime = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_WBDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_WBDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }

                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.OrderBy(x => x.WBDateTime).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();

                    dic.Add("WeightBackID", item.WeightBackID.ToString());
                    dic.Add("PondID", item.PondID.ToString());
                    dic.Add("PondName", item.Pond.PondName);
                    dic.Add("FarmID", item.Pond.FarmId.ToString());
                    dic.Add("WBDateTime", item.WBDateTime.ToShortDateString());
                    dic.Add("Turtle", item.Turtle.ToString());
                    dic.Add("Trash", item.Trash.ToString());
                    dic.Add("Shad", item.Shad.ToString());
                    dic.Add("Carp", item.Carp.ToString());
                    dic.Add("Bream", item.Bream.ToString());
                    dic.Add("LiveDisease", item.LiveDisease.ToString());
                    dic.Add("DressedDisease", item.DressedDisease.ToString());
                    dic.Add("Backs", item.Backs.ToString());
                    dic.Add("RedFillet", item.RedFillet.ToString());
                    dic.Add("BigFish", item.BigFish.ToString());
                    dic.Add("DOAs", item.DOAs.ToString());
                    dic.Add("DressedDiseasePct", item.DressedDiseasePct.ToString());
                    dic.Add("RedFilletPct", item.RedFilletPct.ToString());
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
        public HttpResponseMessage WeighBacksFromSamplings([FromBody] WeighBackDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new WeighBackRepository();
                var u = new WeighBack();
                if (cqDTO.WBDateTime != null)
                {
                    cqDTO.Start_WBDateTime = DateTime.Parse(cqDTO.WBDateTime).ToString();
                    cqDTO.End_WBDateTime = DateTime.Parse(cqDTO.WBDateTime).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_WBDateTime = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_WBDateTime = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_WBDateTime = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_WBDateTime = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_WBDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_WBDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                SGApp.DTOs.GenericDTO dto = new GenericDTO();
                dto.StartDate = DateTime.Parse(cqDTO.Start_WBDateTime);
                dto.EndDate = DateTime.Parse(cqDTO.End_WBDateTime);
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
                    var response = client.PostAsJsonAsync("api/Remote/GetKeithsData", dto).Result;
                    response.EnsureSuccessStatusCode();
                    JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                    Sampling[] samplingResultsArray = json_serializer.Deserialize<Sampling[]>(response.Content.ReadAsStringAsync().Result); // new List<Sampling>();
                    //Sampling[] samplingResultsArray = response.Content.ReadAsAsync<Sampling[]>().Result;
                    //samplingResults = samplingResultsArray.ToList();
                    //JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                    
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
                data = data.OrderBy(x => x.WBDateTime).ToList();

                foreach (Sampling sam in samplingResults)
                {

                    WeighBack fy = data.Where(x => x.Pond.InnovaName == sam.farmPond).FirstOrDefault();
                    Pond pd = pr.GetPondFromInnovaName(sam.farmPond);
                    var dic = new Dictionary<string, string>();
                    if (fy != null)
                    {
                        dic.Add("WeightBackID", fy.WeightBackID.ToString());
                        dic.Add("PondID", fy.PondID.ToString());
                        dic.Add("PondName", sam.farmPond);
                        dic.Add("FarmID", fy.Pond.FarmId.ToString());
                        dic.Add("WBDateTime", fy.WBDateTime.ToShortDateString());
                        dic.Add("Turtle", fy.Turtle.ToString());
                        dic.Add("Trash", fy.Trash.ToString());
                        dic.Add("Shad", fy.Shad.ToString());
                        dic.Add("Carp", fy.Carp.ToString());
                        dic.Add("Bream", fy.Bream.ToString());
                        dic.Add("LiveDisease", fy.LiveDisease.ToString());
                        dic.Add("DressedDisease", fy.DressedDisease.ToString());
                        dic.Add("Backs", fy.Backs.ToString());
                        dic.Add("RedFillet", fy.RedFillet.ToString());
                        dic.Add("BigFish", fy.BigFish.ToString());
                        dic.Add("DOAs", fy.DOAs.ToString());
                        dic.Add("DressedDiseasePct", fy.DressedDiseasePct.ToString());
                        dic.Add("RedFilletPct", fy.RedFilletPct.ToString());
                    }
                    else
                    {
                        dic.Add("YieldId", "-1");
                        dic.Add("PondID", pd.PondId.ToString() != null ? pd.PondId.ToString() : "");
                        dic.Add("PondName", sam.farmPond != null ? sam.farmPond : "");
                        dic.Add("FarmID", pd.FarmId.ToString() != null ? pd.FarmId.ToString() : "");
                        dic.Add("WBDateTime", cqDTO.Start_WBDateTime);
                        dic.Add("Turtle", "");
                        dic.Add("Trash", "");
                        dic.Add("Shad", "");
                        dic.Add("Carp", "");
                        dic.Add("Bream", "");
                        dic.Add("LiveDisease", "");
                        dic.Add("DressedDisease", "");
                        dic.Add("Backs", "");
                        dic.Add("RedFillet", "");
                        dic.Add("BigFish", "");
                        dic.Add("DOAs", "");
                        dic.Add("DressedDiseasePct", "");
                        dic.Add("RedFilletPct", "");

                    }

                    col.Add(dic);

                }
                foreach (WeighBack fy in data)
                {

                    Sampling samp = samplingResults.Where(x => x.farmPond == fy.Pond.InnovaName).FirstOrDefault();
                    var dic = new Dictionary<string, string>();
                    if (samp == null)
                    {
                        dic.Add("YieldId", fy.WeightBackID.ToString());
                        dic.Add("PondID", fy.PondID.ToString());
                        dic.Add("PondName", fy.Pond.InnovaName != null ? fy.Pond.InnovaName : fy.Pond.PondName);
                        dic.Add("FarmID", fy.Pond.FarmId.ToString());
                        dic.Add("WBDateTime", fy.WBDateTime.ToShortDateString());
                        dic.Add("Turtle", fy.Turtle.ToString());
                        dic.Add("Trash", fy.Trash.ToString());
                        dic.Add("Shad", fy.Shad.ToString());
                        dic.Add("Carp", fy.Carp.ToString());
                        dic.Add("Bream", fy.Bream.ToString());
                        dic.Add("LiveDisease", fy.LiveDisease.ToString());
                        dic.Add("DressedDisease", fy.DressedDisease.ToString());
                        dic.Add("Backs", fy.Backs.ToString());
                        dic.Add("RedFillet", fy.RedFillet.ToString());
                        dic.Add("BigFish", fy.BigFish.ToString());
                        dic.Add("DOAs", fy.DOAs.ToString());
                        dic.Add("DressedDiseasePct", fy.DressedDiseasePct.ToString());
                        dic.Add("RedFilletPct", fy.RedFilletPct.ToString());
                        col.Add(dic);
                    }


                }

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


        [HttpPost]
        public HttpResponseMessage WeighBacks([FromBody] WeighBackDTO cqDTO)
        {
            return WeighBacks(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage WeighBackList([FromBody] WeighBackDTO cqDTO)
        {
            return WeighBacks(Request, cqDTO);
        }

        [HttpPost]
        public HttpResponseMessage WeighBackDates([FromBody] WeighBackDTO cqDTO)
        {
            return WeighBackDates(Request, cqDTO);
        }
        internal HttpResponseMessage WeighBackDates(HttpRequestMessage request, WeighBackDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new WeighBackRepository();
                var u = new WeighBack();
                if (cqDTO.WBDateTime != null)
                {
                    cqDTO.Start_WBDateTime = DateTime.Parse(cqDTO.WBDateTime).ToString();
                    cqDTO.End_WBDateTime = DateTime.Parse(cqDTO.WBDateTime).AddDays(1).ToString();
                }
                else
                {
                    int sm = int.Parse(cqDTO.StartDateMonth);
                    if (sm == 1)
                    {
                        cqDTO.Start_WBDateTime = DateTime.Parse("12/23/" + (int.Parse(cqDTO.StartDateYear) - 1).ToString()).ToString();
                        cqDTO.End_WBDateTime = DateTime.Parse("2/14/" + cqDTO.StartDateYear).ToString();
                    }
                    else if (sm == 12)
                    {
                        cqDTO.Start_WBDateTime = DateTime.Parse("11/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_WBDateTime = DateTime.Parse("1/14/" + (int.Parse(cqDTO.StartDateYear) + 1).ToString()).ToString();
                    }
                    else
                    {
                        cqDTO.Start_WBDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) - 1).ToString() + "/23/" + cqDTO.StartDateYear).ToString();
                        cqDTO.End_WBDateTime = DateTime.Parse((int.Parse(cqDTO.StartDateMonth) + 1).ToString() + "/14/" + cqDTO.StartDateYear).ToString();
                    }

                    cqDTO.StartDateMonth = null;
                    cqDTO.StartDateYear = null;
                }
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                data = data.GroupBy(x => x.WBDateTime).Select(x => x.First()).OrderBy(x => x.WBDateTime).ToList();
                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();


                    dic.Add("WBDateTime", item.WBDateTime.ToShortDateString());

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