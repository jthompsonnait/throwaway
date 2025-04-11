using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicingSystem.ViewModels
{
    public class StandardJobView
    {
        public int StandardJobID { get; set; }
        public string Description { get; set; }
        public decimal StandardHours { get; set; }
    }
}
