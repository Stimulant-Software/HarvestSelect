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
    public class DownTimeRepository : RepositoryBase<DownTime>
    {
        public override System.Linq.IQueryable<DownTime> EntityCollection
        {
            get
            {
                return DbContext.DownTimes.AsQueryable();
            }
        }

        protected override DownTime DeleteRecord(DownTime entity)
        {
            throw new System.NotImplementedException();
        }

        protected override DownTime InsertRecord(DownTime entity)
        {
            DbContext.DownTimes.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override DownTime UpdateRecord(DownTime entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<DownTime> GetByPredicate(string predicate)
        {
            var iq = DbContext.DownTimes.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<DownTime> GetDownTimes()
        {

            return DbContext.DownTimes
            .OrderBy(x => x.DownTimeDate).ToList();
        }


        public override DownTime GetById(int id)
        {
            return DbContext.DownTimes.Where(x => x.DownTimeID == id).SingleOrDefault();
        }

        public List<DownTime> GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(1);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.DownTimes.Include("DownTimeType.Department").Where(x => x.DownTimeDate > reportDate && x.DownTimeDate < endDate).ToList();
        }
        public List<DownTime> GetByWeek(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(6);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.DownTimes.Where(x => x.DownTimeDate > reportDate && x.DownTimeDate < endDate).ToList();
        }

        public List<DownTime> GetByDateAndDepartment(DateTime reportDate1, int depid)
        {
            DateTime endDate = reportDate1.AddDays(1);
            reportDate1 = reportDate1.AddSeconds(-1);
            return DbContext.DownTimes.Where(x => x.DownTimeDate > reportDate1 && x.DownTimeDate < endDate && x.DownTimeType.DepartmentID == depid).ToList();
        }
    }


}
