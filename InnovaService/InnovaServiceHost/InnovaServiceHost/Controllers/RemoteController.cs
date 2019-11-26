﻿using System;
using System.Linq;
using System.Web.Http;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Configuration;
using InnovaServiceHost.DTOs;
using System.Globalization;
using InnovaService;
using System.Linq.Dynamic;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;


namespace InnovaServiceHost.Controllers {
    public class RemoteController : BaseController {

        #region Private Methods

        private bool ValidateKey(string key) {
            try {
                var cipherTextBytes = Convert.FromBase64String(key);
                var keyBytes = new Rfc2898DeriveBytes(Constants.hash, Encoding.ASCII.GetBytes(Constants.salt)).GetBytes(256 / 8);
                var symmetricKey = new RijndaelManaged() {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.None
                };

                var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(Constants.VIKey));
                var memoryStream = new MemoryStream(cipherTextBytes);
                var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                var plainTextBytes = new byte[cipherTextBytes.Length];

                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                var decryptedString = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
                var companyIndex = decryptedString.IndexOf("||");
                var companyIdFromKey = int.Parse(decryptedString.Substring(0, companyIndex));
                var companyId = int.Parse(ConfigurationManager.AppSettings["CompanyId"]);
                if(companyId != companyIdFromKey) {
                    return false;
                }
                decryptedString = decryptedString.Substring(companyIndex + 2);

                var lastPipe = decryptedString.LastIndexOf("||");
                var timeString = decryptedString.Substring(lastPipe + 2, decryptedString.Length - lastPipe - 2);
                var time = DateTime.ParseExact(timeString, Constants.SecurityTokenDateFormat, CultureInfo.InvariantCulture);

                var ts = DateTime.Now.Subtract(time);
                return ts.Minutes < 15;
            }
            catch(Exception) {
                return false;
            }
        }


        #endregion
        [HttpGet]
        public object UpdateSalesData()
        {
            SqlConnection con = new SqlConnection("data source=localhost;initial catalog=StimulantStage;persist security info=True;user id=StimulantUpdateSvc;password=Stimulant123!;MultipleActiveResultSets=True;");
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd = new SqlCommand("GetSalesDailyUpdate", con);
            cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.AddWithValue("@SuperID", id);//if you have parameters.
            da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            con.Close();
            return ds;
        }

        [HttpGet]
        public object UpdateAdagioFile()
        {
            var dic = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            var fileToUpdate = dic.First().Value;
            SqlConnection con = new SqlConnection("data source=localhost;initial catalog=StimulantStage;persist security info=True;user id=StimulantUpdateSvc;password=Stimulant123!;MultipleActiveResultSets=True;");
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            string sp = "";
            switch (fileToUpdate)
            {
                case "AN81CITM":
                    sp = "GetAdagioItems";
                    break;
                case "AN81CILO":
                    sp = "GetAdagioItemLocations";
                    break;
                case "AO80AHED":
                    sp = "GetAdagioOrderHeaders";
                    break;
                case "AO80ALIN":
                    sp = "GetAdagioOrderDetails";
                    break;

            }
            cmd = new SqlCommand(sp, con);
            cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.AddWithValue("@SuperID", id);//if you have parameters.
            da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            con.Close();
            return ds;
        }

        [HttpPost]
        public object GetCurrentShipping([FromBody] InnovaDto dto)
        {

