using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InvestoAPI.Core.Interfaces
{
    public interface IStockService
    {
        DateTime GetMaxDate(string symbol);
        Stock GetStock(string symbol, DateTime? date);
        IEnumerable<StockDaily> GetStockDailyCurrentAll();
        IEnumerable<Stock> GetStockHistory(int stockId, string from, string to, string interval);
        IEnumerable<StockDaily> GetStockDaily(int stockId, string period);
        Task UpdatePrices(int stockId, string symbol);
        Task Split(int stockId, float value);
    }
}
