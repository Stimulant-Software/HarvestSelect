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

namespace SGApp.Repository.Application
{
    public class RoleRepository : RepositoryBase<Role>
    {
        public override System.Linq.IQueryable<Role> EntityCollection
        {
            get
            {
                return DbContext.Roles.AsQueryable();
            }
        }

        protected override Role DeleteRecord(Role entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Role InsertRecord(Role entity)
        {
            DbContext.Roles.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override Role UpdateRecord(Role entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<Role> GetByPredicate(string predicate)
        {
            var iq = DbContext.Roles.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).Take(50).ToList() : iq.Take(50).ToList();
        }

        public List<Role> GetRoles()
        {

            return DbContext.Roles
            .OrderBy(x => x.RoleName).Where(x => x.RoleName != "SGAdmin").ToList();
        }


        public override Role GetById(int id)
        {
            return DbContext.Roles.Where(x => x.RoleId == id).SingleOrDefault();
        }


    }


}
