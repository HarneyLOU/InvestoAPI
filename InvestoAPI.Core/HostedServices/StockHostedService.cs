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

        //GMT+1 Time Zone
        private int marketOpenTime = 1530;
        private int marketCloseTime = 2200;

        private bool updated = false;
        private bool websocketFailure = false;

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
            if(IsMarketOpen()) _realTimeStockService.Send();
        }

        private void WebsocketCheckIfFailed(object sender)
        {
            if (websocketFailure && _websocketService.IsOpen())
            {
                _websocketService.Disconnect("Websocket doesn't provide data", CancellationToken.None);
                _logger.LogDebug($"Websocket disconnected due to no respond");
            }
        }

        private void WebsocketSetFailure(object sender)
        {
            websocketFailure = true;
        }

        //Przerobić na dwa niezależne serwisy - spaghetti alert
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Init();
            _realTimeStockService.Send();
            var timer1 = new Timer(PriceUpdate, null, 0, 1000);
            //var timer2 = new Timer(WebsocketCheckIfFailed, null, 120_000, 120_000);
            //var timer3 = new Timer(WebsocketSetFailure, null, 0, 12_000);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (IsMarketOpen())
                {
                    updated = false;
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
                            while (IsMarketOpen())
                            {
                                UpdateWithWebsocket(await _websocketService.Receive(buffer, stoppingToken));
                            }
                            await SubscribeStocks(false);
                            await _websocketService.Disconnect("Market Closed", stoppingToken);
                            _logger.LogDebug("Websocket disconnected");
                            DateTime lastUpdateTime = UpdateWithDatabase().ToLocalTime();
                            while ((lastUpdateTime.Hour * 100 + lastUpdateTime.Minute) < marketCloseTime)
                            {
                                _logger.LogDebug($"Waiting for closed data..");
                                lastUpdateTime = UpdateWithDatabase().ToLocalTime();
                                await Task.Delay(10_000);
                            }
                            _realTimeStockService.Send();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"ERROR - {ex.Message}");
                    }
                    await Task.Delay(1_000);
                    //UpdateWithDatabase();
                    _realTimeStockService.Send();
                }
                else
                {
                    UpdateWithDatabase();
                    _realTimeStockService.Send();
                    var leftToOpening = GetTimeToOpenMarket();
                    while (leftToOpening.TotalMilliseconds > 0)
                    {
                        Console.WriteLine("Market opening in: " + leftToOpening.ToString(@"hh\:mm\:ss"));
                        await Task.Delay(10_000);
                        leftToOpening = GetTimeToOpenMarket();
                    }
                    updated = true;
                }
            }
        }

        private bool IsMarketOpen()
        {
            DateTime now = DateTime.Now;
            return (((now.Hour * 100 + now.Minute) < marketCloseTime) && ((now.Hour * 100 + now.Minute) >= marketOpenTime) 
                && !(now.DayOfWeek == DayOfWeek.Saturday) 
                && !(now.DayOfWeek == DayOfWeek.Sunday));
        }

        private TimeSpan GetTimeToOpenMarket()
        {
            DateTime now = DateTime.Now;
            if (now.DayOfWeek == DayOfWeek.Saturday) now = now.AddDays(2);
            else if (now.DayOfWeek == DayOfWeek.Sunday) now = now.AddDays(1);
            else if ((now.Hour * 100 + now.Minute) > marketCloseTime) now = now.AddDays(1);
            return new DateTime(now.Year, now.Month, now.Day, marketOpenTime/100, marketOpenTime%100, 00) - DateTime.Now;
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
