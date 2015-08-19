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
    
    public partial class UserFarm
    {
        public UserFarm()
        {
            this.Feedings = new HashSet<Feeding>();
            this.O2Readings = new HashSet<O2Reading>();
        }
    
        public int UserFarmId { get; set; }
        public int UserId { get; set; }
        public int FarmId { get; set; }
        public int StatusId { get; set; }
    
        public virtual Farm Farm { get; set; }
        public virtual ICollection<Feeding> Feedings { get; set; }
        public virtual Status Status { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<O2Reading> O2Readings { get; set; }
    }
}
