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
    
    public partial class StockTakeLine
    {
        public int LineID { get; set; }
        public int ItemID { get; set; }
        public int StockTakeID { get; set; }
        public int Quantity { get; set; }
    
        public virtual StockItem StockItem { get; set; }
        public virtual StockTake StockTake { get; set; }
    }
}
