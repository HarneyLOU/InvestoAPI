using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int WalletId { get; set; }
        public Wallet Wallet { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public ICollection<Transaction> Transactions { get; set; }

        public int Amount { get; set; }

        public bool Buy { get; set; }

        public bool Active { get; set; }

        public DateTime Created { get; set; }
    }
}
