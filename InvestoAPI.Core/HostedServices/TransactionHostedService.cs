using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Enums;
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
            //Init();

            _logger.LogDebug("Transactions has started");
            while (!stoppingToken.IsCancellationRequested)
            {
                //Check if market is open
                if (_realTimeStockService.IsMarketOpen())
                {
                    //Check if opening prices are pulled
                    if (_realTimeStockService.Ready)
                    {
                        try
                        {
                            //Retrieve active orders
                            _orderQueueService.AddOrders(orderService.GetActiveOrders());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"RETRIVAL OF ORDERS ERROR - {ex.Message}");
                            await Task.Delay(10_000);
                        }
                        while (!_orderQueueService.IsEmpty())
                        {
                            try
                            {
                                //Take first order from the queue
                                Order order = _orderQueueService.GetOrder();
                                //Take current price for the stock
                                var price = _realTimeStockService.GetPrice(order.StockId);
                                //Check whether order with limit fulfill a condition
                                if (
                                   ((order.Buy && price <= order.Limit) || (!order.Buy && price >= order.Limit))
                                   || order.Limit == null || order.ExpiryDate <= DateTime.Now)
                                {
                                    Transaction transaction = new Transaction
                                    {
                                        Order = order,
                                        Amount = order.Amount,
                                        Price = order.Amount * price,
                                        Realised = DateTime.Now
                                    };
                                    //Realize an order and dequeue if it's finished or cancelled for some reasons
                                    var consumedOrder = await orderService.ConsumeTransaction(transaction);
                                    if (consumedOrder.StatusEnum != OrderStatusEnum.Partially) _orderQueueService.FinishOrder();
                                }
                                else
                                {
                                    _orderQueueService.FinishOrder();
                                }
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
                    _logger.LogDebug($"Transactions has been stopped");
                    await Task.Delay(leftToOpening);
                }
            }
        }
    }
}
