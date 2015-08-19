using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SGApp.Utility {
    public class SecurityUtils {
        public static byte[] GetBinaryPassword(string password) {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedPassword;
            UTF8Encoding encoder = new UTF8Encoding();
            hashedPassword = md5Hasher.ComputeHash(encoder.GetBytes(password));
            return hashedPassword;
        }

        public static string CreateUserSecurityKey(string userName, string password, int companyId) {
            var time = DateTime.Now;
            var stringToEncrypt = companyId + "||" + userName + "||" + password + "||" + time.ToString(Constants.SecurityTokenDateFormat);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(stringToEncrypt);
            byte[] keyBytes = new Rfc2898DeriveBytes(Constants.hash, Encoding.ASCII.GetBytes(Constants.salt)).GetBytes(32);
            var symmetricKey = new RijndaelManaged() {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.Zeros
            };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(Constants.VIKey));
            byte[] cipherTextBytes;
            using (var memoryStream = new MemoryStream()) {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }
    }
}
