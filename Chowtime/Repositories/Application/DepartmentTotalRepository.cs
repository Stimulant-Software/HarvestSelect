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
    public class DepartmentTotalRepository : RepositoryBase<DepartmentTotal>
    {
        public override System.Linq.IQueryable<DepartmentTotal> EntityCollection
        {
            get
            {
                return DbContext.DepartmentTotals.AsQueryable();
            }
        }

        protected override DepartmentTotal DeleteRecord(DepartmentTotal entity)
        {
            throw new System.NotImplementedException();
        }

        protected override DepartmentTotal InsertRecord(DepartmentTotal entity)
        {
            DbContext.DepartmentTotals.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override DepartmentTotal UpdateRecord(DepartmentTotal entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<DepartmentTotal> GetByPredicate(string predicate)
        {
            var iq = DbContext.DepartmentTotals.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }
        public void SaveRepoChanges()
        {
            DbContext.SaveChanges();
        }
        public List<DepartmentTotal> GetDepartmentTotals()
        {

            return DbContext.DepartmentTotals
            .OrderBy(x => x.DTDate).ToList();
        }


        public override DepartmentTotal GetById(int id)
        {
            return DbContext.DepartmentTotals.Where(x => x.DepartmentTotalID == id).SingleOrDefault();
        }

        public List<DepartmentTotal> GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(1);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.DepartmentTotals.Where(x => x.DTDate > reportDate && x.DTDate < endDate).ToList();
        }
        public List<DepartmentTotal> GetByWeek(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(6);
            reportDate = reportDate.AddSeconds(-1);
            return DbContext.DepartmentTotals.Where(x => x.DTDate > reportDate && x.DTDate < endDate).ToList();
        }

        public DepartmentTotal GetByDateAndDepartment(DateTime reportDate1, int depid)
        {
            DateTime endDate = reportDate1.AddDays(1);
            reportDate1 = reportDate1.AddSeconds(-1);
            return DbContext.DepartmentTotals.Where(x => x.DTDate > reportDate1 && x.DTDate < endDate && x.DepartmentID == depid).FirstOrDefault();
        }
    }


}
