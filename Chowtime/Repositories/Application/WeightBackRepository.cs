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
    public class WeighBackRepository : RepositoryBase<WeighBack>
    {
        public override System.Linq.IQueryable<WeighBack> EntityCollection
        {
            get
            {
                return DbContext.WeighBacks.AsQueryable();
            }
        }

        protected override WeighBack DeleteRecord(WeighBack entity)
        {
            DbContext.WeighBacks.Remove(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override WeighBack InsertRecord(WeighBack entity)
        {
            DbContext.WeighBacks.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override WeighBack UpdateRecord(WeighBack entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<WeighBack> GetByPredicate(string predicate)
        {
            var iq = DbContext.WeighBacks.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<WeighBack> GetWeighBacks()
        {

            return DbContext.WeighBacks
            .OrderBy(x => x.WBDateTime).ToList();
        }


        public override WeighBack GetById(int id)
        {
            return DbContext.WeighBacks.Where(x => x.WeightBackID == id).SingleOrDefault();
        }


        public List<WeighBack> GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(2);
            return DbContext.WeighBacks.Where(x => x.WBDateTime > reportDate && x.WBDateTime < endDate).ToList();
        }
    }


}
