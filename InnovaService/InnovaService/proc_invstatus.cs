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
    
    public partial class proc_invstatus
    {
        public int id { get; set; }
        public int inventory { get; set; }
        public Nullable<int> material { get; set; }
        public Nullable<int> supplier { get; set; }
        public Nullable<int> po { get; set; }
        public Nullable<int> batch { get; set; }
        public Nullable<int> lot { get; set; }
        public Nullable<int> plot { get; set; }
        public Nullable<int> rmarea { get; set; }
        public Nullable<System.DateTime> prday { get; set; }
        public Nullable<int> units { get; set; }
        public Nullable<float> weight { get; set; }
        public Nullable<float> nominal { get; set; }
        public Nullable<int> pieces { get; set; }
        public Nullable<float> value { get; set; }
        public Nullable<int> snapshot { get; set; }
        public System.DateTime regtime { get; set; }
        public System.DateTime xacttime { get; set; }
        public short rtype { get; set; }
        public Nullable<int> order { get; set; }
        public Nullable<int> porder { get; set; }
    
        public virtual base_companies base_companies { get; set; }
        public virtual proc_lots proc_lots { get; set; }
        public virtual proc_materials proc_materials { get; set; }
        public virtual proc_orders proc_orders { get; set; }
        public virtual proc_orders proc_orders1 { get; set; }
    }
}
