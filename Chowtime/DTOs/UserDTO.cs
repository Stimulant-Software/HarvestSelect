using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class UserDTO : IKey {

         public string Key { get; set; }


        public string UserId {
            get;
            set;
        }

        public string CompanyId {
            get;
            set;
        }


        public string FirstName {
            get;
            set;
        }
        public string LastName {
            get;
            set;
        }

        public string EmailAddress {
            get;
            set;
        }



        public string Phone {
            get;
            set;
        }

        public string StatusId
        {
            get;
            set;
        }


    




    }
}