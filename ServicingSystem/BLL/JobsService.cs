using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBike2025Context.DAL;
using eBike2025Context.Entities;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using ServicingSystem.ViewModels;

namespace ServicingSystem.BLL
{
    public class JobsService
    {
        private readonly ServicingSystemContext _servicingSystemContext;
        internal JobsService(ServicingSystemContext servicingSystemContext)
        {
            _servicingSystemContext = servicingSystemContext;
        }

        public List<StandardJobView> GetStandardJobs()
        {
            return _servicingSystemContext.StandardJobs.Where(s => s.RemoveFromViewFlag == false)
                .Select(s => new StandardJobView
                {
                    StandardJobID = s.StandardJobID,
                    Description = s.Description,
                    StandardHours = s.StandardHours

                }).ToList();
        }

        public List<CategoryView> GetCategories()
        {
            return _servicingSystemContext.Categories.Where(c => c.RemoveFromViewFlag == false)
                .Select(c => new CategoryView
            {
                Name = c.Description,
                CatergoryID = c.CategoryID

            }).ToList();
        }

        public List<PartView> GetParts(int catergoryID)
        {
            return _servicingSystemContext.Parts.Where(p => p.CategoryID == catergoryID && p.RemoveFromViewFlag == false)
                .Select(p => new PartView
                {
                    PartID = p.PartID,
                    CategoryID = p.CategoryID,
                    Description = p.Description,
                    Price = p.SellingPrice,
                    QOH = p.QuantityOnHand,
                    QOO = p.QuantityOnOrder

                }).ToList();
        }

        public CouponView VerifyCoupon(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("Please enter a non empty coupon value.");
            }

            return _servicingSystemContext.Coupons.Where(c => c.CouponIDValue == value && c.RemoveFromViewFlag == false)
                .Select(c => new CouponView
                {
                    CouponId = c.CouponID,
                    CouponName = c.CouponIDValue,
                    Discount = c.CouponDiscount,
                })
                .FirstOrDefault();

        }

        public void AddJob(JobView job)
        {

            List<Exception> errorList = new List<Exception>();
     
            Job newJob = new Job();

            newJob.JobDateStarted = DateTime.Now;
            newJob.EmployeeID = job.EmployeeID;
            newJob.RemoveFromViewFlag = false;

            List<JobDetail> newJobDetailList = new List<JobDetail>();

            foreach (JobDetailView d in job.JobDetails)
            {
                JobDetail newJobDetail = new JobDetail();

                newJobDetail.JobID = d.JobID;
                newJobDetail.EmployeeID = d.EmployeeId;
                newJobDetail.Description = d.Description;
                newJobDetail.JobHours = d.JobHours;
                newJobDetail.Comments = d.JobComments;
                newJobDetail.CouponID = d.CouponId;
                newJobDetail.RemoveFromViewFlag = false;
                newJobDetailList.Add(newJobDetail);
            }

            List<JobPart> newJobPartList = new List<JobPart>();

            foreach (JobPartView jobPart in job.JobParts)
            {
                JobPart newJobPart = new JobPart();

                newJobPart.JobID = jobPart.JobID;
                newJobPart.PartID = jobPart.PartID;
                newJobPart.Quantity = jobPart.Quantity;
                newJobPart.SellingPrice = jobPart.SellingPrice;
                newJobPart.RemoveFromViewFlag = false;

                //newJobPart.Part.QuantityOnOrder += jobPart.Quantity;

                newJobPartList.Add(newJobPart);
            }

            if (errorList.Count > 0)
            {

            } else
            {
                _servicingSystemContext.Jobs.Add(newJob);

                foreach (JobDetail jobDetail in newJobDetailList)
                {
                    _servicingSystemContext.JobDetails.Add(jobDetail);
                }

                foreach (JobPart jobPart in newJobPartList)
                {
                    _servicingSystemContext.JobParts.Add(jobPart);
                }

            }
        }

    }
}
