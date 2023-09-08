//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EquipmentApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RentalRequest
    {
        public int Id { get; set; }
        public Nullable<int> User_Id { get; set; }
        public Nullable<int> Equipment_Id { get; set; }
        public Nullable<System.DateTime> RequestDate { get; set; }
        public Nullable<System.DateTime> RentalStartDate { get; set; }
        public Nullable<System.DateTime> RentalEndDate { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }
        public string Receipt { get; set; }
        public Nullable<bool> RequestStatus { get; set; }
    
        public virtual Equipment Equipment { get; set; }
        public virtual User User { get; set; }
    }
}
