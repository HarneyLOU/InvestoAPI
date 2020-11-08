using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InvestoAPI.Core.Interfaces
{
    public interface IStockService
    {
        Stock GetStockCurrent(string symbol);
        IEnumerable<Stock> GetStockHistory(string symbol, string from, string to, string interval = "d");
    }
}
