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
    
    public partial class CategoryOption
    {
        public int OptionID { get; set; }
        public string Name { get; set; }
        public string Option { get; set; }
        public int CategoryID { get; set; }
        public bool Deleted { get; set; }
    
        public virtual StockCategory StockCategory { get; set; }
    }
}
