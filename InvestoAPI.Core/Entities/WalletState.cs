using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class WalletState
    {
        public int WalletId { get; set; }

        public Wallet Wallet { get; set; }

        public int StockId { get; set; }

        public Company Stock { get; set; }

        public int Amount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal AveragePrice { get; set; }

        public DateTime Updated { get; set; }
    }
}
