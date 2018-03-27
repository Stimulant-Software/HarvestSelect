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
using SGApp.Models.Common;
using System.Web;
using System.Data.Objects;

namespace SGApp.Repository.Application
{
    public class FarmRepository : RepositoryBase<Farm>
    {
        public override System.Linq.IQueryable<Farm> EntityCollection
        {
            get
            {
                return DbContext.Farms.AsQueryable();
            }
        }

        protected override Farm DeleteRecord(Farm entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Farm InsertRecord(Farm entity)
        {
            DbContext.Farms.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

		internal List<UserFarm> GetUserFarmsWithBins(int userId, int statusId) {
			return DbContext.UserFarms.Include("Farm.Bins").Where(x => x.UserId == userId && x.StatusId == statusId && x.Farm.StatusId == statusId).ToList();
		}

		protected override Farm UpdateRecord(Farm entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<Farm> GetByPredicate(string predicate)
        {
            var iq = DbContext.Farms.Include("Bins").AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<Farm> GetFarms()
        {

            return DbContext.Farms
            .OrderBy(x => x.FarmName).ToList();
        }

        public List<Farm> GetUserFarms(int userId)
        {
            var uf = new UserFarmRepository();
            return uf.EntityCollection.Include("Farm").Where(x => x.UserId == userId && x.Farm.StatusId == 1).Select(x => x.Farm).ToList();
        }

        

        public override Farm GetById(int id)
        {
            return DbContext.Farms.Where(x => x.FarmId == id).SingleOrDefault();
        }
        public class FeedingTotal
        {
            private int _totalFeed;
            public int TotalFeed
            {
                get { return _totalFeed; }
                set
                {
                    _totalFeed = value;
                }
            }

            private DateTime? _feedDate;
            public DateTime? FeedDate
            {
                get { return _feedDate; }
                set
                {
                    _feedDate = value;
                }
            }
            private decimal _totalFeedPerAcre;
            public decimal TotalFeedPerAcre
            {
                get { return _totalFeedPerAcre; }
                set
                {
                    _totalFeedPerAcre = value;
                }
            }

        }
        public IList<FeedingTotal> GetFarmTotals(int farmID)
        {
            IList<FeedingTotal> feedings = new List<FeedingTotal>();
            feedings = DbContext.Feedings.Include("Ponds").Where(x => x.Pond.Farm.FarmId == farmID).GroupBy(fd => EntityFunctions.TruncateTime(fd.FeedDate))
                .Select(t => new FeedingTotal { TotalFeed = t.Sum(f => f.PoundsFed), TotalFeedPerAcre = t.Sum(f => f.PoundsFed) / t.Sum(f => f.Pond.Size), FeedDate = t.Key.Value })
                .OrderByDescending(x => x.FeedDate)
                .Take(7)
                .OrderBy(x => x.FeedDate)
                .ToList();
            return feedings;

        }
    }


}
