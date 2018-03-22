//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InnovaService
{
    using System;
    using System.Collections.Generic;
    
    public partial class proc_orders
    {
        public proc_orders()
        {
            this.proc_invstatus = new HashSet<proc_invstatus>();
            this.proc_invstatus1 = new HashSet<proc_invstatus>();
            this.proc_orderl = new HashSet<proc_orderl>();
            this.proc_orders1 = new HashSet<proc_orders>();
            this.proc_packs = new HashSet<proc_packs>();
            this.proc_packs1 = new HashSet<proc_packs>();
            this.proc_packs2 = new HashSet<proc_packs>();
        }
    
        public int order { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string shname { get; set; }
        public string extcode { get; set; }
        public string pattern { get; set; }
        public Nullable<int> dimension1 { get; set; }
        public Nullable<int> dimension2 { get; set; }
        public Nullable<int> dimension3 { get; set; }
        public Nullable<int> dimension4 { get; set; }
        public string description1 { get; set; }
        public string description2 { get; set; }
        public string description3 { get; set; }
        public string description4 { get; set; }
        public string description5 { get; set; }
        public string description6 { get; set; }
        public string description7 { get; set; }
        public string description8 { get; set; }
        public string xmldata { get; set; }
        public Nullable<int> itgrsite { get; set; }
        public string itgrstatus { get; set; }
        public bool active { get; set; }
        public Nullable<int> customer { get; set; }
        public Nullable<int> deliverycustomer { get; set; }
        public Nullable<int> agent { get; set; }
        public Nullable<int> transporter { get; set; }
        public Nullable<int> sourcepo { get; set; }
        public Nullable<int> sourceplot { get; set; }
        public Nullable<int> transferinventory { get; set; }
        public short transferstatus { get; set; }
        public Nullable<int> destination { get; set; }
        public Nullable<int> parentorder { get; set; }
        public short numbermethod { get; set; }
        public short orderstatus { get; set; }
        public Nullable<System.DateTime> validfrom { get; set; }
        public Nullable<System.DateTime> validto { get; set; }
        public short ordertype { get; set; }
        public short accepttype { get; set; }
        public Nullable<int> priority { get; set; }
        public Nullable<System.DateTime> begtime { get; set; }
        public Nullable<System.DateTime> endtime { get; set; }
        public Nullable<System.DateTime> dispatchtime { get; set; }
        public Nullable<int> oclayout { get; set; }
        public Nullable<int> matpricelist { get; set; }
        public Nullable<int> labelcultureid { get; set; }
        public Nullable<int> shipment { get; set; }
        public Nullable<float> maxamount { get; set; }
        public Nullable<float> amount { get; set; }
        public Nullable<float> curamount { get; set; }
        public Nullable<float> pndamount { get; set; }
        public short amountum { get; set; }
        public Nullable<float> completepercentage { get; set; }
        public Nullable<float> allowpercentage { get; set; }
        public bool allowadd { get; set; }
        public Nullable<float> curnominal { get; set; }
        public Nullable<float> pndnominal { get; set; }
    
        public virtual base_companies base_companies { get; set; }
        public virtual base_companies base_companies1 { get; set; }
        public virtual base_companies base_companies2 { get; set; }
        public virtual base_companies base_companies3 { get; set; }
        public virtual ICollection<proc_invstatus> proc_invstatus { get; set; }
        public virtual ICollection<proc_invstatus> proc_invstatus1 { get; set; }
        public virtual ICollection<proc_orderl> proc_orderl { get; set; }
        public virtual ICollection<proc_orders> proc_orders1 { get; set; }
        public virtual proc_orders proc_orders2 { get; set; }
        public virtual ICollection<proc_packs> proc_packs { get; set; }
        public virtual ICollection<proc_packs> proc_packs1 { get; set; }
        public virtual ICollection<proc_packs> proc_packs2 { get; set; }
    }
}
