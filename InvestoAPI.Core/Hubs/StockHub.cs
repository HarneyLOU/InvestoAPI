using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace InvestoAPI.Core.Hubs
{
    public class StockHub : Hub
    {

        private readonly RealTimeStockService _realTimeStockService;

        public StockHub(RealTimeStockService realTimeStockService)
        {
            _realTimeStockService = realTimeStockService;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("stockupdate", _realTimeStockService.getCurrentStocks());
            await base.OnConnectedAsync();
        }
    }
}
