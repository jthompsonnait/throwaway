using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SalesReturnsSystem.BLL;
using SalesReturnsSystem.ViewModels;

namespace ProjectWebApp.Components.Pages.SalesReturns
{
    public partial class SalesReturns
    {
        #region fields
        private List<CategoryView> Categories = new();
        private List<PartView> Parts = new();
        private List<SaleDetailView> ShoppingCart = new();
        private List<SaleDetailView> SaleDetails = new();
        private SaleView sale = new();
        public List<string> itemToCart = new();
        public List<string> removeFromCart = new();
        private List<SaleView> matchingSales = new();
        private List<CustomerView> Customers = new();
        private List<SaleView> CustomerSales = new();
        private List<CustomerView> FilteredCustomers = new();
       

        private EditContext editContext;

        private List<string> errorDetails = new();
        private string errorMessage = string.Empty;
        private string CouponCode = string.Empty;
        private string PaymentMethod = string.Empty;
        private string feedbackMessage = string.Empty;
        private string successMessage = string.Empty;
        private string RefundMessage = string.Empty;
        private string couponMessage = string.Empty;


        private decimal Subtotal = 0.00m;
        private decimal Tax = 0.00m;
        private decimal Discount = 0.00m;
        private decimal Total = 0.00m;

        private int SelectedCategoryId;
        private int SelectedPartId;
        private int SelectedPartQty = 1;
        private List<PartView> FilteredParts = new();

        private bool IsSalesManager = false;
        private bool isCouponApplied = false;
        private string currentView = "Shopping";
        private bool showDialog = false;
        private string dialogMessage = string.Empty;
        private TaskCompletionSource<bool?> dialogCompletionSource;
        private bool CanProcessReturn => RefundDetails.Any(d => d.Refundable && d.QuantityToReturn > 0 && !string.IsNullOrWhiteSpace(d.Reason));

        private CustomerView? SelectedCustomer;
        private string CurrentCustomerName = string.Empty;
        private decimal discountPercentage = 0m;
        private CouponView? ValidCoupon;


