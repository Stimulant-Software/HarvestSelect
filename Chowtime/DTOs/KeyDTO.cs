using System.Collections.Generic;
using SGApp.Models.Common;

namespace SGApp.DTOs
{
    public class KeyDTO {
        public string Key {
            get;
            set;
        }

        public ICollection<Dictionary<string, string>> UserRoles {
            get;
            set;
        }

        public string UserID { get; set; }

        public string CompanyId { get; set; }
    }
}