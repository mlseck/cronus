//------------------------------------------------------------------------------
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
    using System.Collections.Generic;

    public partial class hoursworked
    {
        public int entryID { get; set; }
        public decimal hours { get; set; }
        public System.DateTime date { get; set; }
        public string comments { get; set; }
        public string TimePeriod_employeeID { get; set; }
        public System.DateTime TimePeriod_periodEndDate { get; set; }
        public int Activity_activityID { get; set; }
        public int Project_projectID { get; set; }
        public virtual activity activity { get; set; }
        public virtual employeetimeperiod employeetimeperiod { get; set; }
        public virtual project project { get; set; }
        public DayOfWeek currentDay { get; set;}
        public Boolean isDeleted { get; set; }
    }
}
