using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using PurchasingSystem.BLL;
using PurchasingSystem.ViewModels;
using static MudBlazor.Icons;

namespace ProjectWebApp.Components.Pages.Purchasing
{
    public partial class Purchasing
    {
        #region Fields
        private MudForm purchasingForm = new();
        private List<VendorView> vendors = new();
        private VendorView vendorSelect = new();
        private VendorView vendorView = new();
        private PurchaseOrderView purchaseOrderView = new();
        private List<PartView> parts = new();

        // Authentication Fields
        private string fullName = string.Empty;
        private string userID = string.Empty;
        #endregion

        #region Validation
        private bool isFormValid;
        private bool hasDataChanged = false;
        #endregion

        #region Feedback & Error Messages
        private string feedbackMessage = string.Empty;
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);

        private string? errorMessage;
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);
        private List<string> errorDetails = new();
        #endregion

        #region Properties
        [Inject]
        protected VendorService VendorService { get; set; } = default!;

        [Inject]
        protected PurchaceOrderService PurchaceOrderService { get; set; } = default!;

        [Inject]
        protected PartService PartService { get; set; } = default!;

        [Inject]
        protected IDialogService DialogService { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; set; } = default!;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            try
            {
                vendors = VendorService.GetVendors();

                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity is { IsAuthenticated: true })
                {
                    fullName = user.FindFirst("FullName")?.Value ?? "Unknown User";
                    userID = user.FindFirst("EmployeeID")?.Value ?? "";
                }

                StateHasChanged();
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
                errorMessage = $"{errorMessage} Unable to search load vendors";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }

        #region Order CRUD Methods
        public async Task FindOrder()
        {
            try
            {
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;

                if (vendorSelect.VendorID <= 0)
                {
                    throw new ArgumentException("Please select a valid vendor.");
                }

                if (hasDataChanged && vendorView.VendorID != 0)
                {
                    bool? results = await DialogService.ShowMessageBox(
                        "Confirm Switch Vendor",
                        "Do you wish to change to a different vendor? All unsaved changes will be lost.",
                        yesText: "Yes", cancelText: "No"
                    );

                    if (results == null)
                    {
                        return;
                    }
                }

                // Getting the vendor information
                vendorView = VendorService.GetVendor(vendorSelect.VendorID);

                // Getting the order with the vendor, or creating a new one
                purchaseOrderView = PurchaceOrderService.GetActiveOrder(vendorView.VendorID);

                if (purchaseOrderView.VendorID == 0)
                {
                    purchaseOrderView.VendorID = vendorSelect.VendorID;
                }

                // Getting suggestions if this is a new order
                if (purchaseOrderView.PurchaseOrderNumber == 0)
                {
                    purchaseOrderView.Parts = PurchaceOrderService.GetSuggestions(vendorView.VendorID);
                    hasDataChanged = true;
                    UpdateTotals();
                }
                else
                {
                    hasDataChanged = false;
                }

                // Getting a list of parts not in the order
                List<int> exsistingParts = new List<int>();
                foreach (var part in purchaseOrderView.Parts)
                {
                    exsistingParts.Add(part.PartID);
                }

                parts = PartService.GetParts(vendorView.VendorID, exsistingParts);
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
                errorMessage = $"{errorMessage} Unable to find or create an order";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }

        public async Task UpdateSaveOrder()
        {
            bool? results = await DialogService.ShowMessageBox(
                    "Confirm Save",
                    $"Are you sure you would like to " +
                    $"{(purchaseOrderView.PurchaseOrderNumber == 0 ? "save" : "update")}" +
                    $" this order?",
                    yesText: "Yes", cancelText: "No"
                );

            if (results == null)
            {
                return;
            }

            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
            ValidateOrder();

            if (isFormValid)
            {
                try
                {
                    purchaseOrderView = PurchaceOrderService.SaveUpdateOrder(purchaseOrderView, userID);
                    feedbackMessage = "Data was successfully saved!";
                    purchasingForm.ResetTouched();
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
                    errorMessage = $"{errorMessage}Unable to {(purchaseOrderView.PurchaseOrderNumber == 0 ? "save" : "update")} the order";
                    foreach (var error in ex.InnerExceptions)
                    {
                        errorDetails.Add(error.Message);
                    }
                }
            }
        }

        public async Task PlaceOrder()
        {
            bool? results = await DialogService.ShowMessageBox(
                    "Confirm Order Placement",
                    "Do you wish to place the order? Order Details cannot be edited one the order is placed.",
                    yesText: "Yes", cancelText: "No"
                );

            if (results == null)
            {
                return;
            }


            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
            ValidateOrder();

            if (isFormValid)
            {
                try
                {
                    if (hasDataChanged)
                    {
                        purchaseOrderView = PurchaceOrderService.SaveUpdateOrder(purchaseOrderView, userID);
                    }

                    string oldVendorName = VendorService.GetVendor(vendorSelect.VendorID).VendorName;

                    purchaseOrderView = PurchaceOrderService.Place(purchaseOrderView);
                    vendorSelect.VendorID = 0;
                    vendorView = new();
                    feedbackMessage = $"The order with {oldVendorName} successfully placed!";
                    purchasingForm.ResetTouched();
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
                    errorMessage = $"{errorMessage}Unable to {(purchaseOrderView.PurchaseOrderNumber == 0 ? "save" : "update")} the order";
                    foreach (var error in ex.InnerExceptions)
                    {
                        errorDetails.Add(error.Message);
                    }
                }
            }
        }

        public async Task DeleteOrder()
        {


            bool? results = await DialogService.ShowMessageBox(
                    $"Confirm {(purchaseOrderView.PurchaseOrderNumber != 0 ? "Delete" : "Cancel")}",
                    $"Are you sure you would like to {(purchaseOrderView.PurchaseOrderNumber != 0 ? "delete" : "cancel")} this order?",
                    yesText: "Yes", cancelText: "No"
                );

            if (results == null)
            {
                return;
            }

            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
            try
            {
                if (purchaseOrderView.PurchaseOrderNumber != 0)
                {
                    purchaseOrderView = PurchaceOrderService.DeleteOrder(purchaseOrderView);
                    feedbackMessage = $"The order has been deleted.";
                }
                else
                {
                    purchaseOrderView = new();
                    feedbackMessage = $"The order has been canceled.";
                }
                vendorSelect.VendorID = 0;
                vendorView = new();
                purchasingForm.ResetTouched();
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
                errorMessage = $"{errorMessage}Unable to delete the order";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }

            
        }

        public async Task ClearOrder()
        {
            if (hasDataChanged)
            {
                bool? results = await DialogService.ShowMessageBox(
                    "Confirm Cancel",
                    "Do you wish to close the order editor? All unsaved changes will be lost.",
                    yesText: "Yes", cancelText: "No"
                );

                if (results == null)
                {
                    return;
                }
            }

            vendorSelect.VendorID = 0;
            vendorView = new();
        }
        #endregion

        #region Managing Parts Methods
        private async Task QTOChanged(PurchaseOrderDetailView item, int newValue)
        {
            item.QTO = newValue;
            await purchasingForm.Validate();
            hasDataChanged = true;
            ValidateOrder();
            UpdateTotals();
            StateHasChanged();
        }

        private async Task PriceChanged(PurchaseOrderDetailView item, decimal newValue)
        {
            item.PurchasePrice = newValue;
            await purchasingForm.Validate();
            hasDataChanged = true;
            ValidateOrder();
            UpdateTotals();
            StateHasChanged();
        }

        private void UpdateTotals()
        {
            purchaseOrderView.Subtotal = purchaseOrderView.Parts
                .Where(x => !x.RemoveFromViewFlag)
                .Sum(x => x.QTO * x.PurchasePrice);
            purchaseOrderView.GST = purchaseOrderView.Parts
                .Where(x => !x.RemoveFromViewFlag)
                .Sum(x => x.QTO * x.PurchasePrice * 0.05m);
            purchaseOrderView.Total = purchaseOrderView.Subtotal + purchaseOrderView.GST;
        }
        public async Task RemovePartFromOrder(int partID)
        {
            var part = PartService.GetPart(partID);
            if (part != null)
            {
                parts.Add(part);
                parts = parts.OrderBy(x => x.Description).ToList();

                var invoiceLine = purchaseOrderView.Parts.FirstOrDefault(x => x.PartID == partID);
                if (invoiceLine != null)
                {
                    purchaseOrderView.Parts.Remove(invoiceLine);
                    UpdateTotals();
                }

                hasDataChanged = true;
                // ValidateOrder();
                await InvokeAsync(StateHasChanged);
            }
        }

        public async Task AddPartToOrder(int partID)
        {
            var part = parts.FirstOrDefault(x => x.PartID == partID);
            if (part != null)
            {
                purchaseOrderView.Parts.Add(new PurchaseOrderDetailView
                {
                    PartID = part.PartID,
                    Description = part.Description,
                    QOH = part.QOH,
                    ROL = part.ROL,
                    QTO = 0,
                    PurchasePrice = part.Price
                });
                parts.Remove(part);
                UpdateTotals();

                hasDataChanged = true;
                // ValidateOrder();
                await InvokeAsync(StateHasChanged);
            }
        }

        #endregion

        #region Vaidate Order
        public void ValidateOrder()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
            isFormValid = true;

            List<string> errorsFound = new();

            if(purchaseOrderView.Parts.Count == 0)
            {
                errorsFound.Add("The order must have at least one item in it.");
            }
            else
            {
                foreach (var podView in purchaseOrderView.Parts)
                {

                    if (podView.PurchasePrice < 0)
                    {
                        errorsFound.Add($"The price of {podView.Description} must be greater must be equal to or greater than $0.00");
                    }

                    if (podView.QTO <= 0)
                    {
                        errorsFound.Add($"The quantity of {podView.Description} must be greater than 0");
                    }
                }
            }

            if (errorsFound.Count != 0)
            {
                errorMessage = "The Order is not valid. Please fix the following issues";
                isFormValid = false;
                errorDetails = errorsFound;
            }
        }
        #endregion
    }

}

