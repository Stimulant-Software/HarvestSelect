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
    public class FarmController : BaseApiController
    {
        internal HttpResponseMessage Farms(HttpRequestMessage request, FarmDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var UserId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            AppUserRoleRepository aur1 = new AppUserRoleRepository();


            if (UserId > 0 && aur1.IsInRole(UserId, "User"))
            {
                var ur = new FarmRepository();
                var u = new Farm();
                cqDTO.CompanyId = companyId.ToString();
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();

                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();

                    dic.Add("FarmId", item.FarmId.ToString());
                    dic.Add("FarmName", item.FarmName);
                    dic.Add("StatusId", item.StatusId.ToString());
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

        public HttpResponseMessage FarmO2Last3Days([FromBody] FarmDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Airtime"))
            {
                var pr = new PondRepository();
                var ponddata = pr.GetActivePondsForO2(int.Parse(uDto.FarmId));
                DateTime startdate = DateTime.Parse(uDto.CurrentTime);
                var pondcol = new Dictionary<string, Dictionary<string, Collection<Dictionary<string, string>>>>();
                foreach (Pond ponditem in ponddata)
                {                   
                    var datecol = new Dictionary<string, Collection<Dictionary<string, string>>>();
                    int i = 0;
                    while (i > -3)
                    {
                        var O2r = new O2ReadingRepository();
                        var data = O2r.GetPondO2ReadingsByDate(ponditem.PondId, startdate.AddDays(i));
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
                            col.Add(dic);

                        }
                        datecol.Add(startdate.AddDays(i).ToShortDateString(), col);
                        i--;
                    }
                    pondcol.Add(ponditem.PondId.ToString(), datecol);
                }



                var retVal = new FarmO2DTO
                {
                    Key = key,
                    ReturnData = pondcol
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        public HttpResponseMessage FarmO2Last7Days([FromBody] FarmDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Airtime"))
            {
                var pr = new PondRepository();
                var ponddata = pr.GetActivePondsForO2(int.Parse(uDto.FarmId));
                DateTime startdate = DateTime.Parse(uDto.CurrentTime);
                var pondcol = new Dictionary<string, Dictionary<string, Collection<Dictionary<string, string>>>>();
                foreach (Pond ponditem in ponddata)
                {
                    var datecol = new Dictionary<string, Collection<Dictionary<string, string>>>();

                        var O2r = new O2ReadingRepository();
                        var data = O2r.GetPondO2Readings7Days(ponditem.PondId, startdate);
                        var col = new Collection<Dictionary<string, string>>();
                        foreach (var item in data)
                        {

                            var dic = new Dictionary<string, string>();

                            dic.Add("PondId", item.PondId.ToString());
                            dic.Add("ReadingId", item.ReadingId.ToString());
                            dic.Add("ReadingDate", item.ReadingDate.ToString());
                            dic.Add("O2Level", item.O2Level.ToString());
                            
                            col.Add(dic);

                        }
                        datecol.Add(startdate.ToShortDateString(), col);

                    pondcol.Add(ponditem.PondId.ToString(), datecol);
                }



                var retVal = new FarmO2DTO
                {
                    Key = key,
                    ReturnData = pondcol
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }
        public HttpResponseMessage FarmFeedLast7Days([FromBody] FarmDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Chowtime"))
            {
                var pr = new PondRepository();
                var ponddata = pr.GetActivePonds(int.Parse(uDto.FarmId));
                DateTime startdate = DateTime.Parse(uDto.CurrentTime);
                var pondcol = new Dictionary<string, Collection<Dictionary<string, string>>>();
                foreach (Pond ponditem in ponddata)
                {
                    int i = 0;
                    var col = new Collection<Dictionary<string, string>>();
                    while (i > -7)
                    {
                        var fr = new FeedingRepository();
                        var data = fr.GetPondFeedingsByDate(ponditem.PondId, startdate.AddDays(i));

                        if (data != null)
                        {
                            var dic = new Dictionary<string, string>();

                            dic.Add("PondId", data.PondId.ToString());
                            dic.Add("FeedingId", data.FeedingId.ToString());
                            dic.Add("FeedDate", data.FeedDate.ToString());
                            dic.Add("PoundsFed", data.PoundsFed.ToString());
                            col.Add(dic);
                        }
                        else
                        {
                            var dic = new Dictionary<string, string>();

                            dic.Add("PondId", ponditem.PondId.ToString());
                            dic.Add("FeedingId", "");
                            dic.Add("FeedDate", startdate.AddDays(i).Date.ToShortDateString());
                            dic.Add("PoundsFed", "0");
                            col.Add(dic);
                        }
                        i--;
                    }
                    pondcol.Add(ponditem.PondId.ToString(), col);
                }



                var retVal = new FarmFeedDTO
                {
                    Key = key,
                    ReturnData = pondcol
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }
        public HttpResponseMessage FarmFeedLast7DaysTotals([FromBody] FarmDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Chowtime"))
            {
                
                DateTime startdate = DateTime.Parse(uDto.CurrentTime);

                    int i = 0;
                    var col = new Collection<Dictionary<string, string>>();
                    while (i > -7)
                    {
                        var fr = new FeedingRepository();
                        var data = fr.GetFarmFeedingsByDate(int.Parse(uDto.FarmId), startdate.AddDays(i));

                        if (data != null)
                        {
                            var dic = new Dictionary<string, string>();
                            int totalfeed = data.Sum(x => x.PoundsFed);
                            int totalfeedcount = data.Count();
                            
                            if (totalfeedcount > 0)
                            {
                                
                                decimal totalacres = data.Sum(x => x.Pond.Size);
                                decimal averagefeed = totalfeed / totalacres;
                                averagefeed = Math.Round(averagefeed, 0);
                                dic.Add("FeedDate", startdate.AddDays(i).ToShortDateString());
                                dic.Add("TotalPoundsFed", totalfeed.ToString());
                                dic.Add("TotalFeeds", totalfeedcount.ToString());
                                dic.Add("AveragePoundsFed", averagefeed.ToString());
                                col.Add(dic);
                            }
                            else
                            {
                                dic.Add("FeedDate", startdate.AddDays(i).ToShortDateString());
                                dic.Add("TotalPoundsFed", "0");
                                dic.Add("TotalFeeds", "0");
                                dic.Add("AveragePoundsFed", "0");
                                col.Add(dic);
                            }
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

        public HttpResponseMessage FarmFeedLast7FeedsTotals([FromBody] FarmDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Chowtime"))
            {

                DateTime startdate = DateTime.Parse(uDto.CurrentTime);
                var fr = new FeedingRepository();
                var dates = fr.GetFarmFeedingsLast7Dates(int.Parse(uDto.FarmId));
                var col = new Collection<Dictionary<string, string>>();
                foreach (Feeding f in dates)
                {
                    var data = fr.GetFarmFeedingsByDate(int.Parse(uDto.FarmId), f.FeedDate);

                    if (data != null)
                    {
                        var dic = new Dictionary<string, string>();
                        int totalfeed = data.Sum(x => x.PoundsFed);
                        int totalfeedcount = data.Count();
                        if (totalfeedcount > 0)
                        {

                            decimal totalacres = data.Sum(x => x.Pond.Size);
                            decimal averagefeed = totalfeed / totalacres;
                            averagefeed = Math.Round(averagefeed, 0);
                            dic.Add("FeedDate", f.FeedDate.ToShortDateString());
                            dic.Add("TotalPoundsFed", totalfeed.ToString());
                            dic.Add("TotalFeeds", totalfeedcount.ToString());
                            dic.Add("AveragePoundsFed", averagefeed.ToString());
                            col.Add(dic);
                        }
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

        public HttpResponseMessage FarmLast7Feeds([FromBody] FarmDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Chowtime"))
            {
                var pr = new PondRepository();
                var ponddata = pr.GetActivePonds(int.Parse(uDto.FarmId));
                DateTime startdate = DateTime.Parse(uDto.CurrentTime);
                var pondcol = new Dictionary<string, Collection<Dictionary<string, string>>>();
                foreach (Pond ponditem in ponddata)
                {
                    
                    var col = new Collection<Dictionary<string, string>>();
                    
                        var fr = new FeedingRepository();
                        var data = fr.GetPondLast7Feedings(ponditem.PondId);
                        foreach (var item in data)
                        {

                            if (item != null)
                            {
                                var dic = new Dictionary<string, string>();

                                dic.Add("PondId", item.PondId.ToString());
                                dic.Add("FeedingId", item.FeedingId.ToString());
                                dic.Add("FeedDate", item.FeedDate.ToString());
                                dic.Add("PoundsFed", item.PoundsFed.ToString());
                                col.Add(dic);
                            }

                        }
                    pondcol.Add(ponditem.PondId.ToString(), col);
                }



                var retVal = new FarmFeedDTO
                {
                    Key = key,
                    ReturnData = pondcol
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }


        [HttpPost]
        public HttpResponseMessage Farms([FromBody] FarmDTO cqDTO)
        {
            return Farms(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage FarmList([FromBody] FarmDTO cqDTO)
        {
            return Farms(Request, cqDTO);
        }

        [HttpPost]
        public HttpResponseMessage FarmDetail([FromBody] FarmDTO cqDTO)
        {
            return Farms(Request, cqDTO);
        }
        [HttpPut]
        public HttpResponseMessage FarmAddOrEdit([FromBody] FarmDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Admin"))
            {
                var Farm = new Farm();
                var errors = ValidateDtoData(uDto, Farm);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEFarmId = 0;
                if (int.TryParse(uDto.FarmId, out NEFarmId))
                {
                    if (NEFarmId == -1)
                    {
                        //  creating new Farm record   
                        return ProcessNewFarmRecord(Request, uDto, key, companyId, UserId);
                    }
                    else
                    {
                        //  editing existing Farm record  
                        return ProcessExistingFarmRecord(Request, uDto, NEFarmId, key, companyId, UserId);
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
        public HttpResponseMessage ChangeFarmStatus([FromBody] FarmDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var UserId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            AppUserRoleRepository aur = new AppUserRoleRepository();


            if (UserId > 0 && aur.IsInRole(UserId, "Admin"))
            {
                var Farm = new Farm();
                var errors = ValidateDtoData(uDto, Farm);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEFarmId = 0;
                if (int.TryParse(uDto.FarmId, out NEFarmId))
                {
                    if (NEFarmId != -1)
                    {
 
                        return ChangeThisFarmStatus(Request, uDto, NEFarmId, key, companyId, UserId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }
        private HttpResponseMessage ProcessNewFarmRecord(HttpRequestMessage request, FarmDTO uDto, string key, int companyId, int UserId)
        {
            var ur = new FarmRepository();
            var Farm = new Farm();
            bool newfromsetup;
            if (uDto.CompanyId == null)
            {
                uDto.CompanyId = companyId.ToString();
                newfromsetup = false;
            }
            else
            {
                newfromsetup = true;
            }
            var validationErrors = GetValidationErrors(ur, Farm, uDto, companyId, UserId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors... 
            //Farm.CompanyId = companyId;

            Farm = ur.Save(Farm);
            if (newfromsetup)
            {
                var aur = new AppUserRepository();
                var users = aur.GetByCompanyId(Farm.CompanyId);
                foreach (User user in users)
                {
                    var ufr = new UserFarmRepository();
                    var ufarm = new UserFarm();
                    //ur = aur.GetByUserAndRoleIds(contactId, int.Parse(cqDto.RoleID)); 
                    ufarm.UserId = user.UserId;
                    ufarm.FarmId= Farm.FarmId;
                    ufarm.StatusId = 1;
                    ufr.Save(ufarm);
                }
            }
            uDto.Key = key;
            uDto.FarmId = Farm.FarmId.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = Farm.FarmId
            }));
            return response;
        }

        private HttpResponseMessage ProcessExistingFarmRecord(HttpRequestMessage request, FarmDTO cqDto, int contactId, string key, int companyId, int UserId)
        {
            var ur = new FarmRepository();
            var Farm = new Farm();
            Farm = ur.GetById(contactId);
            //  is the Farm eligible to update the prospect?

            var validationErrors = GetValidationErrors(ur, Farm, cqDto, companyId, UserId);
            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors...              
            ur.Save(Farm);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private HttpResponseMessage ChangeThisFarmStatus(HttpRequestMessage request, FarmDTO cqDto, int contactId, string key, int companyId, int UserId)
        {
            var ur = new FarmRepository();
            var Farm = new Farm();
            Farm = ur.GetById(contactId);
            Farm.StatusId = int.Parse(cqDto.StatusId);
            //  no validation errors...              
            ur.Save(Farm);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(FarmRepository pr, Farm contact, FarmDTO cqDto, int companyId, int FarmId)
        {
            contact.ProcessRecord(cqDto);

            return pr.Validate(contact);
        }
    }
}