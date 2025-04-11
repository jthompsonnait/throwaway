using ReceivingSystem.DAL;
using ReceivingSystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReceivingSystem.ViewModels;

namespace ReceivingSystem.BLL
{
    public class PurchaseOrderService
    {
        private readonly ReceivingContext _receivingContext;

        internal PurchaseOrderService(ReceivingContext receivingContext)
        {
            _receivingContext = receivingContext;
        }

        public List<PurchaseOrderView> GetPurchaseOrder()
        {
            return _receivingContext.PurchaseOrders
                .Where(x => x.OrderDate != null && !x.Closed)
                .Select(x => new PurchaseOrderView
                {
                    PO = x.PurchaseOrderID,
                    Date = x.OrderDate,
                    Vendor = x.Vendor.VendorName,
                    Contact = x.Vendor.Phone
                }).ToList();

        }
    }
}
