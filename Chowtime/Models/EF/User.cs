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
    
    public partial class User
    {
        public User()
        {
            this.UserFarms = new HashSet<UserFarm>();
            this.UserRoles = new HashSet<UserRole>();
        }
    
        public int UserId { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public Nullable<int> StatusId { get; set; }
        public byte[] Password { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<UserFarm> UserFarms { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
