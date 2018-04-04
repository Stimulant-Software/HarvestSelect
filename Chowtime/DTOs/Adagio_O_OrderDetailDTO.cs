﻿﻿using SGApp.Models.Common;
using System;
namespace SGApp.DTOs
{
    public class Adagio_O_OrderDetailDTO
    {

       
        public string OrderKey { get; set; }
        public string Line { get; set; }
        public string DDocType_T { get; set; }
        public string DDoc { get; set; }
        public string LineType_T { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string PickingSeq { get; set; }
        public string DiscLevel { get; set; }
        public string DCategory { get; set; }
        public string PriceUnit { get; set; }
        public string PriceOverride { get; set; }
        public string ExtensionOverride { get; set; }
        public string ReturntoInventory { get; set; }
        public string SerialCount { get; set; }
        public string TaxStatus { get; set; }
        public string Commissionable { get; set; }
        public string UnitFactor { get; set; }
        public string UnitWeight { get; set; }
        public string QtyOriginalOrdered { get; set; }
        public string QtyOrdered { get; set; }
        public string QtyShippedtoDate { get; set; }
        public string QtyShipped { get; set; }
        public string QtyBackordered { get; set; }
        public string ExtWeight { get; set; }
        public string ShipStockingUnits { get; set; }
        public string Loc { get; set; }
        public string Complete { get; set; }
        public string Curr { get; set; }
        public string Decimals { get; set; }
        public string PriceList { get; set; }
        public string OrderedInPO { get; set; }
        public string UnitDecimals { get; set; }
        public string UnitPrice { get; set; }
        public string UnitCost { get; set; }
        public string ExtPrice { get; set; }
        public string ExtCost { get; set; }
        public string TaxAmt { get; set; }
        public string DTax1 { get; set; }
        public string DTax2 { get; set; }
        public string DTax3 { get; set; }
        public string DTax4 { get; set; }
        public string DTax5 { get; set; }
        public string DBase1 { get; set; }
        public string DBase2 { get; set; }
        public string DBase3 { get; set; }
        public string DBase4 { get; set; }
        public string DBase5 { get; set; }
        public string DiscAmt { get; set; }
        public string DiscExtension { get; set; }
        public string BasePrice { get; set; }
        public string ExtOrderPrice { get; set; }
        public string PriceFactor { get; set; }
        public string Serial1 { get; set; }
        public string Serial2 { get; set; }
        public string Serial3 { get; set; }
        public string Serial4 { get; set; }
        public string Serial5 { get; set; }
        public string DExpectedShipDate { get; set; }
        public string MiscAmount { get; set; }
        public string MiscBasePrice { get; set; }
        public string MiscShortDescription { get; set; }
        public string MiscQuantity { get; set; }
        public string MiscFiller { get; set; }
        public string RNumAO80ALIN { get; set; }
    }
}