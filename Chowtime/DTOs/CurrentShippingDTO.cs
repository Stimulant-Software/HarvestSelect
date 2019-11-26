using System.Collections.Generic;
using SGApp.Models.Common;
using System;

namespace SGApp.DTOs
{
    public class ShippingReportDTO : IKey
    {
        public string Key { get; set; }
        public List<CurrentShippingDTO> ShippingTotals { get; set; }
    }

    public class CurrentShippingDTO
    {
        public int AppID { get; set; }
        public string CustomerName { get; set; }
        public string ItemDescription { get; set; }
        public string ItemCode { get; set; }
        public Nullable<decimal> OrderAmount { get; set; }
        public Nullable<decimal> QuantityOnHand { get; set; }
        public string OrderDate { get; set; }
        public Nullable<decimal> ShippedAmount { get; set; }
        public Nullable<decimal> TodayUnits { get; set; }
    }
}