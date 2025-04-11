using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem.ViewModels
{
    
    
        public class PurchaseOrderView
        {
            public int PO { get; set; }
            public DateTime? Date { get; set; }
            public string Vendor { get; set; }
            public string Contact { get; set; }

        }
    
}
