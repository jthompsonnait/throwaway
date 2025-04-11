using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem.ViewModels
{
    public class UnorderedItemView
    {
       
        public int UnorderedItemID { get; set; }
        public string Description { get; set; }
        public string VendorPartID { get; set; }
        public int Qty { get; set; }
    }
}
