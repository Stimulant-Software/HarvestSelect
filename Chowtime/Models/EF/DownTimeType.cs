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
    
    public partial class DownTimeType
    {
        public DownTimeType()
        {
            this.DownTimes = new HashSet<DownTime>();
        }
    
        public int DownTimeTypeID { get; set; }
        public string DownTimeName { get; set; }
        public int DepartmentID { get; set; }
    
        public virtual Department Department { get; set; }
        public virtual ICollection<DownTime> DownTimes { get; set; }
    }
}