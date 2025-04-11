using PurchasingSystem.DAL;
using PurchasingSystem.Entities;
using PurchasingSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.BLL
{
    public class PurchaceOrderService
    {
        #region Fields
        private readonly eBike_2025Context _eBikeContext;
        #endregion

        internal PurchaceOrderService(eBike_2025Context eBikeContext)
        {
            _eBikeContext = eBikeContext;
        }

        public PurchaseOrderView GetActiveOrder(int vendorID)
        {
            #region Business Logic and Parameter Exceptions
            List<Exception> errorList = new List<Exception>();

            bool vendorExists = _eBikeContext.Vendors
                .Where(v => v.VendorID == vendorID)
                .Any();

            if (!vendorExists)
            {
                throw new Exception("Please provide a valid vendor ID");
            }
            #endregion

            PurchaseOrderView purchaseOrderView = new PurchaseOrderView();

            bool orderExists = _eBikeContext.PurchaseOrders
                .Where(po => po.VendorID == vendorID
                             && po.OrderDate == null
                             && po.Closed == false
                             && po.RemoveFromViewFlag == false)
            .Any();

            if (orderExists)
            {
                purchaseOrderView = _eBikeContext.PurchaseOrders
                    .Where(po => po.VendorID == vendorID
                                && po.OrderDate == null
                                && po.RemoveFromViewFlag == false
                                && po.Closed == false)
                    .Select(po => new PurchaseOrderView
                    {
                        PurchaseOrderID = po.PurchaseOrderID,
                        PurchaseOrderNumber = po.PurchaseOrderNumber,
                        OrderDate = po.OrderDate,
                        Subtotal = po.SubTotal,
                        GST = po.TaxAmount,
                        Total = po.SubTotal + po.TaxAmount,
                        Parts = po.PurchaseOrderDetails
                            .Select(pod => new PurchaseOrderDetailView
                            {
                                PurchaseOrderDetailID = pod.PurchaseOrderDetailID,
                                PartID = pod.PartID,
                                Description = pod.Part.Description,
                                QOH = pod.Part.QuantityOnHand,
                                ROL = pod.Part.ReorderLevel,
                                QOO = pod.Part.QuantityOnOrder,
                                QTO = pod.Quantity,
                                PurchasePrice = pod.PurchasePrice,
                                VendoerPartNumber = pod.VendorPartNumber,
                                RemoveFromViewFlag = po.RemoveFromViewFlag
                            })
                            .ToList(),
                        VendorID = po.VendorID,
                        RemoveFromViewFlag = po.RemoveFromViewFlag
                    })
                    .FirstOrDefault();
            }

            return purchaseOrderView;
        }

        public List<PurchaseOrderDetailView> GetSuggestions(int vendorID)
        {
            #region Business Logic and Parameter Exceptions
            List<Exception> errorList = new List<Exception>();

            bool vendorExsists = _eBikeContext.Vendors
                .Where(v => v.VendorID == vendorID)
                .Any();

            if (!vendorExsists)
            {
                throw new Exception("Please provide a valid vendor ID");
            }
            #endregion

            return _eBikeContext.Parts
                .Where(p => p.ReorderLevel - (p.QuantityOnHand + p.QuantityOnOrder) > 0
                            && p.VendorID == vendorID
                            && !p.RemoveFromViewFlag
                )
                .Select(p => new PurchaseOrderDetailView
                {
                    PartID = p.PartID,
                    Description = p.Description,
                    QOH = p.QuantityOnHand,
                    ROL = p.ReorderLevel,
                    QOO = p.QuantityOnOrder,
                    QTO = p.ReorderLevel - (p.QuantityOnHand + p.QuantityOnOrder),
                    PurchasePrice = p.PurchasePrice
                })
                .ToList();
        }

        public PurchaseOrderView SaveUpdateOrder(PurchaseOrderView poView, string userID)
        {
            #region Business Logic and Parameter Exceptions
            List<Exception> errorList = new List<Exception>();

            if (poView == null)
            {
                throw new ArgumentNullException("No purchase order was supplied");
            }

            if (string.IsNullOrWhiteSpace(userID))
            {
                throw new ArgumentNullException("You must be authenticated to update or save orders");
            }

            if (poView.Parts.Count == 0)
            {
                errorList.Add(new Exception("The order must have at least one item in it."));
            }

            foreach (var podView in poView.Parts)
            {
                if (podView.PurchasePrice < 0)
                {
                    errorList.Add(new Exception($"The price of {podView.Description} must be greater must be equal to or greater than $0.00"));
                }

                if (podView.QTO <= 0)
                {
                    errorList.Add(new Exception($"The quantity of {podView.Description} must be greater than 0"));
                }
            }
            #endregion

            PurchaseOrder purchaseOrder = _eBikeContext.PurchaseOrders
                .Where(po => po.PurchaseOrderID == poView.PurchaseOrderID)
                .Select(po => po).FirstOrDefault();

            if (purchaseOrder == null)
            {
                purchaseOrder = new PurchaseOrder();
            }

            purchaseOrder.TaxAmount = poView.GST;
            purchaseOrder.SubTotal = poView.Subtotal;
            purchaseOrder.VendorID = poView.VendorID;
            purchaseOrder.EmployeeID = userID;

            if (purchaseOrder.PurchaseOrderNumber == 0)
            {
                purchaseOrder.PurchaseOrderNumber  = _eBikeContext.PurchaseOrders
                    .Any() ? _eBikeContext.PurchaseOrders
                    .Max(x => x.PurchaseOrderNumber) + 1 : 1;
            }

            foreach (var podView in poView.Parts)
            {
                PurchaseOrderDetail poDetail = _eBikeContext.PurchaseOrderDetails
                    .Where(pod => pod.PurchaseOrderDetailID == podView.PurchaseOrderDetailID)
                    .Select(pod => pod).FirstOrDefault();

                if (poDetail == null)
                {
                    poDetail = new PurchaseOrderDetail();
                }

                poDetail.PartID = podView.PartID;
                poDetail.Quantity = podView.QTO;
                poDetail.PurchasePrice = podView.PurchasePrice;
                poDetail.VendorPartNumber = podView.VendoerPartNumber;


                if (poDetail.PurchaseOrderDetailID == 0)
                {
                    purchaseOrder.PurchaseOrderDetails.Add(poDetail);
                }
                else
                {
                    _eBikeContext.PurchaseOrderDetails.Update(poDetail);
                }
            }

            if (purchaseOrder.PurchaseOrderID == 0)
            {
                _eBikeContext.PurchaseOrders.Add(purchaseOrder);
            }
            else
            {
                _eBikeContext.PurchaseOrders.Update(purchaseOrder);
            }

            if (errorList.Count > 0)
            {
                _eBikeContext.ChangeTracker.Clear();
                string errorMsg = "Unable to add or edit the order. Please check error messages(s)";
                throw new AggregateException(errorMsg, errorList);
            }
            else
            {
                _eBikeContext.SaveChanges();
            }
            return GetActiveOrder(purchaseOrder.VendorID);
        }

        public PurchaseOrderView Place(PurchaseOrderView poView)
        {
            #region Business Logic and Parameter Exceptions
            List<Exception> errorList = new List<Exception>();

            if (!_eBikeContext.PurchaseOrders
                .Where(po => po.PurchaseOrderID == poView.PurchaseOrderID)
                .Any())
            {
                throw new ArgumentNullException("No valid purchase order was supplied");
            }
            #endregion

            PurchaseOrder purchaseOrder = _eBikeContext.PurchaseOrders
                .Where(po => po.PurchaseOrderID == poView.PurchaseOrderID)
                .Select(po => po).FirstOrDefault();

            purchaseOrder.OrderDate = DateTime.Today;

            foreach (var partView in purchaseOrder.PurchaseOrderDetails)
            {
                Part part = _eBikeContext.Parts
                    .Where(p => p.PartID == partView.PartID)
                    .Select(p => p).FirstOrDefault();

                part.QuantityOnOrder += partView.Quantity;

                _eBikeContext.Parts.Update(part);
            }
            _eBikeContext.PurchaseOrders.Update(purchaseOrder);

            if (errorList.Count > 0)
            {
                _eBikeContext.ChangeTracker.Clear();
                string errorMsg = "Unable to place the order. Please check error messages(s)";
                throw new AggregateException(errorMsg, errorList);
            }
            else
            {
                _eBikeContext.SaveChanges();
            }
            return new PurchaseOrderView();
        }

        public PurchaseOrderView DeleteOrder(PurchaseOrderView poView)
        {
            #region Business Logic and Parameter Exceptions
            List<Exception> errorList = new List<Exception>();

            if (!_eBikeContext.PurchaseOrders
                .Where(po => po.PurchaseOrderID == poView.PurchaseOrderID)
                .Any())
            {
                throw new ArgumentNullException("No valid purchase order was supplied");
            }
            #endregion

            PurchaseOrder purchaseOrder = _eBikeContext.PurchaseOrders
                .Where(po => po.PurchaseOrderID == poView.PurchaseOrderID)
                .Select(po => po).FirstOrDefault();

            purchaseOrder.RemoveFromViewFlag = true;
            _eBikeContext.PurchaseOrders.Update(purchaseOrder);

            if (errorList.Count > 0)
            {
                _eBikeContext.ChangeTracker.Clear();
                string errorMsg = "Unable to delete the order. Please check error messages(s)";
                throw new AggregateException(errorMsg, errorList);
            }
            else
            {
                _eBikeContext.SaveChanges();
            }
            return new PurchaseOrderView();
        }
    }
}
