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
    
    public partial class Pond
    {
        public Pond()
        {
            this.FarmYields = new HashSet<FarmYield>();
            this.Feedings = new HashSet<Feeding>();
            this.Harvests = new HashSet<Harvest>();
            this.O2Readings = new HashSet<O2Reading>();
            this.PlantPondWeights = new HashSet<PlantPondWeight>();
            this.WeighBacks = new HashSet<WeighBack>();
            this.ProductionTotals = new HashSet<ProductionTotal>();
        }
    
        public int PondId { get; set; }
        public int FarmId { get; set; }
        public string PondName { get; set; }
        public int StatusId { get; set; }
        public decimal Size { get; set; }
        public bool NoFeed { get; set; }
        public int HealthStatus { get; set; }
        public int SortOrder { get; set; }
        public int OldPondID { get; set; }
        public string InnovaName { get; set; }
        public string InnovaCode { get; set; }
    
        public virtual Farm Farm { get; set; }
        public virtual ICollection<FarmYield> FarmYields { get; set; }
        public virtual ICollection<Feeding> Feedings { get; set; }
        public virtual ICollection<Harvest> Harvests { get; set; }
        public virtual HealthStatus HealthStatus1 { get; set; }
        public virtual ICollection<O2Reading> O2Readings { get; set; }
        public virtual ICollection<PlantPondWeight> PlantPondWeights { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<WeighBack> WeighBacks { get; set; }
        public virtual ICollection<ProductionTotal> ProductionTotals { get; set; }
    }
}
