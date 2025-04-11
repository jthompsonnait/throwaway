using SalesReturnsSystem.DAL;
using SalesReturnsSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.BLL
{
    public class LookupService
    {

        private readonly SalesReturnsContext _context;

        internal LookupService(SalesReturnsContext context)
        {
            _context = context;
        }

        public List<CategoryView> GetCategories()
        {
            return _context.Categories
                .Where(c => !c.RemoveFromViewFlag)
                .Select(c => new CategoryView
                {
                    CategoryID = c.CategoryID,
                    Description = c.Description + " (" +
                        _context.Parts.Count(p => p.CategoryID == c.CategoryID && !p.Discontinued) + " Parts)",
                    RemoveFromViewFlag = c.RemoveFromViewFlag
                }).ToList();
        }


        public List<PartView> GetItemsByCategoryID(int categoryID)
        {
            if (categoryID <= 0) throw new ArgumentException("Invalid Category ID");

            return _context.Parts
                .Where(p => p.CategoryID == categoryID && !p.Discontinued)
                .Select(p => new PartView
                {
                    PartID = p.PartID,
                    Description = p.Description,
                    SellingPrice = p.SellingPrice,
                    QuantityOnHand = p.QuantityOnHand,
                    QuantityOnOrder = p.QuantityOnOrder,
                    ReorderLevel = p.ReorderLevel,
                    Discontinued = p.Discontinued,
                    VendorID = p.VendorID,
                    CategoryID = p.CategoryID,
                    RemoveFromViewFlag = p.RemoveFromViewFlag
                }).ToList();
        }

        public List<CustomerView> GetAllCustomers()
        {
            return _context.Customers
                .Select(c => new CustomerView
                {
                    CustomerID = c.CustomerID,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    ContactPhone = c.ContactPhone,
                    Address = c.Address,
                    City = c.City,
                    Province = c.Province,
                    PostalCode = c.PostalCode
                }).ToList();
        }





        public List<SaleView> GetSalesByCustomer(int customerId)
        {
            return _context.Sales
                .Where(s => s.CustomerID == customerId)
                .Select(s => new SaleView
                {
                    SaleID = s.SaleID,
                    CustomerID = s.CustomerID,
                    SaleDate = s.SaleDate,
                    SubTotal = s.SubTotal,
                    TaxAmount = s.TaxAmount,
                    CouponID = s.CouponID,
                    PaymentType = s.PaymentType,
                    EmployeeID = s.EmployeeID
                })
                .ToList();
        }
    }
}
