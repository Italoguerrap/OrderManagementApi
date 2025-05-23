using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Infrastructure.Context;
using System;

namespace OrderManagementApi.PipelineExtensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connString = configuration["ConnectionStrings:database"];
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connString));
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();

            return services;
        }
    }
}
