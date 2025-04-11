using PurchasingSystem.DAL;
using PurchasingSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.BLL
{
    public class PartService
    {
        #region Fields
        private readonly eBike_2025Context _eBikeContext;
        #endregion

        internal PartService(eBike_2025Context eBikeContext)
        {
            _eBikeContext = eBikeContext;
        }

        public List<PartView> GetParts(int vendorID, List<int> existingPartIDs)
        {
            #region Business Logic and Parameter Exceptions
            if (vendorID <= 0)
            {
                throw new Exception("Please provide a valid vendor ID");
            }
            #endregion

            return _eBikeContext.Parts.Where(p => !existingPartIDs.Contains(p.PartID)
                                    && p.VendorID == vendorID
                                    && !p.RemoveFromViewFlag)

                        .Select(x => new PartView
                        {
                            PartID = x.PartID,
                            Description = x.Description,
                            QOH = x.QuantityOnHand,
                            ROL = x.ReorderLevel,
                            QOO = x.QuantityOnOrder,
                            Buffer = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder),
                            Price = x.PurchasePrice
                        })
                        .OrderBy(x => x.Description)
                        .ToList();
        }

        public PartView GetPart(int partID)
        {
            #region Business Logic and Parameter Exceptions
            if (partID <= 0)
            {
                throw new Exception("Please provide a valid vendor ID");
            }
            #endregion

            return _eBikeContext.Parts.Where(p => p.PartID == partID
                                                  && !p.RemoveFromViewFlag)

                        .Select(x => new PartView
                        {
                            PartID = x.PartID,
                            Description = x.Description,
                            QOH = x.QuantityOnHand,
                            ROL = x.ReorderLevel,
                            QOO = x.QuantityOnOrder,
                            Buffer = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder),
                            Price = x.PurchasePrice
                        })
                        .OrderBy(x => x.Description)
                        .FirstOrDefault();
        }
    }
}
