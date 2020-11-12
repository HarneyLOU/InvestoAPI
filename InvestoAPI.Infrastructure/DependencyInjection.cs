using AutoMapper;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Infrastructure.HostedServices;
using InvestoAPI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;


namespace InvestoAPI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddDbContext<ApplicationContext>(opts =>
                opts.UseSqlServer(config.GetConnectionString("sqlConnection")));
            //services.AddHttpClient<IStockService, StockService>(c =>
            //{
            //    c.Timeout = TimeSpan.FromMilliseconds(10000);
            //});
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddSingleton<RealTimeStockService>();
            services.AddSingleton<WebsocketService>();
            services.AddHostedService<StockHostedService>();
            return services;
        }
    }
}
