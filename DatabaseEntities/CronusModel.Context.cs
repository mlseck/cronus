﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DatabaseEntities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CronusDatabaseEntities : DbContext
    {
        public CronusDatabaseEntities()
            : base("name=CronusDatabaseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<activity> activities { get; set; }
        public virtual DbSet<audittrail> audittrails { get; set; }
        public virtual DbSet<employee> employees { get; set; }
        public virtual DbSet<favorite> favorites { get; set; }
        public virtual DbSet<group> groups { get; set; }
        public virtual DbSet<hoursworked> hoursworkeds { get; set; }
        public virtual DbSet<project> projects { get; set; }
        public virtual DbSet<timeperiod> timeperiods { get; set; }
        public virtual DbSet<database_firewall_rules> database_firewall_rules { get; set; }
    }
}