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
    public class StatusRepository : RepositoryBase<Status>
    {
        public override System.Linq.IQueryable<Status> EntityCollection
        {
            get
            {
                return DbContext.Statuses.AsQueryable();
            }
        }

        protected override Status DeleteRecord(Status entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Status InsertRecord(Status entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Status UpdateRecord(Status entity)
        {
            throw new System.NotImplementedException();
        }


        public override List<Status> GetByPredicate(string predicate)
        {
            var iq = DbContext.Statuses.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).Take(50).ToList() : iq.Take(50).ToList();
        }

        public List<Status> GetStatuses(int companyId)
        {

            return DbContext.Statuses
            .OrderBy(x => x.StatusName).ToList();
        }

        internal int GetActiveStatusID(string statusName)
        {
            return DbContext.Statuses.Where(x => x.StatusName == statusName).Single().StatusId;
        }

        public override Status GetById(int id)
        {
            throw new System.NotImplementedException();
        }
    }


}
