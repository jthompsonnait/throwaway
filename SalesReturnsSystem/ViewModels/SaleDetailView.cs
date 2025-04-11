using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class SaleDetailView
    {
        public int SaleDetailID { get; set; }
        public int SaleID { get; set; }
        public int PartID { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public bool RemoveFromViewFlag { get; set; }

        public PartView Part { get; set; }
    }
}
