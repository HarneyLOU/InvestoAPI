using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Interfaces
{
    public interface IWalletStateService
    {
        Order UpsertWalletState(Transaction transaction);
    }
}
