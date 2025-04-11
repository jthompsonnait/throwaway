using ReceivingSystem.DAL;
using ReceivingSystem.Entities;
using ReceivingSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem.BLL
{
    public class OrderDetailsService
    {
        private readonly ReceivingContext _receivingContext;

        internal OrderDetailsService(ReceivingContext receivingContext)
        {
            _receivingContext = receivingContext;
        }

        public List<OrderDetailsView> GetOrderDetails()
        {
            return _receivingContext.PurchaseOrderDetails
                .Select(x => new OrderDetailsView
                {
                    PO = x.PurchaseOrderID,
                    Vendor = x.PurchaseOrder.Vendor.VendorName,
                    Phone = x.PurchaseOrder.Vendor.Phone,
                    PartID = x.PartID,
                    Description = x.Part.Description,
                    OrderQty = x.Quantity,
                    Outstanding = x.Quantity - x.ReceiveOrderDetails
                                              .Sum(r => r.QuantityReceived)
                }).ToList();



        }
    }
}
