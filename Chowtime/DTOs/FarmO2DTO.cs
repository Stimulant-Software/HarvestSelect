using System.Collections.Generic;
using SGApp.Models.Common;
using System.Collections.ObjectModel;

namespace SGApp.DTOs
{
    public class FarmO2DTO : IKey
    {
        #region IKey Members

        public string Key
        {
            get;
            set;
        }

        #endregion

        public virtual Dictionary<string, Dictionary<string, Collection<Dictionary<string, string>>>> ReturnData
        {
            get;
            set;
        }
    }
}