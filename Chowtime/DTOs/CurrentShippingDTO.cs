using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGApp.DTOs
{

    public class CurrentShippingDTO
    {
        public int AppID { get; set; }
        public string CustomerName { get; set; }
        public string ItemDescription { get; set; }
        public string ItemCode { get; set; }
        public Nullable<decimal> OrderAmount { get; set; }
        public Nullable<decimal> QuantityOnHand { get; set; }
        public string OrderDate { get; set; }
    }
}