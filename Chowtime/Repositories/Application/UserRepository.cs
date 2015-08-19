using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using SGApp.Models.EF;
using SGApp.Repository;

namespace SGApp.Repository.Application {


    public class UserRepository : RepositoryBase<User> {


        public override System.Linq.IQueryable<User> EntityCollection {
            get {
                return DbContext.Users.AsQueryable();
            }
        }

        protected override User DeleteRecord(User entity) {
            throw new System.NotImplementedException();
        }

        protected override User InsertRecord(User entity) {
            DbContext.Users.Add(entity);
            DbContext.SaveChanges();
            return entity;
        }

        protected override User UpdateRecord(User entity) {
            DbContext.SaveChanges();
            return entity;
        }

        public override System.Collections.Generic.List<User> GetByPredicate(string predicate) {
            var iq = DbContext.Users.Include("UserFarms").Include("UserRoles").AsQueryable();
            return predicate.Length > 0 ? iq.Where(predicate, null).Take(50).ToList() : iq.Take(50).ToList();
        }

        public override User GetById(int id) {
            return DbContext.Users.Where(x => x.UserId == id).SingleOrDefault();
        }

        public User GetByEmail(string email)
        {
            return EntityCollection.SingleOrDefault(w => w.EmailAddress == email);
        }


        //public User GetUserByID(int userid) {
        //    return DbContext.Users.Include("Company").Where(x => x.UserID == userid).SingleOrDefault();
        //}


    }
}
