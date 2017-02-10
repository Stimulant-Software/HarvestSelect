using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Configuration;
using InnovaServiceHost.DTOs;
using System.Globalization;
using InnovaService;
using System.Linq.Dynamic;


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
                var endDate = dto.EndDate;
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
                        Terms = p.Terms
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
    }
}
