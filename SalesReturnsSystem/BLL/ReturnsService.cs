using SalesReturnsSystem.DAL;
using SalesReturnsSystem.Entities;
using SalesReturnsSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.BLL
{
    public class ReturnsService
    {
        private readonly SalesReturnsContext _context;

        internal ReturnsService(SalesReturnsContext context)
        {
            _context = context;
        }

        public SaleRefundView GetSaleRefund(int saleRefundID)
        {
           
            var refund = _context.SaleRefunds
                .Where(r => r.SaleRefundID == saleRefundID)
                .Select(r => new SaleRefundView
                {
                    SaleRefundID = r.SaleRefundID,
                    SaleRefundDate = r.SaleRefundDate,
                    SaleID = r.SaleID,
                    EmployeeID = r.EmployeeID,
                    SubTotal = r.SubTotal,
                    TaxAmount = r.TaxAmount,
                    RemoveFromViewFlag = r.RemoveFromViewFlag,
                    SaleRefundDetails = r.SaleRefundDetails.Select(d => new SaleRefundDetailView
                    {
                        SaleRefundDetailID = d.SaleRefundDetailID,
                        SaleRefundID = d.SaleRefundID,
                        PartID = d.PartID,
                        Quantity = d.Quantity,
                        SellingPrice = d.SellingPrice,
                        Reason = d.Reason,
                        RemoveFromViewFlag = d.RemoveFromViewFlag
                    }).ToList()
                })
                .FirstOrDefault();

            return refund;
        }

        public SaleRefundView SaveRefund(SaleRefundView refundView)
        {
            List<Exception> errors = new List<Exception>();

            if (refundView == null || refundView.SaleRefundDetails == null || !refundView.SaleRefundDetails.Any())
            {
                throw new ArgumentNullException("Refund must contain at least one item.");
            }

            if (string.IsNullOrWhiteSpace(refundView.EmployeeID))
            {
                errors.Add(new Exception("Employee ID is required."));
            }

            if (refundView.SaleID <= 0)
            {
                errors.Add(new Exception("Invalid Sale ID."));
            }

            foreach (var detail in refundView.SaleRefundDetails)
            {
                if (detail.Quantity <= 0)
                    errors.Add(new Exception($"Quantity must be greater than 0 for PartID {detail.PartID}"));

                var originalSale = _context.SaleDetails
                    .FirstOrDefault(sd => sd.SaleID == refundView.SaleID && sd.PartID == detail.PartID);

                if (originalSale == null)
                {
                    errors.Add(new Exception($"Item {detail.PartID} was not sold in the original sale."));
                    continue;
                }

                if (detail.Quantity > originalSale.Quantity)
                {
                    errors.Add(new Exception($"Refund quantity for item {detail.PartID} exceeds original sale quantity."));
                }

                if (detail.SellingPrice != originalSale.SellingPrice)
                {
                    errors.Add(new Exception($"Refund price for item {detail.PartID} must match original sale price."));
                }

                if (string.IsNullOrWhiteSpace(detail.Reason))
                {
                    errors.Add(new Exception($"Reason is required for returning PartID {detail.PartID}"));
                }
            }

            if (errors.Any())
            {
                _context.ChangeTracker.Clear();
                throw new AggregateException("Refund could not be saved.", errors);
            }

            var refundEntity = new SaleRefund
            {
                SaleRefundDate = DateTime.Now,
                SaleID = refundView.SaleID,
                EmployeeID = refundView.EmployeeID,
                SubTotal = refundView.SubTotal,
                TaxAmount = refundView.TaxAmount,
                RemoveFromViewFlag = false,
                SaleRefundDetails = refundView.SaleRefundDetails.Select(rd => new SaleRefundDetail
                {
                    PartID = rd.PartID,
                    Quantity = rd.Quantity,
                    SellingPrice = rd.SellingPrice,
                    Reason = rd.Reason,
                    RemoveFromViewFlag = false
                }).ToList()
            };

            _context.SaleRefunds.Add(refundEntity);

            foreach (var rd in refundView.SaleRefundDetails)
            {
                var part = _context.Parts.FirstOrDefault(p => p.PartID == rd.PartID);
                if (part != null)
                {
                    part.QuantityOnHand += rd.Quantity;
                }
            }

            _context.SaveChanges();
            return GetSaleRefund(refundEntity.SaleRefundID);
        }


        public int GetPreviousReturns(int saleID, int partID)
        {
            return _context.SaleRefundDetails
        .Where(rd => rd.SaleRefund.SaleID == saleID && rd.PartID == partID)
        .Sum(rd => rd.Quantity);
        }


    }


}
