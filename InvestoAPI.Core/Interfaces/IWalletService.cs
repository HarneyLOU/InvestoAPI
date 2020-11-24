using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Interfaces
{
    public interface IWalletService
    {
        Wallet GetWallet(int id);
        IEnumerable<Wallet> GetWallets(int userId);
        void AddWallet(Wallet wallet);
        void UpdateWallet(Wallet wallet);
        void DeleteWallet(int id);
    }
}
