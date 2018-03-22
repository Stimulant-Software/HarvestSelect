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
    public class BinRepository : RepositoryBase<Bin>
    {
        public override System.Linq.IQueryable<Bin> EntityCollection
        {
            get
            {
                return DbContext.Bins.AsQueryable();
            }
        }

        protected override Bin DeleteRecord(Bin entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Bin InsertRecord(Bin entity)
        {
            DbContext.Bins.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override Bin UpdateRecord(Bin entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<Bin> GetByPredicate(string predicate)
        {
            var iq = DbContext.Bins.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<Bin> GetBins()
        {

            return DbContext.Bins
            .OrderBy(x => x.BinName).ToList();
        }


        public override Bin GetById(int id)
        {
            return DbContext.Bins.Where(x => x.BinID == id).SingleOrDefault();
        }


    }


}
