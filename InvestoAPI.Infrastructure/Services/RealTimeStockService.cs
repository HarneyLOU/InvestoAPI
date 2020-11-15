using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services
{
    public class RealTimeStockService
    {
        private readonly ILogger<RealTimeStockService> _logger;
        private readonly IHubContext<StockHub> _hub;

        public IList<StockTrade> currentStockPrices { get; set; }

        public RealTimeStockService(
            ILogger<RealTimeStockService> logger,
            IHubContext<StockHub> hub)
        {
            _logger = logger;
            _hub = hub;
            currentStockPrices = new List<StockTrade>();
            
        }

        public void Send()
        {
            _hub.Clients.All.SendAsync("stockupdate", currentStockPrices);
        }

        public DateTime LastUpdate()
        {
            return currentStockPrices.Max(c => c.Date);
        }

        public void Update(StockTrade stock)
        {
            var toUpdate = currentStockPrices.FirstOrDefault(s => s.Symbol == stock.Symbol);
            if (toUpdate == null) currentStockPrices.Add(stock);
            else
            {
                var stockDate = stock.Date;
                if(toUpdate.Date < stockDate)
                {
                    toUpdate.Price = stock.Price;
                    toUpdate.Date = stockDate;
                }
            }
        }

    }
}
