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
    public class HarvestRepository : RepositoryBase<Harvest>
    {
        public override System.Linq.IQueryable<Harvest> EntityCollection
        {
            get
            {
                return DbContext.Harvests.AsQueryable();
            }
        }

        protected override Harvest DeleteRecord(Harvest entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Harvest InsertRecord(Harvest entity)
        {
            DbContext.Harvests.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override Harvest UpdateRecord(Harvest entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<Harvest> GetByPredicate(string predicate)
        {
            var iq = DbContext.Harvests.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).Take(50).ToList() : iq.Take(50).ToList();
        }

        public List<Harvest> GetHarvests(int companyId)
        {

            return DbContext.Harvests
            .OrderBy(x => x.HarvestDate).ToList();
        }



        public override Harvest GetById(int id)
        {
            throw new System.NotImplementedException();
        }
    }


}
