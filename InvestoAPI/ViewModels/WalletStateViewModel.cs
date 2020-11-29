using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Web.ViewModels
{
    public class WalletStateViewModel
    {
        public string Symbol { get; set; }

        public int Amount { get; set; }

        public decimal AveragePrice { get; set; }
    }
}
