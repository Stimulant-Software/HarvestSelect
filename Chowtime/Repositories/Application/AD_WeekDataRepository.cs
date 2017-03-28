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
    public class AD_WeekDataRepository : RepositoryBase<AD_WeekData>
    {
        public override System.Linq.IQueryable<AD_WeekData> EntityCollection
        {
            get
            {
                return DbContext.AD_WeekData.AsQueryable();
            }
        }

        protected override AD_WeekData DeleteRecord(AD_WeekData entity)
        {
            throw new System.NotImplementedException();
        }

        protected override AD_WeekData InsertRecord(AD_WeekData entity)
        {
            DbContext.AD_WeekData.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override AD_WeekData UpdateRecord(AD_WeekData entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<AD_WeekData> GetByPredicate(string predicate)
        {
            var iq = DbContext.AD_WeekData.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<AD_WeekData> GetAD_WeekDatas()
        {

            return DbContext.AD_WeekData
            .OrderBy(x => x.AD_WeekEnd).ToList();
        }


        public override AD_WeekData GetById(int id)
        {
            return DbContext.AD_WeekData.Where(x => x.AD_WeekDataID == id).SingleOrDefault();
        }

        public List<AD_WeekData> GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(1);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.AD_WeekData.Include("AD_Products").Where(x => x.AD_WeekEnd > reportDate && x.AD_WeekEnd < endDate).ToList();
        }

        public List<AD_WeekData> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            
            startDate = startDate.AddSeconds(-1);
            return DbContext.AD_WeekData.Where(x => x.AD_WeekEnd > startDate && x.AD_WeekEnd < endDate.AddDays(1)).GroupBy(x => x.AD_WeekEnd).Select(x => x.First()).ToList();
        }
        public List<AD_WeekData> GetByWeek(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(6);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.AD_WeekData.Where(x => x.AD_WeekEnd > reportDate && x.AD_WeekEnd < endDate).ToList();
        }

        public List<int> GetAllProducts()
        {

            return DbContext.AD_Products.Select( x => x.AD_ProductID).ToList();
        }


    }


}
