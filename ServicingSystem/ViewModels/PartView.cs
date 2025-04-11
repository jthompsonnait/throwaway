using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicingSystem.ViewModels
{
    public class PartView
    {
        public int PartID { get; set; }
        public int CategoryID { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int QOH { get; set; }
        public int QOO { get; set; }

        public int Quantity = 0;


    }
}
