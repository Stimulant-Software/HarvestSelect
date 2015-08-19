using SGApp.Models.Common;

namespace SGApp.DTOs
{
    public class UserFarmDTO : IKey
    {
        #region IKey Members

        public string Key
        {
            get;
            set;
        }

        #endregion
        public string FarmID
        {
            get;
            set;
        }
        public string FarmName
        {
            get;
            set;
        }
        public string UserID
        {
            get;
            set;
        }

        public string AddRemove
        {
            get;
            set;
        }


    }
}