using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SGApp.Models.Common;

namespace SGApp.DTOs
{
    public class UtilityDTO : IKey
    {
        #region IKey Members

        public string Key
        {
            get;
            set;
        }

        #endregion
    }
}