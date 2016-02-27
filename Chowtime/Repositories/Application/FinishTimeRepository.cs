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
    public class FinishTimeRepository : RepositoryBase<FinishTime>
    {
        public override System.Linq.IQueryable<FinishTime> EntityCollection
        {
            get
            {
                return DbContext.FinishTimes.AsQueryable();
            }
        }

        protected override FinishTime DeleteRecord(FinishTime entity)
        {
            throw new System.NotImplementedException();
        }

        protected override FinishTime InsertRecord(FinishTime entity)
        {
            DbContext.FinishTimes.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override FinishTime UpdateRecord(FinishTime entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<FinishTime> GetByPredicate(string predicate)
        {
            var iq = DbContext.FinishTimes.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<FinishTime> GetFinishTimes()
        {

            return DbContext.FinishTimes
            .OrderBy(x => x.FinishDateTime).ToList();
        }


        public override FinishTime GetById(int id)
        {
            return DbContext.FinishTimes.Where(x => x.FinishTimeID == id).SingleOrDefault();
        }

        public FinishTime GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(2);
            return DbContext.FinishTimes.Where(x => x.FinishDateTime > reportDate && x.FinishDateTime < endDate).FirstOrDefault();
        }

        public List<FinishTime> GetByDateAndDepartment(DateTime reportDate1, int depid)
        {
            DateTime endDate = reportDate1.AddDays(1);
            reportDate1 = reportDate1.AddSeconds(-1);
            return DbContext.FinishTimes.Where(x => x.FinishDateTime > reportDate1 && x.FinishDateTime < endDate && x.DepartmentID == depid).ToList();
        }
    }


}
