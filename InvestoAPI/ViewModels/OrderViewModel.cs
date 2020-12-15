using System;
using System.Collections.Generic;

namespace InvestoAPI.Web.ViewModels
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }

        public int WalletId { get; set; }

        public int StockId { get; set; }

        public string Symbol { get; set; }

        public IEnumerable<TransactionViewModel> Transactions {get; set;}

        public int Amount { get; set; }

        public bool Buy { get; set; }

        public decimal? Limit { get; set; }

        public DateTimeOffset? ActivationDate { get; set; }

        public DateTimeOffset? ExpiryDate { get; set; }

        public string Status { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}
