using InvestoAPI.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int WalletId { get; set; }
        public Wallet Wallet { get; set; }

        public int StockId { get; set; }

        [ForeignKey("StockId")]
        public Company Company { get; set; }

        public ICollection<Transaction> Transactions { get; set; }

        public int Amount { get; set; }

        public bool Buy { get; set; }

        public bool Active { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? Limit { get; set; }

        public DateTimeOffset? ActivationDate { get; set; }

        public DateTimeOffset? ExpiryDate { get; set; }

        public OrderStatusEnum StatusEnum { get; set; }

        public string Status { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}
