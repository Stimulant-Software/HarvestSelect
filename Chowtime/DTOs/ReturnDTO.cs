using System.Collections.Generic;
using SGApp.Models.Common;

namespace SGApp.DTOs
{
    public class ReturnDTO : IKey {

        #region IKey Members

        public string Key {
            get;
            set;
        }

        #endregion

        public virtual ICollection<IKey> ReturnData{get; set;}
    }
}