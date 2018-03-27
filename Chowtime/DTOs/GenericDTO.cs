using System.Collections.Generic;
using SGApp.Models.Common;
using System;
namespace SGApp.DTOs
{
    public class GenericDTO : IKey {
        #region IKey Members

        public string Key {
            get;
            set;
        }

        #endregion

        public virtual ICollection<Dictionary<string, string>> ReturnData { get; set; }
        public string startDate {  get; set; }
        public string endDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string OrderCode { get; set; }
        public virtual ICollection<Dictionary<string, string>> ReturnData1 { get; set; }
        public string CustomerNumber { get; set; }
        public string OrderNumber { get; set; }
		public virtual ICollection<Dictionary<string, string>> Bins { get; set; }
    }
}