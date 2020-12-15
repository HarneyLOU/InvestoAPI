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
        public ActionResult GetHistory(int stockId, string from, string to, string interval)
        {
            return Ok(_stockService.GetStockHistory(stockId, from, to, interval));
        }

        [HttpGet("last")]
        public ActionResult GetLast(int stockId, string period)
        {
            return Ok(_stockService.GetStockDaily(stockId, period));
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
                        Low = _realTimeStockService.GetLow(stock.StockId),
                        High = _realTimeStockService.GetHigh(stock.StockId),
                        Change = decimal.Round(((_realTimeStockService.GetPrice(stock.StockId) - stock.Close) / stock.Close), 4, MidpointRounding.AwayFromZero),
                        Date = _realTimeStockService.GetDate(stock.StockId)
                    }
                );
            return Ok(stocksShort);
        }
    }
}
