using AutoMapper;
using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Infrastructure;
using InvestoAPI.Infrastructure.Services.StockProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services
{

    public class StockService : IStockService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ApplicationContext _context;
        static readonly CancellationTokenSource s_cts = new CancellationTokenSource();

        public StockService(
            ILogger<StockService> logger,
            IMapper mapper,
            ApplicationContext context)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
        }

        public DateTime GetMaxDate(string symbol)
        {
            return _context.Stocks.Max(s => s.Date);
        }

        public Stock GetStock(string symbol, DateTime? date = null)
        {
            int companyId = _context.Companies.Where(s => s.Symbol == symbol).Select(x => x.StockId).FirstOrDefault();
            return _context.Stocks.Where(s => s.Date == date && s.StockId == companyId).FirstOrDefault();
        }

        public IEnumerable<StockDaily> GetStockDailyCurrentAll()
        {
            DateTime lastDate = _context.StocksDaily.Max(s => s.Date);
            return _context.StocksDaily.Where(s => s.Date == lastDate).Distinct();
        }

        public IEnumerable<Stock> GetStockHistory(string symbol, string from, string to, string interval = "d")
        {
            DateTime fromDate;
            if (!DateTime.TryParse(from, out fromDate)) fromDate = DateTime.Parse("2010-01-01");
            DateTime toDate;
            if (!DateTime.TryParse(to, out toDate)) toDate = DateTime.Now;

            int companyId = _context.Companies.Where(s => s.Symbol == symbol).Select(x => x.StockId).FirstOrDefault();
            try
            {
                return _context.Stocks.Where(
                    s => s.StockId == companyId &&
                    s.Date >= fromDate &&
                    s.Date <= toDate).Take(1000); ;
            }
            catch(Exception ex)
            {
                _logger.LogError($"ERROR - {ex.Message}");
                return null;
            }

        }
    }
}
