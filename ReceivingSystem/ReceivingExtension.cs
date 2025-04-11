using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReceivingSystem.BLL;
using ReceivingSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem
{
    public static class ReceivingExtension
    {
        public static void ReceivingDependencies(this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<ReceivingContext>(options);

            //GetPurchaseOrder
            services.AddTransient<PurchaseOrderService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<ReceivingContext>();

                return new PurchaseOrderService(context);
            });
            services.AddTransient<OrderDetailsService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<ReceivingContext>();

                return new OrderDetailsService(context);
            });
        }
    }
}
