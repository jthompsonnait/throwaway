using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.ViewModels
{
    public class PurchaseOrderView
    {
        public int PurchaseOrderID;
        public int PurchaseOrderNumber;
        public DateTime? OrderDate;
        public decimal Subtotal;
        public decimal GST;
        public decimal Total;
        public List<PurchaseOrderDetailView> Parts;
        public int VendorID;
        public bool RemoveFromViewFlag;
    }

}
