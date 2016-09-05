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
    public class EmailRepository : RepositoryBase<Email>
    {
        public override System.Linq.IQueryable<Email> EntityCollection
        {
            get
            {
                return DbContext.Emails.AsQueryable();
            }
        }

        protected override Email DeleteRecord(Email entity)
        {
            DbContext.Emails.Remove(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override Email InsertRecord(Email entity)
        {
            DbContext.Emails.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override Email UpdateRecord(Email entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<Email> GetByPredicate(string predicate)
        {
            var iq = DbContext.Emails.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<Email> GetEmails()
        {

            return DbContext.Emails
            .OrderBy(x => x.EmailAddress).ToList();
        }


        public override Email GetById(int id)
        {
            return DbContext.Emails.Where(x => x.EmailID == id).SingleOrDefault();
        }

    }


}
