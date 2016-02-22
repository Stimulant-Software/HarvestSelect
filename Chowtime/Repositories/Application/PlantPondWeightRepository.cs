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
    public class PlantPondWeightRepository : RepositoryBase<PlantPondWeight>
    {
        public override System.Linq.IQueryable<PlantPondWeight> EntityCollection
        {
            get
            {
                return DbContext.PlantPondWeights.AsQueryable();
            }
        }

        protected override PlantPondWeight DeleteRecord(PlantPondWeight entity)
        {
            DbContext.PlantPondWeights.Remove(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override PlantPondWeight InsertRecord(PlantPondWeight entity)
        {
            DbContext.PlantPondWeights.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override PlantPondWeight UpdateRecord(PlantPondWeight entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<PlantPondWeight> GetByPredicate(string predicate)
        {
            var iq = DbContext.PlantPondWeights.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<PlantPondWeight> GetPlantPondWeights()
        {

            return DbContext.PlantPondWeights
            .OrderBy(x => x.PPWDateTime).ToList();
        }


        public override PlantPondWeight GetById(int id)
        {
            return DbContext.PlantPondWeights.Where(x => x.PlantPondWeightID == id).SingleOrDefault();
        }


        public List<PlantPondWeight> GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(1);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.PlantPondWeights.Where(x => x.PPWDateTime > reportDate && x.PPWDateTime < endDate).ToList();
        }

        public List<PlantPondWeight> GetByDateRange(DateTime reportDate1, DateTime reportDate2)
        {
            DateTime endDate = reportDate2.AddDays(1);
            return DbContext.PlantPondWeights.Where(x => x.PPWDateTime > reportDate1 && x.PPWDateTime < endDate).ToList();
        }
    }


}
