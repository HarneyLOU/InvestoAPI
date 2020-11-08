using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvestoAPI.Infrastructure.Services
{
    public class WebsocketStocksService : IWebsocketStocksService
    {
        public IList<StockTrade> websocketStocks { get; set; }
        public ILogger<WebsocketStocksService> _logger { get; set; }

        public WebsocketStocksService(ILogger<WebsocketStocksService> logger)
        {
            _logger = logger;
            websocketStocks = new List<StockTrade>();
        }

        public void Update(StockTrade stock)
        {
            var toUpdate = websocketStocks.FirstOrDefault(s => s.Symbol == stock.Symbol);
            if (toUpdate == null) websocketStocks.Add(stock);
            else toUpdate = stock;
            //SIGNALR
            _logger.LogInformation($"Symbol: {stock.Symbol} - Price: {stock.Price}");
        }
    }
}
