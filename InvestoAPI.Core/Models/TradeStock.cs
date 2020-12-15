using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Models
{
    public class TradeStock
    {
        public string Symbol { get; set; }

        public decimal Price { get; set; }

        public decimal Low { get; set; }

        public decimal High { get; set; }

        public DateTime Date { get; set; }
    }
}
