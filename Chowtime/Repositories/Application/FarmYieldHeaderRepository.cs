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
    public class FarmYieldHeaderRepository : RepositoryBase<FarmYieldHeader>
    {
        public override System.Linq.IQueryable<FarmYieldHeader> EntityCollection
        {
            get
            {
                return DbContext.FarmYieldHeaders.AsQueryable();
            }
        }

        protected override FarmYieldHeader DeleteRecord(FarmYieldHeader entity)
        {
            DbContext.FarmYieldHeaders.Remove(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override FarmYieldHeader InsertRecord(FarmYieldHeader entity)
        {
            DbContext.FarmYieldHeaders.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override FarmYieldHeader UpdateRecord(FarmYieldHeader entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<FarmYieldHeader> GetByPredicate(string predicate)
        {
            var iq = DbContext.FarmYieldHeaders.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<FarmYieldHeader> GetFarmYieldHeaders()
        {

            return DbContext.FarmYieldHeaders
            .OrderBy(x => x.YieldDate).ToList();
        }


        public override FarmYieldHeader GetById(int id)
        {
            return DbContext.FarmYieldHeaders.Where(x => x.FarmYieldHeaderID == id).SingleOrDefault();
        }
        public FarmYieldHeader GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(2);
            return DbContext.FarmYieldHeaders.Where(x => x.YieldDate > reportDate && x.YieldDate < endDate).FirstOrDefault();
        }

    }


}
