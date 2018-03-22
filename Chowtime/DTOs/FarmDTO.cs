using SGApp.Models.Common;

namespace SGApp.DTOs
{
    public class FarmDTO : IKey
    {

        public string Key { get; set; }




        public string CompanyId
        {
            get;
            set;
        }


        public string FarmName
        {
            get;
            set;
        }
        public string FarmId
        {
            get;
            set;
        }



        public string StatusId
        {
            get;
            set;
        }

        public string CurrentTime
        {
            get;
            set;
        }

        public string InnovaName
        {
            get;
            set;
        }

        public string InnovaCode
        {
            get;
            set;
        }







    }
}