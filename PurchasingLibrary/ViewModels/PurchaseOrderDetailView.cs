using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.ViewModels
{
    public class PurchaseOrderDetailView
    {
        public int PurchaseOrderDetailID;
        public int PartID;
        public string VendoerPartNumber;
        public string Description;
        public int QOH;
        public int ROL;
        public int QOO;
        public int QTO;
        public decimal PurchasePrice;
        public bool RemoveFromViewFlag;
    }
}
