using InvestoAPI.Core.Interfaces;
using InvestoAPI.Infrastructure.HostedServices;
using InvestoAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace InvestoAPI.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebsocketController : ControllerBase
    {
        private readonly WebsocketService _websocketService;
        private readonly ICompanyService _companyService;
        private readonly IStockService _stockService;

        public WebsocketController(WebsocketService websocketService, 
            ICompanyService companyService,
            IStockService stockService)
        {
            _websocketService = websocketService;
            _companyService = companyService;
            _stockService = stockService;
        }

        [HttpGet("status")]
        public ActionResult Status()
        {
            return Ok(_websocketService.WebSocket.State.ToString());
        }

        [HttpGet("connect")]
        async public Task<ActionResult> Connect()
        {
            await _websocketService.Connect(new CancellationToken());
            return Ok(_websocketService.WebSocket.State.ToString());
        }

        [HttpGet("disconnect")]
        async public Task<ActionResult> Disconnect()
        {
            await _websocketService.Disconnect("Closure requested from controller", new CancellationToken());
            return Ok(String.Format("{0}\n{1}",_websocketService.WebSocket.CloseStatus.ToString(), _websocketService.WebSocket.CloseStatusDescription));
        }

        [HttpGet("send")]
        async public Task<ActionResult> Send()
        {
            string[] symbols = _companyService.GetDowJones().Select(c => c.Symbol).ToArray();
            foreach (var symbol in symbols)
            {
                var data = JsonSerializer.Serialize(new { type = "subscribe", symbol });
                await _websocketService.Send(data, new CancellationToken());
            }
            //var data3 = JsonSerializer.Serialize(new { type = "subscribe", symbol = "BINANCE:BTCUSDT" });
            //await _websocketService.Send(data3, new CancellationToken());
            return Ok("Sent");
        }
    }
}
