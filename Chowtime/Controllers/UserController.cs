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
    public class UserController : BaseApiController
    {
        internal HttpResponseMessage Users(HttpRequestMessage request, UserDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new UserRepository();
                var u = new User();
                cqDTO.CompanyId = companyId.ToString();
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();

                foreach (var item in data)
                {

                        var dic = new Dictionary<string, string>();

                        dic.Add("UserId", item.UserId.ToString());
                        dic.Add("FirstName", item.FirstName);
                        dic.Add("LastName", item.LastName);
                        dic.Add("EmailAddress", item.EmailAddress);
                        dic.Add("Phone", item.Phone);
                        dic.Add("StatusId", item.StatusId.ToString());
                        col.Add(dic);
                        var ufdic = new Dictionary<string, string>();
                        

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

        internal HttpResponseMessage UserDetail(HttpRequestMessage request, UserDTO cqDTO)
        {
            string key;
            var aur = new AppUserRepository();
            var companyId = 0;
            var userId = aur.ValidateUser(cqDTO.Key, out key, ref companyId);
            if (userId > 0)
            {
                var ur = new UserRepository();
                var u = new User();
                var predicate = ur.GetPredicate(cqDTO, u, companyId);
                var data = ur.GetByPredicate(predicate);
                var col = new Collection<Dictionary<string, string>>();
                string ufarms = "";
                string uroles = "";

                foreach (var item in data)
                {

                    var dic = new Dictionary<string, string>();

                    dic.Add("UserId", item.UserId.ToString());
                    dic.Add("FirstName", item.FirstName);
                    dic.Add("LastName", item.LastName);
                    dic.Add("EmailAddress", item.EmailAddress);
                    dic.Add("Phone", item.Phone);
                    dic.Add("StatusId", item.StatusId.ToString());
                    
                    foreach (var farmitem in item.UserFarms)
                    {
                        ufarms = ufarms + farmitem.FarmId.ToString() + ",";
                         
                    }
                    if (ufarms.Length > 0)
                    {
                        //ufarms = ufarms.Remove(uroles.Length - 1);
                        dic.Add("Farms", ufarms);
                    
                    }
                    foreach (var roleitem in item.UserRoles)
                    {
                        uroles = uroles + roleitem.RoleId.ToString() + ",";
                        
                    }
                    if (uroles.Length > 0)
                    {
                        //uroles = uroles.Remove(uroles.Length - 1) ;
                        dic.Add("Roles", uroles);
                    }
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


        [HttpPost]
        public HttpResponseMessage Users([FromBody] UserDTO cqDTO)
        {
            return Users(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage UserList([FromBody] UserDTO cqDTO)
        {
            return Users(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage UserDetail([FromBody] UserDTO cqDTO)
        {
            return UserDetail(Request, cqDTO);
        }
        [HttpPost]
        public HttpResponseMessage AllRoles([FromBody] UserDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            if (userId > 0)
            {
                var user = new User();
                var errors = ValidateDtoData(uDto, user);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var col = new Collection<Dictionary<string, string>>();
                var pr = new RoleRepository();
                var data = pr.GetRoles();
                    foreach (var item in data)
                    {

                        
                        var dic = new Dictionary<string, string>();

                        dic.Add("RoleId", item.RoleId.ToString());
                        dic.Add("RoleName", item.RoleName);
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
        [HttpPut]
        public HttpResponseMessage UserAddOrEdit([FromBody] UserDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            if (userId > 0)
            {
                var user = new User();
                var errors = ValidateDtoData(uDto, user);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.UserId, out NEUserId))
                {
                    if (NEUserId == -1)
                    {
                        //  creating new User record   
                        return ProcessNewUserRecord(Request, uDto, key, companyId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return ProcessExistingUserRecord(Request, uDto, NEUserId, key, companyId, userId);
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
        public HttpResponseMessage ChangeUserStatus([FromBody] UserDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            if (userId > 0)
            {
                var user = new User();
                var errors = ValidateDtoData(uDto, user);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.UserId, out NEUserId))
                {
                    if (NEUserId != -1)
                    {
                         //  editing existing User record  
                        return ChangeThisUserStatus(Request, uDto, NEUserId, key, companyId, userId);
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
        public HttpResponseMessage SetPassword([FromBody] LoginDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            if (userId > 0)
            {
                var user = new User();
                var errors = ValidateDtoData(uDto, user);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.UserId, out NEUserId))
                {
                    if (NEUserId != -1)
                    {
                        //  editing existing User record  
                        return SetPassword(Request, uDto, NEUserId, key, companyId, userId);
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
        public HttpResponseMessage UserAddOrRemoveRole([FromBody] UserRoleDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            if (userId > 0)
            {
                var user = new User();
                var errors = ValidateDtoData(uDto, user);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.UserID, out NEUserId))
                {
                    if (bool.Parse(uDto.AddRemove) == false)
                    {
                        //  creating new User record   
                        return RemoveUserRole(Request, uDto, NEUserId, key, companyId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return AddUserRole(Request, uDto, NEUserId, key, companyId, userId);
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
        public HttpResponseMessage UserAddOrRemoveFarm([FromBody] UserFarmDTO uDto)
        {
            string key;
            var ur = new AppUserRepository();
            var companyId = 0;
            var userId = ur.ValidateUser(uDto.Key, out key, ref companyId);

            if (userId > 0)
            {
                var user = new User();
                var errors = ValidateDtoData(uDto, user);
                if (errors.Any())
                {
                    return ProcessValidationErrors(Request, errors, key);
                }
                var NEUserId = 0;
                if (int.TryParse(uDto.UserID, out NEUserId))
                {
                    if (bool.Parse(uDto.AddRemove) == false)
                    {
                        //  creating new User record   
                        return RemoveUserFarm(Request, uDto, NEUserId, key, companyId, userId);
                    }
                    else
                    {
                        //  editing existing User record  
                        return AddUserFarm(Request, uDto, NEUserId, key, companyId, userId);
                    }
                }
                //  no idea what this is
                var msg = "invalid data structure submitted";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var message = "validation failed";
            return Request.CreateResponse(HttpStatusCode.NotFound, message);
        }

        private HttpResponseMessage ProcessNewUserRecord(HttpRequestMessage request, UserDTO uDto, string key, int companyId, int userId)
        {
            var ur = new UserRepository();

            //var userRepository = new AppUserRepository();
            var user = new User();
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
            //int? companyIdx = -1;
            if (ur.GetByEmail(uDto.EmailAddress) != null)
            {

                var msg = "Unable to add new user.  Email Address already Exists.";
                return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            var validationErrors = GetValidationErrors(ur, user, uDto, companyId, userId);

            if (validationErrors.Any())
            {
                return ProcessValidationErrors(request, validationErrors, key);
            }
            //  no validation errors... 
            //if (uDto.Password != "sg")
            //{
            //    user.Password = SecurityUtils.GetBinaryPassword(uDto.Password.ToString());
            //}
            //else
            //{
            //    SGApp.Models.EF.User userexist = userRepository.GetUser(uDto.EmailAddress, SecurityUtils.GetBinaryPassword(uDto.Password), ref companyIdx);
            //    user.Password = userexist.Password;
            //}
            //user.CompanyId = companyId;
            //user.UserId = null;
            user = ur.Save(user);
            if (newfromsetup)
            {
                var rr = new RoleRepository();
                var roles = rr.GetRoles();
                foreach (Role role in roles)
                {
                    var aur = new AppUserRoleRepository();
                    var urole = new UserRole();
                    //ur = aur.GetByUserAndRoleIds(contactId, int.Parse(cqDto.RoleID)); 
                    urole.UserId = user.UserId;
                    urole.RoleId = role.RoleId;
                    aur.Save(urole);
                }
            }
            uDto.Key = key;
            uDto.UserId = user.UserId.ToString();
            var response = request.CreateResponse(HttpStatusCode.Created, uDto);
            response.Headers.Location = new Uri(Url.Link("Default", new
            {
                id = user.UserId
            }));
            return response;
        }

        private HttpResponseMessage ProcessExistingUserRecord(HttpRequestMessage request, UserDTO cqDto, int contactId, string key, int companyId, int userId)
        {
            var ur = new UserRepository();
            //var userRepository = new AppUserRepository();
            var user = new User();
            user = ur.GetById(contactId);
            //int? companyIdx = -1;
            //  is the user eligible to update the prospect?
            bool newfromsetup;
            if (cqDto.CompanyId == null)
            {
                cqDto.CompanyId = companyId.ToString();
                newfromsetup = false;
            }
            else
            {
                newfromsetup = true;
            }
           
                var validationErrors = GetValidationErrors(ur, user, cqDto, companyId, userId);
                if (validationErrors.Any())
                {
                    return ProcessValidationErrors(request, validationErrors, key);
                }
                //  no validation errors...
                //if (cqDto.Password != "sg")
                //{
                //    user.Password = SecurityUtils.GetBinaryPassword(cqDto.Password.ToString());
                //}
                //else
                //{
                //    SGApp.Models.EF.User userexist = userRepository.GetUser(cqDto.EmailAddress, SecurityUtils.GetBinaryPassword(cqDto.Password), ref companyIdx);
                //    user.Password = userexist.Password;
                //}
            
                ur.Save(user);
                if (newfromsetup)
                {
                    var rr = new RoleRepository();
                    var roles = rr.GetRoles();
                    foreach (Role role in roles)
                    {
                        var aur = new AppUserRoleRepository();
                        var urole = new UserRole();
                        //ur = aur.GetByUserAndRoleIds(contactId, int.Parse(cqDto.RoleID)); 
                        urole.UserId = user.UserId;
                        urole.RoleId = role.RoleId;
                        aur.Save(urole);
                    }
                }
                cqDto.Key = key;
                return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }



        private HttpResponseMessage AddUserRole(HttpRequestMessage request, UserRoleDTO cqDto, int contactId, string key, int companyId, int userId)
        {
            var aur = new AppUserRoleRepository();
            var ur = new UserRole();
            //ur = aur.GetByUserAndRoleIds(contactId, int.Parse(cqDto.RoleID)); 
            ur.UserId = contactId;
            ur.RoleId = int.Parse(cqDto.RoleID);
            aur.Save(ur);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }

        private HttpResponseMessage RemoveUserRole(HttpRequestMessage request, UserRoleDTO cqDto, int contactId, string key, int companyId, int userId)
        {
            var aur = new AppUserRoleRepository();
            var ur = new UserRole();
            ur = aur.GetByUserAndRoleIds(contactId, int.Parse(cqDto.RoleID));
            aur.Delete(ur);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }

        private HttpResponseMessage AddUserFarm(HttpRequestMessage request, UserFarmDTO cqDto, int contactId, string key, int companyId, int userId)
        {
            var aur = new UserFarmRepository();
            var ur = new UserFarm();
            //ur = aur.GetByUserFarmIds(contactId, int.Parse(cqDto.FarmID));
            if (cqDto.FarmID == "0")
            {
                var fr = new FarmRepository();
                foreach (Farm f in fr.GetFarms())
                {
                    ur.FarmId = f.FarmId;
                    ur.UserId = contactId;
                    ur.StatusId = 1;
                    aur.Save(ur);
                }
            }
            else
            {
                ur.FarmId = int.Parse(cqDto.FarmID);
                ur.UserId = contactId;
                ur.StatusId = 1;
                aur.Save(ur);
            }
            
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }

        private HttpResponseMessage RemoveUserFarm(HttpRequestMessage request, UserFarmDTO cqDto, int contactId, string key, int companyId, int userId)
        {
            var aur = new UserFarmRepository();
            var ur = new UserFarm();
            if (cqDto.FarmID == "0")
            {
                foreach (UserFarm uf in aur.GetByUserId(int.Parse(cqDto.UserID)))
                {
                    aur.Delete(uf);
                }
            }
            else
            {
                ur = aur.GetByUserFarmIds(contactId, int.Parse(cqDto.FarmID));
                aur.Delete(ur);
            }
            
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private HttpResponseMessage ChangeThisUserStatus(HttpRequestMessage request, UserDTO cqDto, int contactId, string key, int companyId, int userId)
        {
            var ur = new UserRepository();
            var user = new User();
            user = ur.GetById(contactId);

            user.StatusId = int.Parse(cqDto.StatusId);

            ur.Save(user);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
        private List<DbValidationError> GetValidationErrors(UserRepository pr, User contact, UserDTO cqDto, int companyId, int userId)
        {
            contact.ProcessRecord(cqDto);
            return pr.Validate(contact);
        }

        private HttpResponseMessage SetPassword(HttpRequestMessage request, LoginDTO cqDto, int contactId, string key, int companyId, int userId)
        {
            var ur = new UserRepository();
            var user = new User();
            user = ur.GetById(contactId);

            user.Password = SecurityUtils.GetBinaryPassword(cqDto.Password.ToString());

            ur.Save(user);
            cqDto.Key = key;
            return request.CreateResponse(HttpStatusCode.Accepted, cqDto);

        }
    }
}