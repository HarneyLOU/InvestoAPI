using InvestoAPI.Core.Interfaces;
using InvestoAPI.Core.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using InvestoAPI.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace InvestoAPI.Infrastructure.HostedServices
{
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
                Time = stock.Time,
                Volume = stock.Volume
            };
        }
    }

    public class WebsocketHostedService : BackgroundService
    {
        private readonly ILogger<WebsocketHostedService> _logger;
        private readonly IWebsocketStocksService _webstock;
        private readonly IServiceScope _scope;

        private readonly ICompanyService _companyService;

        public WebsocketHostedService(
            ILogger<WebsocketHostedService> logger,
            IWebsocketStocksService webstock,
            IServiceProvider services
            )
        {
            _logger = logger;
            _webstock = webstock;
            _companyService = services.CreateScope().ServiceProvider.GetRequiredService<ICompanyService>();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Init() pull current from StockProvider
            while (!stoppingToken.IsCancellationRequested)
                using (var socket = new ClientWebSocket())
                    try
                    {
                        await socket.ConnectAsync(new Uri("wss://ws.finnhub.io?token=bpv6tnnrh5rabkt31r10"), stoppingToken);
                        _logger.LogDebug("CONNECTED");
                        string[] symbols = _companyService.GetDowJones().Select(c => c.Symbol).ToArray();
                        foreach (var symbol in symbols)
                        {
                            var data = JsonSerializer.Serialize(new { type = "subscribe", symbol });
                            await socket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, stoppingToken);
                        }
                        _logger.LogDebug("RECEIVING");
                        await Receive(socket, stoppingToken);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError
                            ($"ERROR - {ex.Message}");
                    }
        }

        private async Task Send(ClientWebSocket socket, string data, CancellationToken stoppingToken) =>
        await socket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, stoppingToken);

        private async Task Receive(ClientWebSocket socket, CancellationToken stoppingToken)
        {
            var buffer = new ArraySegment<byte>(new byte[2048]);
            while (!stoppingToken.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await socket.ReceiveAsync(buffer, stoppingToken);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    ms.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(ms, Encoding.UTF8);
                    try
                    {
                        var data = await reader.ReadToEndAsync();
                        JsonDocument document = JsonDocument.Parse(data);
                        if (document.RootElement.TryGetProperty("data", out JsonElement stockJson))
                        {
                            foreach (var s in stockJson.EnumerateArray())
                            {
                                var stock = JsonSerializer.Deserialize<Stock>(s.GetRawText());
                                _webstock.Update(Stock.ToStockTrade(stock));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex.Message);
                    }
                }
            };
        }
    }
}
