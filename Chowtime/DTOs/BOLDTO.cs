using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGApp.DTOs
{
    public class BOLs
    {
        public List<BOLs> bols { get; set; }

    }
    public class BOL
    {
        //public string code {get; set;}

        public string OrderCode { get; set; }
        public string CustNumber { get; set; }
        public string CustShort { get; set; }
        public string CustLong { get; set; }
        public string OrderDate { get; set; }
        public string DispDate { get; set; }
        public string PO1 { get; set; }
        public string PO2 { get; set; }
        public string PO3 { get; set; }
        public string Comments { get; set; }
        public string OrderTerms { get; set; }
        public string ShipToName { get; set; }
        public string ShipToAddress { get; set; }
        public string ShipToPhone { get; set; }
        public string MaterialID { get; set; }
        public string ProdCode { get; set; }
        public string ProdName { get; set; }
        public string OrderedAmt { get; set; }
        public string ShippedQty { get; set; }
        public string ShippedWeight { get; set; }
        public string ApproxUnitWeight { get; set; }
        public string HowPacked { get; set; }
        public string WeightLabel { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerAddress2 { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerZip { get; set; }
        public string CustomerPhone { get; set; }
        public string Terms { get; set; }
    }
}