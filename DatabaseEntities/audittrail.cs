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
    
    public partial class audittrail
    {
        public int auditID { get; set; }
        public string employeeTimeperiod_employeeID { get; set; }
        public System.DateTime employeeTimeperiod_periodEndDate { get; set; }
        public System.DateTime editDateTime { get; set; }
        public string Comment { get; set; }
        public string Employee_employeeID { get; set; }
    
        public virtual employee employee { get; set; }
        public virtual employeetimeperiod employeetimeperiod { get; set; }
    }
}
