using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Web.ViewModels
{
    public class WalletViewModel
    {
        public int WalletId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int OwnerId { get; set; }

        public decimal InitMoney { get; set; }

        public decimal Balance { get; set; }

        public ICollection<WalletStateViewModel> Possesions { get; set; }

        public DateTime Created { get; set; }
    }
}
