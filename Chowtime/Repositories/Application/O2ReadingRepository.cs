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
    public class O2ReadingRepository : RepositoryBase<O2Reading>
    {
        public override System.Linq.IQueryable<O2Reading> EntityCollection
        {
            get
            {
                return DbContext.O2Readings.AsQueryable();
            }
        }

        protected override O2Reading DeleteRecord(O2Reading entity)
        {
            throw new System.NotImplementedException();
        }

        protected override O2Reading InsertRecord(O2Reading entity)
        {
            DbContext.O2Readings.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override O2Reading UpdateRecord(O2Reading entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<O2Reading> GetByPredicate(string predicate)
        {
            var iq = DbContext.O2Readings.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<O2Reading> GetO2Readings(int companyId)
        {

            return DbContext.O2Readings
            .OrderBy(x => x.PondId).ToList();
        }

        public List<O2Reading> GetPondO2ReadingsByDate(int pondid, DateTime readingdate)
        {
            DateTime startdate;
            startdate = readingdate;
            
            string datepart = startdate.ToShortDateString();
            DateTime begindate = DateTime.Parse(datepart);
            return DbContext.O2Readings.Where(x => x.PondId == pondid && x.DayPeriod == begindate)
            .OrderBy(x => x.PondId).ToList();
        }
        public List<O2Reading> GetPondO2Readings7Days(int pondid, DateTime readingdate)
        {
            DateTime startdate;
            startdate = readingdate;

            string datepart = startdate.ToShortDateString();
            DateTime enddate = DateTime.Parse(datepart);
            DateTime begindate = DateTime.Parse(datepart).AddDays(-8); 
            return DbContext.O2Readings.Where(x => x.PondId == pondid && x.DayPeriod > begindate && x.DayPeriod < enddate)
            .OrderBy(x => x.ReadingDate).ToList();
        }
        public O2Reading GetLastPondReadingByPond(int pondid)
        {


            return DbContext.O2Readings.Where(x => x.PondId == pondid).OrderByDescending(x => x.ReadingId).First();
            
        }
        public List<O2Reading> GetLast2PondReadingsByPond(int pondid)
        {


            return DbContext.O2Readings.Where(x => x.PondId == pondid).OrderByDescending(x => x.ReadingId).Take(2).ToList();

        }
        public List<O2Reading> GetFarmO2ReadingsByDate(int pondid, DateTime readingdate)
        {
            DateTime startdate;
            DateTime enddate;
            if (readingdate.Hour < 12)
            {
                startdate = DateTime.Parse(readingdate.AddDays(-1).Year.ToString() + "-" + readingdate.AddDays(-1).Month.ToString() + "-" + readingdate.AddDays(-1).Day.ToString() + " 12:00:00 PM");
            }
            else
            {
                startdate = DateTime.Parse(readingdate.Year.ToString() + "-" + readingdate.Month.ToString() + "-" + readingdate.Day.ToString() + " 12:00:00 PM");
            }
            enddate = startdate.AddHours(24);
            return DbContext.O2Readings.Where(x => x.PondId == pondid && x.ReadingDate > startdate && x.ReadingDate < enddate)
            .OrderBy(x => x.PondId).ToList();
        }



        public override O2Reading GetById(int id)
        {
            return DbContext.O2Readings.Where(x => x.ReadingId == id).SingleOrDefault();
        }
    }


}
