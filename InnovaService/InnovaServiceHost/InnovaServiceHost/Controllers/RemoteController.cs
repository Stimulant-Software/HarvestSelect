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
        public object GetProcSizes([FromBody] InnovaDto dto) {
            //  validate the key
            //if(ValidateKey(dto.Key)) {
                var context = new innova01Entities();
                try {
                    var data = context.proc_sizes.Where(x => x.size == 1).ToList();
                    return returnPackage(Request, data);
                }
                catch(Exception e) {
                    var s = "";
                    throw;
                }
                
                
            //}
            return null;
        }
    }
}
