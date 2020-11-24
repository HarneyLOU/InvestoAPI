using InvestoAPI.Core.HostedServices;
using InvestoAPI.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvestoAPI.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration config)
        {
            services.AddSignalR();
            services.AddSingleton<RealTimeStockService>();
            services.AddSingleton<OrderQueueService>();
            services.AddSingleton<WebsocketService>();
            services.AddHostedService<StockHostedService>();
            services.AddHostedService<TransactionHostedService>();
            return services;
        }
    }
}
