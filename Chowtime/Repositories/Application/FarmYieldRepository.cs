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
    public class FarmYieldRepository : RepositoryBase<FarmYield>
    {
        public override System.Linq.IQueryable<FarmYield> EntityCollection
        {
            get
            {
                return DbContext.FarmYields.AsQueryable();
            }
        }

        protected override FarmYield DeleteRecord(FarmYield entity)
        {
            DbContext.FarmYields.Remove(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override FarmYield InsertRecord(FarmYield entity)
        {
            DbContext.FarmYields.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override FarmYield UpdateRecord(FarmYield entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<FarmYield> GetByPredicate(string predicate)
        {
            var iq = DbContext.FarmYields.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<FarmYield> GetFarmYields()
        {

            return DbContext.FarmYields
            .OrderBy(x => x.YieldDate).ToList();
        }


        public override FarmYield GetById(int id)
        {
            return DbContext.FarmYields.Where(x => x.YieldID == id).SingleOrDefault();
        }

        public decimal? GetPondWeightByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(2);
            return DbContext.FarmYields.Where(x => x.YieldDate > reportDate && x.YieldDate < endDate).Sum(x => x.PoundsHeaded);
        }

        public List<FarmYield> GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(2);
            return DbContext.FarmYields.Where(x => x.YieldDate > reportDate && x.YieldDate < endDate).ToList();
        }
    }


}
