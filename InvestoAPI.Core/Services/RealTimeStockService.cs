using InvestoAPI.Core.Hubs;
using InvestoAPI.Core.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvestoAPI.Core.Services
{
    public class RealTimeStockService
    {
        private readonly ILogger<RealTimeStockService> _logger;
        private readonly IHubContext<StockHub> _hub;

        private Dictionary<int, TradeStock> currentStockPrices;

        public RealTimeStockService(
            ILogger<RealTimeStockService> logger,
            IHubContext<StockHub> hub)
        {
            _logger = logger;
            _hub = hub;
            currentStockPrices = new Dictionary<int, TradeStock>();
        }

        public void Send()
        {
            _hub.Clients.All.SendAsync("stockupdate", currentStockPrices.Values.ToList());
        }
        
        public IEnumerable<TradeStock> getCurrentStocks()
        {
            return currentStockPrices.Values.ToList();
        }

        public DateTime LastUpdate()
        {
            return currentStockPrices.Values.Max(c => c.Date);
        }

        public void Add(int id, TradeStock stock)
        {
            currentStockPrices.Add(id, stock);
        }

        public void Update(TradeStock stock)
        {
            var toUpdate = currentStockPrices.Values.FirstOrDefault(s => s.Symbol == stock.Symbol);
                var stockDate = stock.Date;
                if(toUpdate.Date < stockDate)
                {
                    toUpdate.Price = stock.Price;
                    toUpdate.Date = stockDate;
                }
        }

        public decimal GetPrice(int id)
        {
            return currentStockPrices[id].Price;
        }

    }
}
