using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PurchasingSystem.BLL;
using PurchasingSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem
{
    public static class PurchasingExtention
    {
        public static void PurchasingDependancies(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<eBike_2025Context>(options);

            services.AddTransient<PartService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBike_2025Context>();
                return new PartService(context);
            });
            
            services.AddTransient<PurchaceOrderService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBike_2025Context>();
                return new PurchaceOrderService(context);
            });

            services.AddTransient<VendorService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBike_2025Context>();
                return new VendorService(context);
            });
        }
    }
}
