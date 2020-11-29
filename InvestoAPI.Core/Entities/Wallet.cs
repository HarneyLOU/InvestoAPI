using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class Wallet
    {
        [Key]
        public int WalletId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }

        public ICollection<WalletState> Possesions { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal InitMoney { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Balance { get; set; }

        public DateTime Created { get; set; }
    }
}
