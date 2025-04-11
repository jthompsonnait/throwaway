using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBike2025Context.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServicingSystem.BLL;

namespace ServicingSystem
{
    public static class ServicingExtension
    {
        public static void ServicingDependencies(this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<ServicingSystemContext>(options);

            //GetCustomer
            services.AddTransient<CustomerService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<ServicingSystemContext>();

                return new CustomerService(context);
            });

            //GEtJOb
            services.AddTransient<JobsService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<ServicingSystemContext>();

                return new JobsService(context);
            });
        }
    }
}
