using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Web.ViewModels
{
    public class StockShortViewModel
    {
        public int StockId { get; set; }

        public string Symbol { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Image { get; set; }

        public decimal Change { get; set; }

        public decimal Open { get; set; }

        public decimal PrevClose { get; set; }

        public decimal Low { get; set; }

        public decimal High { get; set; }

        public DateTime Date { get; set; }
    }
}