        private string _searchPhoneNumber = string.Empty;
        private string SearchPhoneNumber
        {
            get => _searchPhoneNumber;
            set
            {
                _searchPhoneNumber = value;
                FilteredCustomers = string.IsNullOrWhiteSpace(value)
                    ? Customers
                    : Customers.Where(c =>
                        !string.IsNullOrWhiteSpace(c.ContactPhone) &&
                        c.ContactPhone.Contains(value, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }



        #endregion

        private class RefundDetail
        {
            public SaleDetailView SaleDetail { get; set; } = default!;
            public int QuantityToReturn { get; set; } = 0;
            public int ReturnedQuantity { get; set; } = 0;
            public string Reason { get; set; } = string.Empty;
            public bool Refundable { get; set; } = true;
        }

        private List<RefundDetail> RefundDetails = new();

        #region Properties
        [Inject] public SalesService SalesService { get; set; } = default!;
        [Inject] public ReturnsService ReturnsService { get; set; } = default!;
        [Inject] public LookupService LookupService { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;

        #endregion

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;
                couponMessage = string.Empty;
                successMessage = string.Empty;
                RefundMessage = string.Empty;
                PaymentMethod = string.Empty;
                Categories = LookupService.GetCategories();

                Customers = LookupService.GetAllCustomers();
                FilteredCustomers = Customers;

                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                IsSalesManager = user.IsInRole("Sales Manager");

                sale = new SaleView
                {
                    SaleDate = DateTime.Now,
                    EmployeeID = "1",
                    SaleDetails = new List<SaleDetailView>()
                };

                ShoppingCart = new List<SaleDetailView>();
                SaleDetails = new List<SaleDetailView>();

                editContext = new EditContext(sale);

                await InvokeAsync(StateHasChanged);
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}Unable to load Sales Returns page.";
                foreach (var err in ex.InnerExceptions)
                {
                    errorDetails.Add(err.Message);
                }
            }
        }

        #region Methods




        private void LoadAllParts()
        {
            Parts = LookupService.GetItemsByCategoryID(0);
        }
  






        #endregion

        #region Utility Methods

        private void ClearCustomerSearch()
        {
            SearchPhoneNumber = string.Empty;
            FilteredCustomers = Customers;
            SelectedCustomer = null;
            ShoppingCart.Clear();
            CouponCode = string.Empty;
            PaymentMethod = string.Empty;
            Subtotal = Discount = Tax = Total = 0m;
        }


        private async Task SelectCustomer(TableRowClickEventArgs<CustomerView> args)
        {
            if (ShoppingCart.Any() || !string.IsNullOrWhiteSpace(CouponCode))
            {
                var confirm = await DialogService.ShowMessageBox(
                    "Unsaved Data",
                    "Changing customers will clear your current cart. Continue?",
                    "Yes", "Cancel");
                if (confirm != true) return;
            }

            ApplyCustomer(args.Item);
        }

        private void ApplyCustomer(CustomerView customer)
        {
            SelectedCustomer = customer;
            sale.CustomerID = customer.CustomerID;
            ShoppingCart.Clear();
            CouponCode = string.Empty;
            //PaymentMethod = string.Empty;
            Subtotal = Discount = Tax = Total = 0m;
            feedbackMessage = $"Customer set to: {customer.FirstName} {customer.LastName}";
            StateHasChanged();
        }






       



        private void ClearMessages()
        {
            errorMessage = feedbackMessage = successMessage = couponMessage = RefundMessage = string.Empty;
            errorDetails.Clear();
        }

        private void RefreshTotals()
        {
            Subtotal = ShoppingCart.Sum(item => item.SellingPrice * item.Quantity);
            CalculateTotals();
        }

        

        private void ClearScreen()
        {
            RefundDetails.Clear();
            Subtotal = Tax = Discount = Total = 0m;
            RefundMessage = feedbackMessage = string.Empty;
            SelectedCustomer = null;
            sale = new SaleView();

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




        #endregion

        #region Sales Methods

        private void ClearCoupon()
        {
            CouponCode = string.Empty;
            Discount = 0;
            isCouponApplied = false;
            ValidCoupon = null;
            couponMessage = string.Empty;
            CalculateTotals();
            StateHasChanged();
        }



        private void ClearPartSelection()
        {
            SelectedPartId = 0;
            SelectedPartQty = 1;
            SelectedCategoryId = 0;
            FilteredParts.Clear();
        }




        private void OnCategoryChanged(int categoryId)
        {
            SelectedCategoryId = categoryId;
            FilteredParts = categoryId == 0
                ? new List<PartView>()
                : LookupService.GetItemsByCategoryID(categoryId);

            SelectedPartId = 0;
            SelectedPartQty = 1;
            StateHasChanged();
        }

        private void AddPartToCart()
        {
            try
            {
                var part = FilteredParts.FirstOrDefault(p => p.PartID == SelectedPartId);
                if (part == null) throw new Exception("Invalid part selection");

                if (SelectedPartQty < 1 || SelectedPartQty > part.QuantityOnHand)
                    throw new Exception($"Quantity must be between 1 and {part.QuantityOnHand}");

                var existingItem = ShoppingCart.FirstOrDefault(p => p.PartID == part.PartID);
                if (existingItem != null)
                {
                    if ((existingItem.Quantity + SelectedPartQty) > part.QuantityOnHand)
                        throw new Exception($"Max available: {part.QuantityOnHand - existingItem.Quantity}");

                    existingItem.Quantity += SelectedPartQty;
                }
                else
                {
                    ShoppingCart.Add(new SaleDetailView
                    {
                        PartID = part.PartID,
                        Quantity = SelectedPartQty,
                        SellingPrice = part.SellingPrice,
                        Part = part 
                    });

                }

                RefreshTotals();
                feedbackMessage = $"{SelectedPartQty} x {part.Description} added to cart";
            }
            catch (Exception ex)
            {
                feedbackMessage = ex.Message;
            }
            finally
            {
                ClearPartSelection();
                StateHasChanged();
            }
        }

        private void RemoveFromCart(SaleDetailView cartItem)
        {
            ShoppingCart.Remove(cartItem);
            RefreshTotals();
            StateHasChanged();
        }





        #endregion

        #region Shopping Methods


        private void OnPaymentMethodSelected(string method)
        {
            PaymentMethod = method;
            StateHasChanged();
        }

        private async Task TryChangeCustomer(CustomerView newCustomer)
        {
            if (ShoppingCart.Any() || !string.IsNullOrWhiteSpace(CouponCode))
            {
                var confirm = await DialogService.ShowMessageBox(
                    "Unsaved Data",
                    "Changing customers will clear your current cart. Continue?",
                    "Yes", "Cancel");

                if (confirm != true) return;
            }

            ApplyCustomer(newCustomer);
        }

        private int GetMaxQuantity()
        {
            var selected = FilteredParts.FirstOrDefault(p => p.PartID == SelectedPartId);
            var inCart = ShoppingCart.FirstOrDefault(p => p.PartID == SelectedPartId)?.Quantity ?? 0;
            return selected == null ? 1 : Math.Max(1, selected.QuantityOnHand - inCart);
        }

        private void SearchCustomer()
        {
            if (string.IsNullOrWhiteSpace(SearchPhoneNumber))
            {
                feedbackMessage = "Please enter a phone number to search.";
                return;
            }

            FilteredCustomers = Customers
                .Where(c => c.ContactPhone != null &&
                            c.ContactPhone.Contains(SearchPhoneNumber, StringComparison.OrdinalIgnoreCase))
                .ToList();

            feedbackMessage = FilteredCustomers.Any()
                ? $"{FilteredCustomers.Count} customer(s) found."
                : "No customers found.";
        }




        

        private async Task ConfirmAndReset()
        {
            var confirm = await DialogService.ShowMessageBox("Confirm Cancel", "Are you sure you want to cancel this sale?", "Yes", "No");
            if (confirm == true)
            {
                Reset();
            }
        }



       

        private Dictionary<int, int> PartQuantities = new();

        





        private void ApplyCoupon()
        {
            couponMessage = string.Empty;
            Discount = 0;
            isCouponApplied = false;
            ValidCoupon = null;

            if (string.IsNullOrWhiteSpace(CouponCode))
            {
                couponMessage = "Please enter a coupon code";
                return;
            }

            try
            {
                var coupon = SalesService.ValidateCoupon(CouponCode); // throws if invalid

                ValidCoupon = coupon;
                isCouponApplied = true;

                Discount = Subtotal * (coupon.CouponDiscount / 100m);
                couponMessage = $"{coupon.CouponDiscount}% discount applied";

                CalculateTotals();
            }
            catch (Exception ex)
            {
                couponMessage = ex.Message;
                Discount = 0;
                isCouponApplied = false;
                ValidCoupon = null;

                CalculateTotals();
            }

            StateHasChanged();
        }





        private bool ValidateOrder()
        {
            var errors = new List<string>();

            if (SelectedCustomer == null)
                errors.Add("No customer selected");

            if (!ShoppingCart.Any())
                errors.Add("No items in cart");

            if (string.IsNullOrWhiteSpace(PaymentMethod))
                errors.Add("Payment method not selected");

            // Check stock levels
            foreach (var item in ShoppingCart)
            {
                var part = Parts.FirstOrDefault(p => p.PartID == item.PartID);
                if (part?.QuantityOnHand < item.Quantity)
                    errors.Add($"{part.Description} quantity exceeds available stock");
            }

            if (errors.Any())
            {
                errorMessage = string.Join("\n", errors);
                StateHasChanged();
                return false;
            }

            return true;
        }

        private void ResetForm()
        {
            SelectedCustomer = null;
            SelectedCategoryId = 0;
            SelectedPartId = 0;
            SelectedPartQty = 1;
            CouponCode = string.Empty;
            PaymentMethod = string.Empty;
            Subtotal = 0m;
            Discount = 0m;
            Tax = 0m;
            Total = 0m;
            FilteredCustomers = Customers;
            successMessage = string.Empty;
            StateHasChanged();

        }

        private async Task PlaceOrder()
        {
            try
            {
                
                if (!ValidateOrder())
                    return;

              
                if (!string.IsNullOrWhiteSpace(CouponCode) && !isCouponApplied)
                {
                    var confirm = await DialogService.ShowMessageBox(
                        "Unverified Coupon",
                        "You have entered a coupon but not verified it. Continue without coupon?",
                        "Yes", "Cancel");

                    if (confirm != true) return;

                   
                    ClearCoupon();
                }

                
                var saleView = new SaleView
                {
                    SaleDate = DateTime.Now,
                    EmployeeID = "1",
                    CustomerID = SelectedCustomer.CustomerID,
                    SubTotal = Subtotal,
                    TaxAmount = Tax,
                    CouponID = isCouponApplied ? ValidCoupon?.CouponID : null,
                    PaymentType = PaymentMethod,
                    SaleDetails = ShoppingCart.Select(item => new SaleDetailView
                    {
                        PartID = item.PartID,
                        Quantity = item.Quantity,
                        SellingPrice = item.SellingPrice
                    }).ToList()
                };

                var savedSale = SalesService.SaveSale(saleView);

                foreach (var item in saleView.SaleDetails)
                {
                    var part = Parts.FirstOrDefault(p => p.PartID == item.PartID);
                    if (part != null) part.QuantityOnHand -= item.Quantity;
                }

                successMessage = $"Sale completed successfully! Sale ID: {savedSale.SaleID}";
                ShoppingCart.Clear();
                ResetForm();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error processing sale: {ex.Message}";
                StateHasChanged();
            }
        }


        private void CalculateTotals()
        {
            Tax = (Subtotal - Discount) * 0.05m; 
            Total = (Subtotal - Discount) + Tax;
            StateHasChanged();
        }



        private void Reset()
        {
            Parts.Clear();
            ShoppingCart.Clear();
            RefundDetails.Clear();
            Subtotal = Tax = Discount = Total = 0m;
            CouponCode = PaymentMethod = string.Empty;
            isCouponApplied = false;
            currentView = "Shopping";
            successMessage = string.Empty;

        }

        #endregion

        #region Return Methods



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

                if (saleID <= 0)
                {
                    feedbackMessage = "Please enter a valid Sale ID.";
                    return;
                }


                sale.SaleID = loadedSale.SaleID;
                sale.SaleDate = loadedSale.SaleDate;
                discountPercentage = loadedSale.Coupon?.CouponDiscount ?? 0;
                sale.PaymentType = loadedSale.PaymentType;
                sale.CustomerID = loadedSale.CustomerID;

                var customer = Customers.FirstOrDefault(c => c.CustomerID == sale.CustomerID);
                CurrentCustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "";

                CustomerView loadedCustomer = new CustomerView
                {
                    CustomerID = loadedSale.Customer.CustomerID,
                    FirstName = loadedSale.Customer.FirstName,
                    LastName = loadedSale.Customer.LastName,
                    ContactPhone = loadedSale.Customer.ContactPhone
                };

                SelectedCustomer = loadedCustomer;

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
                StateHasChanged(); 
            }
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



        private void UpdateReturnQuantity(RefundDetail detail)
{
    int maxAvailableQuantity = detail.SaleDetail.Quantity - detail.ReturnedQuantity;

    if (detail.QuantityToReturn < 0 || detail.QuantityToReturn > maxAvailableQuantity)
    {
        feedbackMessage = $"Invalid return quantity for item ID {detail.SaleDetail.PartID}.";
        return;
    }

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



        private async void ProcessRefund(int saleID)
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

            var processedRefund = ReturnsService.SaveRefund(refundView);

            await DialogService.ShowMessageBox(
                title: "Return Completed",
                markupMessage: (MarkupString)$"Refund processed successfully!<br><strong>Refund ID: {processedRefund.SaleRefundID}</strong>",
                yesText: "OK"
            );

            RefundDetails.Clear();
            sale = new SaleView();
            Subtotal = Discount = Tax = Total = 0;
            SelectedCustomer = null;
            discountPercentage = 0;
            SearchPhoneNumber = string.Empty;
            FilteredCustomers = Customers;
            StateHasChanged();
        }





        #endregion
    }
}
