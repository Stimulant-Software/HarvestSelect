
using System;
namespace InnovaServiceHost.DTOs
{
    public class Adagio_O_OrderDetailDTO
    {

        
        public string OrderKey { get; set; }
        public Nullable<short> Line { get; set; }
        public string DDocType_T { get; set; }
        public Nullable<int> DDoc { get; set; }
        public string LineType_T { get; set; }
        public Nullable<int> Item { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string PickingSeq { get; set; }
        public string DiscLevel { get; set; }
        public Nullable<short> DCategory { get; set; }
        public string PriceUnit { get; set; }
        public Nullable<short> PriceOverride { get; set; }
        public Nullable<short> ExtensionOverride { get; set; }
        public Nullable<short> ReturntoInventory { get; set; }
        public Nullable<short> SerialCount { get; set; }
        public Nullable<short> TaxStatus { get; set; }
        public Nullable<short> Commissionable { get; set; }
        public Nullable<short> UnitFactor { get; set; }
        public Nullable<short> UnitWeight { get; set; }
        public Nullable<short> QtyOriginalOrdered { get; set; }
        public Nullable<short> QtyOrdered { get; set; }
        public Nullable<short> QtyShippedtoDate { get; set; }
        public Nullable<short> QtyShipped { get; set; }
        public Nullable<short> QtyBackordered { get; set; }
        public Nullable<short> ExtWeight { get; set; }
        public Nullable<short> ShipStockingUnits { get; set; }
        public Nullable<short> Loc { get; set; }
        public Nullable<short> Complete { get; set; }
        public string Curr { get; set; }
        public string Decimals { get; set; }
        public Nullable<short> PriceList { get; set; }
        public Nullable<short> OrderedInPO { get; set; }
        public Nullable<short> UnitDecimals { get; set; }
        public Nullable<int> UnitPrice { get; set; }
        public Nullable<float> UnitCost { get; set; }
        public Nullable<float> ExtPrice { get; set; }
        public Nullable<float> ExtCost { get; set; }
        public Nullable<float> TaxAmt { get; set; }
        public Nullable<float> DTax1 { get; set; }
        public Nullable<float> DTax2 { get; set; }
        public Nullable<float> DTax3 { get; set; }
        public Nullable<short> DTax4 { get; set; }
        public Nullable<short> DTax5 { get; set; }
        public Nullable<float> DBase1 { get; set; }
        public Nullable<float> DBase2 { get; set; }
        public Nullable<float> DBase3 { get; set; }
        public Nullable<short> DBase4 { get; set; }
        public Nullable<short> DBase5 { get; set; }
        public Nullable<float> DiscAmt { get; set; }
        public Nullable<float> DiscExtension { get; set; }
        public Nullable<int> BasePrice { get; set; }
        public Nullable<float> ExtOrderPrice { get; set; }
        public Nullable<short> PriceFactor { get; set; }
        public string Serial1 { get; set; }
        public string Serial2 { get; set; }
        public Nullable<int> Serial3 { get; set; }
        public string Serial4 { get; set; }
        public string Serial5 { get; set; }
        public Nullable<System.DateTime> DExpectedShipDate { get; set; }
        public Nullable<float> MiscAmount { get; set; }
        public Nullable<System.DateTime> MiscBasePrice { get; set; }
        public string MiscShortDescription { get; set; }
        public Nullable<short> MiscQuantity { get; set; }
        public string MiscFiller { get; set; }
        public string RNumAO80ALIN { get; set; }
    }
}