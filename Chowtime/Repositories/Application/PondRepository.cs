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
    public class PondRepository : RepositoryBase<Pond>
    {
        public override System.Linq.IQueryable<Pond> EntityCollection
        {
            get
            {
                return DbContext.Ponds.AsQueryable();
            }
        }

        protected override Pond DeleteRecord(Pond entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Pond InsertRecord(Pond entity)
        {
            DbContext.Ponds.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override Pond UpdateRecord(Pond entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<Pond> GetByPredicate(string predicate)
        {
            var iq = DbContext.Ponds.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).OrderBy(x => x.OldPondID).ToList() : iq.OrderBy(x => x.OldPondID).ToList();
        }

        public List<Pond> GetPonds(int farmId)
        {

            return DbContext.Ponds
            .OrderBy(x => x.PondName).ToList();
        }

        public Pond GetPondFromInnovaName(string n)
        {

            return DbContext.Ponds.Where(x => x.InnovaName == n).FirstOrDefault();
        }

        public IList<Pond> GetActivePonds(int farmID)
        {
            int activeStatusID = DbContext.Statuses.Where(x => x.StatusName == "Active").Single().StatusId;
            return DbContext.Ponds.Where(x => x.FarmId == farmID && x.StatusId == activeStatusID).ToList();
        }
        public IList<Pond> GetActivePondsForO2(int farmID)
        {
            int activeStatusID = DbContext.Statuses.Where(x => x.StatusName == "Active").Single().StatusId;
            return DbContext.Ponds.Where(x => x.FarmId == farmID && x.StatusId == activeStatusID).ToList();

            //return DbContext.Ponds.Where(x => x.FarmId == farmID).ToList();
        }

        public IList<Pond> GetInactivePonds(int farmID)
        {
            int activeStatusID = DbContext.Statuses.Where(x => x.StatusName == "InActive").Single().StatusId;
            return DbContext.Ponds.Where(x => x.FarmId == farmID && x.StatusId == activeStatusID).ToList();
        }

        public override Pond GetById(int id)
        {
            return DbContext.Ponds.Where(x => x.PondId == id).SingleOrDefault();
        }

        public IList<Feeding> GetLastFeedings(int pondID, int count)
        {
            Pond p = DbContext.Ponds.Include("Feedings").Where(x => x.PondId == pondID).Single();
            if (p.Feedings.Count() > 0)
            {
                if (p.Feedings.Count() > count)
                {
                    return p.Feedings.OrderByDescending(x => x.FeedDate).Take(count).ToList();
                }
                else
                {
                    return p.Feedings.OrderByDescending(x => x.FeedDate).ToList();
                }
            }
            else
            {
                return null;
            }
        }
    }


}
