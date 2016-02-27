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
    public class AbsenceRepository : RepositoryBase<Absence>
    {
        public override System.Linq.IQueryable<Absence> EntityCollection
        {
            get
            {
                return DbContext.Absences.AsQueryable();
            }
        }

        protected override Absence DeleteRecord(Absence entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Absence InsertRecord(Absence entity)
        {
            DbContext.Absences.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override Absence UpdateRecord(Absence entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<Absence> GetByPredicate(string predicate)
        {
            var iq = DbContext.Absences.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<Absence> GetAbsences()
        {

            return DbContext.Absences
            .OrderBy(x => x.AbsenceDate).ToList();
        }


        public override Absence GetById(int id)
        {
            return DbContext.Absences.Where(x => x.AbsenceID == id).SingleOrDefault();
        }

        public Absence GetByDate(DateTime reportDate)
        {
            DateTime endDate = reportDate.AddDays(2);
            return DbContext.Absences.Where(x => x.AbsenceDate > reportDate && x.AbsenceDate < endDate).FirstOrDefault();
        }

        public List<Absence> GetByDateAndDepartment(DateTime reportDate1, int depid)
        {
            DateTime endDate = reportDate1.AddDays(1);
            reportDate1 = reportDate1.AddSeconds(-1);
            return DbContext.Absences.Where(x => x.AbsenceDate > reportDate1 && x.AbsenceDate < endDate && x.DepartmentID == depid).ToList();
        }
    }


}
