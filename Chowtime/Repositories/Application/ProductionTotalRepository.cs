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
    public class ProductionTotalRepository : RepositoryBase<ProductionTotal>
    {
        public override System.Linq.IQueryable<ProductionTotal> EntityCollection
        {
            get
            {
                return DbContext.ProductionTotals.AsQueryable();
            }
        }

        protected override ProductionTotal DeleteRecord(ProductionTotal entity)
        {
            DbContext.ProductionTotals.Remove(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override ProductionTotal InsertRecord(ProductionTotal entity)
        {
            DbContext.ProductionTotals.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override ProductionTotal UpdateRecord(ProductionTotal entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<ProductionTotal> GetByPredicate(string predicate)
        {
            var iq = DbContext.ProductionTotals.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<ProductionTotal> GetProductionTotals()
        {

            return DbContext.ProductionTotals
            .OrderBy(x => x.ProductionDate).ToList();
        }


        public override ProductionTotal GetById(int id)
        {
            return DbContext.ProductionTotals.Where(x => x.ProductionTotalID == id).SingleOrDefault();
        }


        public List<ProductionTotal> GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(2);
            return DbContext.ProductionTotals.Where(x => x.ProductionDate > reportDate && x.ProductionDate < endDate).ToList();
        }

        public List<ProductionTotal> GetByDateRange(DateTime reportDate1, DateTime reportDate2)
        {
            DateTime endDate = reportDate2.AddDays(1);
            return DbContext.ProductionTotals.Where(x => x.ProductionDate > reportDate1 && x.ProductionDate < endDate).ToList();
        }

        public ProductionTotal GetByDateAndPond(DateTime reportDate1, int pondid)
        {
            DateTime endDate = reportDate1.AddDays(1);
            reportDate1 = reportDate1.AddSeconds(-1);
            return DbContext.ProductionTotals.Where(x => x.ProductionDate > reportDate1 && x.ProductionDate < endDate && x.PondId == pondid).FirstOrDefault();
        }
    }


}
