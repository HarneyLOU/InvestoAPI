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

        public int marketOpenTime { get; } = 1530;
        public int marketCloseTime { get; } = 2300;

        public bool Ready { get; set; } = false;

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
        
        public IList<TradeStock> getCurrentStocks()
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

        public DateTime GetDate(int id)
        {
            return currentStockPrices[id].Date;
        }

        public bool IsMarketOpen()
        {
            DateTime now = DateTime.Now;
            return (((now.Hour * 100 + now.Minute) < marketCloseTime) && ((now.Hour * 100 + now.Minute) >= marketOpenTime)
                && !(now.DayOfWeek == DayOfWeek.Saturday)
                && !(now.DayOfWeek == DayOfWeek.Sunday));
        }

        public TimeSpan GetTimeToOpenMarket()
        {
            DateTime now = DateTime.Now;
            if (now.DayOfWeek == DayOfWeek.Saturday) now = now.AddDays(2);
            else if (now.DayOfWeek == DayOfWeek.Sunday) now = now.AddDays(1);
            else if ((now.Hour * 100 + now.Minute) > marketCloseTime) now = now.AddDays(1);
            return new DateTime(now.Year, now.Month, now.Day, marketOpenTime / 100, marketOpenTime % 100, 00) - DateTime.Now;
        }

    }
}
