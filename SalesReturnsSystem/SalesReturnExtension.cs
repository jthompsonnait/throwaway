using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SalesReturnsSystem.BLL;
using SalesReturnsSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SalesReturnsSystem
{
    public static class SalesReturnExtension
    {
        public static void SalesReturnDependencies(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<SalesReturnsContext>(options);

            services.AddTransient<ReturnsService>((serviceProvider) =>
            {
                var context = serviceProvider.GetService<SalesReturnsContext>();
                return new ReturnsService(context);
            });

            services.AddTransient<LookupService>((serviceProvider) =>
            {
                var context = serviceProvider.GetService<SalesReturnsContext>();
                return new LookupService(context);
            });

            services.AddTransient<SalesService>((serviceProvider) =>
            {
                var context = serviceProvider.GetService<SalesReturnsContext>();
                return new SalesService(context);
            });

        }
    }
}