using InvestoAPI.Core.Interfaces;
using InvestoAPI.Core.Services;
using InvestoAPI.Infrastructure;
using InvestoAPI.Web.ViewModels;
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
        private readonly ICompanyService _companyService;
        private readonly RealTimeStockService _realTimeStockService;

        public StockController(IStockService stockService, ICompanyService companyService, RealTimeStockService realTimeStockService)
        {
            _stockService = stockService;
            _companyService = companyService;
            _realTimeStockService = realTimeStockService;
        }

        [HttpGet("history")]
        public ActionResult GetHistory(string symbol, string from, string to, string interval = "d")
        {
            return Ok(_stockService.GetStockHistory(symbol, from, to, interval));
        }

        [HttpGet("short")]
        public ActionResult GetStockShortAll()
        {
            var companies = _companyService.GetAll().ToList();
            var stocksShort = _stockService.GetStockDailyCurrentAll().Join(companies,
                stock => stock.StockId,
                company => company.StockId,
                (stock, company) =>
                    new StockShortViewModel()
                    {
                        StockId = stock.StockId,
                        Symbol = stock.Symbol,
                        Name = company.Name,
                        Image = company.Image,
                        Price = _realTimeStockService.GetPrice(stock.StockId),
                        Open = stock.Open,
                        PrevClose = stock.Close,
                        Low = stock.Low,
                        High = stock.High,
                        Change = decimal.Round(((_realTimeStockService.GetPrice(stock.StockId) - stock.Close) / stock.Close), 4, MidpointRounding.AwayFromZero),
                        Date = DateTime.SpecifyKind(_realTimeStockService.GetDate(stock.StockId), DateTimeKind.Utc)
                    }
                );
            return Ok(stocksShort);
        }
    }
}
