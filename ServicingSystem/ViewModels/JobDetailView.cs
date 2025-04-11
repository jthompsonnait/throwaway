using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicingSystem.ViewModels
{
    public class JobDetailView
    {
        public int JobID { get; set; }
        public string EmployeeId { get; set; }
        public string Description { get; set; }
        public decimal JobHours { get; set; }
        public string JobComments { get; set; }
        public string StatusCode { get; set; } 
        public int? CouponId { get; set; }
        public bool RemoveFromViewFlag { get; set; }



    }
}