            var context = new innova01Entities();
            var startDate = DateTime.Now.Date;
            var pl = from r in context.proc_packs.Where(x => x.regtime >= startDate)
                     orderby r.material
                     group r by r.material into grp
                     select new { key = grp.Key, cnt = grp.Count() };
            //var endDate = startDate.AddDays(5);
            return (from a in context.proc_orders.Where(x => x.dispatchtime >= startDate)
                    join b in context.proc_orderl
                    on a.order equals b.order

                    join p in context.proc_invstatus.Where(x => x.regtime >= startDate)
                    on b.material equals p.material into ps

                    from p in ps.DefaultIfEmpty()

                    join l in context.proc_materials
                    on b.material equals l.material

                    from tp in pl.Where(x => x.key == b.material).DefaultIfEmpty()
                    //on b.material equals tp.key
                    join bc in context.base_companies
                    on a.customer equals bc.company

                    select new
                    {
                        CustomerName = bc.name,
                        ItemDescription = l.name,
                        ItemCode = l.code,
                        OrderAmount = b.maxamount,
                        QuantityOnHand = p.units,
                        OrderDate = a.dispatchtime,
                        ShippedAmount = b.curamount,
                        TodayUnits = tp != null ? tp.cnt : 0
                    }
                    );
        }

        [HttpPost]
        public object GetKeithsData([FromBody] InnovaDto dto) {
            //  validate the key
            //if(ValidateKey(dto.Key)) {
                var context = new innova01Entities();
                //try {
                //    var data = context.proc_sizes.Where(x => x.size == 1).ToList();
                //    return returnPackage(Request, data);
                //}
                //catch(Exception e) {
                //    var s = "";
                //    throw;
                //}
                //var startDate = new DateTime(2015,7,1);
                //var endDate = new DateTime(2015,7,2);
                var startDate = dto.StartDate;
                var endDate = dto.StartDate.AddDays(1);
            return (from m in context.proc_materials
                                        .Where(x => x.shname == "Sample")                                        
                        join p in context.proc_packs.Where(x => x.rtype != 4
                                                            && x.regtime >= startDate
                                                            && x.regtime <= endDate)
                        on m.material equals p.material
                        join l in context.proc_lots
                        on p.lot equals l.lot
                        join bc in context.base_companies
                        on l.customer equals bc.company
                                        
                            select new {
                                m.code,
                                Farm = bc.name,
                                Pond = l.shname,
                                FarmPond = l.name,
                                Date = p.regtime,
                                RangeName = m.code,
                                RangeValue = m.name,
                                Weight = p.weight
                            }
                        ).OrderBy(x => x.code);
                
            //}
            return null;
        }

        [HttpPost]
        public object GetBOLData([FromBody] InnovaDto dto)
        {
            
            var context = new DB323Entities();
           
            var startDate = dto.StartDate;
            var endDate = dto.EndDate;
            //var mypredicate = "OrderDate == DateTime.Parse(\"" + startDate.ToShortDateString() + "\")";// +" && OrderDate <= " + startDate.ToShortDateString();
            //if (dto.CustomerNumber != "")
            //{
            //    mypredicate += " && CustNumber == " + dto.CustomerNumber + "";
            //}
            //if (dto.OrderNumber != null || dto.OrderNumber != "")
            //{
            //    mypredicate += " && OrderCode == \"" + dto.OrderNumber + "\"";
            //}


