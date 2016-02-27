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
    public class DownTimeTypeRepository : RepositoryBase<DownTimeType>
    {
        public override System.Linq.IQueryable<DownTimeType> EntityCollection
        {
            get
            {
                return DbContext.DownTimeTypes.AsQueryable();
            }
        }

        protected override DownTimeType DeleteRecord(DownTimeType entity)
        {
            throw new System.NotImplementedException();
        }

        protected override DownTimeType InsertRecord(DownTimeType entity)
        {
            DbContext.DownTimeTypes.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override DownTimeType UpdateRecord(DownTimeType entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<DownTimeType> GetByPredicate(string predicate)
        {
            var iq = DbContext.DownTimeTypes.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<DownTimeType> GetDownTimeTypes()
        {

            return DbContext.DownTimeTypes.ToList();
        }


        public override DownTimeType GetById(int id)
        {
            return DbContext.DownTimeTypes.Where(x => x.DownTimeTypeID == id).SingleOrDefault();
        }

    }


}
