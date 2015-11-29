using System.Collections.Generic;
using SGApp.Models.Common;

namespace SGApp.DTOs
{
    public class DailyReportDTO : IKey
    {
        #region IKey Members

        public string Key
        {
            get;
            set;
        }

        #endregion

        public virtual ICollection<Dictionary<string, string>> Header
        {
            get;
            set;
        }
        public virtual ICollection<Dictionary<string, string>> Employees
        {
            get;
            set;
        }
        public virtual ICollection<Dictionary<string, string>> Freezing
        {
            get;
            set;
        }
        public virtual ICollection<Dictionary<string, string>> Samplings
        {
            get;
            set;
        }
        public virtual ICollection<Dictionary<string, string>> Finish
        {
            get;
            set;
        }
        public virtual ICollection<Dictionary<string, string>> Ponds
        {
            get;
            set;
        }
        public string ReportDate { get; set; }

    }
}