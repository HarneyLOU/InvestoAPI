using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Core.Services;
using Microsoft.Extensions.DependencyInjection;
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
        private IOrderService orderService;
        private IWalletStateService walletStateService;
        private ITransactionService transactionService;

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
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
            walletStateService = scope.ServiceProvider.GetRequiredService<IWalletStateService>();
            //Init();

            _logger.LogDebug("Transactions begin");
            while (!stoppingToken.IsCancellationRequested)
            {
                if(true)//_realTimeStockService.IsMarketOpen())
                {
                    if(true)//_realTimeStockService.Ready)
                    {
                        _orderQueueService.AddOrders(orderService.GetActiveOrders());
                        while (!_orderQueueService.IsEmpty())
                        {
                            Order order = _orderQueueService.GetOrder();
                            try
                            {
                                var price = _realTimeStockService.GetPrice(order.StockId);
                                _logger.LogDebug($"Order {order.OrderId} realised for { order.Amount * price }");
                                var transaction = new Transaction
                                {
                                    Order = order,
                                    Amount = order.Amount,
                                    Price = order.Amount * price,
                                    Realised = DateTime.Now
                                };
                                var newOrder = walletStateService.UpsertWalletState(transaction);
                                orderService.UpdateOrder(newOrder);
                                if(newOrder.Status == "Success") transactionService.AddTransaction(transaction);
                                _orderQueueService.FinishOrder();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"ERROR - {ex.Message}");
                            }
                        }
                        await Task.Delay(10_000);
                    } 
                    else
                    {
                        await Task.Delay(10_000);
                    }
                } 
                else
                {
                    _orderQueueService.CleanQueue();
                    var leftToOpening = _realTimeStockService.GetTimeToOpenMarket();
                    await Task.Delay(leftToOpening);
                }
            }
        }
    }
}
