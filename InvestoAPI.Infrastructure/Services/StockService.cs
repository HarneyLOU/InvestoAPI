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

        public IEnumerable<Stock> GetStockHistory(int stockId, string from, string to, string interval = "d")
        {
            DateTime fromDate;
            if (!DateTime.TryParse(from, out fromDate)) fromDate = DateTime.Now.AddYears(-5);
            DateTime toDate;
            if (!DateTime.TryParse(to, out toDate)) toDate = DateTime.Now;
            try
            {
                return _context.Stocks.Where(
                    s => s.StockId == stockId &&
                    s.Date >= fromDate &&
                    s.Date <= toDate).Take(2000);
            }
            catch(Exception ex)
            {
                _logger.LogError($"ERROR - {ex.Message}");
                return null;
            }
        }

        public IEnumerable<StockDaily> GetStockDaily(int stockId, string period)
        {
            try
            {
                var dailyStocks = _context.StocksDaily.Where(
                    s => s.StockId == stockId && (
                     ((s.Date.Hour * 100 + s.Date.Minute >= 1430) && (s.Date.Hour * 100 + s.Date.Minute < 2105))));
                if (period == "d")
                {
                    var maxDate = _context.StocksDaily.Where(s => s.StockId == stockId).Max(d => d.Date);
                    return dailyStocks.Where(d => d.Date.Year == maxDate.Year && d.Date.Month == maxDate.Month && d.Date.Day == maxDate.Day);
                }
                else
                {
                    return dailyStocks.Where(d => d.Date >= DateTime.Now.AddDays(-7));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR - {ex.Message}");
                return null;
            }
        }

        public void Add(Stock[] stocks)
        {
            _context.Stocks.AddRange(stocks);
            _context.SaveChanges();
        }

        public async Task UpdatePrices(int stockId, string symbol)
        {
            try
            {
                var dataProvider = new FMProvider();
                var lastPriceDate = _context.Stocks.Where(s => s.StockId == stockId).Max(d => d.Date).AddDays(1);
                var fmStocks = await dataProvider.GetHistoricalPrices(symbol, lastPriceDate);
                if (fmStocks != null)
                {
                    foreach (var fmStock in fmStocks)
                    {
                        _context.Add(new Stock()
                        {
                            StockId = stockId,
                            Close = fmStock.close,
                            Open = fmStock.open,
                            Low = fmStock.low,
                            High = fmStock.high,
                            Volume = (long)fmStock.volume,
                            Date = DateTime.Parse(fmStock.date)
                        });
                    }
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("ERROR " + ex.Message);
            }
        }

        public async Task Split(int stockId, float value)
        {
            string query = $"UPDATE Stocks SET [Open] = [Open] / {value}, High = High / {value}, Low = Low / {value}, [Close] = [Close] / {value} WHERE StockId = {stockId}";
            await _context.Database.ExecuteSqlRawAsync(query);
            _logger.LogWarning($"{stockId} split by {value}");
        }
    }
}
