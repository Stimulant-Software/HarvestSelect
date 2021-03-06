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
    
    public partial class proc_lots
    {
        public proc_lots()
        {
            this.proc_packs1 = new HashSet<proc_packs>();
        }
    
        public int lot { get; set; }
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
        public short lotstatus { get; set; }
        public Nullable<System.DateTime> validfrom { get; set; }
        public Nullable<System.DateTime> validto { get; set; }
        public Nullable<int> brcountry { get; set; }
        public Nullable<int> ricountry { get; set; }
        public Nullable<int> ricountry2 { get; set; }
        public Nullable<int> ricountry3 { get; set; }
        public Nullable<int> ricountry4 { get; set; }
        public Nullable<int> ricountry5 { get; set; }
        public Nullable<int> slhouse { get; set; }
        public Nullable<System.DateTime> slday { get; set; }
        public Nullable<int> slsequence { get; set; }
        public Nullable<int> processor { get; set; }
        public Nullable<int> processor2 { get; set; }
        public Nullable<int> customer { get; set; }
        public Nullable<int> bom { get; set; }
        public Nullable<System.DateTime> reftime { get; set; }
        public Nullable<int> expectedcount { get; set; }
        public Nullable<int> currentcount { get; set; }
        public Nullable<int> qamark1 { get; set; }
        public Nullable<int> qamark2 { get; set; }
        public Nullable<int> qamark3 { get; set; }
        public Nullable<int> qamark4 { get; set; }
        public string startpos { get; set; }
        public string endpos { get; set; }
        public Nullable<int> startrmarea { get; set; }
        public Nullable<int> endrmarea { get; set; }
        public Nullable<int> group { get; set; }
        public Nullable<int> subgroup { get; set; }
        public Nullable<int> killmethod { get; set; }
        public Nullable<System.DateTime> begtime { get; set; }
        public Nullable<System.DateTime> endtime { get; set; }
        public string qamark1code { get; set; }
        public string qamark2code { get; set; }
        public string qamark3code { get; set; }
        public string qamark4code { get; set; }
        public Nullable<int> sourcepack { get; set; }
        public Nullable<int> sourcepallet { get; set; }
        public Nullable<int> topflockid { get; set; }
        public Nullable<int> flocknumber { get; set; }
    
        public virtual base_companies base_companies { get; set; }
        public virtual base_companies base_companies1 { get; set; }
        public virtual base_companies base_companies2 { get; set; }
        public virtual base_companies base_companies3 { get; set; }
        public virtual proc_packs proc_packs { get; set; }
        public virtual ICollection<proc_packs> proc_packs1 { get; set; }
    }
}
