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
    public class CompanyRepository : RepositoryBase<Company>
    {
        public override System.Linq.IQueryable<Company> EntityCollection
        {
            get
            {
                return DbContext.Companies.AsQueryable();
            }
        }

        protected override Company DeleteRecord(Company entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Company InsertRecord(Company entity)
        {
            DbContext.Companies.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override Company UpdateRecord(Company entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<Company> GetByPredicate(string predicate)
        {
            var iq = DbContext.Companies.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).Take(50).ToList() : iq.Take(50).ToList();
        }

        public List<Company> GetCompanys(int companyId)
        {

            return DbContext.Companies
            .OrderBy(x => x.CompanyName).ToList();
        }



        public override Company GetById(int id)
        {
            return DbContext.Companies.Where(x => x.CompanyId == id).SingleOrDefault();
        }
    }


}
