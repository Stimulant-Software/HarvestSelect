﻿using SGApp.Repository.Application;
using SGApp.DTOs;
using SGApp.Models.EF;
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
    public class FarmController : BaseApiController {

	    private HttpResponseMessage GetBinInfo(HttpRequestMessage request, BinDto dto) {
			string key;
		    var aur = new AppUserRepository();
		    var companyId = 0;
		    var UserId = aur.ValidateUser(dto.Key, out key, ref companyId);
		    var aur1 = new AppUserRoleRepository();
		    if (UserId > 0 && aur1.IsInRole(UserId, "Admin")) {
				var br = new BinRepository();
			    var binCol = new Collection<Dictionary<string, string>>();
			    var bin = br.GetById(dto.BinID);
				binCol.Add(new Dictionary<string, string>() {
					{"BinID", bin.BinID.ToString()},
					{"BinName", bin.BinName},
                    {"FarmIDs",  "[" + string.Join(",", bin.BinFarms.Select(x => x.FarmID).ToArray()) + "]"},
                    {"CurrentTicket", bin.CurrentTicket.HasValue ? bin.CurrentTicket.Value.ToString() : ""},
					{"CurrentPounds", bin.CurrentPounds.HasValue ?bin.CurrentPounds.Value.ToString() : ""},
					{"LastDisbursement", bin.LastDisbursement.HasValue ? bin.LastDisbursement.Value.ToShortDateString() : ""},
					{"LastLoaded", bin.LastLoaded.HasValue ? bin.LastLoaded.Value.ToShortDateString() : ""}
				});
			    var retVal = new GenericDTO() {
				    Key = key,
				    Bins = binCol
			    };
			    return request.CreateResponse(HttpStatusCode.OK, retVal);
			};
		    var message = "validation failed";
		    return request.CreateResponse(HttpStatusCode.NotFound, message);
		}

		private HttpResponseMessage GetAllFarms(HttpRequestMessage request, FarmDTO dto) {
			string key;
		    var aur = new AppUserRepository();
		    var companyId = 0;
		    var UserId = aur.ValidateUser(dto.Key, out key, ref companyId);
		    var aur1 = new AppUserRoleRepository();
		    if (UserId > 0 && aur1.IsInRole(UserId, "Admin")) {
			    var fr = new FarmRepository();
			    var farmCol = new Collection<Dictionary<string, string>>();
			    var farms = fr.GetAllFarms();
			    foreach (var farm in farms) {
				    farmCol.Add(new Dictionary<string, string>() {
					    {"FarmId", farm.FarmId.ToString()},
					    {"StatusId", farm.StatusId.ToString()},
					    {"FarmName", farm.FarmName},
					    {"CompanyId", farm.CompanyId.ToString() },
					    {"OldFarmId", farm.OldFarmId.ToString()},
					    {"InnovaName", !string.IsNullOrEmpty(farm.InnovaName) ? farm.InnovaName : ""},
					    {"InnovaCode", !string.IsNullOrEmpty(farm.InnovaCode) ? farm.InnovaCode : ""}
				    });
			    }
			    var retVal = new GenericDTO() {
				    Key = key,
					ReturnData = farmCol
			    };
			    return request.CreateResponse(HttpStatusCode.OK, retVal);
		    };
		    var message = "validation failed";
		    return request.CreateResponse(HttpStatusCode.NotFound, message);
		}

		private HttpResponseMessage GetBinsForAdminPage(HttpRequestMessage request, BinDto dto) {
			string key;
		    var aur = new AppUserRepository();
		    var companyId = 0;
		    var UserId = aur.ValidateUser(dto.Key, out key, ref companyId);
		    var aur1 = new AppUserRoleRepository();
		    if (UserId > 0 && aur1.IsInRole(UserId, "Admin")) {
			    var br = new BinRepository();
			    var binCol = new Collection<Dictionary<string, string>>();
			    var bins = br.GetBins();
			    foreach (var bin in bins) {
				    binCol.Add(new Dictionary<string, string>() {
					    {"BinID", bin.BinID.ToString()},
					    {"BinName", bin.BinName},
                        {"FarmIDs", "[" + string.Join(",", bin.BinFarms.Select(x => x.FarmID).ToArray()) + "]"},
                        {"CurrentTicket", bin.CurrentTicket.HasValue ? bin.CurrentTicket.Value.ToString() : ""},
					    {"CurrentPounds", bin.CurrentPounds.HasValue ?bin.CurrentPounds.Value.ToString() : ""},
					    {"LastDisbursement", bin.LastDisbursement.HasValue ? bin.LastDisbursement.Value.ToShortDateString() : ""},
					    {"LastLoaded", bin.LastLoaded.HasValue ? bin.LastLoaded.Value.ToShortDateString() : ""}
				    });
			    }
			    var retVal = new GenericDTO() {
				    Key = key,
				    Bins = binCol
			    };
			    return request.CreateResponse(HttpStatusCode.OK, retVal);
		    };
		    var message = "validation failed";
		    return request.CreateResponse(HttpStatusCode.NotFound, message);
		}

		private HttpResponseMessage AddOrEditBin(HttpRequestMessage request, BinDto dto) {
			string key;
		    var aur = new AppUserRepository();
		    var companyId = 0;
		    var UserId = aur.ValidateUser(dto.Key, out key, ref companyId);
		    var aur1 = new AppUserRoleRepository();
		    if (UserId > 0 && aur1.IsInRole(UserId, "Admin")) {
			    var br = new BinRepository();
			    var binId = dto.BinID;
			    var bin = new Bin();
			    if (binId == -1) {
				    //	adding bin
					bin = br.GetNewBinRecord();
				} else {
					// editing bin
				    bin = br.GetById(binId);
			    }
				bin.BinName = dto.BinName;
			    bin.FarmID = dto.FarmID;
			    bin.CurrentTicket = dto.CurrentTicket;
			    bin.LastDisbursement = dto.LastDisbursement;
			    bin.LastLoaded = dto.LastLoaded;
				var binDisb = new BinDisbursement();
			    if (dto.Reconciliation.HasValue) {
				    binDisb = br.GetNewBinDisbursementRecord();
				    binDisb.BinID = bin.BinID;
				    binDisb.TicketNumber = bin.CurrentTicket.HasValue ? bin.CurrentTicket.Value : 0;
				    binDisb.Pounds = Math.Abs(dto.Reconciliation.Value);
				    binDisb.Note = "Adjusted by Reconciliation";
				    binDisb.DisbursementType = br.GetDisbursementType("Reconciliation");
				    binDisb.DisbursementDate = DateTime.Now;
				    binDisb.DateCreated = DateTime.Now;
				    binDisb.FeedID = null;	//	no feeding record when reconciling
				    binDisb.UserID = UserId;
			    }
			    br.SaveChanges();
                br.UpdateBinFarms(dto.BinFarms, bin.BinID);
                if (binDisb.BinDisbursementID > 0) {
				    br.UpdateBinCurrentPounds(null, binDisb);
				    bin = br.GetById(binId);
				    dto.CurrentPounds = bin.CurrentPounds;
			    }
			    dto.Key = key;
			    dto.BinID = bin.BinID;
			    return request.CreateResponse(HttpStatusCode.OK, dto);
			};
		    var message = "validation failed";
		    return request.CreateResponse(HttpStatusCode.NotFound, message);
		}

		private HttpResponseMessage AddBinLoad(HttpRequestMessage request, BinLoadDto dto) {
		    string key;
		    var aur = new AppUserRepository();
		    var companyId = 0;
		    var UserId = aur.ValidateUser(dto.Key, out key, ref companyId);
		    var aur1 = new AppUserRoleRepository();
		    if (UserId > 0 && aur1.IsInRole(UserId, "User")) {
			    var br = new BinRepository();
                var binLoadrec = br.GetNewBinLoadRecord();
                binLoadrec.BinID = dto.BinID;
                //binLoadrec.TicketNumber = dto.TicketNumber;
                binLoadrec.DateLoaded = dto.DateLoaded;
                var currPounds = br.GetBinData(dto.BinID).CurrentPounds;
                binLoadrec.PoundsLoaded = currPounds != null ? -currPounds.Value : 0;
                binLoadrec.Vendor = "---";
                binLoadrec.Note = "RECONCILIATION";
                binLoadrec.CreatedDate = DateTime.Now;
                binLoadrec.UserID = UserId;
                br.SaveChanges();
                br.UpdateBinCurrentPounds(binLoadrec, null);
                var binLoad = br.GetNewBinLoadRecord();
			    binLoad.BinID = dto.BinID;
			    binLoad.TicketNumber = dto.TicketNumber;
			    binLoad.DateLoaded = dto.DateLoaded;
			    binLoad.PoundsLoaded = dto.PoundsLoaded;
			    binLoad.Vendor = dto.Vendor;
			    binLoad.Note = dto.Note;
			    binLoad.CreatedDate = DateTime.Now;
			    binLoad.UserID = UserId;
			    br.SaveChanges();
				br.UpdateBinCurrentPounds(binLoad, null);
				var retVal = new FarmDTO() {
				    Key = key,
					loadSkipCount = 0,
					Bin = new BinDto() {
						BinID = binLoad.BinID
					}
			    };
			    return GetBinLoads(request, retVal);
		    }
		    var message = "validation failed";
		    return request.CreateResponse(HttpStatusCode.NotFound, message);
		}

		private HttpResponseMessage FarmsForBinLoads(HttpRequestMessage request, FarmDTO dto) {
			string key;
		    var aur = new AppUserRepository();
		    var companyId = 0;
		    var UserId = aur.ValidateUser(dto.Key, out key, ref companyId);
		    var aur1 = new AppUserRoleRepository();
		    if (UserId > 0 && aur1.IsInRole(UserId, "User")) {
			    var statusId = int.Parse(dto.StatusId);
			    var fr = new FarmRepository();
			    var f = new Farm();
			    var userFarms = fr.GetUserFarmsWithBins(UserId, statusId);
				var col = new Collection<Dictionary<string, string>>();
				var binCol = new Collection<Dictionary<string, string>>();
			    foreach (var farm in userFarms.Select(x => x.Farm)) {
				    if (farm.Bins.Any()) {
						var dic = new Dictionary<string, string> {
						    { "FarmId", farm.FarmId.ToString() },
						    { "FarmName", farm.FarmName },
						    { "StatusId", farm.StatusId.ToString() }
					    };
					    col.Add(dic);
					    foreach (var bin in farm.Bins.OrderBy((x => x.BinName))) {
						    dic = new Dictionary<string, string>() {
							    {"BinID", bin.BinID.ToString()},
							    {"BinName", bin.BinName },
							    {"FarmID", bin.FarmID.ToString()},
							    {"CurrentTicket", bin.CurrentTicket.ToString() },
							    {"CurrentPounds", bin.CurrentPounds.ToString() },
							    {"LastDisbursement", bin.LastDisbursement.HasValue ? bin.LastDisbursement.Value.ToShortDateString() : "" },
							    {"LastLoaded", bin.LastLoaded.HasValue ? bin.LastLoaded.Value.ToShortDateString() : "" }
						    };
						    binCol.Add(dic);
					    }
					}					
			    }

			    var retVal = new GenericDTO {
				    Key = key,
				    ReturnData = col,
				    Bins = binCol
			    };
			    return Request.CreateResponse(HttpStatusCode.OK, retVal);
		    }
		    var message = "validation failed";
		    return request.CreateResponse(HttpStatusCode.NotFound, message);
		}

		private HttpResponseMessage GetBinLoads(HttpRequestMessage request, FarmDTO dto) {
			string key;
			var aur = new AppUserRepository();
			var companyId = 0;
			var UserId = aur.ValidateUser(dto.Key, out key, ref companyId);
			var aur1 = new AppUserRoleRepository();
			if (UserId > 0 && aur1.IsInRole(UserId, "User")) {
				var br = new BinRepository();
				var binId = dto.Bin.BinID;
				var skip = dto.loadSkipCount;
				var binLoads = br.GetBinLoads(binId, skip.HasValue ? skip.Value : 0);
				var col = new Collection<Dictionary<string, string>>();
				foreach (var binLoad in binLoads) {
					var dic = new Dictionary<string, string>() {
							{"BinTicketID", binLoad.BinTicketID.ToString()},
							{"BinID", binLoad.BinID.ToString()},
							{"TicketNumber", binLoad.TicketNumber.ToString() },
							{"DateLoaded", binLoad.DateLoaded.ToShortDateString()},
							{"PoundsLoaded", binLoad.PoundsLoaded.ToString() },
							{"Vendor", binLoad.Vendor },
							{"Note", binLoad.Note }
						};
					col.Add(dic);
				}
				var bin = br.GetBinData(binId);
				var retVal = new GenericDTO {
					Key = key,
					ReturnData = col,
					Bins = new Collection<Dictionary<string, string>>() {
						new Dictionary<string, string>() {
							{"BinID", bin.BinID.ToString()},
							{"CurrentTicket", bin.CurrentTicket.HasValue ? bin.CurrentTicket.Value.ToString() : ""},
							{"CurrentPounds", bin.CurrentPounds.HasValue ? bin.CurrentPounds.Value.ToString() : ""},
							{"LastLoaded", bin.LastLoaded.HasValue ? bin.LastLoaded.Value.ToShortDateString() : ""},
							{"LastDisbursement", bin.LastDisbursement.HasValue ? bin.LastDisbursement.Value.ToShortDateString() : ""}
						}
					}
				};
				return request.CreateResponse(HttpStatusCode.OK, retVal);
			}
			var message = "validation failed";
			return request.CreateResponse(HttpStatusCode.NotFound, message);
		}


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
				//var binCol = new Collection<Dictionary<string, string>>();
				foreach (var item in data) {
                    var dic = new Dictionary<string, string>();
                    dic.Add("FarmId", item.FarmId.ToString());
                    dic.Add("FarmName", item.FarmName);
                    dic.Add("StatusId", item.StatusId.ToString());
                    dic.Add("InnovaName", item.InnovaName != null ? item.InnovaName : "");
                    dic.Add("InnovaCode", item.InnovaCode != null ? item.InnovaCode : "");
                    col.Add(dic);
					//foreach (var bin in item.Bins.OrderBy((x => x.BinName))) {
					//	dic = new Dictionary<string, string>() {
					//		{"BinID", bin.BinID.ToString()},
					//		{"BinName", bin.BinName },
					//		{"FarmID", bin.FarmID.ToString()},
					//		{"CurrentTicket", bin.CurrentTicket.ToString() },
					//		{"CurrentPounds", bin.CurrentPounds.ToString() },
					//		{"LastDisbursement", bin.LastDisbursement.HasValue ? bin.LastDisbursement.Value.ToShortDateString() : "" },
					//		{"LastLoaded", bin.LastLoaded.HasValue ? bin.LastLoaded.Value.ToShortDateString() : "" }
					//	};
					//	binCol.Add(dic);
					//}
                }

                var retVal = new GenericDTO
                {
                    Key = key,
                    ReturnData = col//,
					//Bins = binCol
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
	    public HttpResponseMessage AddBinLoad([FromBody] BinLoadDto dto) {
		    return AddBinLoad(Request, dto);

	    }

	    [HttpPost]
	    public HttpResponseMessage GetBinLoads([FromBody] FarmDTO dto) {
		    return GetBinLoads(Request, dto);
	    }

		[HttpPost] 
		public HttpResponseMessage AddOrEditBin([FromBody] BinDto dto) {
			return AddOrEditBin(Request, dto);
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
	    public HttpResponseMessage GetAllFarms([FromBody] FarmDTO dto) {
		    return GetAllFarms(Request, dto);
	    }

		

		[HttpPost]
	    public HttpResponseMessage FarmsForBinLoads([FromBody] FarmDTO dto) {
		    return FarmsForBinLoads(Request, dto);
	    }

		[HttpPost]
		public HttpResponseMessage GetBinsForAdminPage([FromBody] BinDto dto) {
			return GetBinsForAdminPage(Request, dto);
		}

		[HttpPost]
		public HttpResponseMessage GetBinInfo([FromBody] BinDto dto) {
			return GetBinInfo(Request, dto);
		}

        [HttpPost]
        public HttpResponseMessage YearlyFeedByPondAndMonth([FromBody] FeedReportDTO dto)
        {
            return YearlyFeedByPondAndMonth(Request, dto);
        }

        private HttpResponseMessage YearlyFeedByPondAndMonth(HttpRequestMessage request, FeedReportDTO dto)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var UserId = aur.ValidateUser(dto.Key, out key, ref companyId);
            var aur1 = new AppUserRoleRepository();
            if (UserId > 0 && aur1.IsInRole(UserId, "User"))
            {
                var db = new AppEntities();
                var currentFeeds = db.usp_FeedTotalsByPondMonth(dto.Year, dto.FarmId).Select(x => new MonthPondTotalDTO
                {
                    Month = x.Month,
                    MonthName = x.MonthName,
                    Pond = x.PondName,
                    PoundsFed = x.PoundsFed,
                    Year = x.Year
                }).ToList();

                var retVal = new FeedReportDTO
                {
                    Key = key,
                    MonthPondTotals = currentFeeds
                };
                return request.CreateResponse(HttpStatusCode.OK, retVal);
            }
            var message = "validation failed";
            return request.CreateResponse(HttpStatusCode.NotFound, message);
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