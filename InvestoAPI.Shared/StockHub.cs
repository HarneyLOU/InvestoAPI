using InvestoAPI.Core.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace InvestoAPI.Shared
{
    public class StockHub : Hub
    {

        public StockHub()
        {
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
