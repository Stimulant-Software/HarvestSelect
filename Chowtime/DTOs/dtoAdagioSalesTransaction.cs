using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGApp.DTOs
{
    public class dtoAdagioSalesTransaction
    {
        public string Cust { get; set; }
        public string Shipto { get; set; }
        public string JulianDateKey { get; set; }
        public string HexUniquifer { get; set; }
        public string TransactionUnquifier { get; set; }
        public string Item { get; set; }
        public string ItemSegment1CATEGORY { get; set; }
        public string ItemSegment2ITEM { get; set; }
        public string ItemSegment3CTNWGT { get; set; }
        public string QIItem { get; set; }
        public string PrefixDocumentNumber { get; set; }
        public string Prefix { get; set; }
        public string DocumentNumber { get; set; }
        public string Uniquifier { get; set; }
        public string Type_T { get; set; }
        public string Date_2 { get; set; }
        public string Salesperson { get; set; }
        public string Qty { get; set; }
        public string Amt { get; set; }
        public string Cost { get; set; }
        public string BasePrice { get; set; }
        public string CustomerTaxStatus { get; set; }
        public string ItemTaxStatus { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Order_2 { get; set; }
        public string TaxGroup { get; set; }
        public string RecordSource_T { get; set; }
        public string ItemSource_T { get; set; }
        public string PriceList { get; set; }
        public string DiscountDate { get; set; }
        public string DueDate { get; set; }
        public string DatePaid { get; set; }
        public string InvoicePaid_T { get; set; }
        public string Reference { get; set; }
        public string PONumber { get; set; }
        public string OrginalInvoice { get; set; }
        public string Line { get; set; }
        public string Territory { get; set; }
        public string Itemreportgroup { get; set; }
        public string Customerreportgroup { get; set; }
        public string ManualStyleCode { get; set; }
        public string AutomaticStyleCode { get; set; }
        public string SourceDecimals { get; set; }
        public string HomeCurr { get; set; }
        public string RateType { get; set; }
        public string SourceCurr { get; set; }
        public string RateDate { get; set; }
        public string Rate { get; set; }
        public string RateRep { get; set; }
        public string DateMatching { get; set; }
        public string SourceAmt { get; set; }
        public string SourceCost { get; set; }
        public string SourceBasePrice { get; set; }
        public string LineItemDescription { get; set; }
        public string RNumAS90ATRN { get; set; }
    }
}