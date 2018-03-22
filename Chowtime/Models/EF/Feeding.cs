//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SGApp.Models.EF
{
    using System;
    using System.Collections.Generic;
    
    public partial class Feeding
    {
        public Feeding()
        {
            this.BinDispersements = new HashSet<BinDispersement>();
        }
    
        public int FeedingId { get; set; }
        public int PondId { get; set; }
        public System.DateTime FeedDate { get; set; }
        public int PoundsFed { get; set; }
        public int UsersFarmId { get; set; }
        public Nullable<int> BinID { get; set; }
    
        public virtual UserFarm UserFarm { get; set; }
        public virtual Pond Pond { get; set; }
        public virtual ICollection<BinDispersement> BinDispersements { get; set; }
        public virtual Bin Bin { get; set; }
    }
}
