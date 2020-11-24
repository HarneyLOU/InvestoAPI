using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvestoAPI.Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly ILogger _logger;
        private readonly ApplicationContext _context;

        public WalletService(
            ILogger<WalletService> logger,
            ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void AddWallet(Wallet wallet)
        {
            _context.Wallets.Add(wallet);
            _context.SaveChanges();
        }

        public void DeleteWallet(int id)
        {
            var wallet = _context.Wallets.SingleOrDefault(x => x.WalletId == id);
            _context.Wallets.Remove(wallet);
            _context.SaveChanges();
        }

        public Wallet GetWallet(int id)
        {
            return _context.Wallets.SingleOrDefault(x => x.WalletId == id);
        }

        public IEnumerable<Wallet> GetWallets(int userId)
        {
            return _context.Wallets.Where(x => x.OwnerId == userId);
        }

        public void UpdateWallet(Wallet wallet)
        {
            var walletToUpdate = GetWallet(wallet.WalletId);
            walletToUpdate.Name = wallet.Name;
            walletToUpdate.Description = wallet.Description;
            _context.Wallets.Update(walletToUpdate);
            _context.SaveChanges();
        }
    }
}
