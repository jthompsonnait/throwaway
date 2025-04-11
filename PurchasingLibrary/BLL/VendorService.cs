using PurchasingSystem.DAL;
using PurchasingSystem.Entities;
using PurchasingSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.BLL
{
    public class VendorService
    {
        #region Fields
        private readonly eBike_2025Context _eBikeContext;
        #endregion

        internal VendorService(eBike_2025Context eBikeContext)
        {
            _eBikeContext = eBikeContext;
        }

        public List<VendorView> GetVendors()
        {
            return _eBikeContext.Vendors
                .Where(v => v.RemoveFromViewFlag == false)
                .Select(v => new VendorView
                {
                    VendorID = v.VendorID,
                    VendorName = v.VendorName,
                    Phone = v.Phone,
                    Address = v.Address,
                    City = v.City,
                    Province = v.ProvinceID,
                    PostalCode = v.PostalCode,
                    PONumber = v.Phone,
                    RemoveFromViewFlag = v.RemoveFromViewFlag
                })
                .OrderBy(v => v.VendorName)
                .ToList();
        }

        public VendorView GetVendor(int vendorID)
        {
            #region Business Logic and Parameter Exceptions
            if (vendorID <= 0)
            {
                throw new Exception("Please provide a valid vendor ID");
            }
            #endregion

            return _eBikeContext.Vendors
                .Where(v => v.VendorID == vendorID &&
                            v.RemoveFromViewFlag == false)
                .Select(v => new VendorView
                {
                    VendorID = v.VendorID,
                    VendorName = v.VendorName,
                    Phone = v.Phone,
                    Address = v.Address,
                    City = v.City,
                    Province = v.ProvinceID,
                    PostalCode = v.PostalCode,
                    PONumber = v.Phone,
                    RemoveFromViewFlag = v.RemoveFromViewFlag
                })
                .FirstOrDefault();
        }
    }
}
