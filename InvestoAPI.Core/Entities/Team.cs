using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        public Guid Code { get; set; }

        public string Name { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }

        public ICollection<TeamUser> Members { get; set; }

        public ICollection<Wallet> Wallets { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}
