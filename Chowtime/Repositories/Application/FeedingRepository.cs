using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using SGApp.Models.EF;
using MoreLinq;


namespace SGApp.Repository.Application {
	public class FeedingRepository : RepositoryBase<Feeding>
    {
        public override System.Linq.IQueryable<Feeding> EntityCollection {
            get
            {
                return DbContext.Feedings.AsQueryable();
            }
        }

        protected override Feeding DeleteRecord(Feeding entity)
        {
            throw new System.NotImplementedException();
        }

        protected override Feeding InsertRecord(Feeding entity)
        {
            DbContext.Feedings.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override Feeding UpdateRecord(Feeding entity)
        {
            DbContext.SaveChanges();
            return entity;
        }


        public override List<Feeding> GetByPredicate(string predicate)
        {
            var iq = DbContext.Feedings.AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
        }

        public List<Feeding> GetFeedings(int companyId)
        {

            return DbContext.Feedings
            .OrderBy(x => x.FeedDate).ToList();
        }



        public override Feeding GetById(int id)
        {
            return DbContext.Feedings.Where(x => x.FeedingId == id).SingleOrDefault();
        }

        public Feeding GetPondFeedingsByDate(int pondid, DateTime readingdate)
        {
            string datepart = readingdate.ToShortDateString();
            DateTime begindate = DateTime.Parse(datepart);
            DateTime enddate = begindate.AddDays(1);
            return DbContext.Feedings.Where(x => x.PondId == pondid && x.FeedDate >= begindate && x.FeedDate < enddate).FirstOrDefault();
        }

        public List <Feeding> GetFarmFeedingsByDate(int farmid, DateTime readingdate)
        {
            string datepart = readingdate.ToShortDateString();
            DateTime begindate = DateTime.Parse(datepart);
            DateTime enddate = begindate.AddDays(1);
            return DbContext.Feedings.Where(x => x.Pond.FarmId == farmid && x.FeedDate >= begindate && x.FeedDate < enddate).ToList();
        }
        public List<Feeding> GetFarmFeedingsLast7Dates(int farmid)
        {

            return DbContext.Feedings.Where(x => x.Pond.FarmId == farmid).DistinctBy(y => y.FeedDate.Date).OrderByDescending(x => x.FeedDate).Take(7).ToList();
           // return DbContext.Feedings.Where(x => x.Pond.FarmId == farmid).GroupBy(y => EntityFunctions.TruncateTime(y.FeedDate)).Select(grp => grp.FirstOrDefault()).OrderByDescending(x => x.FeedDate).Take(7).ToList();
        }
        public List<Feeding> GetPondLast7Feedings(int pondid)
        {
            
            return DbContext.Feedings.Where(x => x.PondId == pondid).OrderByDescending(x => x.FeedDate).Take(7).ToList();
        }
        public List<Feeding> GetFarmLast7Feedings(int farmid)
        {

            return DbContext.Feedings.Where(x => x.Pond.FarmId == farmid).OrderByDescending(x => x.FeedDate).Take(7).ToList();
        }
    }


}
