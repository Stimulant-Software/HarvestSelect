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
    
    public partial class O2Reading
    {
        public int ReadingId { get; set; }
        public int PondId { get; set; }
        public System.DateTime ReadingDate { get; set; }
        public decimal O2Level { get; set; }
        public int PortableCount { get; set; }
        public int StaticCount { get; set; }
        public int UsersFarmId { get; set; }
        public string Note { get; set; }
        public System.DateTime DayPeriod { get; set; }
    
        public virtual UserFarm UserFarm { get; set; }
        public virtual Pond Pond { get; set; }
    }
}
