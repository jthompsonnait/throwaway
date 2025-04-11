using Microsoft.AspNetCore.Components;
using MudBlazor;
using SalesReturnsSystem.BLL;
using SalesReturnsSystem.ViewModels;

namespace ProjectWebApp.Components.Pages.SalesReturns
{
    public partial class Returns
    {
        #region Fields
        private List<RefundDetail> RefundDetails = new();
        private SaleView sale = new();
        private CustomerView? SelectedCustomer;
        private string errorMessage = string.Empty;
        private string feedbackMessage = string.Empty;
        private decimal Subtotal = 0.00m;
        private decimal Tax = 0.00m;
        private decimal Discount = 0.00m;
        private decimal Total = 0.00m;
        private decimal discountPercentage = 0m;

        private bool CanProcessReturn => RefundDetails.Any(d => d.Refundable && d.QuantityToReturn > 0 && !string.IsNullOrWhiteSpace(d.Reason));
        #endregion

        #region Properties
        [Inject] public ReturnsService ReturnsService { get; set; } = default!;
        [Inject] public SalesService SalesService { get; set; } = default!;
        [Inject] public LookupService LookupService { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ResetScreen();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error initializing returns page: {ex.Message}";
            }
        }

        #region Methods

        private async Task ConfirmAndLoadSale(int saleID)
        {
            try
            {
                if (RefundDetails.Any(d => d.QuantityToReturn > 0 || !string.IsNullOrWhiteSpace(d.Reason)))
                {
                    var confirm = await DialogService.ShowMessageBox(
                        "Unsaved Changes",
                        "You have unsaved return data. Continue and lose changes?",
                        yesText: "Yes", cancelText: "Cancel");

                    if (confirm != true) return;
                }

                var loadedSale = SalesService.GetSale(saleID);

                if (loadedSale == null || !loadedSale.SaleDetails.Any())
                {
                    feedbackMessage = "No sale details found for the given Sale ID.";
                    return;
                }

                sale = loadedSale;
                discountPercentage = loadedSale.Coupon?.CouponDiscount ?? 0;

                SelectedCustomer = loadedSale.Customer;

                RefundDetails = loadedSale.SaleDetails
                    .Select(detail => new RefundDetail
                    {
                        SaleDetail = detail,
                        ReturnedQuantity = ReturnsService.GetPreviousReturns(saleID, detail.PartID),
                        Refundable = detail.Part?.Refundable ?? true
                    })
                    .ToList();

                CalculateRefundTotals();
            }
            catch (Exception ex)
            {
                errorMessage = $"Unable to load sale: {ex.Message}";
            }
        }

        private void OnQuantityChanged(RefundDetail context, int val)
        {
            context.QuantityToReturn = val;
            CalculateRefundTotals();
        }

        private void OnReasonChanged(RefundDetail context, string val)
        {
            context.Reason = val;
            CalculateRefundTotals();
        }

        private void CalculateRefundTotals()
        {
            Subtotal = RefundDetails.Sum(detail =>
                detail.SaleDetail.SellingPrice * detail.QuantityToReturn * (1 - discountPercentage / 100m));

            Discount = RefundDetails.Sum(detail =>
                detail.SaleDetail.SellingPrice * detail.QuantityToReturn * (discountPercentage / 100m));

            Tax = Subtotal * 0.05m;
            Total = Subtotal + Tax;
        }

        private async Task ProcessRefund(int saleID)
        {
            if (!RefundDetails.Any(d => d.QuantityToReturn > 0))
            {
                feedbackMessage = "No items selected for refund.";
                return;
            }

            foreach (var item in RefundDetails.Where(d => d.QuantityToReturn > 0))
            {
                if (!item.Refundable)
                {
                    feedbackMessage = $"Item {item.SaleDetail.Part.Description} is not refundable.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(item.Reason))
                {
                    feedbackMessage = $"Please provide a reason for returning {item.SaleDetail.Part.Description}.";
                    return;
                }

                int maxReturnable = item.SaleDetail.Quantity - item.ReturnedQuantity;
                if (item.QuantityToReturn > maxReturnable)
                {
                    feedbackMessage = $"Cannot return more than {maxReturnable} units of {item.SaleDetail.Part.Description}.";
                    return;
                }
            }

            var refundView = new SaleRefundView
            {
                SaleID = saleID,
                SaleRefundDate = DateTime.Now,
                EmployeeID = "1",
                SubTotal = Subtotal,
                TaxAmount = Tax,
                SaleRefundDetails = RefundDetails
                    .Where(detail => detail.QuantityToReturn > 0)
                    .Select(detail => new SaleRefundDetailView
                    {
                        PartID = detail.SaleDetail.PartID,
                        Quantity = detail.QuantityToReturn,
                        SellingPrice = detail.SaleDetail.SellingPrice,
                        Reason = detail.Reason
                    }).ToList()
            };

            try
            {
                var processedRefund = ReturnsService.SaveRefund(refundView);

                await DialogService.ShowMessageBox(
                    title: "Return Completed",
                    markupMessage: (MarkupString)$"Refund processed successfully!<br><strong>Refund ID: {processedRefund.SaleRefundID}</strong>",
                    yesText: "OK"
                );

                ResetScreen();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error processing refund: {ex.Message}";
            }
        }

        private void ResetScreen()
        {
            sale = new SaleView();
            SelectedCustomer = null;
            RefundDetails.Clear();
            Subtotal = Discount = Tax = Total = 0m;
            feedbackMessage = string.Empty;
            errorMessage = string.Empty;
        }

        private string GetPaymentMethodName(string code)
        {
            return code switch
            {
                "C" => "Cash",
                "D" => "Debit",
                "M" => "Credit",
                _ => "Unknown"
            };
        }

        #endregion

        #region Nested Classes
        private class RefundDetail
        {
            public SaleDetailView SaleDetail { get; set; } = default!;
            public int QuantityToReturn { get; set; } = 0;
            public int ReturnedQuantity { get; set; } = 0;
            public string Reason { get; set; } = string.Empty;
            public bool Refundable { get; set; } = true;
        }
        #endregion
    }
}