using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InnovaServiceHost.DTOs
{
    public class CurrentShipping
    {
        public string CustomerName { get; set; }
        public string ItemDescription { get; set; }
        public string ItemCode { get; set; }
        public Nullable<decimal> OrderAmount { get; set; }
        public Nullable<decimal> QuantityOnHand { get; set; }
        public Nullable<System.DateTime> OrderDate { get; set; }
    }
}