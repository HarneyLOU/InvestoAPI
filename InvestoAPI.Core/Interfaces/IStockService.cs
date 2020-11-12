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
        IEnumerable<Stock> GetStockHistory(string symbol, string from, string to, string interval = "d");
    }
}
