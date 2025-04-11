using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using eBike2025Context.DAL;
using ServicingSystem.ViewModels;

namespace ServicingSystem.BLL
{
    public class CustomerService
    {
        private readonly ServicingSystemContext _servicingSystemContext;
        internal CustomerService(ServicingSystemContext servicingSystemContext)
        {
            _servicingSystemContext = servicingSystemContext;
        }
        public List<CustomerView> GetCustomers(string lastName)
        {

            if (string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("Please enter the customer's full or partial last name.");
            }

            return _servicingSystemContext.Customers.Where(c => c.LastName.Contains(lastName) && c.RemoveFromViewFlag == false)
                .Select(c => new CustomerView
                {
                    Id = c.CustomerID,
                    Name = c.FirstName + " " + c.LastName,
                    ContactPhone = c.ContactPhone,
                    Address = c.Address + " " + c.City + ", " + c.Province + " " + c.PostalCode
                } )
                .OrderBy(x => x.Name)
                .ToList();

           

        }

        public List<CustomerVehicleView> GetCustomerVehicle(int customerID)
        {
            if (customerID == 0)
            {
                throw new ArgumentNullException("Please enter a vaild customer.");
            }

            return _servicingSystemContext.CustomerVehicles.Where(c => c.CustomerID == customerID && c.RemoveFromViewFlag == false)
                .Select(c => new CustomerVehicleView
                {
                    VIN = c.VehicleIdentification,
                    CustomerId = c.CustomerID,
                    CarName = c.Make + ", " + c.Model

                })
                .OrderBy(x => x.CarName)
                .ToList();
                
        }
        
    }
}
