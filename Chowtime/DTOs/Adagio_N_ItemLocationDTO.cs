using SGApp.Models.Common;
using System;
namespace SGApp.DTOs
{
    public class Adagio_N_ItemLocationDTO : IKey
    {

        public string Key { get; set; }
        public Nullable<int> Item { get; set; }
        public Nullable<short> Loc { get; set; }
        public Nullable<int> Item2 { get; set; }
        public string PickingSeq { get; set; }
        public Nullable<short> CarriedinLoc { get; set; }
        public Nullable<short> QtyonHandPlus { get; set; }
        public Nullable<short> QtyonPO { get; set; }
        public Nullable<short> QtyonSO { get; set; }
        public Nullable<short> QtyTempCommitted { get; set; }
        public Nullable<short> QtyShippednotCost { get; set; }
        public Nullable<short> RNumAN81CILO { get; set; }
    }
}