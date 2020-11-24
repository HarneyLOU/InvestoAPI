using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; }

        public int Amount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }

        public DateTime Realised { get; set; }
    }
}
