using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SGApp.DTOs;
using SGApp.BusinessLogic.Application;
using SGApp.Controllers;
using SGApp.Repository.Application;
using SGApp.Utility;

namespace SGApp.Controllers {
    public class LoginController : BaseApiController {


        [HttpPost]
        public HttpResponseMessage ValidateLogin([FromBody] LoginDTO data) {

            var userName = data.UserName;
            var password = data.Password;

            var userDomain = new AppUserDomain();
            //Validate if user exist in the system
            int? companyId = -1;
            if (userDomain.ValidateLogin(userName, password, ref companyId)) {

                //Create repositories
                var userRepository = new AppUserRepository();
                var roleRepository = new AppUserRoleRepository();

                //Get user                 
                SGApp.Models.EF.User user = userRepository.GetUser(userName, SecurityUtils.GetBinaryPassword(password), ref companyId);


                //Get user roles
                IList<SGApp.Models.EF.UserRole> userRoles = roleRepository.GetUserRoles(user.UserId);

                var dic = new List<Dictionary<string, string>>();
                foreach (var item in userRoles) {
                    var d = new Dictionary<string, string>();
                    d.Add("RoleID", item.RoleId.ToString());
                    d.Add("RoleDescription", item.Role.RoleName);
                    dic.Add(d);
                }
                int compid = (int)companyId;
                var retVal = new KeyDTO {
                    UserID = user.UserId.ToString(),
                    CompanyId = user.CompanyId.ToString(),
                    UserRoles = dic,
                    Key = SecurityUtils.CreateUserSecurityKey(userName, password, compid)/*,
                    UserRoles = userRoles*/
                };
                return Request.CreateResponse(HttpStatusCode.OK, retVal);
                //return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(
                //            retVal,
                //            Formatting.Indented,
                //            new JsonSerializerSettings() {
                //                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                //                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                //            }
                //        ));
            } else {
                var message = "Invalid user name and/or password";
                return Request.CreateResponse(HttpStatusCode.NotFound, message);
            }
        }




    }
}
