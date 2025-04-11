using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicingSystem.ViewModels
{
    public class JobPartView
    {
        public int JobID { get; set; }
        public string? Description { get; set; }
        public int PartID { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public bool RemoveFromViewFlag { get; set; }
    }
}
