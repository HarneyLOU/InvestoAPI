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

        private int marketOpenHour = 15;
        private int marketOpenMinute = 30;
        private int marketCloseHour = 22;
        private int marketCloseMinute = 00;


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
                            _logger.LogDebug("Websocket is working");
                            var buffer = new ArraySegment<byte>(new byte[2096]);
                            while (!stoppingToken.IsCancellationRequested)
                            {
                                UpdateWithWebsocket(await _websocketService.Receive(buffer, stoppingToken));
                            }
                        }
                        if (_websocketService.autoConnect && !_websocketService.IsOpen() && _websocketService.WebSocket.State != WebSocketState.Connecting) await _websocketService.Reconnect(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"ERROR - {ex.Message}");
                    }
                    await Task.Delay(10_000);
                }
                else
                {
                    UpdateWithDatabase();
                    _realTimeStockService.Send();
                    await Task.Delay((int)GetTimeToOpenMarket().TotalMilliseconds);
                }
            }
        }

        private bool IsMarketOpen()
        {
            return (!(DateTime.Now.Hour >= marketCloseHour && DateTime.Now.Minute >= marketCloseMinute) ||
                (DateTime.Now.Hour <= marketOpenHour && DateTime.Now.Minute <= marketOpenMinute));
        }

        private TimeSpan GetTimeToOpenMarket()
        {
            DateTime now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day + 1, marketOpenHour, marketOpenMinute, 00) - DateTime.Now;
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

        private void UpdateWithDatabase()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _logger.LogDebug("Waiting for open market");
                var _stockService = scope.ServiceProvider.GetRequiredService<IStockService>();
                var stocks = _stockService.GetStockDailyCurrentAll().Select(s => new StockTrade()
                {
                    Symbol = s.Symbol,
                    Price = s.Price,
                    Date = s.Date
                });
                foreach (var stock in stocks) _realTimeStockService.Update(stock);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
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
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
