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
    
    public partial class Farm
    {
        public Farm()
        {
            this.UserFarms = new HashSet<UserFarm>();
            this.Ponds = new HashSet<Pond>();
        }
    
        public int FarmId { get; set; }
        public int StatusId { get; set; }
        public string FarmName { get; set; }
        public int CompanyId { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<UserFarm> UserFarms { get; set; }
        public virtual ICollection<Pond> Ponds { get; set; }
    }
}
