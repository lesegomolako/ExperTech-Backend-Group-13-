//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExperTech_Api.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Sale
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Sale()
        {
            this.Bookings = new HashSet<Booking>();
            this.ClientPackages = new HashSet<ClientPackage>();
            this.SaleLines = new HashSet<SaleLine>();
        }
    
        public int SaleID { get; set; }
        public int StatusID { get; set; }
        public Nullable<int> ReminderID { get; set; }
        public Nullable<int> PaymentTypeID { get; set; }
        public int ClientID { get; set; }
        public int SaleTypeID { get; set; }
        public System.DateTime Date { get; set; }
        public Nullable<decimal> Payment { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual Client Client { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientPackage> ClientPackages { get; set; }
        public virtual PaymentType PaymentType { get; set; }
        public virtual Reminder Reminder { get; set; }
        public virtual SaleStatu SaleStatu { get; set; }
        public virtual SaleType SaleType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SaleLine> SaleLines { get; set; }
    }
}
