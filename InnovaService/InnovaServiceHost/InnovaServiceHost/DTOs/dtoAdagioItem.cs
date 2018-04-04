﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InnovaServiceHost.DTOs
{
    public class dtoAdagioItem
    {
        public string Key { get; set; }
        public Nullable<int> Item { get; set; }
        public Nullable<short> SetKey { get; set; }
        public Nullable<int> Item2 { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ReportGroup { get; set; }
        public Nullable<short> StockItem { get; set; }
        public Nullable<short> ItemsUsedinBOMCount { get; set; }
        public Nullable<short> SerialCount { get; set; }
        public Nullable<short> LayawayCounter { get; set; }
        public string Unit { get; set; }
        public string AltUnit1 { get; set; }
        public string AltUnit2 { get; set; }
        public string AltUnit3 { get; set; }
        public string AltUnit4 { get; set; }
        public string PurchaseUnit { get; set; }
        public string PriceUnit { get; set; }
        public Nullable<short> AltFactor1 { get; set; }
        public Nullable<short> AltFactor2 { get; set; }
        public Nullable<short> AltFactor3 { get; set; }
        public Nullable<short> AltFactor4 { get; set; }
        public Nullable<float> BasePrice { get; set; }
        public string PickingSeq { get; set; }
        public Nullable<short> StdCost { get; set; }
        public Nullable<short> UnitWeight { get; set; }
        public Nullable<System.DateTime> SaleStartDate { get; set; }
        public Nullable<System.DateTime> SaleEndDate { get; set; }
        public Nullable<float> SalePrice { get; set; }
        public Nullable<short> CtrlAcctSet { get; set; }
        public Nullable<short> TaxStatus { get; set; }
        public Nullable<short> UserDefinedCost1 { get; set; }
        public Nullable<short> UserDefinedCost2 { get; set; }
        public string DiscType_T { get; set; }
        public string DiscFormat_T { get; set; }
        public string DiscBase_T { get; set; }
        public Nullable<short> DiscMarkupQty1 { get; set; }
        public Nullable<short> DiscMarkupQty2 { get; set; }
        public Nullable<short> DiscMarkupQty3 { get; set; }
        public Nullable<short> DiscMarkupQty4 { get; set; }
        public Nullable<short> DiscMarkupQty5 { get; set; }
        public Nullable<short> QtyonPO { get; set; }
        public Nullable<short> QtyonSO { get; set; }
        public Nullable<short> QtyTempCommitted { get; set; }
        public Nullable<short> QtyShippednotCost { get; set; }
        public Nullable<System.DateTime> LastShipmentDate { get; set; }
        public Nullable<System.DateTime> LastRcptDate { get; set; }
        public Nullable<float> MostRecentCost { get; set; }
        public Nullable<short> MarkupFactor { get; set; }
        public Nullable<System.DateTime> PrevPriceChangeDate { get; set; }
        public Nullable<float> PrevPrice { get; set; }
        public Nullable<System.DateTime> PrevStdCostDate { get; set; }
        public Nullable<short> PrevStdCost { get; set; }
        public Nullable<System.DateTime> PrevRecentCostDate { get; set; }
        public Nullable<float> PrevRecentCost { get; set; }
        public string Rcpt1 { get; set; }
        public string Rcpt2 { get; set; }
        public string Rcpt3 { get; set; }
        public string Rcpt4 { get; set; }
        public string Rcpt5 { get; set; }
        public Nullable<System.DateTime> RcptDate1 { get; set; }
        public Nullable<System.DateTime> RcptDate2 { get; set; }
        public Nullable<System.DateTime> RcptDate3 { get; set; }
        public Nullable<System.DateTime> RcptDate4 { get; set; }
        public Nullable<System.DateTime> RcptDate5 { get; set; }
        public Nullable<int> QtyonHand1 { get; set; }
        public Nullable<short> QtyonHand2 { get; set; }
        public Nullable<short> QtyonHand3 { get; set; }
        public Nullable<short> QtyonHand4 { get; set; }
        public Nullable<short> QtyonHand5 { get; set; }
        public Nullable<float> TotalCost1 { get; set; }
        public Nullable<float> TotalCost2 { get; set; }
        public Nullable<short> TotalCost3 { get; set; }
        public Nullable<short> TotalCost4 { get; set; }
        public Nullable<short> TotalCost5 { get; set; }
        public Nullable<short> AvgDaysBetweenShipments { get; set; }
        public Nullable<float> AvgUnitsShipped { get; set; }
        public Nullable<int> ShipmentCount { get; set; }
        public Nullable<int> UnitsSoldPeriod1 { get; set; }
        public Nullable<int> UnitsSoldPeriod2 { get; set; }
        public Nullable<int> UnitsSoldPeriod3 { get; set; }
        public Nullable<int> UnitsSoldPeriod4 { get; set; }
        public Nullable<int> UnitsSoldPeriod5 { get; set; }
        public Nullable<int> UnitsSoldPeriod6 { get; set; }
        public Nullable<int> UnitsSoldPeriod7 { get; set; }
        public Nullable<int> UnitsSoldPeriod8 { get; set; }
        public Nullable<int> UnitsSoldPeriod9 { get; set; }
        public Nullable<int> UnitsSoldPeriod10 { get; set; }
        public Nullable<int> UnitsSoldPeriod11 { get; set; }
        public Nullable<int> UnitsSoldPeriod12 { get; set; }
        public Nullable<int> UnitsSoldPeriod13 { get; set; }
        public Nullable<int> UnitsSoldYTD { get; set; }
        public Nullable<int> UnitsSoldLY { get; set; }
        public Nullable<float> AmtSoldPeriod1 { get; set; }
        public Nullable<float> AmtSoldPeriod2 { get; set; }
        public Nullable<float> AmtSoldPeriod3 { get; set; }
        public Nullable<float> AmtSoldPeriod4 { get; set; }
        public Nullable<float> AmtSoldPeriod5 { get; set; }
        public Nullable<float> AmtSoldPeriod6 { get; set; }
        public Nullable<float> AmtSoldPeriod7 { get; set; }
        public Nullable<float> AmtSoldPeriod8 { get; set; }
        public Nullable<float> AmtSoldPeriod9 { get; set; }
        public Nullable<float> AmtSoldPeriod10 { get; set; }
        public Nullable<float> AmtSoldPeriod11 { get; set; }
        public Nullable<float> AmtSoldPeriod12 { get; set; }
        public Nullable<float> AmtSoldPeriod13 { get; set; }
        public Nullable<float> AmtSoldYTD { get; set; }
        public Nullable<float> AmtSoldLY { get; set; }
        public Nullable<float> TotalCostsPeriod1 { get; set; }
        public Nullable<float> TotalCostsPeriod2 { get; set; }
        public Nullable<float> TotalCostsPeriod3 { get; set; }
        public Nullable<float> TotalCostsPeriod4 { get; set; }
        public Nullable<float> TotalCostsPeriod5 { get; set; }
        public Nullable<float> TotalCostsPeriod6 { get; set; }
        public Nullable<float> TotalCostsPeriod7 { get; set; }
        public Nullable<float> TotalCostsPeriod8 { get; set; }
        public Nullable<float> TotalCostsPeriod9 { get; set; }
        public Nullable<float> TotalCostsPeriod10 { get; set; }
        public Nullable<float> TotalCostsPeriod11 { get; set; }
        public Nullable<float> TotalCostsPeriod12 { get; set; }
        public Nullable<float> TotalCostsPeriod13 { get; set; }
        public Nullable<float> TotalCostsYTD { get; set; }
        public Nullable<float> TotalCostsLY { get; set; }
        public Nullable<int> UnitsLostYTD { get; set; }
        public Nullable<int> UnitsLostLY { get; set; }
        public Nullable<short> DiscDecimals { get; set; }
        public Nullable<short> DiscMarkupAmt1 { get; set; }
        public Nullable<short> DiscMarkupAmt2 { get; set; }
        public Nullable<short> DiscMarkupAmt3 { get; set; }
        public Nullable<short> DiscMarkupAmt4 { get; set; }
        public Nullable<short> DiscMarkupAmt5 { get; set; }
        public Nullable<int> QtyOnHand { get; set; }
        public Nullable<short> Selected { get; set; }
        public string ActiveItem { get; set; }
        public Nullable<short> RNumAN81CITM { get; set; }
    }
}