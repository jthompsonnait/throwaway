using Microsoft.AspNetCore.Components;
using ServicingSystem.ViewModels;
using ServicingSystem.BLL;
using eBike2025Context.Entities;
using MudBlazor;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ProjectWebApp.Components.Pages.Servicing
{
    public partial class Servicing
    {
        private int vehicleSelect = 0;

        private string searchValue = string.Empty;
        private int customerId = 0;
        private string selectedVehicleVIN = string.Empty;
        private StandardJobView selectedStandardJob;
        private int catergoryId = 0;
        private decimal serviceSubtotal = 0;
        private decimal partsSubtotal = 0;
        private int quantitySelect = 0;
        private decimal subTotal = 0;
        private decimal discount = 0;
        private decimal gst = 0;
        private decimal total = 0;
        private string couponValue = string.Empty;
        

        private List<CustomerView> CustomerList = new List<CustomerView>();
        private List<CustomerVehicleView> CustomerVehicleList = new List<CustomerVehicleView>();

        private List<StandardJobView> DefaultServiceList = new List<StandardJobView>();
        private StandardJobView SelectedStandardJob
        {
            get => selectedStandardJob;
            set
            {
                selectedStandardJob = value;
                ServiceItemUpdate();
            }
        }
        private JobDetailView ServiceItem = new JobDetailView { JobID = 0, Description = string.Empty, JobHours = 0.0m, JobComments = string.Empty };
        private List<JobDetailView> ServiceOrderList = new List<JobDetailView>();

        private List<CategoryView> CategoryList = new List<CategoryView>();
        private List<PartView> PartList = new List<PartView>();
        private List<JobPartView> PartOrderList = new List<JobPartView>();
        private CouponView Coupon = new CouponView();

        private bool hasDataChanged = false;
        private string feedbackMessage = string.Empty;
        private string errorMessage = string.Empty;
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);
        private List<string> errorDetails = new List<string>();

        [Inject]
        protected CustomerService CustomerService { get; set; } = default!;
        [Inject]
        protected JobsService JobsService { get; set; } = default!;
        [Inject]
        protected IDialogService DialogService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            try
            {
                PartList = JobsService.GetParts(1);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        private void CustomerSearch()
        {
            try
            {

                feedbackMessage = string.Empty;

                errorDetails.Clear();

                errorMessage = string.Empty;

                CustomerList.Clear();

                if (string.IsNullOrWhiteSpace(searchValue))
                {
                    throw new ArgumentException("Please enter the customer's full or partial last name.");
                }

                CustomerList = CustomerService.GetCustomers(searchValue);
                
                if (CustomerList.Count > 0)
                {
                    feedbackMessage = "Customer search was successful.";
                }
                else
                {
                    feedbackMessage = "No customers match that search query.";
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
                errorMessage = $"{errorMessage} Customer Search was unsuccessful.";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }

        private async Task ClearCustomerSearch()
        {

            feedbackMessage = string.Empty;

            errorDetails.Clear();

            errorMessage = string.Empty;

            CustomerList.Clear();

            searchValue = string.Empty;
        }

        private async Task CustomerSelect(int id)
        {
            if (customerId != 0 && customerId != id)
            {
                bool? changeCustomerConfirmation = await DialogService.ShowMessageBox("Confirm Customer Change", $"Are you sure that you wish to change the Customer? You will lose all of your current data.", yesText: "Change", cancelText: "Cancel");
                
                if (changeCustomerConfirmation == false)
                {
                    return;
                }
            }
            try
            {
                customerId = id;
                ServiceItem = new JobDetailView { JobID = 0, Description = string.Empty, JobHours = 0.0m, JobComments = string.Empty };

                CustomerVehicleList = CustomerService.GetCustomerVehicle(customerId);
                DefaultServiceList = JobsService.GetStandardJobs();
                CategoryList = JobsService.GetCategories();
                UpdateServiceTotals();
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
                errorMessage = $"{errorMessage} Customer Vehicle Search was unsuccessful.";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
            

        }

        private void ServiceItemUpdate()
        {
            ServiceItem.Description = SelectedStandardJob.Description;
            ServiceItem.JobHours = SelectedStandardJob.StandardHours;
            ServiceItem.JobComments = string.Empty;
            StateHasChanged();
        }

        private async Task ServiceItemAdd()
        {
            if (string.IsNullOrEmpty(ServiceItem.Description))
            {
                feedbackMessage = string.Empty;

                errorDetails.Clear();

                errorMessage = string.Empty;

                errorMessage = "Please input a service.";

                return;
            }
            ServiceOrderList.Add(ServiceItem);
            serviceSubtotal = ServiceOrderList.Sum(item => item.JobHours * 65.50m);
            UpdateServiceTotals();
            ServiceItem = new JobDetailView();
            StateHasChanged();
        }

        private void ServiceItemReset()
        {
            ServiceItem = new JobDetailView();
            StateHasChanged();
        }

        private void ServiceItemRemove(JobDetailView serviceItem)
        {
            ServiceOrderList.Remove(serviceItem);
            serviceSubtotal = ServiceOrderList.Sum(item => item.JobHours * 65.50m);
            StateHasChanged();
        }

        private void PartsByCatergory()
        {
            try
            {
                PartList = JobsService.GetParts(catergoryId);
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
                errorMessage = $"{errorMessage} Retrival of Parts was unsuccessful.";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }

        }

        private async Task PartItemAdd(PartView partItem)
        {
            JobPartView partOrderItem = new JobPartView();

            partOrderItem.PartID = partItem.PartID;
            partOrderItem.Description = partItem.Description;
            partOrderItem.Quantity = partItem.Quantity;
            partOrderItem.SellingPrice = partItem.Price;


            PartOrderList.Add(partOrderItem);
            partsSubtotal = PartOrderList.Sum(item => item.Quantity * item.SellingPrice);
            UpdateServiceTotals();
            StateHasChanged();
        }

        private void PartItemRemove(JobPartView partItem)
        {
            PartOrderList.Remove(partItem);
            partsSubtotal = PartOrderList.Sum(item => item.Quantity * item.SellingPrice);
            UpdateServiceTotals();
            StateHasChanged();
        }

        private void UpdateServiceTotals()
        {
            subTotal = serviceSubtotal + partsSubtotal;
            gst = (subTotal - discount) * 0.05m;
            total = subTotal - discount + gst;
        }

        private void CouponVerification()
        {
            try
            {
                feedbackMessage = string.Empty;

                errorDetails.Clear();

                errorMessage = string.Empty;

                Coupon = JobsService.VerifyCoupon(couponValue);

                if (Coupon == null)
                {
                    errorMessage = "This coupon is not vaild";
                    return;
                }
                else
                {
                    discount = Coupon.Discount;
                    feedbackMessage = "Coupon is Vaild!";
                }
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
                errorMessage = $"{errorMessage} Verifying Coupon was unsuccessful.";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }


        }

        private void ServiceSave()
        {
            try
            {
                if (string.IsNullOrEmpty(selectedVehicleVIN))
                {
                    errorDetails.Add("No Vehicle has been selected.");
                }

                if (errorDetails.Count > 0)
                {
                    throw new Exception("Please fix the page errors listed below:");
                }
            } catch
            {

            }
        }
    }
}
