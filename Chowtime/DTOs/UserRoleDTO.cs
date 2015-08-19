using SGApp.Models.Common;

namespace SGApp.DTOs
{
    public class UserRoleDTO : IKey {
        #region IKey Members

        public string Key {
            get;
            set;
        }

        #endregion
        public string RoleID {
            get;
            set;
        }
        public string RoleDescription {
            get;
            set;
        }
        public string UserID {
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