            return (from p in context.vwBOLProds.Where(x => x.OrderCode == dto.OrderCode)
                    
                    select new
                    {
                        OrderCode = p.OrderCode,
                        CustNumber = p.CustNumber,
                        CustShort = p.CustShort,
                        CustLong = p.CustLong,
                        OrderDate = p.OrderDate,
                        DispDate = p.DispDate,
                        PO1 = p.PO1,
                        PO2 = p.PO2,
                        PO3 = p.PO3,
                        Comments = p.Comments,
                        OrderTerms = p.OrderTerms,
                        ShipToName = p.ShipToName,
                        ShipToAddress = p.ShipToAddress,
                        ShipToPhone = p.ShipToPhone,
                        MaterialID = p.MaterialID,
                        ProdCode = p.ProdCode,
                        ProdName = p.ProdName,
                        OrderedAmt = p.OrderedAmt,
                        ShippedQty = p.ShippedQty,
                        ShippedWeight = p.ShippedWeight,
                        ApproxUnitWeight = p.ApproxUnitWeight,
                        HowPacked = p.HowPacked,
                        WeightLabel = p.WeightLabel,
                        CustomerAddress = p.CustomerAddress,
                        CustomerAddress2 = p.CustomerAddress2,
                        CustomerCity = p.CustomerCity,
                        CustomerState = p.CustomerState,
                        CustomerZip = p.CustomerZip,
                        CustomerPhone = p.CustomerPhone,
                        Terms = p.Terms,
                        CWItem = p.CWItem
                    });

            
        }
        [HttpPost]
        public object GetDailyProductionTotal([FromBody] InnovaDto dto)
        {
            var context = new innova01Entities();
            var startDate = dto.StartDate;
            var endDate = dto.EndDate;
            //List<int> stations = new List<int>();
            //stations.Add(8);
            //stations.Add(10);
            //stations.Add(12);
            //stations.Add(13);
            //stations.Add(14);
            //stations.Add(15);
            //stations.Add(16);
            return (from p in context.proc_packs.Where(x => //stations.Contains(x.station.Value) &&
                                                         x.prday >= startDate
                                                        && x.prday <= endDate
                                                        && x.conum != null)
                                                        group p by p.device into g
                    select new
                    {
                        Station = g.Key,
                        Nominal = g.Sum(x => x.nominal),
                        Weight = g.Sum(x => x.weight)
                    });

        }
        [HttpPost]
        public object GetTodaysProductionTotal([FromBody] InnovaDto dto)
        {
            var context = new innova01Entities();
            var startDate = DateTime.Now.Date;

            return (from p in context.proc_packs.Where(x => x.prday >= startDate && x.conum != null)
                    group p by p.device into g
                    select new
                    {
                        Station = g.Key,
                        Nominal = g.Sum(x => x.nominal),
                        Weight = g.Sum(x => x.weight)
                    });

        }

        [HttpPost]
        public object GetAdagioItems([FromBody] AdagioItemsImport dto)
        {
            var db = new StimulantStageEntities();
            var retVal = db.Database.SqlQuery<AdagioItemsImport>("execute GetAdagioItems");
            
            return retVal;

        }

        [HttpPost]
        public object GetAdagioOrderDetails([FromBody] OrderDetailsImport dto)
        {
            var db = new StimulantStageEntities();
            var retVal = db.Database.SqlQuery<OrderDetailsImport>("execute GetAdagioOrderDetails").ToArray();
            return retVal;

        }

        [HttpPost]
        public object GetAdagioOrderDetailsForToday([FromBody] OrderDetailsImport dto)
        {
            var db = new StimulantStageEntities();
            var retVal = db.Database.SqlQuery<OrderDetailsImport>("execute GetAdagioOrderDetailsForToday").ToArray();
            return retVal;

        }
        [HttpPost]
        public object GetAdagioOrdersForToday([FromBody] OrderHeadersImport dto)
        {
            var db = new StimulantStageEntities();
            var retVal = db.Database.SqlQuery<OrderHeadersImport>("execute GetAdagioOrdersForToday").ToArray();
            return retVal;

        }

        [HttpPost]
        public object GetAdagioOrderHeaders([FromBody] OrderHeadersImport dto)
        {
            var db = new StimulantStageEntities();
            var retVal = db.Database.SqlQuery<OrderHeadersImport>("execute GetAdagioOrderHeaders").ToArray();
            return retVal;

        }

        [HttpPost]
        public object GetAdagioSalesTransactions([FromBody] SalesTransactionsImport dto)
        {
            var db = new StimulantStageEntities();
            var retVal = db.Database.SqlQuery<SalesTransactionsImport>("execute GetDailySalesUpdate").ToArray();
            return retVal;

        }

        [HttpPost]
        public object GetAdagioCustomers([FromBody] CustomersImport dto)
        {
            var db = new StimulantStageEntities();
            var retVal = db.Database.SqlQuery<CustomersImport>("execute GetAdagioCustomers").ToArray();
            return retVal;

        }
    }
}
