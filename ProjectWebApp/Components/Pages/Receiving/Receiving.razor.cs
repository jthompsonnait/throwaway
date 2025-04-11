#nullable disable
using ReceivingSystem.BLL;
using Microsoft.AspNetCore.Components;
using ReceivingSystem.ViewModels;
using MudBlazor;
using eBike2025Context.Entities;
using PurchasingSystem.BLL;



namespace ProjectWebApp.Components.Pages.Receiving
{
    public partial class Receiving

    {
        //part1

        [Inject] protected PurchaseOrderService PurchaseOrderService { get; set; } = default!;
        public List<PurchaseOrderView> PurchaseOrder { get; set; } = new();
        [Inject]
        protected IDialogService DialogService { get; set; } = default!;
        //part2
        [Inject] protected OrderDetailsService OrderDetailsService { get; set; }
        private bool showOrderDetails = false;
        private List<OrderDetailsView> orderDetails = new();
        private string closeReason = string.Empty;

        //part3
        protected List<UnorderedItemView> UnorderedItems { get; set; } = new();
        private string description { get; set; } = string.Empty;
        private string vendorPartID { get; set; } = string.Empty;
        private int qty { get; set; } = 0;
        //Errors and Feedback
        private string feedbackMessage = string.Empty;

        // The error message
        private string? errorMessage = string.Empty;

        // has feedback
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);

        // has error
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);

        // error details
        private List<string> errorDetails = new();

        //part1
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            PurchaseOrder = PurchaseOrderService.GetPurchaseOrder();
            await InvokeAsync(StateHasChanged);
        }

        //part2
        private void LoadOrderDetails(int poNumber)
        {
            try
            {
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = String.Empty;
                showOrderDetails = false;

                orderDetails = OrderDetailsService.GetOrderDetails()
                              .Where(x => x.PO == poNumber)
                              .ToList();

                if (orderDetails.Count > 0)
                {
                    showOrderDetails = true;
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
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}Unable to load for order details";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }

        }

        //part3
        private void RemoveItem(int itemId)
        {
            var selectedItem =
                UnorderedItems.FirstOrDefault(x => x.UnorderedItemID == itemId);
            if (selectedItem != null)
            {
                UnorderedItems.Remove(selectedItem);
            }
        }
        private async Task AddItemToList()
        {
           

                if (!string.IsNullOrWhiteSpace(description) && !string.IsNullOrWhiteSpace(vendorPartID) && qty != 0)
                {
                    int maxID = UnorderedItems.Any() ? UnorderedItems.Max(x => x.UnorderedItemID) + 1 : 1;
                    UnorderedItems.Add(new UnorderedItemView()
                    {
                        UnorderedItemID = maxID,
                        Description = description,
                        VendorPartID = vendorPartID,
                        Qty = qty
                    });
                    errorDetails.Clear();

                    await InvokeAsync(StateHasChanged);
                }

        }
           
              
        
        private async Task Clear()
        {
            bool? results = await DialogService.ShowMessageBox(
                "Confirm Clear",
                "Are you sure you want to clear all items?",
                yesText: "Yes",
                cancelText: "No"
            );


            if (results == true)
            {

                UnorderedItems.Clear();


                description = string.Empty;
                vendorPartID = string.Empty;
                qty = 1;


                await InvokeAsync(StateHasChanged);
            }
  
        }

    }
}
