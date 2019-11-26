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
    public class PondController : BaseApiController
    {
        internal HttpResponseMessage Ponds(HttpRequestMessage request, PondDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var UserId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            AppUserRoleRepository aur1 = new AppUserRoleRepository();


            if (UserId > 0 && aur1.IsInRole(UserId, "User"))
            {
                var ur = new PondRepository();
                var u = new Pond();
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                var bins = new Collection<Dictionary<string, string>>();
                if (cqDTO.FarmId != null)
                {
                    var br = new BinRepository();
                    var binList = br.GetFarmBinList(int.Parse(cqDTO.FarmId));
                    foreach (var bin in binList)
                    {
                        bins.Add(
                            new Dictionary<string, string>() {
                                {"BinID", bin.BinID.ToString()},
                                {"BinName", bin.BinName},
                                {"FarmID", bin.FarmID.HasValue ? bin.FarmID.Value.ToString() : ""},
                                {"CurrentTicket", bin.CurrentTicket.HasValue ? bin.CurrentTicket.Value.ToString() : ""},
                                {"CurrentPounds", bin.CurrentPounds.HasValue ? bin.CurrentPounds.Value.ToString() : ""},
                                {"LastDispersement", bin.LastDisbursement.HasValue ? bin.LastDisbursement.Value.ToShortDateString() : ""},
                                {"LastLoaded", bin.LastLoaded.HasValue ? bin.LastLoaded.Value.ToShortDateString() : ""}
                            }
                        );
                    }

                }
                var farmCol = new Collection<Tuple<int, int>>();
				foreach (var item in data) {
                    var dic = new Dictionary<string, string>();
                    dic.Add("PondId", item.PondId.ToString());
                    dic.Add("PondName", item.PondName);
                    dic.Add("StatusId", item.StatusId.ToString());
                    dic.Add("InnovaName", item.InnovaName != null ? item.InnovaName : "");
                    dic.Add("InnovaCode", item.InnovaCode != null ? item.InnovaCode : "");
                    dic.Add("Size", item.Size.ToString());
                    dic.Add("NoFeed", item.NoFeed.ToString());
                    int poundsfedsinceharvest = 0;
                    if (item.Harvests.OrderByDescending(x => x.HarvestDate).FirstOrDefault() != null)
                    {
                        dic.Add("LastHarvest", item.Harvests.OrderByDescending(x => x.HarvestDate).FirstOrDefault().HarvestDate.ToString());
                        poundsfedsinceharvest = item.Feedings.Where(x => x.FeedDate > item.Harvests.OrderByDescending(y => y.HarvestDate).FirstOrDefault().HarvestDate).Sum(x => x.PoundsFed);
                        
                    }
                    else
                    {
                        dic.Add("LastHarvest", "");
                        poundsfedsinceharvest = item.Feedings.Sum(x => x.PoundsFed);
                    }
                    int salepounds = poundsfedsinceharvest / 2;
                    dic.Add("PoundsFedSinceHarvest", poundsfedsinceharvest.ToString());
                    dic.Add("SalesPoundsSinceHarvest", salepounds.ToString());
                    dic.Add("HealthStatus", item.HealthStatus.ToString());
                    col.Add(dic);
                }

                var retVal = new GenericDTO
                {
                    Key = key,
                    ReturnData = col,
					Bins = bins
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return request.CreateResponse(HttpStatusCode.NotFound, message);

        }

        [HttpPost]
        public HttpResponseMessage PondO2ByDate([FromBody] O2ReadingDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);
            //string dayperiod;
            //if (DateTime.Parse(uDto.ReadingDate).Hour < 12)
            //{
            //    dayperiod = DateTime.Parse(uDto.ReadingDate).AddDays(-1).ToShortDateString();
            //}
            //else
            //{
            //    dayperiod = DateTime.Parse(uDto.ReadingDate).ToShortDateString();
            //}
            //uDto.DayPeriod = dayperiod;

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Airtime"))
            {
                var O2r = new O2ReadingRepository();
                var u = new O2Reading();
                //var predicate = O2r.GetPredicate(uDto, u, companyId);
                var data = O2r.GetPondO2ReadingsByDate(int.Parse(uDto.PondId), DateTime.Parse(uDto.ReadingDate));
                var col = new Collection<Dictionary<string, string>>();

                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();

                    dic.Add("PondId", item.PondId.ToString());
                    dic.Add("ReadingId", item.ReadingId.ToString());
                    dic.Add("ReadingDate", item.ReadingDate.ToString());
                    dic.Add("O2Level", item.O2Level.ToString());
                    dic.Add("StaticCount", item.StaticCount.ToString());
                    dic.Add("PortableCount", item.PortableCount.ToString());
                    dic.Add("Note", item.Note);
                    dic.Add("PondStatus", item.Pond.HealthStatus.ToString());
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
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }
        [HttpPost]
        public HttpResponseMessage FeedById([FromBody] FeedingDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);
            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Chowtime"))
            {
                var O2r = new FeedingRepository();
                var data = O2r.GetById(int.Parse(uDto.FeedingId));
                var col = new Collection<Dictionary<string, string>>();


                    var dic = new Dictionary<string, string>();

                    dic.Add("PondId", data.PondId.ToString());
                    dic.Add("FeedingId", data.FeedingId.ToString());
                    dic.Add("FeedDate", data.FeedDate.ToString());
                    dic.Add("PoundsFed", data.PoundsFed.ToString());
                    col.Add(dic);


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
        public HttpResponseMessage PondFeedLast7Days([FromBody] PondDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Chowtime"))
            {
                var pr = new PondRepository();
                var ponddata = pr.GetById(int.Parse(uDto.PondId));
                DateTime startdate = DateTime.Now;

                    int i = 0;
                    var col = new Collection<Dictionary<string, string>>();
                    while (i > -7)
                    {
                        var fr = new FeedingRepository();
                        var data = fr.GetPondFeedingsByDate(ponddata.PondId, startdate.AddDays(i));
                        if (data != null)
                        {

                            var dic = new Dictionary<string, string>();

                            dic.Add("PondId", data.PondId.ToString());
                            dic.Add("FeedingId", data.FeedingId.ToString());
                            dic.Add("FeedDate", data.FeedDate.ToString());
                            dic.Add("PoundsFed", data.PoundsFed.ToString());
                            col.Add(dic);
                        }

                        i--;
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
        public HttpResponseMessage PondFeedLast7Feeds([FromBody] PondDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Chowtime"))
            {
                var pr = new PondRepository();
                var ponddata = pr.GetById(int.Parse(uDto.PondId));
                DateTime startdate = DateTime.Now;

                int i = 0;
                int j = 0;
                int pondDataCount = 0;

                var col = new Collection<Dictionary<string, string>>();
                while (pondDataCount < 7 && j < 10)
                {
                    var fr = new FeedingRepository();
                    var data = fr.GetPondFeedingsByDate(ponddata.PondId, startdate.AddDays(i));
                    if (data != null)
                    {

                        var dic = new Dictionary<string, string>();

                        dic.Add("PondId", data.PondId.ToString());
                        dic.Add("FeedingId", data.FeedingId.ToString());
                        dic.Add("FeedDate", data.FeedDate.ToString());
                        dic.Add("PoundsFed", data.PoundsFed.ToString());
                        col.Add(dic);
                        pondDataCount++;
                        // reset j - haven't hit null territory yet
                        j = 0;
                    }
                    else { j++; }
                    i--;
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
        public HttpResponseMessage GetLastPondReading([FromBody] O2ReadingDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Airtime"))
            {
                var O2r = new O2ReadingRepository();
                var u = new O2Reading();
                //var predicate = O2r.GetPredicate(uDto, u, companyId);
                var data = O2r.GetLastPondReadingByPond(int.Parse(uDto.PondId));
                var col = new Collection<Dictionary<string, string>>();


                    var dic = new Dictionary<string, string>();

                    dic.Add("PondId", data.PondId.ToString());
                    dic.Add("ReadingId", data.ReadingId.ToString());
                    dic.Add("ReadingDate", data.ReadingDate.ToString());
                    dic.Add("O2Level", data.O2Level.ToString());
                    dic.Add("StaticCount", data.StaticCount.ToString());
                    dic.Add("PortableCount", data.PortableCount.ToString());
                    dic.Add("Note", data.Note);
                    col.Add(dic);

             

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
        public HttpResponseMessage Ponds([FromBody] PondDTO cqDTO)
        {
            return Ponds(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage PondList([FromBody] PondDTO cqDTO)
        {
            return Ponds(Request, cqDTO);
        }

        [HttpPost]
        public HttpResponseMessage PondDetail([FromBody] PondDTO cqDTO)
        {
            return Ponds(Request, cqDTO);
        }
        [HttpPut]
        public HttpResponseMessage PondAddOrEdit([FromBody] PondDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Admin"))
            {
                var Pond = new Pond();
                var errors = ValidateDtoData(uDto, Pond);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEPondId = 0;
                if (int.TryParse(uDto.PondId, out NEPondId))
                {
                    if (NEPondId == -1)
                    {
                        //  creating new Pond record   
                        return ProcessNewPondRecord(Request, uDto, key, companyId, UserId);
                    }
                    else
                    {
                        //  editing existing Pond record  
                        return ProcessExistingPondRecord(Request, uDto, NEPondId, key, companyId, UserId);
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
        public HttpResponseMessage O2AddOrEdit([FromBody] O2ReadingDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Airtime"))
            {
                var thisuser = ur.GetById(UserId);
                var pr = new PondRepository();
                int thisfarm = pr.GetById(int.Parse(uDto.PondId)).FarmId;
                int UsersFarmId = thisuser.UserFarms.Where(x => x.FarmId == thisfarm).SingleOrDefault().UserFarmId;
                uDto.UsersFarmId = UsersFarmId.ToString();
                string dayperiod;
                if (DateTime.Parse(uDto.ReadingDate).Hour < 12)
                {
                    dayperiod = DateTime.Parse(uDto.ReadingDate).AddDays(-1).ToShortDateString();
                }
                else
                {
                    dayperiod = DateTime.Parse(uDto.ReadingDate).ToShortDateString();
                }
                uDto.DayPeriod = dayperiod;
                var Pond = new Pond();
                var errors = ValidateDtoData(uDto, Pond);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEReadingId = 0;
                if (int.TryParse(uDto.ReadingId, out NEReadingId))
                {
                    if (NEReadingId == -1)
                    {
                        //  creating new Pond record   
                        return ProcessNewO2Record(Request, uDto, key, companyId, UserId);
                    }
                    else
                    {
                        //  editing existing Pond record  
                        return ProcessExistingO2Record(Request, uDto, NEReadingId, key, companyId, UserId);
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
        public HttpResponseMessage HarvestPond([FromBody] HarvestDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Chowtime"))
            {
                var hr = new HarvestRepository();
                var harv = new Harvest();
                var errors = ValidateDtoData(uDto, harv);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
   
                        return ProcessNewHarvestRecord(Request, uDto, key, companyId, UserId);

                
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }
        [HttpPut]
        public HttpResponseMessage FeedAddOrEdit([FromBody] FeedingDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);
            
            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Chowtime"))
            {
                var thisuser = ur.GetById(UserId);
                var pr = new PondRepository();
                int thisfarm = pr.GetById(int.Parse(uDto.PondId)).FarmId;
                int UsersFarmId = thisuser.UserFarms.Where(x => x.FarmId == thisfarm).SingleOrDefault().UserFarmId;
                uDto.UsersFarmId = UsersFarmId.ToString();
                uDto.FarmID = thisfarm;
                
                var feed = new Feeding();
                var errors = ValidateDtoData(uDto, feed);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEFeedingId = 0;
                if (int.TryParse(uDto.FeedingId, out NEFeedingId))
                {
                    if (NEFeedingId == -1)
                    {
                        //  creating new Feeding record   
                        return ProcessNewFeedRecord(Request, uDto, key, companyId, UserId);
                    }
                    else
                    {
                        //  editing existing Feeding record  
                        return ProcessExistingFeedRecord(Request, uDto, NEFeedingId, key, companyId, UserId);
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
        public HttpResponseMessage PondActiveFlag([FromBody] PondDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);
            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Admin"))
            {
                var Pond = new Pond();
                var errors = ValidateDtoData(uDto, Pond);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEPondId = 0;
                if (int.TryParse(uDto.PondId, out NEPondId))
                {
  
                        //  editing existing Pond record  
                        return ChangePondStatus(Request, uDto, NEPondId, key, companyId, UserId);

                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }



        [HttpPut]
        public HttpResponseMessage PondHealthStatus([FromBody] PondDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);
            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Chowtime"))
            {
                var Pond = new Pond();
                var errors = ValidateDtoData(uDto, Pond);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEPondId = 0;
                if (int.TryParse(uDto.PondId, out NEPondId))
                {

                    //  editing existing Pond record  
                    return ChangePondHealthStatus(Request, uDto, NEPondId, key, companyId, UserId);

                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        [HttpPut]
        public HttpResponseMessage ChangePondFeedStatus([FromBody] PondDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);
            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Admin"))
            {
                var Pond = new Pond();
                var errors = ValidateDtoData(uDto, Pond);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEPondId = 0;
                if (int.TryParse(uDto.PondId, out NEPondId))
                {

                    //  editing existing Pond record  
                    return ChangePondFeedStatus(Request, uDto, NEPondId, key, companyId, UserId);

                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewPondRecord(HttpRequestMessage request, PondDTO uDto, string key, int companyId, int UserId)
        {
            var ur = new PondRepository();
            var Pond = new Pond();
 
            var validationErrors = GetValidationErrors(ur, Pond, uDto, companyId, UserId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors... 
            //Pond.CompanyId = companyId;

            Pond = ur.Save(Pond);
            uDto.Key = key;
            uDto.PondId = Pond.PondId.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = Pond.PondId
            }));
            return response;
        }

        private HttpResponseMessage ProcessNewHarvestRecord(HttpRequestMessage request, HarvestDTO uDto, string key, int companyId, int UserId)
        {
            var hr = new HarvestRepository();
            var harv = new Harvest();

            var validationErrors = GetHarvestValidationErrors(hr, harv, uDto, companyId, UserId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors... 
            //Pond.CompanyId = companyId;

            harv = hr.Save(harv);
            uDto.Key = key;
            uDto.HarvestId = harv.HarvestId.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = harv.HarvestId
            }));
            return response;
        }

        private HttpResponseMessage ProcessNewO2Record(HttpRequestMessage request, O2ReadingDTO uDto, string key, int companyId, int UserId)
        {
            var ur = new O2ReadingRepository();
            var o2 = new O2Reading();

            var validationErrors = GetO2ValidationErrors(ur, o2, uDto, companyId, UserId);
            

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors... 
            //Pond.CompanyId = companyId;

            o2 = ur.Save(o2);

            UpdatePondHealthStatusByLevel(o2.PondId, o2.O2Level);
            uDto.Key = key;
            uDto.ReadingId = o2.ReadingId.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = o2.ReadingId
            }));
            return response;
        }

        private HttpResponseMessage ProcessNewFeedRecord(HttpRequestMessage request, FeedingDTO uDto, string key, int companyId, int UserId) {
            var ur = new FeedingRepository();
            var o2 = new Feeding();
            var validationErrors = GetFeedValidationErrors(ur, o2, uDto, companyId, UserId);
            if (validationErrors.Any()) {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors... 
            //Pond.CompanyId = companyId;
            o2 = ur.Save(o2);
            var farmid = uDto.FarmID;

			var br = new BinRepository();
            var binCount = br.GetFarmBinList(farmid).Count();
            if (binCount > 0)
            {
                var binDisb = br.GetNewBinDisbursementRecord();
                binDisb.DateCreated = DateTime.Now;
                var disbType = br.GetDisbursementType("Routine Feeding");
                var ticketNbr = br.GetLastBinLoadTicketNumber(uDto.BinID.Value);
                if (ticketNbr == 0)
                {
                    return request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        string.Format("{0}{1}", "There are no Tickets in BinLoads for BinID ", uDto.BinID));
                }

                var dto = new BinDisbursementDto()
                {
                    BinID = uDto.BinID.Value,
                    TicketNumber = ticketNbr,
                    Pounds = int.Parse(uDto.PoundsFed),
                    Note = "Record created from daily feed disbursement input screen",
                    DisbursementType = disbType,
                    DisbursementDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    UserID = UserId,
                    FeedID = o2.FeedingId
                };
                validationErrors = GetBinDisbursementErrors(br, binDisb, dto, companyId, UserId);
                if (validationErrors.Any())
                {
                    return ProcessValidationErrors(request, validationErrors, key);
                }
                br.SaveChanges();
                br.UpdateBinCurrentPounds(null, binDisb);
            }
	        
			uDto.Key = key;
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new {
                id = o2.FeedingId
            }));
            return response;
        }

        private HttpResponseMessage ProcessExistingPondRecord(HttpRequestMessage request, PondDTO cqDto, int contactId, string key, int companyId, int UserId)
        {
            var ur = new PondRepository();
            var Pond = new Pond();
            Pond = ur.GetById(contactId);
            //  is the Pond eligible to update the prospect?

            var validationErrors = GetValidationErrors(ur, Pond, cqDto, companyId, UserId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors...
            ur.Save(Pond);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private HttpResponseMessage ProcessExistingO2Record(HttpRequestMessage request, O2ReadingDTO cqDto, int contactId, string key, int companyId, int UserId)
        {
            var o2r = new O2ReadingRepository();
            var o2 = new O2Reading();
            o2 = o2r.GetById(contactId);
            //  is the Pond eligible to update the prospect?

            var validationErrors = GetO2ValidationErrors(o2r, o2, cqDto, companyId, UserId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors...
            o2r.Save(o2);
            UpdatePondHealthStatusByLevel(o2.PondId, o2.O2Level);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }

        private HttpResponseMessage ProcessExistingFeedRecord(HttpRequestMessage request, FeedingDTO cqDto, int contactId, string key, int companyId, int UserId)
        {
            var o2r = new FeedingRepository();
            var o2 = new Feeding();
            o2 = o2r.GetById(contactId);
            //  is the Pond eligible to update the prospect?

            var validationErrors = GetFeedValidationErrors(o2r, o2, cqDto, companyId, UserId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
			//  no validation errors...
	        var binDisb = o2r.GetBinDisbursement(o2.FeedingId);
	        if (binDisb != null) {
		        //	there is a BinDisbursement record which has to be modified
		        binDisb.Pounds = o2.PoundsFed;
		        binDisb.UserID = UserId; 
		        o2r.SaveChanges();
				var br = new BinRepository();
				br.UpdateBinCurrentPounds(null, binDisb);
	        }
	        else {
				//	there is no BinDisbursement record to mofify
				o2r.SaveChanges();
			}
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);
        }

        private HttpResponseMessage ChangePondStatus(HttpRequestMessage request, PondDTO cqDto, int contactId, string key, int companyId, int UserId)
        {
            var ur = new PondRepository();
            var Pond = new Pond();
            Pond = ur.GetById(contactId);

            //  no validation errors...
            Pond.StatusId = int.Parse(cqDto.StatusId);
            ur.Save(Pond);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }



        private HttpResponseMessage ChangePondFeedStatus(HttpRequestMessage request, PondDTO cqDto, int contactId, string key, int companyId, int UserId)
        {
            var ur = new PondRepository();
            var Pond = new Pond();
            Pond = ur.GetById(contactId);

            //  no validation errors...
            Pond.NoFeed = bool.Parse(cqDto.NoFeed);
            ur.Save(Pond);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }

        private HttpResponseMessage ChangePondHealthStatus(HttpRequestMessage request, PondDTO cqDto, int contactId, string key, int companyId, int UserId)
        {
            var ur = new PondRepository();
            var Pond = new Pond();
            Pond = ur.GetById(contactId);

            //  no validation errors...
            Pond.HealthStatus = int.Parse(cqDto.HealthStatus);
            ur.Save(Pond);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }

        private List<DbValidationError> GetValidationErrors(PondRepository pr, Pond contact, PondDTO cqDto, int companyId, int PondId)
        {
            contact.ProcessRecord(cqDto);

            return pr.Validate(contact);
        }

        private List<DbValidationError> GetO2ValidationErrors(O2ReadingRepository pr, O2Reading contact, O2ReadingDTO cqDto, int companyId, int PondId)
        {
            contact.ProcessRecord(cqDto);

            return pr.Validate(contact);
        }

        private List<DbValidationError> GetFeedValidationErrors(FeedingRepository pr, Feeding contact, FeedingDTO cqDto, int companyId, int PondId)
        {
            contact.ProcessRecord(cqDto);

            return pr.Validate(contact);
        }

	    private List<DbValidationError> GetBinDisbursementErrors(BinRepository pr, BinDisbursement bd, BinDisbursementDto dto, int companyId, int PondId) {
		    bd.ProcessRecord(dto);
		    return pr.Validate(bd);
	    }


		private List<DbValidationError> GetHarvestValidationErrors(HarvestRepository pr, Harvest contact, HarvestDTO cqDto, int companyId, int PondId)
        {
            contact.ProcessRecord(cqDto);

            return pr.Validate(contact);
        }
        private void UpdatePondHealthStatusByLevel(int pondid, decimal o2level)
        {
            var ur = new PondRepository();
            var Pond = new Pond();
            Pond = ur.GetById(pondid);
            var or = new O2ReadingRepository();
            List<O2Reading> last2 = or.GetLast2PondReadingsByPond(pondid);
           
            
            if (o2level < (decimal)(2.5))
            {
                Pond.HealthStatus = 3;
            }
            else if (last2.Count == 2)
            {

                decimal readingnow = last2.OrderByDescending(x => x.ReadingId).FirstOrDefault().O2Level;
                decimal readinglast = last2.OrderBy(x => x.ReadingId).FirstOrDefault().O2Level;
                if (readingnow / readinglast <= (decimal).5)
                {

                    Pond.HealthStatus = 2;
                }
                else
                {
                    Pond.HealthStatus = 1;

                }
            }
            else
            {
                Pond.HealthStatus = 1;
            }
            //  no validation errors...
            ur.Save(Pond);
        }
    }
}