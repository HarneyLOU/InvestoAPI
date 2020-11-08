using InvestoAPI.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.HostedServices
{
    public class RealTimeStockHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<RealTimeStockHostedService> _logger;
        private Timer _timer;
        //private readonly IStockService _stockSerivce;
        //private IHubContext<ChartHub> _hub;

        public RealTimeStockHostedService(
            ILogger<RealTimeStockHostedService> logger
            //IStockService stockSerivce
            )
        {
            _logger = logger;
            //_stockSerivce = stockSerivce;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");
            //_stockSerivce.AddCompanyProfiles();
            //_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            //_stockSerivce.DoWork();
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
