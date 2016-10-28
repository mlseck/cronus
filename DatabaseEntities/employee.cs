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
    
    public partial class employee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public employee()
        {
            this.audittrails = new HashSet<audittrail>();
            this.favorites = new HashSet<favorite>();
            this.timeperiods = new HashSet<timeperiod>();
            this.groups = new HashSet<group>();
        }
    
        public string employeeID { get; set; }
        public string employeeFirstName { get; set; }
        public string employeeLastName { get; set; }
        public Nullable<int> employeeMinHours { get; set; }
        public Nullable<int> employeeMaxHours { get; set; }
        public int employeePrivileges { get; set; }
        public string employeeEmailAddress { get; set; }
        public string employeePwd { get; set; }
        public string employeeGroupManaged { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<audittrail> audittrails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<favorite> favorites { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<timeperiod> timeperiods { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<group> groups { get; set; }
    }
}