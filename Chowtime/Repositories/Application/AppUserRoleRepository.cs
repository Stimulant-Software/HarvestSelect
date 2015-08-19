using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using SGApp.Utility;
using SGApp.Models.EF;

namespace SGApp.Repository.Application
{
    public class AppUserRoleRepository : RepositoryBase<UserRole>
    {

        #region Public Methods

        public IList<UserRole> GetUserRoles(int userId)
        {
            return this.EntityCollection.Where(r => r.UserId == userId).ToList();
        }

        public bool IsInRole(int userId, string roleName) {
            return GetUserRoles(userId).Any(x => x.Role.RoleName == roleName);
        }

        #endregion



        #region  RepositoryBase<UserRole> implementation


        public override IQueryable<UserRole> EntityCollection
        {
            get
            {
                return this.DbContext.UserRoles.Include("Role").AsQueryable();
            }
        }

        protected override UserRole DeleteRecord(UserRole entity)
        {
            DbContext.UserRoles.Remove(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override UserRole InsertRecord(UserRole entity)
        {
            DbContext.UserRoles.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override UserRole UpdateRecord(UserRole entity)
        {
            DbContext.SaveChanges();
            return entity;
        }

        

        #endregion

        public override List<UserRole> GetByPredicate(string predicate) {
            var iq = DbContext.UserRoles.Include("User").Include("Role").AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).Take(50).ToList() : iq.Take(50).ToList();
        }



        public override UserRole GetById(int id)
        {
            return DbContext.UserRoles.Where(x => x.UserRoleId == id).SingleOrDefault();
        }

        public UserRole GetByUserAndRoleIds(int userid, int roleid)
        {
            return DbContext.UserRoles.Where(x => x.UserId == userid && x.RoleId == roleid).SingleOrDefault();
        }
    }
}
