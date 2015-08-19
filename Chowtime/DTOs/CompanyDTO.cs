using SGApp.Models.Common;

namespace SGApp.DTOs
{
    public class CompanyDTO : IKey
    {

        public string Key { get; set; }




        public string CompanyId
        {
            get;
            set;
        }


        public string CompanyName
        {
            get;
            set;
        }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }







    }
}