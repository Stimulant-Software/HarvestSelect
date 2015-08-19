using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Security.Cryptography;
using System.Text;
using SGApp.Utility;
using SGApp.Models.EF;

namespace SGApp.Repository.Application {
    public class AppUserRepository : RepositoryBase<User> {
        #region Public Methods

        /// <summary>
        /// Validates the user. Checks if user with given username and password exists in the system.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="companyId">The user's company.</param>
        /// <returns>User id if user exists and -1 if does not.</returns>
        public int ValidateUser(string userName, byte[] password, ref int? companyId) {
            var user = GetUser(userName, password, ref companyId);
            return user != default(User) ? user.UserId : -1;
        }

        public int ValidateUser(string inKey, out string outKey, ref int companyId) {
            int? userID;
            outKey = string.Empty;            
            var cipherTextBytes = Convert.FromBase64String(inKey);
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
            if (companyId > -1) {
                var companyIndex = decryptedString.IndexOf("||");
                companyId = int.Parse(decryptedString.Substring(0, companyIndex));
                decryptedString = decryptedString.Substring(companyIndex + 2);
            }
            var lastPipe = decryptedString.LastIndexOf("||");
            var firstIndex = decryptedString.IndexOf("||");

            string timeString = decryptedString.Substring(lastPipe + 2, decryptedString.Length - lastPipe - 2);
            var time = DateTime.ParseExact(timeString, Constants.SecurityTokenDateFormat, CultureInfo.InvariantCulture);
            
            var ts = DateTime.Now.Subtract(time);
            if (ts.Minutes < 15) {
                int? compId = (int?)companyId;
                var userName = decryptedString.Substring(0, firstIndex);
                var password = decryptedString.Substring(firstIndex + 2, lastPipe - firstIndex - 2);                
                userID = ValidateUser(userName, SecurityUtils.GetBinaryPassword(password), ref compId);
                if (userID.HasValue && userID.Value > 0) {
                    outKey = SecurityUtils.CreateUserSecurityKey(userName, password, companyId);
                    return userID.Value;
                }
                return 0;
            }
            return 0;
        }


        public User GetUser(string userName, byte[] password, ref int? companyId) {
            if(companyId == -1) {
                var user = EntityCollection.SingleOrDefault(
                      u => u.EmailAddress == userName && u.Password == password && u.StatusId == 1);
                if (user != null) {
                    companyId = user.CompanyId;
                    return user;
                }
                return null;
            }
            var coId = companyId;
            return EntityCollection.SingleOrDefault(
                u => u.EmailAddress == userName && u.Password == password && u.StatusId == 1 && u.CompanyId == coId);
        }
        public User GetUser(int userid) {
            return EntityCollection.SingleOrDefault(u => u.UserId == userid);
        }


        #endregion

        #region Overrides of RepositoryBase<Organization>

        public override IQueryable<User> EntityCollection {
            get {
                return this.DbContext.Users.AsQueryable();
            }
        }

        protected override User DeleteRecord(User entity) {
            throw new NotImplementedException();
        }

        protected override User InsertRecord(User entity) {
            throw new NotImplementedException();
        }

        protected override User UpdateRecord(User entity) {
            throw new NotImplementedException();
        }

        #endregion



        public override List<User> GetByPredicate(string predicate) {
            var iq = DbContext.Users.Include("UserRoles.Role").AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).Take(50).ToList() : iq.Take(50).ToList();
        }

        public List<User> GetAllSalesmen(int companyId) {
            var urr = new AppUserRoleRepository();
            return urr.EntityCollection
            .Include("Users")
            .Include("Role")
            .Where(
                    x => x.Role.RoleName == "SalesPerson" 
                    && x.User.CompanyId == companyId 
                    && x.User.StatusId == 1)
            .Select(x => x.User)
            .OrderBy(x => x.LastName).ToList();
        }

        public override User GetById(int userId) {
            return EntityCollection.SingleOrDefault(w => w.UserId == userId);
        }

        public User GetByEmail(string email)
        {
            return EntityCollection.SingleOrDefault(w => w.EmailAddress == email);
        }
        public List<User> GetByCompanyId(int companyId)
        {
            return EntityCollection.Where(w => w.CompanyId == companyId).ToList();
        }

    }
}
