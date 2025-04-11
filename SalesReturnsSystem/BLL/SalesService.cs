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
    public class SalesService
    {
        private readonly SalesReturnsContext _context;

        internal SalesService(SalesReturnsContext context)
        {
            _context = context;
        }

        public SaleView GetSale(int saleID)
        {
            if (saleID <= 0)
                throw new ArgumentException("Invalid Sale ID.");

            var saleRecord = _context.Sales.FirstOrDefault(s => s.SaleID == saleID);

            if (saleRecord == null)
                throw new ArgumentException("Sale not found.");

            return new SaleView
            {
                SaleID = saleRecord.SaleID,
                SaleDate = saleRecord.SaleDate,
                CustomerID = saleRecord.CustomerID,
                EmployeeID = saleRecord.EmployeeID,
                SubTotal = saleRecord.SubTotal,
                TaxAmount = saleRecord.TaxAmount,
                CouponID = saleRecord.CouponID,
                PaymentType = saleRecord.PaymentType,
                RemoveFromViewFlag = saleRecord.RemoveFromViewFlag,
                Customer = _context.Customers.Where(c => c.CustomerID == saleRecord.CustomerID)
                    .Select(c => new CustomerView
                    {
                        CustomerID = c.CustomerID,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Address = c.Address,
                        City = c.City,
                        Province = c.Province,
                        PostalCode = c.PostalCode,
                        ContactPhone = c.ContactPhone,
                        Textable = c.Textable,
                        EmailAddress = c.EmailAddress,
                        RemoveFromViewFlag = c.RemoveFromViewFlag
                    }).FirstOrDefault(),
                Coupon = saleRecord.CouponID.HasValue
    ? _context.Coupons
        .Where(c => c.CouponID == saleRecord.CouponID.Value)
        .Select(c => new CouponView
        {
            CouponID = c.CouponID,
            CouponIDValue = c.CouponIDValue,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            CouponDiscount = c.CouponDiscount,
            SalesOrService = c.SalesOrService,
            RemoveFromViewFlag = c.RemoveFromViewFlag
        }).FirstOrDefault()
    : null,

                SaleDetails = _context.SaleDetails.Where(d => d.SaleID == saleID)
                    .Select(d => new SaleDetailView
                    {
                        SaleDetailID = d.SaleDetailID,
                        SaleID = d.SaleID,
                        PartID = d.PartID,
                        Quantity = d.Quantity,
                        SellingPrice = d.SellingPrice,
                        RemoveFromViewFlag = d.RemoveFromViewFlag,
                        Part = _context.Parts.Where(p => p.PartID == d.PartID)
                            .Select(p => new PartView
                            {
                                PartID = p.PartID,
                                Description = p.Description,
                                PurchasePrice = p.PurchasePrice,
                                SellingPrice = p.SellingPrice,
                                QuantityOnHand = p.QuantityOnHand,
                                ReorderLevel = p.ReorderLevel,
                                QuantityOnOrder = p.QuantityOnOrder,
                                CategoryID = p.CategoryID,
                                Refundable = p.Refundable == "Y",
                                Discontinued = p.Discontinued,
                                VendorID = p.VendorID,
                                RemoveFromViewFlag = p.RemoveFromViewFlag
                            }).FirstOrDefault()
                    }).ToList()
            };
        }

        public SaleView SaveSale(SaleView saleView)
        {
            if (saleView == null || saleView.SaleDetails == null || !saleView.SaleDetails.Any())
                throw new ArgumentException("Sale must have at least one detail.");

            var errors = new List<Exception>();

            if (saleView.CustomerID <= 0)
                errors.Add(new Exception("Customer is required."));

            if (string.IsNullOrWhiteSpace(saleView.EmployeeID))
                errors.Add(new Exception("Employee is required."));

            if (string.IsNullOrWhiteSpace(saleView.PaymentType))
                errors.Add(new Exception("Payment Type is required."));

            foreach (var item in saleView.SaleDetails)
            {
                if (item.Quantity <= 0)
                    errors.Add(new Exception($"Quantity for Part {item.PartID} must be greater than 0."));

                if (item.SellingPrice <= 0)
                    errors.Add(new Exception($"Selling Price for Part {item.PartID} must be greater than 0."));

                var part = _context.Parts.FirstOrDefault(p => p.PartID == item.PartID);
                if (part == null || part.Discontinued)
                    errors.Add(new Exception($"Part {item.PartID} is invalid or discontinued."));
                else if (item.Quantity > part.QuantityOnHand)
                    errors.Add(new Exception($"Insufficient stock for Part {item.PartID}."));
            }

            if (errors.Any())
                throw new AggregateException("Error(s) occurred while saving the sale.", errors);

            var sale = new Sale
            {
                SaleDate = DateTime.Now,
                CustomerID = saleView.CustomerID,
                EmployeeID = saleView.EmployeeID,
                SubTotal = saleView.SubTotal,
                TaxAmount = saleView.TaxAmount,
                CouponID = saleView.CouponID,
                PaymentType = saleView.PaymentType,
                RemoveFromViewFlag = false,
                SaleDetails = saleView.SaleDetails.Select(sd => new SaleDetail
                {
                    PartID = sd.PartID,
                    Quantity = sd.Quantity,
                    SellingPrice = sd.SellingPrice,
                    RemoveFromViewFlag = false
                }).ToList()
            };

            _context.Sales.Add(sale);

            foreach (var item in sale.SaleDetails)
            {
                var part = _context.Parts.First(p => p.PartID == item.PartID);
                part.QuantityOnHand -= item.Quantity;
            }

            _context.SaveChanges();

            return GetSale(sale.SaleID);
        }

        public CouponView ValidateCoupon(string code)
        {
            var coupon = _context.Coupons.FirstOrDefault(c =>
     c.CouponIDValue.Trim().ToUpper() == code.Trim().ToUpper() &&
     c.StartDate <= DateTime.Now &&
     c.EndDate >= DateTime.Now &&
     !c.RemoveFromViewFlag);


            if (coupon == null) throw new ArgumentException("Invalid or expired coupon.");

            return new CouponView
            {
                CouponID = coupon.CouponID,
                CouponIDValue = coupon.CouponIDValue,
                CouponDiscount = coupon.CouponDiscount,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                SalesOrService = coupon.SalesOrService,
                RemoveFromViewFlag = coupon.RemoveFromViewFlag
            };
        }
    }
}
