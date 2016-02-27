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
    public class DepartmentRepository : RepositoryBase<Department>
    {
        public override System.Linq.IQueryable<Department> EntityCollection
        {
            get
            {
                return DbContext.Departments.AsQueryable();
            }
        }

        protected override Department DeleteRecord(Department entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Department InsertRecord(Department entity)
        {
            DbContext.Departments.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override Department UpdateRecord(Department entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<Department> GetByPredicate(string predicate)
        {
            var iq = DbContext.Departments.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<Department> GetDepartments()
        {

            return DbContext.Departments.ToList();
        }


        public override Department GetById(int id)
        {
            return DbContext.Departments.Where(x => x.DepartmentID == id).SingleOrDefault();
        }


    }


}
