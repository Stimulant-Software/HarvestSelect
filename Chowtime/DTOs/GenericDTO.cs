using System.Collections.Generic;
using SGApp.Models.Common;

namespace SGApp.DTOs
{
    public class GenericDTO : IKey {
        #region IKey Members

        public string Key {
            get;
            set;
        }

        #endregion

        public virtual ICollection<Dictionary<string, string>> ReturnData {
            get;
            set;
        }
    }
}