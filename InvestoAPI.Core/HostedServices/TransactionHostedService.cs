using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InvestoAPI.Core.HostedServices
{
    public class TransactionHostedService : BackgroundService
    {
        private readonly OrderQueueService _orderQueueService;
        private readonly ILogger<TransactionHostedService> _logger;
        private readonly RealTimeStockService _realTimeStockService;
        private readonly IServiceProvider _serviceProvider;

        public TransactionHostedService(
            OrderQueueService orderQueueService,
            ILogger<TransactionHostedService> logger,
            RealTimeStockService realTimeStockService,
            IServiceProvider serviceProvider
            )
        {
            _orderQueueService = orderQueueService;
            _logger = logger;
            _realTimeStockService = realTimeStockService;
            _serviceProvider = serviceProvider;
        }

        private void Init()
        {
            _orderQueueService.AddOrder(new Order
            {
                OrderId = 1,
                WalletId = 1,
                CompanyId = 1,
                Amount = 10,
            });
            _orderQueueService.AddOrder(new Order
            {
                OrderId = 2,
                WalletId = 1,
                CompanyId = 2,
                Amount = 7,
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Init();
            _logger.LogDebug("Transactions begin");
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_orderQueueService.IsEmpty())
                {
                    Order order = _orderQueueService.GetOrder();
                    var price = _realTimeStockService.GetPrice(order.CompanyId);
                    _logger.LogDebug($"Order {order.OrderId} realised for { order.Amount * price }");
                    var transaction = new Transaction
                    {
                        Order = order,
                        Amount = order.Amount,
                        Price = order.Amount * price,
                        Realised = DateTime.Now
                    };
                    order.Active = false;
                    //_transactionService.Add(transaction);
                    //_orderService.Update(order);
                    _orderQueueService.FinishOrder();
                }
               await Task.Delay(1_000);
            }
        }
    }
}
