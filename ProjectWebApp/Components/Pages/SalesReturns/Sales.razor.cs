using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SalesReturnsSystem.BLL;
using SalesReturnsSystem.ViewModels;

namespace ProjectWebApp.Components.Pages.SalesReturns
{
    public partial class Sales
    {
        #region Fields
        private List<CategoryView> Categories = new();
        private List<PartView> Parts = new();
        private List<SaleDetailView> ShoppingCart = new();
        private List<CustomerView> Customers = new();
        private List<CustomerView> FilteredCustomers = new();

        private EditContext editContext;

        private string errorMessage = string.Empty;
        private string feedbackMessage = string.Empty;
        private string successMessage = string.Empty;
        private string couponMessage = string.Empty;
        private string CouponCode = string.Empty;
        private string PaymentMethod = string.Empty;

        private decimal Subtotal = 0.00m;
        private decimal Tax = 0.00m;
        private decimal Discount = 0.00m;
        private decimal Total = 0.00m;

        private int SelectedCategoryId;
        private int SelectedPartId;
        private int SelectedPartQty = 1;
        private List<PartView> FilteredParts = new();

        private CustomerView? SelectedCustomer;
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

        #region Properties
        [Inject] public SalesService SalesService { get; set; } = default!;
        [Inject] public LookupService LookupService { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Categories = LookupService.GetCategories();
                Customers = LookupService.GetAllCustomers();
                FilteredCustomers = Customers;

                editContext = new EditContext(new SaleView
                {
                    SaleDate = DateTime.Now,
                    EmployeeID = "1",
                    SaleDetails = new List<SaleDetailView>()
                });

                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error initializing sales page: {ex.Message}";
            }
        }

        #region Methods
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
            ShoppingCart.Clear();
            CouponCode = string.Empty;
            Subtotal = Discount = Tax = Total = 0m;
            feedbackMessage = $"Customer set to: {customer.FirstName} {customer.LastName}";
            StateHasChanged();
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

        private void ClearPartSelection()
        {
            SelectedPartId = 0;
            SelectedPartQty = 1;
            SelectedCategoryId = 0;
            FilteredParts.Clear();
        }

        private void ApplyCoupon()
        {
            couponMessage = string.Empty;
            Discount = 0;
            ValidCoupon = null;

            if (string.IsNullOrWhiteSpace(CouponCode))
            {
                couponMessage = "Please enter a coupon code";
                return;
            }

            try
            {
                var coupon = SalesService.ValidateCoupon(CouponCode);
                ValidCoupon = coupon;
                Discount = Subtotal * (coupon.CouponDiscount / 100m);
                couponMessage = $"{coupon.CouponDiscount}% discount applied";
                CalculateTotals();
            }
            catch (Exception ex)
            {
                couponMessage = ex.Message;
                Discount = 0;
                ValidCoupon = null;
                CalculateTotals();
            }

            StateHasChanged();
        }

        private void ClearCoupon()
        {
            CouponCode = string.Empty;
            Discount = 0;
            ValidCoupon = null;
            couponMessage = string.Empty;
            CalculateTotals();
            StateHasChanged();
        }

        private void RefreshTotals()
        {
            Subtotal = ShoppingCart.Sum(item => item.SellingPrice * item.Quantity);
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            Tax = (Subtotal - Discount) * 0.05m;
            Total = (Subtotal - Discount) + Tax;
            StateHasChanged();
        }

        private async Task PlaceOrder()
        {
            try
            {
                if (SelectedCustomer == null)
                {
                    errorMessage = "No customer selected";
                    return;
                }

                if (!ShoppingCart.Any())
                {
                    errorMessage = "No items in cart";
                    return;
                }

                if (string.IsNullOrWhiteSpace(PaymentMethod))
                {
                    errorMessage = "Payment method not selected";
                    return;
                }

                var saleView = new SaleView
                {
                    SaleDate = DateTime.Now,
                    EmployeeID = "1",
                    CustomerID = SelectedCustomer.CustomerID,
                    SubTotal = Subtotal,
                    TaxAmount = Tax,
                    CouponID = ValidCoupon?.CouponID,
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
        #endregion
    }
}