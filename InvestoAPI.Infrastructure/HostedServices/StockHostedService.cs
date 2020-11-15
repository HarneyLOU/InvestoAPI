using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Infrastructure.Services;
using InvestoAPI.Shared;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.HostedServices
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new Timer(PriceUpdate, null, 0, 1000);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (IsMarketOpen())
                {
                    try
                    {
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
                        if (_websocketService.IsConnecting())
                        {
                            _logger.LogDebug($"Connecting to websocket...");
                        }
                        else if(!_websocketService.IsConnecting())
                        {
                            _websocketService.Connect(stoppingToken);
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
                    var leftToOpening = GetTimeToOpenMarket();
                    while (leftToOpening.TotalMilliseconds > 0)
                    {
                        Console.WriteLine("Market opening in: " + leftToOpening.ToString(@"hh\:mm\:ss"));
                        await Task.Delay(10_000);
                        leftToOpening = GetTimeToOpenMarket();
                    }
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
            if (!String.IsNullOrEmpty(data))
            {
                JsonDocument document = JsonDocument.Parse(data);
                if (document.RootElement.TryGetProperty("data", out JsonElement stockJson))
                {
                    foreach (var s in stockJson.EnumerateArray())
                    {
                        var stock = JsonSerializer.Deserialize<Stock>(s.GetRawText());
                        _realTimeStockService.Update(Stock.ToStockTrade(stock));
                    }
                }
            }
        }

        private DateTime UpdateWithDatabase()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _service = scope.ServiceProvider.GetRequiredService<IStockService>();
                var stocks = _service.GetStockDailyCurrentAll().Select(s => new StockTrade()
                {
                    Symbol = s.Symbol,
                    Price = s.Price,
                    Date = s.Date,
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
                foreach (var symbol in symbols)
                {
                    string type = "";
                    if (subscribe) type = "subscribe";
                    else type = "unsubscribe";
                    var data = JsonSerializer.Serialize(new { type, symbol });
                    await _websocketService.Send(data, new CancellationToken());

                }
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

    class Stock
    {
        [JsonPropertyName("s")]
        public string Symbol { get; set; }
        [JsonPropertyName("p")]
        public decimal Price { get; set; }
        [JsonPropertyName("t")]
        public long Time { get; set; }
        [JsonPropertyName("v")]
        public decimal Volume { get; set; }

        static public StockTrade ToStockTrade(Stock stock)
        {
            return new StockTrade
            {
                Symbol = stock.Symbol,
                Price = stock.Price,
                Date = UnixTimeStampToDateTime(stock.Time),
            };
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp);
            return dtDateTime;
        }
    }
}
