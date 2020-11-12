using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class StockTrade
    {
        public string Symbol { get; set; }

        public decimal Price { get; set; }

        public DateTime Date { get; set; }
    }
}
