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

        private string marketStatus = "Synchronizing";

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

        private void MarketUpdate(object sender)
        {
            _realTimeStockService.NotifyAboutMarketStatus(marketStatus);
        }

        private void NewsUpdate(object sender)
        {
            PullNews();
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Init();
            var marketUpdate = new Timer(MarketUpdate, null, 0, 1000);
            var priceUpdate = new Timer(PriceUpdate, null, 0, 1000);
            var newsUpdate = new Timer(NewsUpdate, null, 0, 1000*60*60);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_realTimeStockService.IsMarketOpen())
                {
                    marketStatus = "Open";
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
                            _realTimeStockService.Ready = true;
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
                            marketStatus = "Closing";
                            _logger.LogDebug("Waiting for market close prices");
                            var lastUpdateDate = UpdateWithDatabaseEnd();

                            while ((lastUpdateDate.Hour * 100 + lastUpdateDate.Minute) < _realTimeStockService.marketCloseTime)
                            {
                                await Task.Delay(5_000);
                                lastUpdateDate = UpdateWithDatabaseEnd();
                            }
                            _realTimeStockService.Send();
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
                    marketStatus = "Closed";
                    var leftToOpening = _realTimeStockService.GetTimeToOpenMarket();
                    _logger.LogDebug("Market opening in: " + leftToOpening.ToString());
                    await Task.Delay(leftToOpening);

                    marketStatus = "Opening";
                    _logger.LogDebug("Waiting for market open prices");
                    var lastUpdateDate = UpdateWithDatabaseOpen();
                    while ((lastUpdateDate.Hour * 100 + lastUpdateDate.Minute) < _realTimeStockService.marketOpenTime)
                    {
                        await Task.Delay(5_000);
                        lastUpdateDate = UpdateWithDatabaseOpen();
                    }
                    PullHistoricalPrices();
                    PullSplits();
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
            PullSplits();
            PullHistoricalPrices();
            using (var scope = _serviceProvider.CreateScope())
            {
                var _service = scope.ServiceProvider.GetRequiredService<IStockService>();
                var stocks = _service.GetStockDailyCurrentAll().Select(s => {
                    return new
                    {
                        Stock = new TradeStock()
                        {
                            Low = s.Low,
                            High = s.High,
                            Symbol = s.Symbol,
                            Price = s.Price,
                            Date = DateTime.SpecifyKind(s.Date, DateTimeKind.Utc).ToLocalTime(),

                        },
                        Id = s.StockId
                    };
                });
                foreach(var stock in stocks) _realTimeStockService.Add(stock.Id, stock.Stock);
                _realTimeStockService.Send();
            }
        }

        private async void PullNews()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _companyService = scope.ServiceProvider.GetRequiredService<ICompanyService>();
                string[] companySymbols = _companyService.GetAll().Select(s => s.Symbol).ToArray();
                var _newsService = scope.ServiceProvider.GetRequiredService<INewsService>();
                await _newsService.UpdateWithDataProvider(companySymbols);
            }
        }

        private async void PullSplits()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _companyService = scope.ServiceProvider.GetRequiredService<ICompanyService>();
                string[] companySymbols = _companyService.GetAll().Select(s => s.Symbol).ToArray();
                var _splitService = scope.ServiceProvider.GetRequiredService<ISplitService>();
                await _splitService.Split(companySymbols);
            }
        }

        private async void PullHistoricalPrices()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _companyService = scope.ServiceProvider.GetRequiredService<ICompanyService>();
                var companies = _companyService.GetAll().ToArray();
                var _stockService = scope.ServiceProvider.GetRequiredService<IStockService>();
                foreach(var company in companies)
                {
                    await _stockService.UpdatePrices(company.StockId, company.Symbol);
                }
            }
        }

        private DateTime UpdateWithDatabaseOpen()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _service = scope.ServiceProvider.GetRequiredService<IStockService>();
                var stocks = _service.GetStockDailyCurrentAll().Select(s => new TradeStock()
                {
                    Low = s.Low,
                    High = s.High,
                    Symbol = s.Symbol,
                    Price = s.Price,
                    Date = DateTime.SpecifyKind(s.Date, DateTimeKind.Utc).ToLocalTime(),
                });
                foreach (var stock in stocks) _realTimeStockService.UpdateOpen(stock);
            }
            _realTimeStockService.Send();
            return _realTimeStockService.LastUpdate();
        }

        private DateTime UpdateWithDatabaseEnd()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _service = scope.ServiceProvider.GetRequiredService<IStockService>();
                var stocks = _service.GetStockDailyCurrentAll().Select(s => new TradeStock()
                {
                    Low = s.Low,
                    High = s.High,
                    Symbol = s.Symbol,
                    Price = s.Price,
                    Date = DateTime.SpecifyKind(s.Date, DateTimeKind.Utc).ToLocalTime(),
                });
                foreach (var stock in stocks) _realTimeStockService.UpdateOpen(stock);
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
