﻿ 
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class AppEntities : DbContext
    {
        public AppEntities()
            : base("name=AppEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Company> Companies { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Feeding> Feedings { get; set; }
        public DbSet<Harvest> Harvests { get; set; }
        public DbSet<UserFarm> UserFarms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<HealthStatus> HealthStatuses { get; set; }
        public DbSet<O2Reading> O2Readings { get; set; }
        public DbSet<FarmYieldHeader> FarmYieldHeaders { get; set; }
        public DbSet<FarmYield> FarmYields { get; set; }
        public DbSet<ShiftEnd> ShiftEnds { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<PlantPondWeight> PlantPondWeights { get; set; }
        public DbSet<WeighBack> WeighBacks { get; set; }
        public DbSet<Pond> Ponds { get; set; }
        public DbSet<ProductionTotal> ProductionTotals { get; set; }
        public DbSet<Absence> Absences { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DownTimeType> DownTimeTypes { get; set; }
        public DbSet<FilletScaleReading> FilletScaleReadings { get; set; }
        public DbSet<FinishTime> FinishTimes { get; set; }
        public DbSet<DepartmentTotal> DepartmentTotals { get; set; }
        public DbSet<DownTime> DownTimes { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<AD_Groups> AD_Groups { get; set; }
        public DbSet<AD_Products> AD_Products { get; set; }
        public DbSet<AD_WeekData> AD_WeekData { get; set; }
    }
}
