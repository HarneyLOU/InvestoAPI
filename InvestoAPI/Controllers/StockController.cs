using InvestoAPI.Core.Interfaces;
using InvestoAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvestoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        public ActionResult Get(string symbol, string from, string to, string interval = "d")
        {
            return Ok(_stockService.GetStockHistory(symbol, from, to, interval));
        }
    }
}
