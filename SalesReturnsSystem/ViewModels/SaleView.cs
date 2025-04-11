using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class SaleView
    {
        public int SaleID { get; set; }
        public DateTime SaleDate { get; set; }
        public int CustomerID { get; set; }
        public string EmployeeID { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public int? CouponID { get; set; }
        public string PaymentType { get; set; }
        public bool RemoveFromViewFlag { get; set; }

        public CustomerView Customer { get; set; }
        public CouponView Coupon { get; set; }
        public List<SaleDetailView> SaleDetails { get; set; }
    }
}
