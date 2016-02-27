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
    
    public partial class Department
    {
        public Department()
        {
            this.Absences = new HashSet<Absence>();
            this.DownTimeTypes = new HashSet<DownTimeType>();
            this.FinishTimes = new HashSet<FinishTime>();
            this.DepartmentTotals = new HashSet<DepartmentTotal>();
        }
    
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
    
        public virtual ICollection<Absence> Absences { get; set; }
        public virtual ICollection<DownTimeType> DownTimeTypes { get; set; }
        public virtual ICollection<FinishTime> FinishTimes { get; set; }
        public virtual ICollection<DepartmentTotal> DepartmentTotals { get; set; }
    }
}
