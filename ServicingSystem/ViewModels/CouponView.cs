using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicingSystem.ViewModels
{
    public class CouponView
    {
        public int CouponId {get; set;}
        public string CouponName {get; set;}
        public decimal Discount {get; set; }
    }
}
