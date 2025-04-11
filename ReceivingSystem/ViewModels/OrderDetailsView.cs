using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem.ViewModels
{
    public class OrderDetailsView
    {
        public int PO { get; set; }
        public string Vendor { get; set; }
        public string Phone { get; set; }
        public int PartID { get; set; }
        public string Description { get; set; }
        public int OrderQty { get; set; }
        public int Outstanding { get; set; }
        public int Received { get; set; }
        public int Returned { get; set; }
        public string Reason { get; set; }
    }
}
