using SGApp.Models.EF;
using SGApp.Repository.Application;
using SGApp.Utility;
using SGApp.BusinessLogic;


namespace SGApp.BusinessLogic.Application
{
    public class AppUserDomain :  BusinessDomainBase<User>
    {
        #region Private Fields 
        private AppUserRepository _appUserRepository;
        #endregion

        #region Constructors

        public AppUserDomain()
        {
            //Init repositories
            this._appUserRepository  = new AppUserRepository();
        }

        public bool ValidateLogin(string userName, string password, ref int? companyId)
        {
            byte[] passwordHash = SecurityUtils.GetBinaryPassword(password);

            int userId = this._appUserRepository.ValidateUser(userName, passwordHash, ref companyId);

            return userId > -1;

        }

        #endregion

        
    }
}
