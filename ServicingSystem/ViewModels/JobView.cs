using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicingSystem.ViewModels
{
    public class JobView
    {
        public int JobID { get; set; }
        public DateTime JobDateStarted { get; set; }
        public string EmployeeID { get; set; }
        public bool RemoveFromViewFlag { get; set; }
        public List<JobDetailView> JobDetails { get; set; }
        public List<JobPartView> JobParts { get; set; }
    }
}
