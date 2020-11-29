using InvestoAPI.Core.Helpers;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Core.Models;
using InvestoAPI.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace InvestoAPI.Core.HostedServices
{
    public class StockHostedService : BackgroundService
    {
        private readonly ILogger<StockHostedService> _logger;
        private readonly RealTimeStockService _realTimeStockService;
        private readonly WebsocketService _websocketService;
        private readonly IServiceProvider _serviceProvider;

        public StockHostedService(
            ILogger<StockHostedService> logger,
            RealTimeStockService realTimeStockService,
            WebsocketService websocketService,
            IServiceProvider serviceProvider
            )
        {
            _logger = logger;
            _realTimeStockService = realTimeStockService;
            _websocketService = websocketService;
            _serviceProvider = serviceProvider;
        }

        private void PriceUpdate(object sender)
        {
            if (_realTimeStockService.IsMarketOpen()) _realTimeStockService.Send();
        }

        //private void WebsocketCheckIfFailed(object sender)
        //{
        //    if (websocketFailure && _websocketService.IsOpen())
        //    {
        //        _websocketService.Disconnect("Websocket doesn't provide data", CancellationToken.None);
        //        _logger.LogDebug($"Websocket disconnected due to no respond");
        //    }
        //}

        //private void WebsocketSetFailure(object sender)
        //{
        //    websocketFailure = true;
        //}

        private bool firstRun = true;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Init();
            var timer1 = new Timer(PriceUpdate, null, 0, 1000);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_realTimeStockService.IsMarketOpen())
                {
                    try
                    {
                        if (!_websocketService.IsConnecting() && !_websocketService.IsOpen())
                        {
                            _websocketService.Connect(stoppingToken);
                        }
                        if (_websocketService.IsConnecting())
                        {
                            _logger.LogDebug($"Connecting to websocket...");
                        }
                        if (_websocketService.IsOpen())
                        {
                            _logger.LogDebug("Websocket connected");
                            SubscribeStocks(true);
                            var buffer = new ArraySegment<byte>(new byte[2096]);
                            while (_realTimeStockService.IsMarketOpen())
                            {
                                UpdateWithWebsocket(await _websocketService.Receive(buffer, stoppingToken));
                            }
                            await SubscribeStocks(false);
                            await _websocketService.Disconnect("Market Closed", stoppingToken);
                            _logger.LogDebug("Websocket disconnected");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"ERROR - {ex.Message}");
                    }
                    await Task.Delay(10_000);
                }
                else
                {
                    _realTimeStockService.Ready = false;
                    _logger.LogDebug("Waiting for market close prices");
                    var lastUpdateDate = UpdateWithDatabase().ToLocalTime();
                    if (!firstRun)
                    {
                        while ((lastUpdateDate.Hour * 100 + lastUpdateDate.Minute) < _realTimeStockService.marketCloseTime)
                        {
                            await Task.Delay(5_000);
                            lastUpdateDate = UpdateWithDatabase().ToLocalTime();
                        }
                    }
                    firstRun = false;
                    var leftToOpening = _realTimeStockService.GetTimeToOpenMarket();
                    _logger.LogDebug("Market opening in: " + leftToOpening.ToString());
                    await Task.Delay(leftToOpening);

                    _logger.LogDebug("Waiting for market open prices");
                    lastUpdateDate = UpdateWithDatabase().ToLocalTime();
                    while ((lastUpdateDate.Hour * 100 + lastUpdateDate.Minute) < _realTimeStockService.marketOpenTime)
                    {
                        await Task.Delay(5_000);
                        lastUpdateDate = UpdateWithDatabase().ToLocalTime();
                    }
                    _realTimeStockService.Ready = true;
                }
            }
        }


        private void UpdateWithWebsocket(string data)
        {
            var stockTrade = WebsocketDataParser.Parse(data, WebsocketDataType.Tiingo);
            if(stockTrade != null) _realTimeStockService.Update(stockTrade);
        }

        private void Init()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _service = scope.ServiceProvider.GetRequiredService<IStockService>();
                var stocks = _service.GetStockDailyCurrentAll().Select(s => {
                    return new
                    {
                        Stock = new TradeStock()
                        {
                            Symbol = s.Symbol,
                            Price = s.Price,
                            Date = DateTime.SpecifyKind(s.Date, DateTimeKind.Utc),
                        },
                        Id = s.StockId
                    };
                });
                foreach(var stock in stocks) _realTimeStockService.Add(stock.Id, stock.Stock);
                _realTimeStockService.Send();
            }
        }

        private DateTime UpdateWithDatabase()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _service = scope.ServiceProvider.GetRequiredService<IStockService>();
                var stocks = _service.GetStockDailyCurrentAll().Select(s => new TradeStock()
                {
                    Symbol = s.Symbol,
                    Price = s.Price,
                    Date = DateTime.SpecifyKind(s.Date, DateTimeKind.Utc),
                });
                foreach (var stock in stocks) _realTimeStockService.Update(stock);
            }
            _realTimeStockService.Send();
            return _realTimeStockService.LastUpdate();
        }

        public async Task SubscribeStocks(bool subscribe)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _service = scope.ServiceProvider.GetRequiredService<ICompanyService>();
                string[] symbols = _service.GetDowJones().Select(c => c.Symbol).ToArray();
                string eventName = subscribe ? "subscribe" : "unsubscribe";
                var data = JsonSerializer.Serialize(new
                {
                    eventName = eventName,
                    authorization = "cd31bfa77579409902970a90850ac542c7670152",
                    eventData = new {
                        thresholdLevel = 5,
                        tickers = symbols
                    }
                });
                await _websocketService.Send(data, new CancellationToken());
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            if(_websocketService.IsOpen())
            {
                await SubscribeStocks(false);
                await _websocketService.Disconnect("Server shut down", stoppingToken);
            }
            _logger.LogInformation(
                "Stock Hosted Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}
