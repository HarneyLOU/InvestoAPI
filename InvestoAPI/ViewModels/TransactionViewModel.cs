using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Web.ViewModels
{
    public class TransactionViewModel
    {
        public int TransactionId { get; set; }

        public int OrderId { get; set; }

        public int Amount { get; set; }

        public decimal Price { get; set; }

        public DateTime Realised { get; set; }
    }
}
