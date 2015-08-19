using SGApp.Models.Common;

namespace SGApp.DTOs
{
    public class LoginDTO : IKey {
        public string Key {
            get;
            set;
        }
        public string UserName {
            get;
            set;
        }

        public string Password {
            get;
            set;
        }

        public string UserId
        {
            get;
            set;
        }
    }
}