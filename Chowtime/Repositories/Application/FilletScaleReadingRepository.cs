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
    public class FilletScaleReadingRepository : RepositoryBase<FilletScaleReading>
    {
        public override System.Linq.IQueryable<FilletScaleReading> EntityCollection
        {
            get
            {
                return DbContext.FilletScaleReadings.AsQueryable();
            }
        }

        protected override FilletScaleReading DeleteRecord(FilletScaleReading entity)
        {
            throw new System.NotImplementedException();
        }

        protected override FilletScaleReading InsertRecord(FilletScaleReading entity)
        {
            DbContext.FilletScaleReadings.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override FilletScaleReading UpdateRecord(FilletScaleReading entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<FilletScaleReading> GetByPredicate(string predicate)
        {
            var iq = DbContext.FilletScaleReadings.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<FilletScaleReading> GetFilletScaleReadings()
        {

            return DbContext.FilletScaleReadings
            .OrderBy(x => x.FSRDateTime).ToList();
        }


        public override FilletScaleReading GetById(int id)
        {
            return DbContext.FilletScaleReadings.Where(x => x.FilletScaleReadingID == id).SingleOrDefault();
        }

        public FilletScaleReading GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(1);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.FilletScaleReadings.Where(x => x.FSRDateTime > reportDate && x.FSRDateTime < endDate).FirstOrDefault();
        }
        public decimal GetByWeek(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(6);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.FilletScaleReadings.Where(x => x.FSRDateTime > reportDate && x.FSRDateTime < endDate).Sum(x => x.FilletScaleReading1);
        }

    }


}
