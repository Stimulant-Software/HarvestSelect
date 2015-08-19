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
    public class UserFarmRepository : RepositoryBase<UserFarm>
    {
        public override System.Linq.IQueryable<UserFarm> EntityCollection
        {
            get
            {
                return DbContext.UserFarms.AsQueryable();
            }
        }

        protected override UserFarm DeleteRecord(UserFarm entity)
        {
            DbContext.UserFarms.Remove(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override UserFarm InsertRecord(UserFarm entity)
        {
            DbContext.UserFarms.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override UserFarm UpdateRecord(UserFarm entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<UserFarm> GetByPredicate(string predicate)
        {
            var iq = DbContext.UserFarms.Include("User").Include("Farm").AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).Take(50).ToList() : iq.Take(50).ToList();
        }




        public override UserFarm GetById(int id)
        {
            return DbContext.UserFarms.Where(x => x.UserFarmId == id).SingleOrDefault();
        }

        public UserFarm GetByUserFarmIds(int userid, int farmid)
        {
            return DbContext.UserFarms.Where(x => x.UserId == userid && x.FarmId == farmid).SingleOrDefault();
        }

        public List<UserFarm> GetByUserId(int userid)
        {
            return DbContext.UserFarms.Where(x => x.UserId == userid).ToList();
        }
    }


}
